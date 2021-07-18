using System.Collections.Generic;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Runtime
{
    [System.Serializable]
    public class BundleDetails
    {
        public string m_FileName;

        public uint m_Crc;

        public string m_Hash;

        //public List<string> m_Dependencies;
    }

    [System.Serializable]
    public class ARExperienceData
    {
        public string BundleName;
        public List<string> AddressableName;
        public List<string> AssetsName;
        public BundleDetails BundleDetails;
    }
}