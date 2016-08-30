﻿using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ses.Abstracts.Extensions
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable<T> NotOnCapturedContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable NotOnCapturedContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static void SwallowException(this Task task)
        {
            task.ContinueWith(_ => { });
        }
    }
}
