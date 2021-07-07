using System;
using System.Threading.Tasks;
using com.Phantoms.ARMODPackageTools.Runtime;
using UnityEngine;
using UnityEngine.Networking;

namespace com.cellstudio.armodsimulater.Runtime
{
    public class SimulatorLoader
    {
        public async Task<String> GetPropertyText(string _projectName)
        {
            UnityWebRequest tmp_Request =
                UnityWebRequest.Get($"http://localhost:8084/{_projectName}/{_projectName}.json");
            await tmp_Request.SendWebRequest();


            if (tmp_Request.isHttpError || tmp_Request.isNetworkError)
            {
                Debug.LogError(tmp_Request.error);
                return string.Empty;
            }

            var tmp_BundleInfo = tmp_Request.downloadHandler.text;
            var bundleObjectUrl = $"http://localhost:8084/{_projectName}/{_projectName}.arexperience";


            var tmp_ExperienceData = JsonUtility.FromJson<ARExperienceData>(tmp_BundleInfo);
            var tmp_PropertyText = await BasePackageLoaderUtility.LoadBundleFromUrl<TextAsset>(
                _uri: new Uri(bundleObjectUrl),
                _timeout:60,
                _wannaLoadAssetsName: "ARProperty",
                _hash128: Hash128.Parse(tmp_ExperienceData.BundleDetails.m_Hash),
                _crc: tmp_ExperienceData.BundleDetails.m_Crc,
                _failedAction: Debug.LogError,
                _progressAction: PrintProgress
            );

            if (string.IsNullOrEmpty(tmp_PropertyText.text))
            {
                throw new NullReferenceException();
            }

            return tmp_PropertyText.text;
        }


        public async Task<GameObject> GetMainObject(string _projectName)
        {
            var tmp_MainPrefab =
                await BasePackageLoaderUtility.LoadAssetFromPackage<GameObject>(_projectName, _projectName);

            return InstantiateUtility.Instantiate(tmp_MainPrefab, "", GameObject.Find("Trackables").transform);
        }

        public async Task<byte[]> GetCodes(string _projectName)
        {
            var tmp_Assembly =
                await BasePackageLoaderUtility.LoadAssetFromPackage<TextAsset>(_projectName, $"{_projectName.ToLower()}.dll");
            return tmp_Assembly.bytes;
        }
        
        
        private void PrintProgress(float _progress)
        {
            Debug.Log($"Download asset percent:{_progress}");
        }
    }
}