﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Ses.Abstracts;
using Ses.Abstracts.Extensions;

namespace Ses.MsSql
{
    internal partial class MsSqlPersistor
    {
        public async Task<IEvent[]> LoadAsync(Guid streamId, int fromVersion, bool pessimisticLock, CancellationToken cancellationToken = new CancellationToken())
        {
            List<IEvent> list = null;
            using (var cnn = new SqlConnection(_connectionString))
            {
                using (var cmd = await cnn.OpenAndCreateCommandAsync(SqlQueries.SelectEvents.Query, cancellationToken).NotOnCapturedContext())
                {
                    cmd
                        .AddInputParam(SqlQueries.SelectEvents.ParamStreamId, DbType.Guid, streamId)
                        .AddInputParam(SqlQueries.SelectEvents.ParamFromVersion, DbType.Int32, fromVersion)
                        .AddInputParam(SqlQueries.SelectEvents.ParamPessimisticLock, DbType.Boolean, pessimisticLock);

                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken).NotOnCapturedContext())
                    {
                        if (reader.HasRows) list = new List<IEvent>(100);

                        while (await reader.ReadAsync(cancellationToken).NotOnCapturedContext()) // read snapshot
                        {
                            if (reader[0] == DBNull.Value) break;

                            // ReSharper disable once PossibleNullReferenceException
                            list.Add(OnReadSnapshot(
                                streamId,
                                reader.GetString(0),
                                reader.GetInt32(1),
                                (byte[])reader[2]));
                        }

                        await reader.NextResultAsync(cancellationToken).NotOnCapturedContext();

                        if (list == null && reader.HasRows) list = new List<IEvent>(30);

                        while (await reader.ReadAsync(cancellationToken).NotOnCapturedContext()) // read events
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            list.Add(OnReadEvent(
                                streamId,
                                reader.GetString(0),
                                reader.GetInt32(1),
                                (byte[])reader[2]));
                        }
                    }
                }
            }
            return list?.ToArray() ?? new IEvent[0];
        }

        public async Task DeleteStreamAsync(Guid streamId, int expectedVersion, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                var query = expectedVersion == ExpectedVersion.Any
                    ? SqlQueries.DeleteStream.QueryAny
                    : SqlQueries.DeleteStream.QueryExpectedVersion;

                using (var cmd = await cnn.OpenAndCreateCommandAsync(query, cancellationToken).NotOnCapturedContext())
                {
                    try
                    {
                        cmd.AddInputParam(SqlQueries.DeleteStream.ParamStreamId, DbType.Guid, streamId);
                        if(expectedVersion != ExpectedVersion.Any)
                            cmd.AddInputParam(SqlQueries.DeleteStream.ParamExpectedVersion, DbType.Int32, expectedVersion);

                        await cmd.ExecuteNonQueryAsync(cancellationToken)
                            .NotOnCapturedContext();
                    }
                    catch (SqlException e)
                    {
                        if (e.Message.StartsWith("WrongExpectedVersion"))
                        {
                            throw new WrongExpectedVersionException($"Deleting stream {streamId} error", e);
                        }
                        throw;
                    }
                }
            }
        }

        public async Task UpdateSnapshotAsync(Guid streamId, int version, string contractName, byte[] payload, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    using (var cmd = await cnn.OpenAndCreateCommandAsync(SqlQueries.UpdateSnapshot.Query, cancellationToken).NotOnCapturedContext())
                    {
                        await cmd
                            .AddInputParam(SqlQueries.UpdateSnapshot.ParamStreamId, DbType.Guid, streamId)
                            .AddInputParam(SqlQueries.UpdateSnapshot.ParamVersion, DbType.Int32, version)
                            .AddInputParam(SqlQueries.UpdateSnapshot.ParamContractName, DbType.AnsiString, contractName)
                            .AddInputParam(SqlQueries.UpdateSnapshot.ParamGeneratedAtUtc, DbType.DateTime, DateTime.UtcNow)
                            .AddInputParam(SqlQueries.UpdateSnapshot.ParamPayload, DbType.Binary, payload)
                            .ExecuteNonQueryAsync(cancellationToken)
                            .NotOnCapturedContext();
                    }
                }
                catch (SqlException e)
                {
                    if (e.Message.StartsWith("WrongExpectedVersion"))
                    {
                        throw new WrongExpectedVersionException($"Updating snapshot for stream {streamId} error", e);
                    }
                    throw;
                }
            }
        }

        public async Task SaveChangesAsync(Guid streamId, Guid commitId, int expectedVersion, EventRecord[] events, byte[] metadata, bool isLockable, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                switch (expectedVersion)
                {
                    case ExpectedVersion.NoStream:
                        await SaveChangesNoStreamAsync(cnn, events, streamId, commitId, metadata, isLockable, cancellationToken);
                        break;
                    case ExpectedVersion.Any:
                        await SaveChangesAnyAsync(cnn, events, streamId, commitId, metadata, isLockable, cancellationToken);
                        break;
                    default:
                        await SaveChangesExpectedVersionAsync(cnn, events, streamId, commitId, expectedVersion, metadata, cancellationToken);
                        break;
                }
            }

            _linearizer?.Start();
        }

        private static async Task SaveChangesNoStreamAsync(SqlConnection cnn, EventRecord[] events, Guid streamId, Guid commitId, byte[] metadata, bool isLockable, CancellationToken cancellationToken)
        {
            try
            {
                using (var cmd = await cnn.OpenAndCreateCommandAsync(SqlQueries.InsertEvents.QueryNoStream, cancellationToken).NotOnCapturedContext())
                {
                    cmd
                        .AddInputParam(SqlQueries.InsertEvents.ParamStreamId, DbType.Guid, streamId)
                        .AddInputParam(SqlQueries.InsertEvents.ParamCommitId, DbType.Guid, commitId)
                        .AddInputParam(SqlQueries.InsertEvents.ParamCreatedAtUtc, DbType.DateTime, DateTime.UtcNow)
                        .AddInputParam(SqlQueries.InsertEvents.ParamMetadataPayload, DbType.Binary, metadata, true)
                        .AddInputParam(SqlQueries.InsertEvents.ParamIsLockable, DbType.Boolean, isLockable)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventContractName, DbType.String, null)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventVersion, DbType.Int32, null)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventPayload, DbType.Binary, null);

                    foreach (var record in events)
                    {
                        cmd.Parameters[5].Value = record.ContractName;
                        cmd.Parameters[6].Value = record.Version;
                        cmd.Parameters[7].Value = record.Payload;

                        await cmd.ExecuteNonQueryAsync(cancellationToken)
                            .NotOnCapturedContext();

                        cmd.Parameters[3].Value = DBNull.Value; // metadata is one per CommitId
                    }
                }
            }
            catch (SqlException e)
            {
                if (e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                {
                    throw new WrongExpectedVersionException($"Saving new stream {streamId} error. Stream exists.", e);
                }
                throw;
            }
        }

        private static async Task SaveChangesAnyAsync(SqlConnection cnn, EventRecord[] events, Guid streamId, Guid commitId, byte[] metadata, bool isLockable, CancellationToken cancellationToken)
        {
            try
            {
                using (var cmd = await cnn.OpenAndCreateCommandAsync(SqlQueries.InsertEvents.QueryAny, cancellationToken).NotOnCapturedContext())
                {
                    cmd
                        .AddInputParam(SqlQueries.InsertEvents.ParamStreamId, DbType.Guid, streamId)
                        .AddInputParam(SqlQueries.InsertEvents.ParamCommitId, DbType.Guid, commitId)
                        .AddInputParam(SqlQueries.InsertEvents.ParamCreatedAtUtc, DbType.DateTime, DateTime.UtcNow)
                        .AddInputParam(SqlQueries.InsertEvents.ParamMetadataPayload, DbType.Binary, metadata, true)
                        .AddInputParam(SqlQueries.InsertEvents.ParamIsLockable, DbType.Boolean, isLockable)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventContractName, DbType.String, null)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventVersion, DbType.Int32, null)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventPayload, DbType.Binary, null);

                    foreach (var record in events)
                    {
                        cmd.Parameters[5].Value = record.ContractName;
                        cmd.Parameters[6].Value = record.Version;
                        cmd.Parameters[7].Value = record.Payload;

                        await cmd.ExecuteNonQueryAsync(cancellationToken)
                            .NotOnCapturedContext();

                        cmd.Parameters[3].Value = DBNull.Value; // metadata is one per CommitId
                    }
                }
            }
            catch (SqlException e)
            {
                // TODO: check concurrency violation
                if(e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                {
                    throw new WrongExpectedVersionException($"Saving new or existing stream {streamId} error. Stream exists.", e);
                }
                throw;
            }
        }

        private static async Task SaveChangesExpectedVersionAsync(SqlConnection cnn, EventRecord[] events, Guid streamId, Guid commitId, int expectedVersion, byte[] metadata, CancellationToken cancellationToken)
        {
            try
            {
                using (var cmd = await cnn.OpenAndCreateCommandAsync(SqlQueries.InsertEvents.QueryExpectedVersion, cancellationToken).NotOnCapturedContext())
                {
                    cmd
                        .AddInputParam(SqlQueries.InsertEvents.ParamStreamId, DbType.Guid, streamId)
                        .AddInputParam(SqlQueries.InsertEvents.ParamCommitId, DbType.Guid, commitId)
                        .AddInputParam(SqlQueries.InsertEvents.ParamCreatedAtUtc, DbType.DateTime, DateTime.UtcNow)
                        .AddInputParam(SqlQueries.InsertEvents.ParamMetadataPayload, DbType.Binary, metadata, true)
                        .AddInputParam(SqlQueries.InsertEvents.ParamExpectedVersion, DbType.Int32, expectedVersion)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventContractName, DbType.String, null)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventVersion, DbType.Int32, null)
                        .AddInputParam(SqlQueries.InsertEvents.ParamEventPayload, DbType.Binary, null);

                    foreach (var record in events)
                    {
                        cmd.Parameters[5].Value = record.ContractName;
                        cmd.Parameters[6].Value = record.Version;
                        cmd.Parameters[7].Value = record.Payload;

                        await cmd.ExecuteNonQueryAsync(cancellationToken)
                            .NotOnCapturedContext();

                        cmd.Parameters[3].Value = DBNull.Value; // metadata is one per CommitId
                    }
                }
            }
            catch (SqlException e)
            {
                // TODO: check concurrency violation
                if (e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                {
                    throw new WrongExpectedVersionException($"Saving new or existing stream {streamId} error. Stream exists.", e);
                }
                throw;
            }
        }
    }
}
