using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace  com.Phantoms.ARMODPackageTools.Core
{
    internal class ContentListHeader : MultiColumnHeader
    {
        Mode mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public ContentListHeader(MultiColumnHeaderState _state)
            : base(_state)
        {
            GetMode = Mode.DefaultHeader;
        }

        public Mode GetMode
        {
            get => mode;
            set
            {
                mode = value;
                switch (mode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column _column, Rect _headerRect,
            int _columnIndex)
        {
            // Default column header gui
            base.ColumnHeaderGUI(_column, _headerRect, _columnIndex);

            // Add additional info for large header
            if (GetMode == Mode.LargeHeader)
            {
                // Show example overlay stuff on some of the columns
                if (_columnIndex > 2)
                {
                    _headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label(_headerRect, 36 + _columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }
}