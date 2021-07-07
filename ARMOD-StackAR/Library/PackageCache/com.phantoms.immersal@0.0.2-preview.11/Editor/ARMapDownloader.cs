/*===============================================================================
Copyright (C) 2020 Immersal Ltd. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sdk@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Immersal.REST;
using Immersal.AR;

namespace Immersal
{
    public class ARMapDownloader : EditorWindow
    {
        private static string m_MapIdsCSV = "";
        private static string m_Token = "";
        private static string m_MapDataPath;
        private ARSpace m_ARSpace = null;
        private bool m_loadAlignment = false;

        //[MenuItem("Immersal SDK/AR Map Downloader")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<ARMapDownloader>("AR Map Downloader");
        }

        private void OnGUI()
        {
            m_Token = ImmersalSDK.Instance.developerToken;

            m_MapIdsCSV = EditorGUILayout.TextField(new GUIContent("Map ids CSV", "Comma-separated values of map ids to download"), m_MapIdsCSV);
            m_ARSpace = (ARSpace)EditorGUILayout.ObjectField(new GUIContent("AR Space", "AR Space Game Object from the scene under which the maps are created"), m_ARSpace, typeof(ARSpace), true);
            if (m_Token.Length == 0)
            {
                EditorGUILayout.HelpBox("No Developer token specified, please log in in the Immersal SDK Settings or manually input in the ImmersalSDK game object", MessageType.Warning, true);
            }
            if (m_ARSpace == null)
            {
                EditorGUILayout.HelpBox("No AR Space selected, a new one will be created", MessageType.Info, true);
            }

            if (GUILayout.Button("Download And Setup Maps"))
            {
                DownloadMaps();
            }

            m_loadAlignment = EditorGUILayout.Toggle(new GUIContent("Load Alignment (Experimental)", "Try to load alignment from map metadata. Coordinate system is unknown (ECEF or Unity's)"), m_loadAlignment);
        }

        private void DownloadMaps()
        {
            m_MapDataPath = Path.Combine("Assets", "Map Data");
            DirectoryInfo di = Directory.CreateDirectory(m_MapDataPath);
            AssetDatabase.Refresh();

            List<int> mapIds = GetMapIds(m_MapIdsCSV);

            if (AssetDatabase.IsValidFolder(m_MapDataPath))
            {
                if (mapIds.Count > 0)
                {
                    foreach (int id in mapIds)
                    {
                        EditorCoroutineUtility.StartCoroutine(DownloadMapMetadata(id), this);
                    }
                }
            }
            else
            {
                Debug.Log("Map Data Folder not valid, aborting");
            }
        }

        private IEnumerator DownloadMapMetadata(int id)
        {
            //
            // Downloads map metadata, saves it to disk, downloads the map file and sets up the AR Map game object
            //

            // Load map metadata from Immersal Cloud Service
            SDKMapMetadataGetRequest r = new SDKMapMetadataGetRequest();
            r.token = m_Token;
            r.id = id;

            string jsonString = JsonUtility.ToJson(r);
            UnityWebRequest request = UnityWebRequest.Put(string.Format(ImmersalHttp.URL_FORMAT, ImmersalSDK.DefaultServer, SDKMapMetadataGetRequest.endpoint), jsonString);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.useHttpContinue = false;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                SDKMapMetadataGetResult result = JsonUtility.FromJson<SDKMapMetadataGetResult>(request.downloadHandler.text);
                if (result.error == "none")
                {
                    // Save metadata file on disk, overwrite existing file
                    string jsonFilePath = Path.Combine(m_MapDataPath, string.Format("{0}-{1}-metadata.json", result.id, result.name));
                    WriteJson(jsonFilePath, request.downloadHandler.text);

                    // Load map file from Immersal Cloud Service
                    EditorCoroutineUtility.StartCoroutine(DownloadMapFile(id, result), this);
                }
            }
        }

        private IEnumerator DownloadMapFile(int id, SDKMapMetadataGetResult result)
        {
            //
            // Load the map file, write it to disk and set up the AR Map game object
            //

            // Load map file from Immersal Cloud Service
            SDKMapDownloadRequest r = new SDKMapDownloadRequest();
            r.token = m_Token;
            r.id = id;

            string jsonString = JsonUtility.ToJson(r);
            UnityWebRequest request = UnityWebRequest.Put(string.Format(ImmersalHttp.URL_FORMAT, ImmersalSDK.DefaultServer, SDKMapDownloadRequest.endpoint), jsonString);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.useHttpContinue = false;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                SDKMapDownloadResult mapDataResult = JsonUtility.FromJson<SDKMapDownloadResult>(request.downloadHandler.text);
                if (mapDataResult.error == "none")
                {
                    // Save map file on disk, overwrite existing file
                    string mapFilepath = Path.Combine(m_MapDataPath, string.Format("{0}-{1}.bytes", result.id, result.name));
                    WriteBytes(mapFilepath, mapDataResult.b64);

                    // Set up AR Map game object
                    SetupARMapInScene(mapFilepath, result);
                }
            }
        }

        private List<int> GetMapIds(string mapIds)
        {
            List<int> ids = mapIds.Split(',').Select(int.Parse).ToList();
            return ids;
        }

        private void WriteJson(string jsonFilepath, string data)
        {
            File.WriteAllText(jsonFilepath, data);
            AssetDatabase.ImportAsset(jsonFilepath);
        }

        private void WriteBytes(string mapFilepath, string b64)
        {
            if (!File.Exists(mapFilepath))
            {
                byte[] data = Convert.FromBase64String(b64);
                File.WriteAllBytes(mapFilepath, data);
                AssetDatabase.ImportAsset(mapFilepath);
            }
        }

        private void SetupARMapInScene(string mapFilepath, SDKMapMetadataGetResult result)
        {
            if(m_ARSpace == null)
            {
                GameObject arSpace = new GameObject("AR Space");
                m_ARSpace = arSpace.AddComponent<ARSpace>();
            }

            string arMapName = string.Format("AR Map {0}-{1}", result.id, result.name);
            ARMap[] arMapsInScene = FindObjectsOfType<ARMap>();
            bool arMapExists = false;
            ARMap arMap = null;
            for(int i=0; i<arMapsInScene.Length; i++)
            {
                if(arMapsInScene[i].name == arMapName)
                {
                    arMapExists = true;
                    arMap = arMapsInScene[i];
                    break;
                }
            }

            if (!arMapExists)
            {
                GameObject go = new GameObject(string.Format("AR Map {0}-{1}", result.id, result.name));
                go.transform.parent = m_ARSpace.transform;

                Color pointCloudColor = ARMap.pointCloudColors[UnityEngine.Random.Range(0, ARMap.pointCloudColors.Length)];

                arMap = go.AddComponent<ARMap>();
                arMap.pointColor = pointCloudColor;

                TextAsset mapFile = (TextAsset)AssetDatabase.LoadAssetAtPath(mapFilepath, typeof(TextAsset));
                arMap.mapFile = mapFile;
            }

            arMap.mapAlignment.tx = result.tx;
            arMap.mapAlignment.ty = result.ty;
            arMap.mapAlignment.tz = result.tz;
            arMap.mapAlignment.qx = result.qx;
            arMap.mapAlignment.qy = result.qy;
            arMap.mapAlignment.qz = result.qz;
            arMap.mapAlignment.qw = result.qw;
            arMap.mapAlignment.scale = result.scale;

            arMap.wgs84.latitude = result.latitude;
            arMap.wgs84.longitude = result.longitude;
            arMap.wgs84.altitude = result.altitude;

            arMap.privacy = result.privacy.ToString();

            if (m_loadAlignment)
            {
                Vector3 posMetadata = new Vector3((float)result.tx, (float)result.ty, (float)result.tz);
                Quaternion rotMetadata = new Quaternion((float)result.qx, (float)result.qy, (float)result.qz, (float)result.qw);
                float scaleMetadata = (float)result.scale; // Only uniform scale metadata is supported

                // IMPORTANT
                // Switch coordinate system handedness back from Immersal Cloud Service's default right-handed system to Unity's left-handed system
                Matrix4x4 a;
                Matrix4x4 b = Matrix4x4.TRS(posMetadata, rotMetadata, new Vector3(scaleMetadata, scaleMetadata, scaleMetadata));
                ARHelper.SwitchHandedness(out a, b);
                Vector3 pos = a.GetColumn(3);
                Quaternion rot = a.rotation;
                Vector3 scl = new Vector3(scaleMetadata, scaleMetadata, scaleMetadata); // Only uniform scale metadata is supported

                // Set AR Map local transform from the converted metadata
                arMap.transform.localPosition = pos;
                arMap.transform.localRotation = rot;
                arMap.transform.localScale = scl;
            }
        }
    }
}
