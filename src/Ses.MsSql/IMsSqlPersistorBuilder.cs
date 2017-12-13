﻿using System;
using System.Threading.Tasks;

namespace Ses.MsSql
{
    public interface IMsSqlPersistorBuilder
    {
        void Destroy(bool ignoreErrors = false);
        void Initialize();
        void RunLinearizer(TimeSpan timeout, TimeSpan? durationWork = null, int batchSize = 500);
        void RunLinearizerNow();
        Task RunLinearizerOnce();
    }
}