using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Karambolo.Common
{
    public class TaskUtilsTest
    {
#if NET45
        private static readonly Task s_completedTask = TaskUtils.CompletedTask;

        private static Task FromException(Exception exception) => FromException(exception);
        private static Task<TResult> FromException<TResult>(Exception exception) => FromException<TResult>(exception);

        private static Task FromCanceled(CancellationToken cancellationToken) => FromCanceled(cancellationToken);
        private static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) => FromCanceled<TResult>(cancellationToken);
#else
        private static readonly Task s_completedTask = Task.CompletedTask;

        private static Task FromException(Exception exception) => Task.FromException(exception);
        private static Task<TResult> FromException<TResult>(Exception exception) => Task.FromException<TResult>(exception);

        private static Task FromCanceled(CancellationToken cancellationToken) => Task.FromCanceled(cancellationToken);
        private static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) => Task.FromCanceled<TResult>(cancellationToken);
#endif

        private class TestException : Exception { }

#if NET45 || NET461 || NETCOREAPP1_0
        [Fact]
        public void CompletedTaskTest()
        {
            Assert.Equal(TaskStatus.RanToCompletion, TaskUtils.CompletedTask.Status);
        }

        [Fact]
        public async Task FromExceptionTest()
        {
            Task task = FromException(new TestException());
            Assert.Equal(TaskStatus.Faulted, task.Status);
            await Assert.ThrowsAsync<TestException>(() => task);

            Task<int> taskWithResult = FromException<int>(new TestException());
            Assert.Equal(TaskStatus.Faulted, taskWithResult.Status);
            await Assert.ThrowsAsync<TestException>(() => taskWithResult);
        }

        [Fact]
        public async Task FromCanceledTest()
        {
            using (var cts = new CancellationTokenSource())
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => FromCanceled(cts.Token));
                cts.Cancel();
                Task task = FromCanceled(cts.Token);
                Assert.Equal(TaskStatus.Canceled, task.Status);
                await Assert.ThrowsAsync<TaskCanceledException>(() => task);
            }

            using (var cts = new CancellationTokenSource())
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => FromCanceled(cts.Token));
                cts.Cancel();
                Task taskWithResult = FromCanceled(cts.Token);
                Assert.Equal(TaskStatus.Canceled, taskWithResult.Status);
                await Assert.ThrowsAsync<TaskCanceledException>(() => taskWithResult);
            }
        }
#endif

#if NET45 || NET461
        [Fact]
        public async Task WaitForExitAsyncTest()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd", "/c echo test")
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            Assert.True(process.HasExited);

            var output = process.StandardOutput.ReadToEnd();
            Assert.Equal("test", output.Trim());
        }
#endif

        [Fact]
        public async Task WithTimeoutTest()
        {
            #region Task

            await Task.Delay(100).WithTimeout(1000);
            await Assert.ThrowsAsync<TimeoutException>(() => Task.Delay(1000).WithTimeout(100));

            #endregion

            #region Task<TResult>

            Assert.True(await Task.Delay(100).ContinueWith(t => true, TaskContinuationOptions.ExecuteSynchronously).WithTimeout(1000));
            await Assert.ThrowsAsync<TimeoutException>(() => Task.Delay(1000).ContinueWith(t => true, TaskContinuationOptions.ExecuteSynchronously).WithTimeout(100));

            #endregion
        }

        [Fact]
        public async Task AsCancelableTest()
        {
            #region Task

            await s_completedTask.AsCancelable(CancellationToken.None);

            var cts = new CancellationTokenSource();
            await s_completedTask.AsCancelable(cts.Token);

            cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() => s_completedTask.AsCancelable(cts.Token));
            await Assert.ThrowsAsync<TaskCanceledException>(() => TaskUtils.AsCancelableAsync(s_completedTask, cts.Token));

            cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            await Task.Delay(100).AsCancelable(cts.Token);

            cts = new CancellationTokenSource();
            cts.CancelAfter(100);
            await Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(1000).AsCancelable(cts.Token));

            #endregion

            #region Task<TResult>

            Assert.True(await Task.FromResult(true).AsCancelable(CancellationToken.None));

            cts = new CancellationTokenSource();
            Assert.True(await Task.FromResult(true).AsCancelable(cts.Token));

            cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() => Task.FromResult(true).AsCancelable(cts.Token));
            await Assert.ThrowsAsync<TaskCanceledException>(() => TaskUtils.AsCancelableAsync(Task.FromResult(true), cts.Token));

            cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            Assert.True(await Task.Delay(100).ContinueWith(t => true, TaskContinuationOptions.ExecuteSynchronously).AsCancelable(cts.Token));

            cts = new CancellationTokenSource();
            cts.CancelAfter(100);
            await Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(1000).ContinueWith(t => true, TaskContinuationOptions.ExecuteSynchronously).AsCancelable(cts.Token));

            #endregion
        }

        [Fact]
        public void WaitAndUnwrapTest()
        {
            #region Task

            s_completedTask.WaitAndUnwrap();

            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.Throws<TaskCanceledException>(() => FromCanceled(cts.Token).WaitAndUnwrap());

            Assert.Throws<TestException>(() => FromException(new TestException()).WaitAndUnwrap());

            #endregion

            #region Task<TResult>

            Assert.True(Task.FromResult(true).WaitAndUnwrap());

            cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.Throws<TaskCanceledException>(() => FromCanceled<bool>(cts.Token).WaitAndUnwrap());

            Assert.Throws<TestException>(() => FromException<bool>(new TestException()).WaitAndUnwrap());

            #endregion
        }

        [Fact]
        public void FireAndForgetTest()
        {
            using (var cts = new CancellationTokenSource())
            {
                Task.Run(() =>
                {
                    Thread.Sleep(100);
                    cts.Cancel();
                }).FireAndForget();

                Assert.True(cts.Token.WaitHandle.WaitOne(1000));
            }
        }

        [Fact]
        public void ExecuteTaskApmTest()
        {
            int flag = 0;
            IAsyncResult asyncResult = TaskUtils.BeginExecuteTask(Task.Delay(100).ContinueWith(t => flag++, TaskContinuationOptions.ExecuteSynchronously), ar =>
            {
                Assert.Equal("state", ar.AsyncState);
                ar.EndExecuteTask().WaitAndUnwrap();
                Assert.Equal(1, flag);
            }, "state");

            SpinWait.SpinUntil(() => asyncResult.IsCompleted);
        }
    }
}
