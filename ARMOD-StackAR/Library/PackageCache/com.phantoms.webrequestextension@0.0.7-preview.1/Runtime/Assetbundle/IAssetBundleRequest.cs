using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.Phantoms.WebRequestExtension.Runtime.AssetBundle
{
    public interface IAssetBundleRequest
    {
        Task<UnityWebRequest> GetAssetBundle(Uri _uri, CachedAssetBundle _cachedAssetBundle,uint _crc);
    }
}