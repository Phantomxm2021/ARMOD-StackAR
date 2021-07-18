using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Video;

namespace com.Phantoms.ARMODPackageTools.Runtime
{
    public class PackageLoaderApiUtility
    {
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
        /// load a video from package by project name
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
    }
}