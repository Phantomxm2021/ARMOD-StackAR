using System.Collections.Generic;

namespace  com.Phantoms.ARMODPackageTools.Core
{
    [System.Serializable]
    public class AssemblyDataModel
    {
        public string name;

        public List<string> references;

        public List<string> includePlatforms;


        public List<string> excludePlatforms;


        public bool allowUnsafeCode;


        public bool overrideReferences;


        public List<string> precompiledReferences;


        public bool autoReferenced;


        public List<string> defineConstraints;

        public List<string> versionDefines;


        public bool noEngineReferences;
    }
}