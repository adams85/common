using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Karambolo.Common
{
    public class TaskUtilsTest
    {
        class TestException : Exception { }

#if NET461 || NETCOREAPP1_0
        [Fact]
        public void CompletedTaskTest()
        {
            Assert.Equal(TaskStatus.RanToCompletion, TaskUtils.CompletedTask.Status);
        }

        [Fact]
        public async Task FromExceptionTest()
        {
            var task = TaskUtils.FromException(new TestException());
            Assert.Equal(TaskStatus.Faulted, task.Status);
            await Assert.ThrowsAsync<TestException>(() => task);

            var taskWithResult = TaskUtils.FromException<int>(new TestException());
            Assert.Equal(TaskStatus.Faulted, taskWithResult.Status);
            await Assert.ThrowsAsync<TestException>(() => taskWithResult);
        }

        [Fact]
        public async Task FromCanceledTest()
        {
            using (var cts = new CancellationTokenSource())
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TaskUtils.FromCanceled(cts.Token));
                cts.Cancel();
                var task = TaskUtils.FromCanceled(cts.Token);
                Assert.Equal(TaskStatus.Canceled, task.Status);
                await Assert.ThrowsAsync<TaskCanceledException>(() => task);
            }

            using (var cts = new CancellationTokenSource())
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TaskUtils.FromCanceled(cts.Token));
                cts.Cancel();
                var taskWithResult = TaskUtils.FromCanceled(cts.Token);
                Assert.Equal(TaskStatus.Canceled, taskWithResult.Status);
                await Assert.ThrowsAsync<TaskCanceledException>(() => taskWithResult);
            }
        }
#endif

#if NET461
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

            await Task.CompletedTask.AsCancelable(CancellationToken.None);

            var cts = new CancellationTokenSource();
            await Task.CompletedTask.AsCancelable(cts.Token);

            cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() => Task.CompletedTask.AsCancelable(cts.Token));
            await Assert.ThrowsAsync<TaskCanceledException>(() => TaskUtils.AsCancelableAsync(Task.CompletedTask, cts.Token));

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

            Task.CompletedTask.WaitAndUnwrap();

            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.Throws<TaskCanceledException>(() => Task.FromCanceled(cts.Token).WaitAndUnwrap());

            Assert.Throws<TestException>(() => Task.FromException(new TestException()).WaitAndUnwrap());

            #endregion

            #region Task<TResult>

            Assert.True(Task.FromResult(true).WaitAndUnwrap());

            cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.Throws<TaskCanceledException>(() => Task.FromCanceled<bool>(cts.Token).WaitAndUnwrap());

            Assert.Throws<TestException>(() => Task.FromException<bool>(new TestException()).WaitAndUnwrap());

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
            var asyncResult = TaskUtils.BeginExecuteTask(Task.Delay(100).ContinueWith(t => flag++, TaskContinuationOptions.ExecuteSynchronously), ar =>
            {
                Assert.Equal("state", ar.AsyncState);
                ar.EndExecuteTask().WaitAndUnwrap();
                Assert.Equal(1, flag);
            }, "state");

            SpinWait.SpinUntil(() => asyncResult.IsCompleted);
        }
    }
}
