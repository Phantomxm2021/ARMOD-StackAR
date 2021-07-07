using System;
using UnityEngine;
using UnityEngine.Networking;

namespace com.Phantoms.WebRequestExtension.Runtime
{
    public class AsyncOperationProgressNotifier
    {
        readonly AsyncOperation asyncOp;
        readonly IProgress<float> progress;

        public AsyncOperationProgressNotifier(AsyncOperation _asyncOp, IProgress<float> _progress)
        {
            asyncOp = _asyncOp;
            progress = _progress;
        }

        public bool NotifyProgress()
        {
            progress.Report(asyncOp.progress);

            return asyncOp.isDone;
        }
    }
}