using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using com.Phantoms.ARMODPackageTools.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;
using com.Phantoms.ActionNotification.Runtime;

namespace com.Phantoms.ARMODAPI.Runtime
{
    public class API
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


        /// <summary>
        /// load a font from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadFont(string _projectName, string _wannaLoadedAssetName,
            Action<Font> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }


        /// <summary>
        /// load a animation clip from package by project name
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadAnimationClip(string _projectName, string _wannaLoadedAssetName,
            Action<AnimationClip> _loadedCallback)
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }

        /// <summary>
        /// Load a unity asset from our package by project name and wanna load asset name.
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        /// <typeparam name="T">Unity Object</typeparam>
        public void LoadAsset<T>(string _projectName, string _wannaLoadedAssetName, Action<T> _loadedCallback)
            where T : UnityEngine.Object
        {
            BasePackageLoaderUtility.LoadAssetFromPackage(_projectName, _wannaLoadedAssetName, _loadedCallback);
        }

        /// <summary>
        /// Away/Task. Load asset from package by project name.
        /// </summary> 
        /// <param name="_projectName"></param>
        /// <param name="_wannaLoadedAssetName"></param>
        /// <typeparam name="T">Unity Object</typeparam>
        /// <returns>Task</returns>
        public async Task<T> LoadAssetAsync<T>(string _projectName, string _wannaLoadedAssetName)
            where T : UnityEngine.Object
        {
            return await BasePackageLoaderUtility.LoadAssetFromPackage<T>(_projectName, _wannaLoadedAssetName);
        }

        /// <summary>
        /// load a material from package by project name,0.0.2 API
        /// </summary>
        /// <param name="_projectName">your project name</param>
        /// <param name="_wannaLoadedAssetName">you wanna load asset name</param>
        /// <param name="_loadedCallback">call back when loaded</param>
        public void LoadMaterial(string _projectName, string _wannaLoadedAssetName,
            Action<Material> _loadedCallback)
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
        /// Resize the AR world scale
        /// </summary>
        /// <param name="_worldScale">new world scale</param>
        public void ResizeARWorldScale(float _worldScale)
        {
            ActionNotificationCenter.DefaultCenter.PostNotification(nameof(ActionParameterDataType.ResizeARWorldScale),
                new ResizeARWorldScaleNotificationData() {WorldScale = _worldScale});
        }

        /// <summary>
        /// Get current sdk version
        /// </summary>
        /// <returns>SDK Version string</returns>
        public string TryAcquireSDKVersion()
        {
            var tmp_VersionObjects = ActionNotificationCenter.DefaultCenter.PostNotificationWithResult(
                nameof(ActionParameterDataType.TryAcquireSDKVersion),
                null);
            if (tmp_VersionObjects != null && tmp_VersionObjects.Count > 0) return tmp_VersionObjects[0] as string;
            return null;
        }

        /// <summary>
        /// Addition AR algorithm, It will allow multiple algorithms to be mixed.
        /// </summary>
        /// <param name="_data"></param>
        public void ChangeARAlgorithmLife(ARAlgorithmNotificationData _data)
        {
            ActionNotificationCenter.DefaultCenter.PostNotification(nameof(ActionParameterDataType.ARAlgorithmLifeCTRL),
                _data);
        }

        /// <summary>
        /// Create an anchor and place game object. If you are main algorithm mode is not a `Anchor`, you must call `AdditionARAlgorithm` to activate it.
        /// </summary>
        /// <param name="_anchorNotification">Data sent to the anchor manager</param>
        public void StickObject(AnchorNotificationData _anchorNotification)
        {
            ActionNotificationCenter.DefaultCenter.PostNotification(nameof(ActionParameterDataType.StickObject),
                _anchorNotification);
        }

        /// <summary>
        /// Turn on and off the Focus algorithm.
        /// </summary>
        /// <param name="_state">True:Turn on，False:Turn off</param>
        public void SetFocusAlgorithmState(bool _state)
        {
            AlgorithmSetterNotificationData tmp_SetterNotificationData = new AlgorithmSetterNotificationData
            {
                AlgorithmState = _state
            };
            ActionNotificationCenter.DefaultCenter.PostNotification(
                nameof(ActionParameterDataType.SetFocusAlgorithmState),
                tmp_SetterNotificationData);
        }


        /// <summary>
        /// Exit the AR
        /// </summary>
        public void ExitAR()
        {
            ActionNotificationCenter.DefaultCenter.PostNotification(nameof(ActionParameterDataType.ExitAR), null);
        }


        /// <summary>
        /// Get current AR frame
        /// </summary>
        /// <returns>It maybe null, plz attention</returns>
        public Texture2D TryAcquireCurrentFrame(TryAcquireCurrentFrameNotificationData _data)
        {
            var tmp_Objects = ActionNotificationCenter.DefaultCenter.PostNotificationWithResult(
                nameof(ActionParameterDataType.TryAcquireCurrentFrame), _data);
            if (tmp_Objects == null || tmp_Objects.Count == 0) return null;
            if (tmp_Objects[0] is Texture2D tmp_Texture2D)
                return tmp_Texture2D;
            return null;
        }


        /// <summary>
        /// Get AR Occlusion frame. SDK 0.0.2
        /// </summary>
        /// <returns>AR Occlusion frame texture2d. It maybe null, plz attention</returns>
        public Texture2D TryAcquireAROcclusionFrame(AROcclusionNotificationData _data)
        {
            var tmp_Objects =
                ActionNotificationCenter.DefaultCenter.PostNotificationWithResult(
                    nameof(ActionParameterDataType.TryAcquireAROcclusionFrame), _data);
            if (tmp_Objects == null || tmp_Objects.Count == 0) return null;
            if (tmp_Objects[0] is Texture2D tmp_Texture2D)
                return tmp_Texture2D;
            return null;
        }


        /// <summary>
        /// Get light estimate value
        /// </summary>
        /// <returns>It maybe null, plz attention</returns>
        public Light TryAcquireLightEstimateValue()
        {
            var tmp_Objects = ActionNotificationCenter.DefaultCenter.PostNotificationWithResult(
                nameof(ActionParameterDataType.TryAcquireLightEstimateValue), null);
            if (tmp_Objects == null || tmp_Objects.Count == 0) return null;
            if (tmp_Objects[0] is Light tmp_Light)
                return tmp_Light;
            return null;
        }

        /// <summary>
        /// Check whether the current device supports ARKit or ARCore.
        /// </summary>
        /// <returns>True:Supports,False:otherwise</returns>
        public bool CheckARAvailability()
        {
            var tmp_Objects =
                ActionNotificationCenter.DefaultCenter.PostNotificationWithResult(
                    nameof(ActionParameterDataType.CheckARAvailability), null);
            if (tmp_Objects == null || tmp_Objects.Count == 0) return false;
            if (tmp_Objects[0] is string tmp_Availability)
                return tmp_Availability.Equals("True");
            return false;
        }


        /// <summary>
        /// Check AR features is support on current device
        /// </summary>
        /// <param name="_featureName"></param>
        /// <returns></returns>
        public bool CheckFeatureAvailability(string _featureName)
        {
            var tmp_BaseData = new BaseNotificationData {BaseData = _featureName};
            var tmp_Results =
                ActionNotificationCenter.DefaultCenter.PostNotificationWithResult(
                    nameof(ActionParameterDataType.CheckARFeaturesAvailability), tmp_BaseData);
            if (tmp_Results == null || tmp_Results.Count == 0) return false;
            if (tmp_Results[0] is string tmp_Availability)
                return tmp_Availability.Equals("True");
            return false;
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
                nameof(ActionParameterDataType.OpenBuiltInBrowser), _data);
        }


        /// <summary>
        /// Try Acquire App Info
        /// </summary>
        /// <param name="_data">Try acquire operation type</param>
        public List<object> TryAcquireAppInfo(TryAcquireAppInfoNotificationData _data)
        {
            return ActionNotificationCenter.DefaultCenter.PostNotificationWithResult(
                nameof(ActionParameterDataType.TryAcquireAppInfo), _data);
        }

        #endregion
    }
}