using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    [Serializable]
    public class TreeElementModel
    {
        [SerializeField] private int id;
        [SerializeField] private string name;
        [SerializeField] private int depth = -1;
        [NonSerialized] private TreeElementModel parent;
        [NonSerialized] private List<TreeElementModel> childrens;

        public int Depth
        {
            get => depth;
            set => depth = value;
        }

        public TreeElementModel Parent
        {
            get => parent;
            set => parent = value;
        }

        public List<TreeElementModel> Children
        {
            get => childrens;
            set => childrens = value;
        }

        public bool HasChildren => childrens != null && childrens.Count > 0;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public int Id
        {
            get => id;
            set => id = value;
        }

        public TreeElementModel()
        {
        }

        public TreeElementModel(string _name, int _depth, int _id)
        {
            name = _name;
            id = _id;
            depth = _depth;
        }
    }
}