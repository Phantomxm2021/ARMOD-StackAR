using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace com.Phantoms.WebRequestExtension.Runtime.AssetBundle
{
    public class AssetBundleRequestAsyncOperationAwaiter : INotifyCompletion
    {
        readonly AssetBundleRequest asyncOperation;

        public bool IsCompleted => asyncOperation.isDone;

        public AssetBundleRequestAsyncOperationAwaiter(AssetBundleRequest _asyncOperation)
        {
            this.asyncOperation = _asyncOperation;
        }

        public void GetResult()
        {
        }

        public AssetBundleRequestAsyncOperationAwaiter GetAwaiter()
        {
            return this;
        }

        public void OnCompleted(Action _continuation)
        {
            asyncOperation.completed += _ => { _continuation(); };
        }
    }
}