using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using com.Phantoms.WebRequestExtension.Runtime;
using com.Phantoms.WebRequestExtension.Runtime.AssetBundle;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace com.Phantoms.ARMODPackageTools.Runtime
{
    public static class BasePackageLoaderUtility
    {
        //all assetbundles manager
        //the keys are assetbundle file name
        //the values are loaded assetbundle
        private static readonly IDictionary<string, AssetBundle> AssetBundleCollection =
            new Dictionary<string, AssetBundle>();

        /// <summary>
        /// load assets from local file
        /// </summary>
        /// <param name="_pathWithFile">The path of the assets to load.</param>
        /// <param name="_wannaLoadAssetsName">wanna load asset name</param>
        /// <param name="_loadFinished"></param>
        /// <param name="_loadFailed"></param>
        /// <typeparam name="T">what type do you wanna loaded</typeparam>
        public static void LoadAssetFromFileAsync<T>(string _pathWithFile, string _wannaLoadAssetsName,
            Action<T> _loadFinished, Action<string> _loadFailed) where T : Object
        {
#if UNITY_EDITOR
            var tmp_BuildTimer = new Stopwatch();
            tmp_BuildTimer.Start();
#endif
            try
            {
                if (string.IsNullOrEmpty(_pathWithFile))
                {
                    _loadFailed?.Invoke($"Error:Can found path{_pathWithFile}");
                    return;
                }

                string tmp_BundleName = Path.GetFileName(_pathWithFile);

                //Reload
                if (AssetBundleCollection.TryGetValue(tmp_BundleName, out AssetBundle tmp_LoadedAssetBundle))
                {
                    var tmp_ReloadAsset = tmp_LoadedAssetBundle.LoadAssetAsync(_wannaLoadAssetsName);

                    tmp_ReloadAsset.completed += _asyncOperation =>
                    {
                        _loadFinished?.Invoke(tmp_ReloadAsset.asset as T);
#if UNITY_EDITOR
                        tmp_BuildTimer.Stop();
                        Debug.Log($"Re Load:{tmp_BuildTimer.ElapsedMilliseconds} ms");
#endif
                    };
                    return;
                }

                var tmp_AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(_pathWithFile);
                tmp_AssetBundleCreateRequest.completed += _operation =>
                {
                    var tmp_AssetBundle = tmp_AssetBundleCreateRequest.assetBundle;
                    var tmp_LoadedAsset = tmp_AssetBundle.LoadAssetAsync(_wannaLoadAssetsName);
                    tmp_LoadedAsset.completed += _asyncOperation =>
                    {
                        AddAssetBundleToGlobalManager(tmp_AssetBundle.name, tmp_AssetBundle);
                        _loadFinished?.Invoke(tmp_LoadedAsset.asset as T);
#if UNITY_EDITOR
                        tmp_BuildTimer.Stop();
                        Debug.Log($"Re Load:{tmp_BuildTimer.ElapsedMilliseconds} ms");
#endif
                    };
                };
            }
            catch (Exception tmp_Exception)
            {
                _loadFailed?.Invoke(tmp_Exception.Message);
            }
        }

        /// <summary>
        /// load assets from url
        /// </summary>
        /// <param name="_uri">The URI of the asset bundle to download.</param>
        /// <param name="_wannaLoadAssetsName">wanna load asset name</param>
        /// <param name="_hash">A version hash. If this hash does not match
        /// the hash for the cached version of this asset bundle, the asset
        /// bundle will be re-downloaded.</param>
        /// <param name="_crc">If nonzero, this number will be compared to
        /// the checksum of the downloaded asset bundle data. If the CRCs
        /// do not match, an error will be logged and the asset bundle
        /// will not be loaded. If set to zero, CRC checking will be skipped.</param>
        /// <param name="_loadFinished"></param>
        /// <param name="_loadFailed"></param>
        /// <param name="_downloadProgressCallback"></param>
        /// <typeparam name="T">what type do you wanna loaded</typeparam>
        public static IEnumerator LoadGameObjectFromUrl<T>(Uri _uri, string _wannaLoadAssetsName, Hash128 _hash,
            uint _crc,
            Action<T> _loadFinished,
            Action<string> _loadFailed,
            Action<float> _downloadProgressCallback) where T : Object
        {
#if UNITY_EDITOR
            var tmp_BuildTimer = new Stopwatch();
            tmp_BuildTimer.Start();
#endif

            var tmp_FileName = Path.GetFileName(_uri.AbsoluteUri);
            //var tmp_CachedPath = Path.Combine("AssetbundleCached", tmp_FileName);
            var tmp_CachedConfigure = new CachedAssetBundle {hash = _hash};


            //Reload
            if (AssetBundleCollection.TryGetValue(tmp_FileName, out AssetBundle tmp_LoadedAssetBundle))
            {
                var tmp_ReloadAsset = tmp_LoadedAssetBundle.LoadAssetAsync(_wannaLoadAssetsName);

                tmp_ReloadAsset.completed += _asyncOperation =>
                {
                    _loadFinished?.Invoke(tmp_ReloadAsset.asset as T);
#if UNITY_EDITOR
                    tmp_BuildTimer.Stop();
                    Debug.Log($"Re Load:{tmp_BuildTimer.ElapsedMilliseconds} ms");
#endif
                };
                yield break;
            }


            var tmp_WebRequestAssetBundle = UnityWebRequestAssetBundle.GetAssetBundle(_uri, tmp_CachedConfigure, _crc);

            yield return tmp_WebRequestAssetBundle.SendWebRequest();

            if (tmp_WebRequestAssetBundle.isHttpError || tmp_WebRequestAssetBundle.isNetworkError)
            {
                Debug.LogError(tmp_WebRequestAssetBundle.error);
                _loadFailed?.Invoke(tmp_WebRequestAssetBundle.error);
                tmp_WebRequestAssetBundle.Dispose();
                yield break;
            }

            while (!tmp_WebRequestAssetBundle.isDone)
            {
                _downloadProgressCallback?.Invoke(tmp_WebRequestAssetBundle.downloadProgress);
                yield return null;
            }


            try
            {
                AssetBundle tmp_AssetBundle = DownloadHandlerAssetBundle.GetContent(tmp_WebRequestAssetBundle);

                var tmp_LoadedAsset = tmp_AssetBundle.LoadAssetAsync(_wannaLoadAssetsName);
                tmp_LoadedAsset.completed += _asyncOperation =>
                {
                    AddAssetBundleToGlobalManager(tmp_AssetBundle.name, tmp_AssetBundle);
                    _loadFinished?.Invoke(tmp_LoadedAsset.asset as T);
#if UNITY_EDITOR
                    tmp_BuildTimer.Stop();
                    Debug.Log($"First Load:{tmp_BuildTimer.ElapsedMilliseconds} ms");
#endif
                };
            }
            catch (Exception tmp_Exception)
            {
                Debug.LogError(tmp_Exception.Message);
            }
        }


        /// <summary>
        /// load asset from package by project name
        /// </summary>
        /// <param name="_projectName">project name</param>
        /// <param name="_wannaLoadedAssetName">asset name</param>
        /// <param name="_loadedCallback">call back</param>
        /// <typeparam name="T">asset type</typeparam>
        public static void LoadAssetFromPackage<T>(string _projectName, string _wannaLoadedAssetName,
            Action<T> _loadedCallback) where T : Object
        {
            _projectName += ".arexperience";
            if (!AssetBundleCollection.TryGetValue(_projectName.ToLower(), out AssetBundle tmp_MatchedAssetBundle))
            {
                throw new NullReferenceException(
                    $"Can not load {_projectName} project's {_wannaLoadedAssetName} asset.");
            }

            var tmp_LoadedOperation = tmp_MatchedAssetBundle.LoadAssetAsync(_wannaLoadedAssetName);
            tmp_LoadedOperation.completed += _operation => { _loadedCallback?.Invoke(tmp_LoadedOperation.asset as T); };
        }


        /// <summary>
        /// Away/Task. Load asset from package by project name. 
        /// </summary>
        /// <param name="_projectName"></param>
        /// <param name="_wannaLoadedAssetName"></param>
        /// <typeparam name="T">Unity Object</typeparam>
        /// <returns>Task</returns>
        /// <exception cref="NullReferenceException"></exception>
        public static async Task<T> LoadAssetFromPackage<T>(string _projectName, string _wannaLoadedAssetName)
            where T : Object
        {
            _projectName += ".arexperience";
            if (!AssetBundleCollection.TryGetValue(_projectName.ToLower(), out AssetBundle tmp_MatchedAssetBundle))
            {
                throw new NullReferenceException(
                    $"Can not load {_projectName} project's {_wannaLoadedAssetName} asset.");
            }


            var tmp_LoadedOperation = await tmp_MatchedAssetBundle.LoadAssetAsync<T>(_wannaLoadedAssetName);
            return tmp_LoadedOperation as T;
        }


        /// <summary>
        /// Away/Task. Load assets from url.
        /// </summary>
        /// <param name="_uri">The URI of the asset bundle to download.</param>
        /// <param name="_wannaLoadAssetsName">wanna load asset name</param>
        /// <param name="_hash128">A version hash. If this hash does not match
        /// the hash for the cached version of this asset bundle, the asset
        /// bundle will be re-downloaded.</param>
        /// <param name="_crc">If nonzero, this number will be compared to
        /// the checksum of the downloaded asset bundle data. If the CRCs
        /// do not match, an error will be logged and the asset bundle
        /// will not be loaded. If set to zero, CRC checking will be skipped.</param>
        /// <param name="_progressAction"></param>
        /// <param name="_failedAction"></param>
        /// <typeparam name="T">what type do you wanna loaded</typeparam>
        /// <returns></returns>
        public static async Task<T> LoadBundleFromUrl<T>(Uri _uri, int _timeout, string _wannaLoadAssetsName,
            Hash128 _hash128,
            uint _crc, Action<float> _progressAction, Action<string> _failedAction) where T : Object
        {
            var tmp_FileName = Path.GetFileName(_uri.AbsoluteUri);
            var tmp_CachedConfigure = new CachedAssetBundle {hash = _hash128};

            //Reload
            if (AssetBundleCollection.TryGetValue(tmp_FileName, out AssetBundle tmp_LoadedAssetBundle))
            {
                tmp_LoadedAssetBundle.LoadAssetAsync(_wannaLoadAssetsName).GetAwaiter();
                var tmp_ReloadAsset = await tmp_LoadedAssetBundle.LoadAssetAsync(_wannaLoadAssetsName);
                return tmp_ReloadAsset as T;
            }


            var tmp_DownloadAssetBundleWithProgress = new DownloadAssetBundleWithProgress(_progressAction, _timeout);
            var tmp_WebRequestAssetBundle =
                await tmp_DownloadAssetBundleWithProgress.GetAssetBundle(_uri, tmp_CachedConfigure, _crc);
//            var tmp_WebRequestAssetBundle = UnityWebRequestAssetBundle.GetAssetBundle(_uri, tmp_CachedConfigure, _crc);

//            await tmp_WebRequestAssetBundle.SendWebRequest();

//            if (tmp_WebRequestAssetBundle.isHttpError || tmp_WebRequestAssetBundle.isNetworkError)
//            {
//                Debug.LogError(tmp_WebRequestAssetBundle.error);
//                _failedAction?.Invoke(tmp_WebRequestAssetBundle.error);
//                tmp_WebRequestAssetBundle.Dispose();
//                return null;
//            }


            try
            {
                AssetBundle tmp_AssetBundle = DownloadHandlerAssetBundle.GetContent(tmp_WebRequestAssetBundle);
                var tmp_LoadedAsset = await tmp_AssetBundle.LoadAssetAsync(_wannaLoadAssetsName);
                AddAssetBundleToGlobalManager(tmp_AssetBundle.name, tmp_AssetBundle);
                return tmp_LoadedAsset as T;
            }
            catch (Exception tmp_Exception)
            {
                Debug.LogError($"Assetbundle load failed: {tmp_Exception.Message}");
            }

            return null;
        }


        /// <summary>
        /// Release all loaded asset bundle 
        /// </summary>
        public static void ReleaseAllAssetBundle()
        {
            var tmp_AllLoadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
            foreach (AssetBundle tmp_LoadedAssetBundle in tmp_AllLoadedAssetBundles)
            {
                tmp_LoadedAssetBundle.Unload(true);
            }

            AssetBundleCollection.Clear();
        }

        /// <summary>
        /// manager the memory
        /// </summary>
        /// <param name="_opHashCode">key</param>
        /// <param name="_assetBundle">value</param>
        private static void AddAssetBundleToGlobalManager(string _opHashCode, AssetBundle _assetBundle)
        {
            if (!AssetBundleCollection.ContainsKey(_opHashCode))
            {
                AssetBundleCollection.Add(_opHashCode, _assetBundle);
            }
        }
    }
}