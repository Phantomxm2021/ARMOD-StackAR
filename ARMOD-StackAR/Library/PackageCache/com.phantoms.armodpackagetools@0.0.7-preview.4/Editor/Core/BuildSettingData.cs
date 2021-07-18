using UnityEditor;

namespace  com.Phantoms.ARMODPackageTools.Core
{
    [System.Serializable]
    public class BuildSettingData
    {
        public BuildTargetGroup BuildTargetGroup = BuildTargetGroup.iOS;
        public BuildTarget BuildTarget = BuildTarget.iOS;
        public BuildCompressionType BuildCompression = BuildCompressionType.LZ4;
    }

    public enum BuildCompressionType
    {
        Uncompressed,
        LZ4,
        LZMA,
        UncompressedRuntime,
        LZ4Runtime
    }
}