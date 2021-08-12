namespace Hagi.Shared.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static async Task<TResult> Timeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using CancellationTokenSource timeoutCancellation = new CancellationTokenSource();
            Task completed = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellation.Token));

            if (completed != task)
            {
                throw new TimeoutException();
            }

            timeoutCancellation.Cancel();
            return await task;
        }
        public static async Task Timeout(this Task task, TimeSpan timeout)
        {
            using CancellationTokenSource timeoutCancellation = new CancellationTokenSource();
            Task completed = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellation.Token));

            if (completed != task)
            {
                throw new TimeoutException();
            }

            timeoutCancellation.Cancel();
            await task;
        }
    }
}