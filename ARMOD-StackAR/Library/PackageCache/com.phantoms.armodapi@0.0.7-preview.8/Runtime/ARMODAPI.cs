using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.Phantoms.ActionNotification.Runtime;
using com.Phantoms.ARMODPackageTools.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace com.Phantoms.ARMODAPI.Runtime
{
    [Obsolete("This API is obsolete. Replace with ARMODAPI.Runtime.API")]
    public class ARMODAPI
    {
         #region UGUI

        private GraphicRaycaster graphicRaycaster;

        /// <summary>
        /// It is used to determine whether ugui objects are touched.
        /// Prevent raycast ray penetration when clicking ugui objects.
        /// </summary>
        /// <returns>True:Touch ugui object,False:Not touching the UGUI object</returns>
        public bool IsPointerOverUi()
        {
            PointerEventData tmp_EventData = new PointerEventData(EventSystem.current)
            {
                pressPosition = Input.mousePosition, position = Input.mousePosition
            };

            List<RaycastResult> tmp_List = new List<RaycastResult>();
            if (null == graphicRaycaster)
                graphicRaycaster = Object.FindObjectOfType<GraphicRaycaster>();
            graphicRaycaster.Raycast(tmp_EventData, tmp_List);
            return tmp_List.Count > 0;
        }

        #endregion

        #region KV

        /// <summary>
        /// Store temporary data locally
        /// </summary>
        /// <param name="_projectName">Use ProjectName+Key storage to prevent Key from being occupied</param>
        /// <param name="_key">Unique name</param>
        /// <param name="_value">Data that needs to be saved</param>
        public void SaveKeyAndValue(string _projectName, string _key, string _value)
        {
            PlayerPrefs.SetString(_projectName + _key, _value);
        }

        /// <summary>
        /// Read data temporarily stored on the device
        /// </summary>
        /// <param name="_projectName">Use ProjectName+Key storage to prevent Key from being occupied</param>
        /// <param name="_key">Unique name</param>
        /// <returns>The data queried</returns>
        public string GetValueByKey(string _projectName, string _key)
        {
            return PlayerPrefs.GetString(_projectName + _key);
        }

        /// <summary>
        /// Removes data and index fields that are temporarily stored locally
        /// </summary>
        /// <param name="_key">Unique name</param>
        public void RemoveKeyAndValue(string _key)
        {
            PlayerPrefs.DeleteKey(_key);
        }

        #endregion

        #region Asset loader

        /// <summary>
        /// load a game object from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadGameObject(string _projectName, string _wannaLoadedAssetName,
            Action<GameObject> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }

        

        /// <summary>
        /// load a Texture2D from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadTexture2D(string _projectName, string _wannaLoadedAssetName,
            Action<Texture2D> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }
        

        /// <summary>
        /// load a Sprite atlas from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadSpriteAtlas(string _projectName, string _wannaLoadedAssetName,
            Action<SpriteAtlas> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }
        


        /// <summary>
        /// load a Audio clip from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadAudioClip(string _projectName, string _wannaLoadedAssetName,
            Action<AudioClip> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }
        


        /// <summary>
        /// load a Audio clip from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadVideoClip(string _projectName, string _wannaLoadedAssetName,
            Action<VideoClip> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }
        

        /// <summary>
        /// load a Text assets from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadTextAsset(string _projectName, string _wannaLoadedAssetName,
            Action<TextAsset> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }
        #endregion

        #region GameObject infomation

        /// <summary>
        /// Find the object by the name of the GameObject (hash storage is very fast)
        /// </summary>
        /// <param name="_name">object name</param>
        /// <returns>object</returns>
        public GameObject FindGameObjectByName(string _name)
        {
            var tmp_Object = InstantiateUtility.FindByName(_name);
            switch (tmp_Object)
            {
                case GameObject tmp_GameObject:
                    return tmp_GameObject;
                case Transform tmp_Transform:
                    return tmp_Transform.gameObject;
            }

            return null;
        }

        /// <summary>
        /// Set the rendering status of the specified GameObject object
        /// </summary>
        /// <param name="_target">specified game object</param>
        /// <param name="_visible">visible state;True:Display,False:Hide</param>
        public void SetVisible(GameObject _target, bool _visible)
        {
            _target.SetActive(_visible);
        }


        /// <summary>
        /// Get the rendering status of the specified object
        /// </summary>
        /// <param name="_target">specified game object</param>
        /// <returns>Rendering status;True:Rendering,False:Hide</returns>
        public bool GetVisible(GameObject _target)
        {
            return _target.activeSelf;
        }


        /// <summary>
        /// Instantiate the prefab and store its instantiated object in the memory in the form of a hash.
        /// </summary>
        /// <param name="_prefab">Prefab</param>
        /// <param name="_uniqueName">instantiated object's name</param>
        /// <param name="_position">instantiated object's position</param>
        /// <param name="_quaternion">instantiated object's rotation</param>
        /// <returns>instantiated object</returns>
        public GameObject InstanceGameObject(GameObject _prefab, string _uniqueName, Vector3 _position,
            Quaternion _quaternion)
        {
            return InstantiateUtility.Instantiate(_prefab, _uniqueName, _position, _quaternion);
        }


        /// <summary>
        /// Instantiate the prefab and store its instantiated object in the memory in the form of a hash.
        /// </summary>
        /// <param name="_prefab">Prefab</param>
        /// <param name="_uniqueName">instantiated object's name</param>
        /// <param name="_parent">instantiated object's parent node</param>
        /// <returns>instantiated object</returns>
        public GameObject InstanceGameObject(GameObject _prefab, string _uniqueName, Transform _parent)
        {
            return InstantiateUtility.Instantiate(_prefab, _uniqueName, _parent);
        }

        #endregion

        #region Device

        /// <summary>
        /// Get the device's info
        /// </summary>
        /// <returns>devices info-Json</returns>
        public string GetDeviceInfo()
        {
            DeviceInfo tmp_DeviceInfo = new DeviceInfo()
            {
                deviceModel = SystemInfo.deviceModel,
                deviceName = SystemInfo.deviceName,
                deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsDeviceType = SystemInfo.deviceType.ToString(),
                graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor,
                graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
                graphicsDeviceID = SystemInfo.graphicsDeviceID,
                graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID,
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                graphicsMultiThreaded = SystemInfo.graphicsMultiThreaded,
                graphicsShaderLevel = SystemInfo.graphicsShaderLevel,
                maxTextureSize = SystemInfo.maxTextureSize,
                npotSupport = SystemInfo.npotSupport.ToString(),
                operatingSystem = SystemInfo.operatingSystem,
                processorCount = SystemInfo.processorCount,
                processorFrequency = SystemInfo.processorFrequency,
                processorType = SystemInfo.processorType,
                supportedRenderTargetCount = SystemInfo.supportedRenderTargetCount,
                supports2DArrayTextures = SystemInfo.supports2DArrayTextures,
                supports3DTextures = SystemInfo.supports3DTextures,
                supportsAccelerometer = SystemInfo.supportsAccelerometer,
                supportsAudio = SystemInfo.supportsAudio,
                supportsComputeShaders = SystemInfo.supportsComputeShaders,
                supportsGyroscope = SystemInfo.supportsGyroscope,
                supportsImageEffects = true,
                supportsLocationService = SystemInfo.supportsLocationService,
                supportsMotionVectors = SystemInfo.supportsMotionVectors,
                supportsVibration = SystemInfo.supportsVibration,
                systemMemorySize = SystemInfo.systemMemorySize,
                unsupportedIdentifier = SystemInfo.unsupportedIdentifier
            };
            return JsonUtility.ToJson(tmp_DeviceInfo);
        }


        /// <summary>
        /// Acquire system language
        /// </summary>
        /// <returns>system language</returns>
        public string GetSystemLanguage()
        {
            return Application.systemLanguage.ToString();
        }


        /// <summary>
        /// set up the screen orientation
        /// </summary>
        /// <param name="_orientation">screen orientation</param>
        public void SetScreenOrientation(ScreenOrientation _orientation)
        {
            Screen.orientation = _orientation;
        }

        #endregion

        #region AR

        /// <summary>
        /// 放置对象
        /// </summary>
        /// <param name="_anchorNotification">Data sent to the anchor manager</param>
        public void StickObject(AnchorNotificationData _anchorNotification)
        {
            ActionNotificationCenter.DefaultCenter.PostNotification(ActionParameterDataType.StickObject.ToString(),
                _anchorNotification);
        }

        /// <summary>
        /// Turn on and off the Focus algorithm
        /// </summary>
        /// <param name="_state">True:Turn on，False:Turn off</param>
        public void SetFocusAlgorithmState(bool _state)
        {
            AlgorithmSetterNotificationData tmp_SetterNotificationData = new AlgorithmSetterNotificationData
            {
                AlgorithmState = _state
            };
            ActionNotificationCenter.DefaultCenter.PostNotification(
                ActionParameterDataType.SetFocusAlgorithmState.ToString(),
                tmp_SetterNotificationData);
        }


        /// <summary>
        /// Exit the AR
        /// </summary>
        public void ExitAR()
        {
            ActionNotificationCenter.DefaultCenter.PostNotification(ActionParameterDataType.ExitAR.ToString(), null);
        }

        #endregion

        #region Native API

        /// <summary>
        /// Use the built-in browser of the app to open the url
        /// </summary>
        /// <param name="_data">Data sent to the built-in browser</param>
        public void OpenBuiltInBrowser(OpenBuiltInNotificationData _data)
        {
            ActionNotificationCenter.DefaultCenter.PostNotification(
                ActionParameterDataType.OpenBuiltInBrowser.ToString(),
                _data);
        }

        #endregion
    }
}