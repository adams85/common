using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Karambolo.Common
{
    public static class TaskUtils
    {
#if NET45 || NETSTANDARD1_0
        public static Task CompletedTask { get; } = Task.FromResult<object>(null);
#elif NET40
        public static Task CompletedTask { get; } = FromResult<object>(null);

        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetResult(result);
            return tcs.Task;
        }
#endif

#if NET40 || NET45 || NETSTANDARD1_0
        public static Task FromException(Exception exception)
        {
            return FromException<object>(exception);
        }

        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static Task FromCanceled(CancellationToken cancellationToken)
        {
            return FromCanceled<object>(cancellationToken);
        }

        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                throw new ArgumentOutOfRangeException(nameof(cancellationToken));

            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetCanceled();
            return tcs.Task;
        }
#else
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
        {
            return Task.FromCanceled<TResult>(cancellationToken);
        }
#endif

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

#if !NET40
        #region Timeout

        internal static async Task WithTimeoutAsync(Task task, TimeSpan timeout)
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

        public static Task WithTimeout(this Task task, int millisecondsTimeout)
        {
            return task.WithTimeout(TimeSpan.FromTicks(millisecondsTimeout * TimeSpan.TicksPerMillisecond));
        }

        internal static async Task<TResult> WithTimeoutAsync<TResult>(Task<TResult> task, TimeSpan timeout)
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

        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, int millisecondsTimeout)
        {
            return task.WithTimeout(TimeSpan.FromTicks(millisecondsTimeout * TimeSpan.TicksPerMillisecond));

        }

        #endregion

        #region Cancellation

        readonly struct CancellationTokenTaskSource<TResult> : IDisposable
        {
            readonly IDisposable _ctr;

            public CancellationTokenTaskSource(CancellationToken cancellationToken)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    var tcs = new TaskCompletionSource<TResult>();
                    _ctr = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
                    Task = tcs.Task;
                }
                else
                {
                    _ctr = null;
                    Task = FromCanceled<TResult>(cancellationToken);
                }
            }

            public Task<TResult> Task { get; }

            public void Dispose()
            {
                _ctr?.Dispose();
            }
        }

        internal static async Task AsCancelableAsync(Task task, CancellationToken cancellationToken)
        {
            using (var ctts = new CancellationTokenTaskSource<object>(cancellationToken))
                await await Task.WhenAny(ctts.Task, task).ConfigureAwait(false);
        }

        public static Task AsCancelable(this Task task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return FromCanceled<object>(cancellationToken);

            return AsCancelableAsync(task, cancellationToken);
        }

        internal static async Task<TResult> AsCancelableAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
        {
            using (var ctts = new CancellationTokenTaskSource<TResult>(cancellationToken))
                return await await Task.WhenAny(ctts.Task, task).ConfigureAwait(false);
        }

        public static Task<TResult> AsCancelable<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return FromCanceled<TResult>(cancellationToken);

            return AsCancelableAsync(task, cancellationToken);
        }

        #endregion

        public static async void FireAndForget(this Task task, Action<Exception> exceptionHandler)
        {
            try { await task.ConfigureAwait(false); }
            catch (Exception ex) { exceptionHandler(ex); }
        }

        public static void WaitAndUnwrap(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        public static TResult WaitAndUnwrap<TResult>(this Task<TResult> task)
        {
            return task.GetAwaiter().GetResult();
        }
#else
        public static void FireAndForget(this Task task, Action<Exception> exceptionHandler)
        {
            task.ContinueWith(t => exceptionHandler(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        static TResult WaitAndUnwrap<TResult>(this Task<TResult> task)
        {
            return task.Result;
        }
#endif

        public static void FireAndForget(this Task task)
        {
            task.FireAndForget(Noop<Exception>.Action);
        }

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
            return ((Task<Task>)asyncResult).WaitAndUnwrap();
        }
    }
}
