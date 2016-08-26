﻿using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ses.Subscriptions.MsSql
{
    internal static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable<T> NotOnCapturedContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable NotOnCapturedContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }
    }
}
