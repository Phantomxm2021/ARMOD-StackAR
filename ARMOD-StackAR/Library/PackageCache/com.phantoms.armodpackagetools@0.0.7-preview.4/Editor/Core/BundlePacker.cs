using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using Debug = UnityEngine.Debug;

namespace com.Phantoms.ARMODPackageTools.Core
{
    [System.Serializable]
    public class ARExperienceDetail
    {
        public string BundleName;
        public List<string> AddressableName;
        public List<string> AssetsName;
        public BundleDetails BundleDetails;
    }

    public static class BundlePacker
    {
        private const string CONST_ASSET_SUFFIX = "arexperience";
        private const string CONST_ASSET_INFO_SUFFIX = "json";

        // ReSharper disable once IdentifierTypo
        public static ReturnCode ScriptableBuildPipeline(
            string _bundleName,
            List<string> _addressableNames,
            List<string> _assetNames,
            BuildTarget _buildTarget,
            BuildTargetGroup _buildTargetGroup,
            BuildCompression _buildCompression,
            string _outPath)
        {
            var tmp_BuildTimer = new Stopwatch();
            tmp_BuildTimer.Start();

            var tmp_BuildContent = GetBundleContent(_bundleName, _addressableNames, _assetNames);
            var tmp_BuildParams = new BundleBuildParameters(_buildTarget, _buildTargetGroup, _outPath)
            {
                BundleCompression = _buildCompression
            };

            ReturnCode tmp_ReturnCode = ContentPipeline.BuildAssetBundles(tmp_BuildParams, tmp_BuildContent,
                out IBundleBuildResults tmp_Result);

            tmp_BuildTimer.Stop();

            if (tmp_ReturnCode == ReturnCode.Success)
            {
                var tmp_ARExperienceDetail = new ARExperienceDetail
                {
                    AddressableName = _addressableNames, AssetsName = _assetNames, BundleName = _bundleName
                };


                foreach (KeyValuePair<string, BundleDetails> tmp_Detail in tmp_Result.BundleInfos)
                {
                    tmp_ARExperienceDetail.BundleDetails = tmp_Detail.Value;
                    var tmp_JsonSavedPath = Path.Combine(_outPath, $"{_bundleName}.{CONST_ASSET_INFO_SUFFIX}");
                    File.WriteAllText(tmp_JsonSavedPath, JsonUtility.ToJson(tmp_ARExperienceDetail));
                }

                Debug.Log($"Packed Success,Consumed {tmp_BuildTimer.Elapsed} ms");
            }

            AssetDatabase.Refresh();
            return tmp_ReturnCode;
        }


        static IBundleBuildContent GetBundleContent(string _bundleName, List<string> _addressableNames,
            List<string> _assetNames)
        {
            List<AssetBundleBuild> tmp_BuildDataList = new List<AssetBundleBuild>();
            AssetBundleBuild tmp_Data = new AssetBundleBuild()
            {
                addressableNames = _addressableNames.ToArray(),
                assetBundleName = $"{_bundleName}.{CONST_ASSET_SUFFIX}",
                assetBundleVariant = "",
                assetNames = _assetNames.ToArray()
            };

            tmp_BuildDataList.Add(tmp_Data);
            IBundleBuildContent tmp_BuildContent = new BundleBuildContent(tmp_BuildDataList);
            return tmp_BuildContent;
        }
    }
}