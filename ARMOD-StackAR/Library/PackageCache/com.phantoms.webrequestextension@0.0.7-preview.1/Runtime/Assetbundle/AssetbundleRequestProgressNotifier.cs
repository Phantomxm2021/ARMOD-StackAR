using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.Phantoms.WebRequestExtension.Runtime.AssetBundle
{
    public static class GetAssetBundleAsyncOperationExtension
    {
        public static AssetBundleRequestAsyncOperationAwaiter ConfigureAwait(
            this AssetBundleRequest _asyncOperation, IProgress<float> _progress)
        {
            var tmp_ProgressNotifier = new AsyncOperationProgressNotifier(_asyncOperation, _progress);
            ProgressUpdater.Instance.AddItem(tmp_ProgressNotifier);

            return new AssetBundleRequestAsyncOperationAwaiter(_asyncOperation);
        }


        public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation _asyncOperation)
        {
            var tmp_Tcs = new TaskCompletionSource<object>();
            _asyncOperation.completed += _operation => { tmp_Tcs.SetResult(null); };
            return ((Task) tmp_Tcs.Task).GetAwaiter();
        }
    }
}