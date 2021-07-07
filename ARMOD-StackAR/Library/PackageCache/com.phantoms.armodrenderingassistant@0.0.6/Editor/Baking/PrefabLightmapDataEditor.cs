using System.Collections.Generic;
using com.Phantoms.RenderingAssistant.Runtime;
using UnityEditor;
using UnityEngine;

namespace com.Phantoms.RenderingAssistant.Editor
{
    [CustomEditor(typeof(PrefabLightmapData))]
    public class PrefabLightmapDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Bake"))
            {
                GenerateLightmapInfo();
            }
        }

        static void GenerateLightmapInfo()
        {
            if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand)
            {
                Debug.LogError(
                    "ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                return;
            }

            Lightmapping.Bake();

            PrefabLightmapData[] tmp_Prefabs = FindObjectsOfType<PrefabLightmapData>();

            foreach (var tmp_Instance in tmp_Prefabs)
            {
                var tmp_GameObject = tmp_Instance.gameObject;
                var tmp_RendererInfos = new List<PrefabLightmapData.RendererInfo>();
                var tmp_Lightmaps = new List<Texture2D>();
                var tmp_LightmapsDir = new List<Texture2D>();
                var tmp_ShadowMasks = new List<Texture2D>();
                var tmp_LightsInfos = new List<PrefabLightmapData.LightInfo>();

                GenerateLightmapInfo(tmp_GameObject, tmp_RendererInfos, tmp_Lightmaps, tmp_LightmapsDir,
                    tmp_ShadowMasks, tmp_LightsInfos);

                tmp_Instance.m_RendererInfo = tmp_RendererInfos.ToArray();
                tmp_Instance.m_Lightmaps = tmp_Lightmaps.ToArray();
                tmp_Instance.m_LightmapsDir = tmp_LightmapsDir.ToArray();
                tmp_Instance.m_LightInfo = tmp_LightsInfos.ToArray();
                tmp_Instance.m_ShadowMasks = tmp_ShadowMasks.ToArray();
#if UNITY_2018_3_OR_NEWER
                var tmp_TargetPrefab =
                    PrefabUtility.GetCorrespondingObjectFromOriginalSource(tmp_Instance.gameObject);
                if (tmp_TargetPrefab == null) continue;
                // 根结点
                GameObject tmp_Root = PrefabUtility.GetOutermostPrefabInstanceRoot(tmp_Instance.gameObject);
                //如果当前预制体是是某个嵌套预制体的一部分（IsPartOfPrefabInstance）
                if (tmp_Root != null)
                {
                    GameObject tmp_RootPrefab = PrefabUtility.GetCorrespondingObjectFromSource(tmp_Instance.gameObject);
                    string tmp_RootPath = AssetDatabase.GetAssetPath(tmp_RootPrefab);
                    //打开根部预制体
                    PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(tmp_Root,
                        PrefabUnpackMode.OutermostRoot);
                    try
                    {
                        //Apply各个子预制体的改变
                        PrefabUtility.ApplyPrefabInstance(tmp_Instance.gameObject, InteractionMode.AutomatedAction);
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        //重新更新根预制体
                        PrefabUtility.SaveAsPrefabAssetAndConnect(tmp_Root, tmp_RootPath,
                            InteractionMode.AutomatedAction);
                    }
                }
                else
                {
                    PrefabUtility.ApplyPrefabInstance(tmp_Instance.gameObject, InteractionMode.AutomatedAction);
                }
#else
            var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
            if (targetPrefab != null)
            {
                //UnityEditor.Prefab
                UnityEditor.PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
            }
#endif
            }
        }

        static void GenerateLightmapInfo(GameObject _root, List<PrefabLightmapData.RendererInfo> _rendererInfos,
            List<Texture2D> _lightmaps, List<Texture2D> _lightmapsDir, List<Texture2D> _shadowMasks,
            List<PrefabLightmapData.LightInfo> _lightsInfo)
        {
            var tmp_Renderers = _root.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer tmp_Renderer in tmp_Renderers)
            {
                if (tmp_Renderer.lightmapIndex != -1)
                {
                    PrefabLightmapData.RendererInfo tmp_Info = new PrefabLightmapData.RendererInfo
                    {
                        Renderer = tmp_Renderer
                    };

                    if (tmp_Renderer.lightmapScaleOffset != Vector4.zero)
                    {
                        tmp_Info.LightmapOffsetScale = tmp_Renderer.lightmapScaleOffset;

                        var tmp_LightmapIndex = tmp_Renderer.lightmapIndex;
                        Texture2D tmp_Lightmap = LightmapSettings.lightmaps[tmp_LightmapIndex].lightmapColor;
                        Texture2D tmp_LightmapDir = LightmapSettings.lightmaps[tmp_LightmapIndex].lightmapDir;
                        Texture2D tmp_ShadowMask = LightmapSettings.lightmaps[tmp_LightmapIndex].shadowMask;

                        tmp_Info.LightmapIndex = _lightmaps.IndexOf(tmp_Lightmap);
                        if (tmp_Info.LightmapIndex == -1)
                        {
                            tmp_Info.LightmapIndex = _lightmaps.Count;
                            _lightmaps.Add(tmp_Lightmap);
                            _lightmapsDir.Add(tmp_LightmapDir);
                            _shadowMasks.Add(tmp_ShadowMask);
                        }

                        _rendererInfos.Add(tmp_Info);
                    }
                }
            }

            var tmp_Lights = _root.GetComponentsInChildren<Light>(true);

            foreach (Light tmp_Light in tmp_Lights)
            {
                PrefabLightmapData.LightInfo tmp_LightInfo = new PrefabLightmapData.LightInfo();
                tmp_LightInfo.Light = tmp_Light;
                tmp_LightInfo.LightmapBaketype = (int) tmp_Light.lightmapBakeType;
#if UNITY_2020_1_OR_NEWER
            tmp_LightInfo.MixedLightingMode = (int)UnityEditor.Lightmapping.lightingSettings.mixedBakeMode;
#elif UNITY_2018_1_OR_NEWER
                tmp_LightInfo.MixedLightingMode = (int) UnityEditor.LightmapEditorSettings.mixedBakeMode;
#else
            tmp_LightInfo.MixedLightingMode = (int)l.bakingOutput.lightmapBakeType;
#endif
                _lightsInfo.Add(tmp_LightInfo);
            }
        }
    }
}