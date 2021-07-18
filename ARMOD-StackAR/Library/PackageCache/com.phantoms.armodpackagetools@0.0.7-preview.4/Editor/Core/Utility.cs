using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


namespace com.Phantoms.ARMODPackageTools.Core
{
    public static class Utility
    {
        public static bool IsCorrectProjectStructure(string _path)
        {
            DirectoryInfo tmp_DirectoryInfo = new DirectoryInfo(_path);
            var tmp_FileInfos = tmp_DirectoryInfo.GetDirectories();
            var tmp_ArtworkFolder = tmp_FileInfos.Where(_info => _info.Name.Equals("Artwork")).ToArray().Length > 0;
            var tmp_ScriptsFolder = tmp_FileInfos.Where(_info => _info.Name.Equals("Scripts")).ToArray().Length > 0;
            return tmp_ArtworkFolder && tmp_ScriptsFolder;
        }

        public static bool IsUrl(string _str)
        {
            try
            {
                const string tmp_URL = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
                return Regex.IsMatch(_str, tmp_URL);
            }
            catch (Exception tmp_Ex)
            {
                Debug.LogError(tmp_Ex.Message);
                return false;
            }
        }

        public static string GetRootDataPath(string _fileName = null)
        {
            var tmp_DataPath = $"Assets/{ConstKey.TOOLS_DATA_CACHE_FOLDER}/Data/";
            if (!Directory.Exists(tmp_DataPath))
                Directory.CreateDirectory(tmp_DataPath);
            return string.IsNullOrEmpty(_fileName) ? tmp_DataPath : Path.Combine(tmp_DataPath, _fileName);
        }

        public static void DrawHorizontalDivLine(int _height = 1)
        {
            Rect tmp_Rect = EditorGUILayout.GetControlRect(false, _height);
            tmp_Rect.height = _height;
            EditorGUI.DrawRect(tmp_Rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        public static string ShortenPath(string _fullPath)
        {
            int tmp_SubStringStartPos = _fullPath.IndexOf("Assets", StringComparison.Ordinal);
            return _fullPath.Substring(tmp_SubStringStartPos, _fullPath.Length - tmp_SubStringStartPos);
        }


        /// <summary>
        /// Copy files to the specified directory of the current project
        /// </summary>
        /// <param name="_sourceFilePath">Source file</param>
        /// <param name="_destFilePath">Dest file</param>
        /// <param name="_suffix">file suffix</param>
        /// <returns>Asset path in current project</returns>
        public static string CopyFileToProject(string _sourceFilePath, string _destFilePath, string _suffix = null)
        {
            if (!File.Exists(_sourceFilePath))
            {
                Debug.LogError("You need to write at least one script in the project!");
                return null;
            }

            string tmp_FileName = new DirectoryInfo(_sourceFilePath).Name.ToLower();
            string tmp_DestPath;
            if (!string.IsNullOrEmpty(_suffix))
                tmp_DestPath = Path.Combine(_destFilePath, tmp_FileName) + "." + _suffix;
            else
                tmp_DestPath = Path.Combine(_destFilePath, tmp_FileName);

            File.Copy(_sourceFilePath, tmp_DestPath, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return ShortenPath(tmp_DestPath);
        }


        public static bool RightClicked(Rect _area)
        {
            return Event.current.type == EventType.MouseDown
                   && Event.current.button == 1
                   && _area.Contains(Event.current.mousePosition);
        }

        public static void SaveProjectConfigCache(ProjectListElementModel _project, string _destFilePath)
        {
            if (!Directory.Exists(_destFilePath))
                Directory.CreateDirectory(_destFilePath);

            var tmp_ConfigCache = JsonUtility.ToJson(_project);
            File.WriteAllText(Path.Combine(_destFilePath, "ConfigCacheJson.json"), tmp_ConfigCache, Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        public static void ReadProjectConfigCache(string _destFilePath, out ProjectListElementModel _projectListElement)
        {
            try
            {
                var tmp_AllText = File.ReadAllText(_destFilePath, Encoding.UTF8);
                _projectListElement = JsonUtility.FromJson<ProjectListElementModel>(tmp_AllText);
            }
            catch (Exception tmp_Exception)
            {
                Debug.LogError(tmp_Exception.Message);
                _projectListElement = null;

            }
        }

//        public static Texture2D GenerateQrCode(string _qrMessage)
//        {
//            var tmp_QrCodeTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
//            tmp_QrCodeTexture.SetPixels32(Encode(_qrMessage, 256, 256));
//            tmp_QrCodeTexture.Apply(false);
//            return tmp_QrCodeTexture;
//        }

//        private static Color32[] Encode(string _formatStr, int _width, int _height)
//        {
//            var tmp_Writer = new BarcodeWriter
//            {
//                Format = BarcodeFormat.QR_CODE,
//                Options = new QrCodeEncodingOptions
//                {
//                    Height = _height,
//                    Width = _width
//                }
//            };
//            return tmp_Writer.Write(_formatStr);
//        }
    }
}