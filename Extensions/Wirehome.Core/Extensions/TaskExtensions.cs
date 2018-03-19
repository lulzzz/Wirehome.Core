﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wirehome.Core.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<Task> WhenAll(this IEnumerable<Task> tasks, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var timeoutTask = Task.Delay(timeout, cancellationToken);
            var workingTask = Task.WhenAll(tasks);
            var result = await Task.WhenAny(timeoutTask, workingTask);

            if (result == timeoutTask)
            {
                if (cancellationToken.IsCancellationRequested && timeoutTask.Status == TaskStatus.Canceled)
                {
                    throw new OperationCanceledException();
                }
                if (timeoutTask.Status == TaskStatus.RanToCompletion && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }

                throw new InvalidOperationException("Not supported result in WhenAll");
            }

            return result;
        }

        public static async Task<Task> WhenAll(this IEnumerable<Task> tasks, CancellationToken cancellationToken)
        {
            var cancellTask = cancellationToken.WhenCanceled();
            var workingTask = Task.WhenAll(tasks);
            var result = await Task.WhenAny(cancellTask, workingTask);

            if (result == cancellTask)
            {
                throw new OperationCanceledException();
            }

            return result;
        }

        public static async Task<R> WhenAny<R>(this IEnumerable<Task> tasks, TimeSpan timeout, CancellationToken cancellationToken) where R : class
        {
            var timeoutTask = Task.Delay(timeout, cancellationToken);
            var result = await Task.WhenAny(tasks.ToList().AddChained(timeoutTask));

            if (result == timeoutTask)
            {
                if (cancellationToken.IsCancellationRequested && timeoutTask.Status == TaskStatus.Canceled)
                {
                    throw new OperationCanceledException();
                }
                if (timeoutTask.Status == TaskStatus.RanToCompletion && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }

                throw new InvalidOperationException("Not supported result in WhenAll");
            }

            return (result as Task<R>)?.Result;
        }

        public static async Task<R> WhenDone<R>(this Task<R> task, TimeSpan timeout, CancellationToken cancellationToken) where R : class
        {
            var timeoutTask = Task.Delay(timeout, cancellationToken);
            var result = await Task.WhenAny(new Task[] { task, timeoutTask });

            if (result == timeoutTask)
            {
                if (cancellationToken.IsCancellationRequested && timeoutTask.Status == TaskStatus.Canceled)
                {
                    throw new OperationCanceledException();
                }
                if (timeoutTask.Status == TaskStatus.RanToCompletion && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }

                throw new InvalidOperationException("Not supported result in Timeout");
            }
            
            return (result as Task<R>)?.Result;
        }

        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

    }
}