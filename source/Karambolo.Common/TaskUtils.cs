using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Karambolo.Common
{
    public static partial class TaskUtils
    {
#if NET40
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetResult(result);
            return tcs.Task;
        }
#endif

#if NET40 || NET45
        public static Task FromException(Exception exception)
        {
            return FromException<object>(exception);
        }

        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetException(exception);
            return tcs.Task;
        }

        public static Task FromCancelled(CancellationToken cancellationToken)
        {
            return FromCancelled<object>(cancellationToken);
        }

        public static Task<TResult> FromCancelled<TResult>(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                throw new ArgumentOutOfRangeException(nameof(cancellationToken));

            var tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetCanceled();
            return tcs.Task;
        }
#endif

        public static IAsyncResult BeginExecuteTask(this Task task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<Task>(state);

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(t);

                callback?.Invoke(tcs.Task);
            }, TaskScheduler.Default);

            return tcs.Task;
        }

        public static Task EndExecuteTask(this IAsyncResult asyncResult)
        {
#if !NET40
            return ((Task<Task>)asyncResult).WaitAndUnwrap();
#else
            return ((Task<Task>)asyncResult).Result;
#endif
        }

#if !NETSTANDARD1_0
        public static Task WaitForExitAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<object>();
            EventHandler exitedHandler = null;
            exitedHandler = (s, e) =>
            {
                process.Exited -= exitedHandler;
                tcs.TrySetResult(null);
            };

            process.EnableRaisingEvents = true;
            process.Exited += exitedHandler;

            return tcs.Task;
        }
#endif
    }

#if !NET40
    public static partial class TaskUtils
    {
        #region Timeout
        static async Task WithTimeoutAsync(Task task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);

            if (completedTask == timeoutTask)
                throw new TimeoutException();

            await completedTask.ConfigureAwait(false);
        }

        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            if (task == null)
                throw new NullReferenceException();

            return WithTimeoutAsync(task, timeout);
        }

        static async Task<TResult> WithTimeoutAsync<TResult>(Task<TResult> task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);

            if (completedTask == timeoutTask)
                throw new TimeoutException();

            return await ((Task<TResult>)completedTask).ConfigureAwait(false);
        }

        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (task == null)
                throw new NullReferenceException();

            return WithTimeoutAsync(task, timeout);
        }
        #endregion

        #region Cancellation
        sealed class CancellationTokenTaskSource<TResult> : IDisposable
        {
            readonly IDisposable _ctr;

            public CancellationTokenTaskSource(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Task = Cancelled<TResult>();
                    return;
                }
                var tcs = new TaskCompletionSource<TResult>();
                _ctr = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
                Task = tcs.Task;
            }

            public Task<TResult> Task { get; private set; }

            public void Dispose()
            {
                _ctr?.Dispose();
            }
        }

        static Task<TResult> Cancelled<TResult>()
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        static async Task AsCancellableAsync(Task task, CancellationToken cancellationToken)
        {
            using (var ctts = new CancellationTokenTaskSource<object>(cancellationToken))
                await await Task.WhenAny(task, ctts.Task).ConfigureAwait(false);
        }

        public static Task AsCancellable(this Task task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return Cancelled<object>();

            return AsCancellableAsync(task, cancellationToken);
        }

        static async Task<TResult> AsCancellableAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
        {
            using (var ctts = new CancellationTokenTaskSource<TResult>(cancellationToken))
                return await await Task.WhenAny(task, ctts.Task).ConfigureAwait(false);
        }

        public static Task<TResult> AsCancellable<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return Cancelled<TResult>();

            return AsCancellableAsync(task, cancellationToken);
        }
        #endregion

        public static void WaitAndUnwrap(this Task task)
        {
            if (task == null)
                throw new NullReferenceException();

            task.GetAwaiter().GetResult();
        }

        public static TResult WaitAndUnwrap<TResult>(this Task<TResult> task)
        {
            if (task == null)
                throw new NullReferenceException();

            return task.GetAwaiter().GetResult();
        }
    }
#endif
}
