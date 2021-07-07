using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using com.Phantoms.WebRequestExtension.Runtime.WebRequest;

namespace com.Phantoms.WebRequestExtension.Runtime.AssetBundle
{
    public class DownloadAssetBundleWithProgress : IAssetBundleRequest, IDisposable
    {
        private UnityWebRequest assetBundleRequest;
        private Action<float> updateProgress;
        private readonly int timeout;

        public DownloadAssetBundleWithProgress(Action<float> _updateProgress, int _timeout = 30)
        {
            updateProgress = _updateProgress;
            timeout = _timeout;
        }


        public void Dispose()
        {
            updateProgress = null;
            assetBundleRequest?.Dispose();
            assetBundleRequest = null;
        }

        public async Task<UnityWebRequest> GetAssetBundle(Uri _uri, CachedAssetBundle _cachedAssetBundle, uint _crc)
        {
            assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(_uri, _cachedAssetBundle, _crc);
            assetBundleRequest.timeout = this.timeout;
            await assetBundleRequest.SendWebRequest().ConfigureAwait(new Progress<float>(updateProgress));
            if (assetBundleRequest.error != null || assetBundleRequest.isHttpError)
            {
                throw new Exception(assetBundleRequest.error);
            }

            return assetBundleRequest;
        }
    }
}