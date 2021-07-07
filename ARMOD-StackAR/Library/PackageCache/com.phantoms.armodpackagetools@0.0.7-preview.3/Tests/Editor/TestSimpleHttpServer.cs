using System;
using System.Collections;
using com.Phantoms.ARMODPackageTools.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.com.cellstudio.armodpackagetools.Tests.Editor
{
    public class TestSimpleHttpServer
    {
        [NUnit.Framework.Test]
        public void TestSimpleHttpServerSimplePasses()
        {
            // Use the Assert class to test conditions.
            GetJson();
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityEngine.TestTools.UnityTest]
        public IEnumerator TestSimpleHttpServerWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }

        private void GetJson()
        {
            UnityWebRequest tmp_Request = UnityWebRequest.Get("http://localhost:8084/");
            tmp_Request.SendWebRequest().completed += async operation =>
            {
                if (tmp_Request.isHttpError || tmp_Request.isNetworkError)
                {
                    Debug.LogError(tmp_Request.error);
                    return;
                }

                var tmp_ExperienceData = JsonUtility.FromJson<ARExperienceData>(tmp_Request.downloadHandler.text);
                var tmp_PropertyText = await BasePackageLoaderUtility.LoadBundleFromUrl<TextAsset>(
                    _uri: new Uri("http://localhost:8084/test_02.json"),
                    _timeout: 30,
                    _wannaLoadAssetsName: "ARProperty",
                    _hash128: Hash128.Parse(tmp_ExperienceData.BundleDetails.m_Hash),
                    _crc: tmp_ExperienceData.BundleDetails.m_Crc,
                    _failedAction: Debug.LogError,
                    _progressAction: null
                );

                if (string.IsNullOrEmpty(tmp_PropertyText.text))
                {
                    throw new NullReferenceException();
                }

                Assert.IsEmpty(tmp_PropertyText.text);
            };
        }
    }
}