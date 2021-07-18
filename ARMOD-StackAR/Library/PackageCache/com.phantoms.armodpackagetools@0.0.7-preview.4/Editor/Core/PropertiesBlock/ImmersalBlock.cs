using System;
using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ImmersalBlock : AbstractBlock
    {
        private bool immersalBlockToggle = true;

        private readonly Configures property;
        // private bool showMapList = false;
        // private bool showServerMapList = false;

        public ImmersalBlock(Configures _property) : base(_property)
        {
            property = _property;
            // if (property.Maps.Count == 0)
            //     property.Maps.Add(string.Empty);
        }

        public override Rect DrawBlock(Rect _area)
        {
            immersalBlockToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(immersalBlockToggle, "Immersal Setting", null,
                    ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();


            if (immersalBlockToggle)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                property.UseServerLocalizer =
                    EditorGUILayout.Toggle("Use Server Localizer", property.UseServerLocalizer);
                property.TurnOffServerLocalizedAfterSuccess = EditorGUILayout.Toggle(
                    "Turn Off Server Localized After Success", property.TurnOffServerLocalizedAfterSuccess);
                property.LocalizationInterval =
                    EditorGUILayout.FloatField("Localization Interval", property.LocalizationInterval);
                property.RenderMode = (RenderMode) EditorGUILayout.EnumPopup("Render Mode", property.RenderMode);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
                Utility.DrawHorizontalDivLine();
            }


            EditorGUILayout.EndFoldoutHeaderGroup();
            return tmp_BlockRect;
        }

        public override bool OnRemoved()
        {
            // property.Maps.Clear();
            return true;
        }

        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}features#immersal-setting");
            return false;
        }


        void ShowHeaderContextMenu(Rect _position)
        {
            var tmp_Menu = new GenericMenu();
            tmp_Menu.AddItem(new GUIContent("Reset"), false, () => OnRemoved());
            tmp_Menu.AddItem(new GUIContent(CONST_HELP_BUTTON_TITLE), false, () => OpenReference());
            tmp_Menu.DropDown(_position);
        }
    }
}