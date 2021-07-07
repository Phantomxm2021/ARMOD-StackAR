using System;
using UnityEngine;

namespace com.Phantoms.RenderingAssistant.Runtime.structs
{
    [Serializable] public struct RendererInfo
    {
        public Renderer renderer;
        public int lightmapIndex;
        public Vector4 lightmapOffsetScale;
    }
}