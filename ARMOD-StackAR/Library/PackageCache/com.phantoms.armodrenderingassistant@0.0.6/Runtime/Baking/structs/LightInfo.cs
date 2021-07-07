using System;
using UnityEngine;

namespace com.Phantoms.RenderingAssistant.Runtime.structs
{
    [Serializable] public struct LightInfo
    {
        public Light light;
        public int lightmapBaketype;
        public int mixedLightingMode;
    }
}
