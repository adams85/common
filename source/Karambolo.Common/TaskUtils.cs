using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Karambolo.Common
{
    public static class TaskUtils
    {
#if NET40 || NET45 || NETSTANDARD1_0
        internal static readonly TaskCreationOptions DefaultTcsCreationOptions = TaskCreationOptions.None;

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static Task<TResult> GetTaskSafe<TResult>(this TaskCompletionSource<TResult> tcs)
        {
            // TCSs should be created with the TaskCreationOptions.RunContinuationsAsynchronously flag
            // (https://blogs.msdn.microsoft.com/seteplia/2018/10/01/the-danger-of-taskcompletionsourcet-class/);
            // the flag is only available as of .NET 4.6 (and is buggy up to 4.6.1) so we resort to async task continuations
            // (https://stackoverflow.com/questions/22579206/how-can-i-prevent-synchronous-continuations-on-a-task)
            return tcs.Task.ContinueWith(CachedDelegates.Identity<Task<TResult>>.Func, default, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
        }
#else
        internal static readonly TaskCreationOptions DefaultTcsCreationOptions = TaskCreationOptions.RunContinuationsAsynchronously;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task<TResult> GetTaskSafe<TResult>(this TaskCompletionSource<TResult> tcs)
        {
            return tcs.Task;
        }
#endif

#if NET40 || NET45 || NETSTANDARD1_0
        public static Task CompletedTask => CachedTasks.Default<object>.Task;

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
        public static Task CompletedTask 
        {
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get => Task.CompletedTask;
        }

        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Task FromException(Exception exception)
        {
            return Task.FromException(exception);
        }

        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            return Task.FromException<TResult>(exception);
        }

        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Task FromCanceled(CancellationToken cancellationToken)
        {
            return Task.FromCanceled(cancellationToken);
        }

        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
        {
            return Task.FromCanceled<TResult>(cancellationToken);
        }
#endif

#if NET40
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetResult(result);
            return tcs.Task;
        }
#else
        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            return Task.FromResult(result);
        }
#endif

#if !NETSTANDARD1_0
        public static Task WaitForExitAsync(this Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            var tcs = new TaskCompletionSource<object>(DefaultTcsCreationOptions);

            EventHandler exitedHandler = null;
            exitedHandler = (s, e) =>
            {
                process.Exited -= exitedHandler;
                tcs.TrySetResult(null);
            };

            process.EnableRaisingEvents = true;
            process.Exited += exitedHandler;

            return tcs.GetTaskSafe();
        }
#endif

#if !NET40
#region Timeout

        internal static async Task WithTimeoutAsync(Task task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            Task completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);

            if (completedTask == timeoutTask)
                throw new TimeoutException();

#pragma warning disable CAC001 // ConfigureAwaitChecker
            // ConfigureAwait(false) is unnecessary as the task returned by Task.WhenAny is already completed
            await completedTask;
#pragma warning restore CAC001 // ConfigureAwaitChecker
        }

        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return WithTimeoutAsync(task, timeout);
        }

        public static Task WithTimeout(this Task task, int millisecondsTimeout)
        {
            return task.WithTimeout(TimeSpan.FromTicks(millisecondsTimeout * TimeSpan.TicksPerMillisecond));
        }

        internal static async Task<TResult> WithTimeoutAsync<TResult>(Task<TResult> task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            Task completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);

            if (completedTask == timeoutTask)
                throw new TimeoutException();

#pragma warning disable CAC001 // ConfigureAwaitChecker
            // ConfigureAwait(false) is unnecessary as the task returned by Task.WhenAny is already completed
            return await (Task<TResult>)completedTask;
#pragma warning restore CAC001 // ConfigureAwaitChecker
        }

        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return WithTimeoutAsync(task, timeout);
        }

        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, int millisecondsTimeout)
        {
            return task.WithTimeout(TimeSpan.FromTicks(millisecondsTimeout * TimeSpan.TicksPerMillisecond));

        }

#endregion

#region Cancellation

        private readonly struct CancellationTokenTaskSource<TResult> : IDisposable
        {
            private readonly IDisposable _ctr;

            public CancellationTokenTaskSource(CancellationToken cancellationToken)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    var tcs = new TaskCompletionSource<TResult>(DefaultTcsCreationOptions);
                    _ctr = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
                    Task = tcs.GetTaskSafe();
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
                // ConfigureAwait(false) is unnecessary as the task returned by Task.WhenAny is already completed
                await await Task.WhenAny(ctts.Task, task).ConfigureAwait(false);
        }

        public static Task AsCancelable(this Task task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return FromCanceled<object>(cancellationToken);

            return AsCancelableAsync(task, cancellationToken);
        }

        internal static async Task<TResult> AsCancelableAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
        {
            using (var ctts = new CancellationTokenTaskSource<TResult>(cancellationToken))
                // ConfigureAwait(false) is unnecessary as the task returned by Task.WhenAny is already completed
                return await await Task.WhenAny(ctts.Task, task).ConfigureAwait(false);
        }

        public static Task<TResult> AsCancelable<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return FromCanceled<TResult>(cancellationToken);

            return AsCancelableAsync(task, cancellationToken);
        }

#endregion

        public static async void FireAndForget(this Task task, Action<Exception> exceptionHandler, bool propagateCancellation = false)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            try { await task.ConfigureAwait(false); }
            catch (OperationCanceledException) when (!propagateCancellation) { }
            catch (Exception ex) when (exceptionHandler != null) { exceptionHandler(ex); }
        }

        public static void WaitAndUnwrap(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.GetAwaiter().GetResult();
        }

        public static TResult WaitAndUnwrap<TResult>(this Task<TResult> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return task.GetAwaiter().GetResult();
        }
#else
        public static void FireAndForget(this Task task, Action<Exception> exceptionHandler, bool propagateCancellation = false)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.ContinueWith(t =>
            {
                Exception ex;
                if (t.Status == TaskStatus.Canceled)
                {
                    if (!propagateCancellation)
                        return;

                    ex = new TaskCanceledException(t);
                }
                else
                    ex = t.Exception;

                if (exceptionHandler != null)
                {
                    exceptionHandler(ex);
                    return;
                }

                throw ex;
            }, TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
        }

        private static TResult WaitAndUnwrap<TResult>(this Task<TResult> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return task.Result;
        }
#endif

        public static void FireAndForget(this Task task, bool propagateCancellation = false)
        {
            task.FireAndForget(null, propagateCancellation);
        }

        public static IAsyncResult BeginExecuteTask(this Task task, AsyncCallback callback, object state)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var tcs = new TaskCompletionSource<Task>(state, DefaultTcsCreationOptions);

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

            return tcs.GetTaskSafe();
        }

        public static Task EndExecuteTask(this IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult));

            return ((Task<Task>)asyncResult).WaitAndUnwrap();
        }
    }
}
