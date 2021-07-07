using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public interface ISubView
    {
        void DrawSubView(Rect _area);
        void Dispose();
    }
}