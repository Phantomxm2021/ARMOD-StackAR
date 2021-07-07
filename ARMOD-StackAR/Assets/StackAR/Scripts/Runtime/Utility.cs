using UnityEngine;

namespace StackAR
{
    public class Utility
    {
        public static Color32 Lerp4(Color32 _a, Color32 _b, Color32 _c, Color32 _d, float _time)
        {
            if (_time < .33f)
            {
                return Color.Lerp(_a, _b, _time / .33f);
            }

            return _time < .66f ? Color.Lerp(_b, _c, (_time - .33f) / .33f) : Color.Lerp(_c, _d, (_time - .66f) / .66f);
        }

        public static void ApplyColorToMesh(Mesh _mesh, float _time, Color32[] _cubeColors)
        {
            Vector3[] tmp_Vertices = _mesh.vertices;
            Color32[] tmp_Colors = new Color32[tmp_Vertices.Length];
            for (int tmp_Index = 0; tmp_Index < tmp_Vertices.Length; tmp_Index++)
            {
                tmp_Colors[tmp_Index] = Lerp4(_cubeColors[0], _cubeColors[1], _cubeColors[2], _cubeColors[3], _time);
            }

            _mesh.colors32 = tmp_Colors;
        }
    }
}