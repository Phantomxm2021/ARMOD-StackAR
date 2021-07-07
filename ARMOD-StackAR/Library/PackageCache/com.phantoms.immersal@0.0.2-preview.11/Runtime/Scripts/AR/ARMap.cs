/*===============================================================================
Copyright (C) 2020 Immersal Ltd. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sdk@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Immersal.AR
{
    [System.Serializable]
    public class MapLocalizedEvent : UnityEvent<int>
    {
    }

    [ExecuteAlways]
    public class ARMap : MonoBehaviour
    {
        public const int MAX_VERTICES = 65535;

		public static readonly Color[] pointCloudColors = new Color[]	{	new Color(0.22f,    1f,     0.46f), 
																            new Color(0.96f,    0.14f,  0.14f),
																            new Color(0.16f,    0.69f,  0.95f),
																            new Color(0.93f,    0.84f,  0.12f),
																            new Color(0.57f,    0.93f,  0.12f),
																            new Color(1f,       0.38f,  0.78f),
																            new Color(0.4f,     0f,     0.9f),
																            new Color(0.89f,    0.4f,   0f)
															            };

        public enum RenderMode { DoNotRender, EditorOnly, EditorAndRuntime }

        public static Dictionary<int, ARMap> mapHandleToMap = new Dictionary<int, ARMap>();
		public static bool pointCloudVisible = true;

        public RenderMode renderMode = RenderMode.EditorOnly;
        public TextAsset mapFile;
        
        [SerializeField]
        private Color m_PointColor = new Color(0.57f, 0.93f, 0.12f);

        [Space(10)]
        [Header("Map Metadata")]

        [SerializeField][ReadOnly]
        private int m_MapId = -1;
        [SerializeField][ReadOnly]
        private string m_MapName = null;
        [ReadOnly]
        public string privacy;
        [ReadOnly]
        public MapAlignment mapAlignment;
        [ReadOnly]
        public WGS84 wgs84;

        [Space(10)]
        [Header("Events")]

        public MapLocalizedEvent OnFirstLocalization = null;
        protected ARSpace m_ARSpace = null;
        private bool m_LocalizedOnce = false;

        public Color pointColor
        {
            get { return m_PointColor; }
            set { m_PointColor = value; }
        }

        public static float pointSize = 0.33f;
        // public static bool isRenderable = true;
        public static bool renderAs3dPoints = true;

        [System.Serializable]
        public struct MapAlignment
        {
            public double tx;
            public double ty;
            public double tz;
            public double qx;
            public double qy;
            public double qz;
            public double qw;
            public double scale;
        }

        [System.Serializable]
        public struct WGS84
        {
            public double latitude;
            public double longitude;
            public double altitude;
        }

        private Shader m_Shader;
        private Material m_Material;
        private Mesh m_Mesh;
        private MeshFilter m_MeshFilter;
        private MeshRenderer m_MeshRenderer;
        private Vector3[] m_pointPositions = new Vector3[MAX_VERTICES];


        public Transform root { get; protected set; }
        public int mapHandle { get; private set; } = -1;

        public int mapId
        {
            get => m_MapId;
            private set => m_MapId = value;
        }

        public string mapName
        {
            get => m_MapName;
            private set => m_MapName = value;
        }

        public static int MapHandleToId(int handle)
        {
            if (mapHandleToMap.ContainsKey(handle))
            {
                return mapHandleToMap[handle].mapId;
            }
            return -1;
        }

        public static int MapIdToHandle(int id)
        {
            if (ARSpace.mapIdToMap.ContainsKey(id))
            {
                return ARSpace.mapIdToMap[id].mapHandle;
            }
            return -1;
        }

        public virtual void FreeMap(bool destroy = false)
        {
            if (mapHandle >= 0)
            {
                Immersal.Core.FreeMap(mapHandle);
                mapHandle = -1;
                ClearMesh();
                Reset();
            }

            if (this.mapId > 0)
            {
                ARSpace.UnregisterSpace(root, this.mapId);
                this.mapId = -1;
            }

            if (destroy)
            {
                GameObject.Destroy(gameObject);
            }
        }

        public virtual void Reset()
        {
            m_LocalizedOnce = false;
        }

        public virtual int LoadMap(byte[] mapBytes = null, int mapId = -1)
        {
            if (mapBytes == null)
            {
                mapBytes = (mapFile != null) ? mapFile.bytes : null;
            }

            if (mapBytes != null)// && mapHandle < 0) //?
            {
                mapHandle = Immersal.Core.LoadMap(mapBytes);
            }

            if (mapId > 0)
            {
                this.mapId = mapId;
            }
            else
            {
                ParseMapIdAndName();
            }

            if (mapHandle >= 0)
            {
                Vector3[] points = new Vector3[MAX_VERTICES];
                int num = Immersal.Core.GetPointCloud(mapHandle, points);
                mapHandleToMap[mapHandle] = this;

                InitializeMesh(points);
            }

            if (this.mapId > 0 && m_ARSpace != null)
            {
                root = m_ARSpace.transform;
                ARSpace.RegisterSpace(root, this, transform.localPosition, transform.localRotation, transform.localScale);
            }

            return mapHandle;
        }

        private void InitializeMesh(Vector3[] pointPositions)
        {
            if (m_Shader == null)
            {
                m_Shader = Shader.Find("Immersal/Point Cloud");
            }

            if (m_Material == null)
            {
                m_Material = new Material(m_Shader);
                m_Material.hideFlags = HideFlags.DontSave;
            }

            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
            }

            int numPoints = pointPositions.Length;

            int[] indices = new int[numPoints];
            Vector3[] pts = new Vector3[numPoints];
            Color32[] col = new Color32[numPoints];

            for (int i = 0; i < numPoints; ++i)
            {
                indices[i] = i;
                pts[i] = pointPositions[i];
            }

            m_Mesh.Clear();
            m_Mesh.vertices = pts;
            m_Mesh.colors32 = col;
            m_Mesh.SetIndices(indices, MeshTopology.Points, 0);
            m_Mesh.bounds = new Bounds(transform.position, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

            if (m_MeshFilter == null)
            {
                m_MeshFilter = gameObject.GetComponent<MeshFilter>();
                if (m_MeshFilter == null)
                {
                    m_MeshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }

            if (m_MeshRenderer == null)
            {
                m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (m_MeshRenderer == null)
                {
                    m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }

            m_MeshFilter.mesh = m_Mesh;
            m_MeshRenderer.material = m_Material;

            m_MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_MeshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            m_MeshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        private void InitializeMesh()
        {
            InitializeMesh(new Vector3[0]);
        }

        private void ClearMesh()
        {
            if(m_Mesh != null)
            {
                m_Mesh.Clear();
            }
        }

        public void NotifySuccessfulLocalization(int mapId)
        {
            if (m_LocalizedOnce)
                return;
            
            OnFirstLocalization?.Invoke(mapId);
            m_LocalizedOnce = true;
        }

        private void Awake()
        {
            m_ARSpace = gameObject.GetComponentInParent<ARSpace>();
            if (!m_ARSpace)
            {
                GameObject go = new GameObject("AR Space");
                m_ARSpace = go.AddComponent<ARSpace>();
                transform.SetParent(go.transform);
            }

            ParseMapIdAndName();
        }

        private void ParseMapIdAndName()
        {
            int id;
            if (GetMapId(out id))
            {
                this.mapId = id;
                this.mapName = mapFile.name.Substring(id.ToString().Length + 1);
            }
        }

        private bool GetMapId(out int mapId)
        {
            if (mapFile == null)
            {
                mapId = -1;
                return false;
            }

            string mapFileName = mapFile.name;
            Regex rx = new Regex(@"^\d+");
            Match match = rx.Match(mapFileName);
            if (match.Success)
            {
                mapId = Int32.Parse(match.Value);
                return true;
            }
            else
            {
                mapId = -1;
                return false;
            }
        }

        private void Start()
        {
            LoadMap();
        }

        private void OnEnable()
        {
            InitializeMesh();
            LoadMap();
        }

        private void OnDisable()
        {
            FreeMap();
        }

        void OnDestroy()
        {
            if (m_Material != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(m_Mesh);
                    Destroy(m_Material);
                }
                else
                {
                    DestroyImmediate(m_Mesh);
                    DestroyImmediate(m_Material);
                }
            }
        }

        private bool IsRenderable()
        {
            if (pointCloudVisible)
            {
                switch (renderMode)
                {
                    case RenderMode.DoNotRender:
                        return false;
                    case RenderMode.EditorOnly:
                        if (Application.isEditor)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case RenderMode.EditorAndRuntime:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }


        private void OnRenderObject()
        {
            if (IsRenderable() && m_Material != null)
            {
                m_MeshRenderer.enabled = true;

                if (renderAs3dPoints)
                {
                    m_Material.SetFloat("_PerspectiveEnabled", 1f);
                    m_Material.SetFloat("_PointSize", Mathf.Lerp(0.002f, 0.14f, Mathf.Max(0, Mathf.Pow(pointSize, 3f))));
                }
                else
                {
                    m_Material.SetFloat("_PerspectiveEnabled", 0f);
                    m_Material.SetFloat("_PointSize", Mathf.Lerp(1.5f, 40f, Mathf.Max(0, pointSize)));
                }
                m_Material.SetColor("_PointColor", m_PointColor);
            }
            else
            {
                m_MeshRenderer.enabled = false;
            }
        }
    }
}