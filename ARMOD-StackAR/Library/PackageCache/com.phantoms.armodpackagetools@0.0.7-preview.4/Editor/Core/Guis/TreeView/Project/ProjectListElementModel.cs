using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    [Serializable]
    public class ProjectListElementModel : TreeElementModel
    {
        public bool IsOpened;

        //public string ProjectPath;
        public Configures Configures;
        public BuildSettingData BuildSettingData;
        [SerializeField] private List<ContentListElementModel> ContentTreeElements;
        [SerializeField] private List<string> Blocks;
        public string SubPath = string.Empty;


        public string GetProjectPath()
        {
            return $"{Application.dataPath.Replace("/Assets", "")}/{SubPath}";
        }

        public ProjectListElementModel(string _name, int _depth, int _id, string _subPath) : base(_name, _depth,
            _id)
        {
            ContentTreeElements = new List<ContentListElementModel>();
            if (ContentTreeElements.Count > 0) return;
            var tmp_ContentTreeElements = new ContentListElementModel("Root", null, -1, -1, -1);
            ContentTreeElements.Add(tmp_ContentTreeElements);


            if (string.IsNullOrEmpty(_subPath)) return;
            SubPath = Utility.ShortenPath(_subPath);
            var tmp_ConfigureFolder = Path.Combine(GetProjectPath(), "Configures");
            var tmp_ShortPath = Utility.ShortenPath(Path.Combine(tmp_ConfigureFolder, "Configure.asset"));
            if (!Directory.Exists(tmp_ConfigureFolder))
            {
                Directory.CreateDirectory(tmp_ConfigureFolder);
            }

            if (!File.Exists(Path.Combine(tmp_ConfigureFolder, "Configure.asset")))
            {
                Configures = ScriptableObject.CreateInstance<Configures>();
                AssetDatabase.CreateAsset(Configures, tmp_ShortPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Configures = AssetDatabase.LoadAssetAtPath<Configures>(tmp_ShortPath);
            }

            //ProjectPath = GetProjectPath;

            BuildSettingData = new BuildSettingData();
            Blocks = new List<string>();
        }


        public int GetBlockCount => Blocks.Count;
        public List<string> GetBlock => Blocks;
        public int GetContentCount => ContentTreeElements.Count;
        public List<ContentListElementModel> GetContentTreeElements => ContentTreeElements;

        public void AddElementFromList<T>(List<T> _list, T _element)
        {
            _list.Add(_element);
            Utility.SaveProjectConfigCache(this, Path.Combine(GetProjectPath(), "AutomaticGenerated"));
        }


        public void RemoveElementFromList<T>(List<T> _list, T _element)
        {
            _list.Remove(_element);
            Utility.SaveProjectConfigCache(this, Path.Combine(GetProjectPath(), "AutomaticGenerated"));
        }

        public void RemoveElementFromList<T>(List<T> _list, int _idx)
        {
            _list.RemoveAt(_idx);
            Utility.SaveProjectConfigCache(this, Path.Combine(GetProjectPath(), "AutomaticGenerated"));
        }
    }
}