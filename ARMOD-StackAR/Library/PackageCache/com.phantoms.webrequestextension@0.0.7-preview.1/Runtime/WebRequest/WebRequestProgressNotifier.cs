using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace com.Phantoms.WebRequestExtension.Runtime.WebRequest
{
    public static class UnityWebRequestAsyncOperationExtension
    {
        public static UnityWebRequestAsyncOperationAwaiter ConfigureAwait(
            this UnityWebRequestAsyncOperation _asyncOperation, IProgress<float> _progress)
        {
            var tmp_ProgressNotifier = new AsyncOperationProgressNotifier(_asyncOperation, _progress);
            ProgressUpdater.Instance.AddItem(tmp_ProgressNotifier);

            return new UnityWebRequestAsyncOperationAwaiter(_asyncOperation);
        }


        public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation _asyncOperation)
        {
            var tmp_Tcs = new TaskCompletionSource<object>();
            _asyncOperation.completed += _operation => { tmp_Tcs.SetResult(null); };
            return ((Task) tmp_Tcs.Task).GetAwaiter();
        }
    }
}