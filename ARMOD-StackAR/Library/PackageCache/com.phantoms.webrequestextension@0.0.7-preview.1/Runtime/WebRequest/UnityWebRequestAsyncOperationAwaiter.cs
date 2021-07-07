using System;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace com.Phantoms.WebRequestExtension.Runtime.WebRequest
{
    public class UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
    {
        readonly UnityWebRequestAsyncOperation asyncOperation;

        public bool IsCompleted => asyncOperation.isDone;

        public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation _asyncOperation)
        {
            this.asyncOperation = _asyncOperation;
        }

        public void GetResult()
        {
        }

        public UnityWebRequestAsyncOperationAwaiter GetAwaiter()
        {
            return this;
        }

        public void OnCompleted(Action _continuation)
        {
            asyncOperation.completed += _ => { _continuation(); };
        }
    }
}