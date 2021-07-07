namespace com.Phantoms.ARMODPackageTools.Core
{
    [System.Serializable]
    public class ContentListElementModel : TreeElementModel
    {
        public string AssetName;
        public string AssetPath;
        public int InstanceId;

        public ContentListElementModel(string _name, string _assetPath, int _depth, int _id, int _instanceId) :
            base(_name, _depth, _id)
        {
            AssetName = _name;
            InstanceId = _instanceId;
            AssetPath = _assetPath;
        }
    }
}