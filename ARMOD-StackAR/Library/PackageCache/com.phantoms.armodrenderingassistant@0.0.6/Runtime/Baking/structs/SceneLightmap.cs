using UnityEngine;

namespace com.Phantoms.RenderingAssistant.Runtime.structs
{
    public struct SceneLightmap
    {
        public int lightMapIndex;
        public Texture2D texColor;
        public Texture2D texDir;
        public Texture2D texShadow;
    }
}
