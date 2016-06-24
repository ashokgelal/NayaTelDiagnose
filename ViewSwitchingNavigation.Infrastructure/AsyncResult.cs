

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using ViewSwitchingNavigation.Infrastructure.Properties;

namespace ViewSwitchingNavigation.Infrastructure
{
    [SuppressMessage("Microsoft.Design", "CA1001",
        Justification = "Calling the End method, which is part of the contract of using an IAsyncResult, releases the IDisposable.")]
    public class AsyncResult<T> : IAsyncResult
    {
        private readonly object lockObject;
        private readonly AsyncCallback asyncCallback;
        private readonly object asyncState;
        private ManualResetEvent waitHandle;
        private T result;
        private Exception exception;
        private bool isCompleted;
        private bool completedSynchronously;
        private bool endCalled;

        public AsyncResult(AsyncCallback asyncCallback, object asyncState)
        {
            lockObject = new object();
            this.asyncCallback = asyncCallback;
            this.asyncState = asyncState;
        }

        public object AsyncState
        {
            get { return asyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                lock (lockObject)
                {
                    if (waitHandle == null)
                    {
                        waitHandle = new ManualResetEvent(IsCompleted);
                    }
                }

                return waitHandle;
            }
        }

        public bool CompletedSynchronously
        {
            get { return completedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return isCompleted; }
        }

        public T Result
        {
            get { return result; }
        }

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes",
            Justification = "Entry point to be used to implement End* methods.")]
        public static AsyncResult<T> End(IAsyncResult asyncResult)
        {
            var localResult = asyncResult as AsyncResult<T>;
            if (localResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            lock (localResult.lockObject)
            {
                if (localResult.endCalled)
                {
                    throw new InvalidOperationException(Resources.EndMethodAlreadyCalled);
                }

                localResult.endCalled = true;
            }

            if (!localResult.IsCompleted)
            {
                localResult.AsyncWaitHandle.WaitOne();
            }

            if (localResult.waitHandle != null)
            {
                localResult.waitHandle.Close();
            }

            if (localResult.exception != null)
            {
                throw localResult.exception;
            }

            return localResult;
        }

        public void SetComplete(T result, bool completedSynchronously)
        {
            this.result = result;

            DoSetComplete(completedSynchronously);
        }

        public void SetComplete(Exception e, bool completedSynchronously)
        {
            exception = e;

            DoSetComplete(completedSynchronously);
        }

        private void DoSetComplete(bool completedSynchronously)
        {
            if (completedSynchronously)
            {
                this.completedSynchronously = true;
                isCompleted = true;
            }
            else
            {
                lock (lockObject)
                {
                    isCompleted = true;
                    if (waitHandle != null)
                    {
                        waitHandle.Set();
                    }
                }
            }

            if (asyncCallback != null)
            {
                asyncCallback(this);
            }
        }
    }
}
