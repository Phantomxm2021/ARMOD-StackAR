using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class DrawPropertyView : ISubView, IDisposable
    {
        private Rect drawArea;
        private Vector2 scrollViewPosition;
        private AlgorithmType currentAlgorithmType;

        //Only Editor,For Avoid duplication
        private readonly Dictionary<string, AbstractBlock> blockSubViewDict;

        private readonly ProjectListElementModel projectListElementModel;
        private const string SCRIPT_NAME_SPACE = "com.Phantoms.ARMODPackageTools.Core.";

        private readonly List<string> menuList = new List<string>
        {
            "Algorithm",
            "Version Checker",
            "Programmable",
            "Visualizer/Plane",
            "Visualizer/Point Cloud",
            "Features/ARWorld Scale",
            "Features/iOS/Coaching Overlay(iOS Only)",
            "Features/Environment Probe",
            "Features/Light Estimation",
            "Features/Post Processing",
            "Features/AR Interaction",
            "Features/Immersal",
            "Features/Face Mesh",
            "Features/Occlusion",
            "Graphics/Quality Level"
        };

        private readonly List<string> blocks = new List<string>
        {
            nameof(VersionBlock),
            nameof(AlgorithmBlock),
            nameof(ProgrammableBlock),
            nameof(PlaneVisualizerBlock),
            nameof(PointCloudVisualizerBlock),
            nameof(ARWorldScaleBlock),
            nameof(CoachingOverlayBlock),
            nameof(EnvironmentProbeBlock),
            nameof(LightEstimationBlock),
            nameof(PostProcessingBlock),
            nameof(ARInteractionBlock),
            nameof(ImmersalBlock),
            nameof(FaceMeshBlock),
            nameof(OcclusionBlock),
            nameof(QualityBlock),
        };

        public DrawPropertyView(ProjectDataModel _projectDataModel)
        {
            var tmp_ProjectDataModel = _projectDataModel;

            projectListElementModel =
                tmp_ProjectDataModel.ProjectTreeElements[tmp_ProjectDataModel.CurrentContentViewId];

            blockSubViewDict = new Dictionary<string, AbstractBlock>();
            
            //Add dfault block
            var tmp_FullPathOfAlgorithmBlock = typeof(AlgorithmBlock).FullName;
            var tmp_FullPathOfVersionCheckerBlock = typeof(VersionBlock).FullName;
            var tmp_FullPathOfARWorldScaleBlock = typeof(ARWorldScaleBlock).FullName;

            if (!projectListElementModel.GetBlock.Contains(tmp_FullPathOfAlgorithmBlock))
            {
                ConvertToBlock(tmp_FullPathOfAlgorithmBlock);
            }

            if (!projectListElementModel.GetBlock.Contains(tmp_FullPathOfVersionCheckerBlock))
            {
                ConvertToBlock(tmp_FullPathOfVersionCheckerBlock);
            }
            if (!projectListElementModel.GetBlock.Contains(tmp_FullPathOfARWorldScaleBlock))
            {
                ConvertToBlock(tmp_FullPathOfARWorldScaleBlock);
            }


            currentAlgorithmType = projectListElementModel.Configures.Algorithm;
            MappingBlock();
        }

        private void MappingBlock()
        {
            foreach (var tmp_Block in projectListElementModel.GetBlock)
            {
                Type tmp_Type = Type.GetType(tmp_Block);
                if (tmp_Type == null) continue;
                object tmp_Instance =
                    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    Activator.CreateInstance(type: tmp_Type, args: projectListElementModel.Configures);
                AddBlock(tmp_Instance as AbstractBlock);
            }
        }

        private bool AddBlock(AbstractBlock _abstractBlock)
        {
            var tmp_UniqueId = _abstractBlock.GetType().FullName;
            bool tmp_AlreadyExists =
                blockSubViewDict.TryGetValue(tmp_UniqueId ?? throw new Exception(), out AbstractBlock _);

            if (!tmp_AlreadyExists)
                blockSubViewDict.Add(tmp_UniqueId, _abstractBlock);

            return !tmp_AlreadyExists;
        }


        private void RemoveBlock(string _abstractBlock)
        {
            try
            {
                if (blockSubViewDict.TryGetValue(_abstractBlock, out AbstractBlock tmp_AbstractBlock))
                {
                    switch (projectListElementModel.Configures.Algorithm)
                    {
                        case AlgorithmType.Immersal:
                            if (tmp_AbstractBlock.GetType() == typeof(ImmersalBlock))
                                throw new Exception();

                            break;
                        case AlgorithmType.FaceMesh:
                            if (tmp_AbstractBlock.GetType() == typeof(FaceMeshBlock))
                                throw new Exception();
                            break;
                    }

                    if (tmp_AbstractBlock.GetType() == typeof(AlgorithmBlock)
                        || tmp_AbstractBlock.GetType() == typeof(VersionBlock)
                        || tmp_AbstractBlock.GetType() == typeof(ARWorldScaleBlock))
                        throw new Exception("Can not removing");

                    tmp_AbstractBlock.OnRemoved();
                    RemoveBlockCol(tmp_AbstractBlock);
                    blockSubViewDict.Remove(_abstractBlock);
                }
            }
            catch (Exception tmp_Exception)
            {
                EditorUtility.DisplayDialog("Error!", "You can not remove this block!", "Ok");
            }
        }


        private void CachingBlockCol(AbstractBlock _abstractBlock)
        {
            string tmp_BlockName = _abstractBlock.GetType().ToString();
            if (!projectListElementModel.GetBlock.Contains(tmp_BlockName))
                projectListElementModel.AddElementFromList(projectListElementModel.GetBlock, tmp_BlockName);
        }

        private void RemoveBlockCol(AbstractBlock _abstractBlock)
        {
            projectListElementModel.RemoveElementFromList(projectListElementModel.GetBlock,
                _abstractBlock.GetType().ToString());
        }

        public void DrawSubView(Rect _area)
        {
            drawArea = _area;
            DrawConfigure();
        }


        private void DrawConfigure()
        {
            CheckDependentBlock();

            scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition);

            //Draw and add remove button for this block
            foreach (var tmp_Block in projectListElementModel.GetBlock)
            {
                if (!blockSubViewDict.TryGetValue(tmp_Block, out AbstractBlock tmp_AbstractBlock)) continue;
                var tmp_Area = tmp_AbstractBlock.DrawBlock(drawArea);
                if (!Utility.RightClicked(tmp_Area)) continue;

                var tmp_Menu = new GenericMenu();
                tmp_Menu.AddItem(new GUIContent("Remove"), false,
                    () => { RemoveBlock(tmp_Block); });
                tmp_Menu.ShowAsContext();
                EditorGUILayout.EndScrollView();
                return;
            }


            GUILayout.Space(20);

            //Paint only when the number of blocks is 1
            if (projectListElementModel.GetBlock.Count <= 1)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox(" Right-click to add configuration options ", MessageType.Info);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();


            if (!Utility.RightClicked(drawArea)) return;

            RightClickedMenu();
        }

        private void CheckDependentBlock()
        {
            if (currentAlgorithmType == projectListElementModel.Configures.Algorithm) return;
            currentAlgorithmType = projectListElementModel.Configures.Algorithm;

            //Remove immersal block when algorithm is not immersal
            if (projectListElementModel.Configures.Algorithm != AlgorithmType.Immersal)
            {
                var tmp_BlockFullName = typeof(ImmersalBlock).FullName;
                if (projectListElementModel.GetBlock.Contains(tmp_BlockFullName))
                {
                    RemoveBlock(tmp_BlockFullName);
                }
            }

            // algorithm is immersal it will add automatic
            if (projectListElementModel.Configures.Algorithm == AlgorithmType.Immersal)
            {
                var tmp_BlockFullName = typeof(ImmersalBlock).FullName;
                if (!projectListElementModel.GetBlock.Contains(tmp_BlockFullName))
                    ConvertToBlock(tmp_BlockFullName);
            }


            if (projectListElementModel.Configures.Algorithm != AlgorithmType.FaceMesh)
            {
                var tmp_BlockFullName = typeof(FaceMeshBlock).FullName;
                if (projectListElementModel.GetBlock.Contains(tmp_BlockFullName))
                {
                    RemoveBlock(tmp_BlockFullName);
                }
            }

            if (projectListElementModel.Configures.Algorithm == AlgorithmType.FaceMesh)
            {
                var tmp_BlockFullName = typeof(FaceMeshBlock).FullName;
                if (!projectListElementModel.GetBlock.Contains(tmp_BlockFullName))
                    ConvertToBlock(tmp_BlockFullName);
            }
        }

        private void RightClickedMenu()
        {
            var tmp_MenuContext = new GenericMenu();
            for (int tmp_MenuIdx = 0; tmp_MenuIdx < menuList.Count; tmp_MenuIdx++)
            {
                var tmp_Idx = tmp_MenuIdx;
                tmp_MenuContext.AddItem(new GUIContent(menuList[tmp_MenuIdx]), false,
                    () => ConvertToBlock($"{SCRIPT_NAME_SPACE}{blocks[tmp_Idx]}"));
            }

            tmp_MenuContext.ShowAsContext();
        }


        /// <summary>
        /// Use the name of the block to convert the corresponding block object.
        /// </summary>
        /// <param name="_blockType">name of the block</param>
        private void ConvertToBlock(string _blockType)
        {
            var tmp_Block = BlockFactory.CreateBlock(_blockType, projectListElementModel.Configures);
            if (AddBlock(tmp_Block))
            {
                CachingBlockCol(tmp_Block);
            }
            else
            {
                Debug.LogError("The block already exists, please do not add it repeatedly.");
                MainView.ShowNotify("The block already exists, please do not add it repeatedly.");
            }
        }

        public void Dispose()
        {
            EditorUtility.SetDirty(projectListElementModel.Configures);
        }
    }
}