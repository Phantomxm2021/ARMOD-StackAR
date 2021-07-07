using UnityEditor.IMGUI.Controls;

namespace  com.Phantoms.ARMODPackageTools.Core
{
    internal class TreeViewItem<T> : TreeViewItem where T : TreeElementModel
    {
        public T data { get; private set; }

        public TreeViewItem (int _id, int _depth, string _displayName, T _data) : base (_id, _depth, _displayName)
        {
            this.data = _data;
        }
    }
}