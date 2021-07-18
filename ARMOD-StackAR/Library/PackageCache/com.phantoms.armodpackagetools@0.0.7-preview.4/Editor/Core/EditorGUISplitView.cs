using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class EditorGuiSplitView
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }

        public Rect ResizeHandleRect;

        readonly Direction splitDirection;
        float splitNormalizedPosition;
        private Vector2 scrollPosition;
        private Rect availableRect;

        public Rect FirstViewRect =>
            new Rect(availableRect.x, availableRect.y, availableRect.width * splitNormalizedPosition,
                availableRect.height);

        public Rect SecondViewRect =>
            new Rect(availableRect.width * splitNormalizedPosition, availableRect.y,
                availableRect.width * (1 - splitNormalizedPosition),
                availableRect.height);

        bool resize;

        public EditorGuiSplitView(Direction _splitDirection, float _splitPercent)
        {
            splitNormalizedPosition = _splitPercent;
            this.splitDirection = _splitDirection;
        }

        public Rect BeginSplitView()
        {
            Rect tmp_TempRect;

            if (splitDirection == Direction.Horizontal)
                tmp_TempRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            else
                tmp_TempRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            if (tmp_TempRect.width > 0.0f)
            {
                availableRect = tmp_TempRect;
            }

            if (splitDirection == Direction.Horizontal)
                scrollPosition = GUILayout.BeginScrollView(scrollPosition,
                    GUILayout.Width(availableRect.width * splitNormalizedPosition));
            else
                scrollPosition = GUILayout.BeginScrollView(scrollPosition,
                    GUILayout.Height(availableRect.height * splitNormalizedPosition));
            return tmp_TempRect;
        }

        public void Split()
        {
            GUILayout.EndScrollView();
            ResizeSplitFirstView();
        }

        public void EndSplitView()
        {
            if (splitDirection == Direction.Horizontal)
                EditorGUILayout.EndHorizontal();
            else
                EditorGUILayout.EndVertical();
        }

        private void ResizeSplitFirstView()
        {
            if (splitDirection == Direction.Horizontal)
                ResizeHandleRect = new Rect(availableRect.width * splitNormalizedPosition, availableRect.y, 1f,
                    availableRect.height);
            else
                ResizeHandleRect = new Rect(availableRect.x, availableRect.height * splitNormalizedPosition,
                    availableRect.width, 1f);

            EditorGUI.DrawRect(ResizeHandleRect, new Color(0, 0, 0, .5f));


            EditorGUIUtility.AddCursorRect(ResizeHandleRect,
                splitDirection == Direction.Horizontal ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);

            if (Event.current.type == EventType.MouseDown && ResizeHandleRect.Contains(Event.current.mousePosition))
            {
                resize = true;
            }

            if (resize)
            {
                if (splitDirection == Direction.Horizontal)
                    splitNormalizedPosition = Event.current.mousePosition.x / availableRect.width;
                else
                    splitNormalizedPosition = Event.current.mousePosition.y / availableRect.height;
            }

            if (Event.current.type == EventType.MouseUp)
                resize = false;
        }
    }
}