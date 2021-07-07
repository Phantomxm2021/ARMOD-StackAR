/*===============================================================================
Copyright (C) 2020 Immersal Ltd. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sdk@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using Immersal.REST;

namespace Immersal.AR
{
	public class SpaceContainer
	{
        public int mapCount = 0;
		public Vector3 targetPosition = Vector3.zero;
		public Quaternion targetRotation = Quaternion.identity;
		public PoseFilter filter = new PoseFilter();
	}

    public class MapOffset
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public SpaceContainer space;
    }

    public class ARSpace : MonoBehaviour
    {
        public static Dictionary<Transform, SpaceContainer> transformToSpace = new Dictionary<Transform, SpaceContainer>();
        public static Dictionary<SpaceContainer, Transform> spaceToTransform = new Dictionary<SpaceContainer, Transform>();
        public static Dictionary<int, MapOffset> mapIdToOffset = new Dictionary<int, MapOffset>();
        public static Dictionary<int, ARMap> mapIdToMap = new Dictionary<int, ARMap>();

        private Matrix4x4 m_InitialOffset = Matrix4x4.identity;

        public Matrix4x4 initialOffset
        {
            get { return m_InitialOffset; }
        }

		void Awake()
		{
			Vector3 pos = transform.position;
			Quaternion rot = transform.rotation;
			Matrix4x4 offset = Matrix4x4.TRS(pos, rot, Vector3.one);

			m_InitialOffset = offset;
		}

		public Pose ToCloudSpace(Vector3 camPos, Quaternion camRot)
		{
			Matrix4x4 trackerSpace = Matrix4x4.TRS(camPos, camRot, Vector3.one);
			Matrix4x4 trackerToCloudSpace = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			Matrix4x4 cloudSpace = trackerToCloudSpace.inverse * trackerSpace;

			return new Pose(cloudSpace.GetColumn(3), cloudSpace.rotation);
		}

		public Pose FromCloudSpace(Vector3 camPos, Quaternion camRot)
		{
			Matrix4x4 cloudSpace = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			Matrix4x4 trackerSpace = Matrix4x4.TRS(camPos, camRot, Vector3.one);
			Matrix4x4 m = trackerSpace * (cloudSpace.inverse);

			return new Pose(m.GetColumn(3), m.rotation);
		}

		public static ARMap LoadAndInstantiateARMap(Transform root, SDKMapResult map, byte[] mapData, ARMap.RenderMode renderMode, Color pointCloudColor = default, bool applyAlignment = false)
		{
			GameObject go = new GameObject(string.Format("AR Map {0}-{1}", map.metadata.id, map.metadata.name));
            if (root != null)
            {
                go.transform.SetParent(root);
            }

            if (applyAlignment)
            {
                Matrix4x4 a;
                Matrix4x4 b = Matrix4x4.TRS(new Vector3((float)map.metadata.tx, (float)map.metadata.ty, (float)map.metadata.tz), 
                    new Quaternion((float)map.metadata.qx, (float)map.metadata.qy, (float)map.metadata.qz, (float)map.metadata.qw), 
                    new Vector3((float)map.metadata.scale, (float)map.metadata.scale, (float)map.metadata.scale)
                );
                ARHelper.SwitchHandedness(out a, b);
                go.transform.localPosition = a.GetColumn(3);
                go.transform.localRotation = a.rotation;
                go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }

			ARMap arMap = go.AddComponent<ARMap>();
            arMap.privacy = map.metadata.privacy.ToString();
            arMap.pointColor = pointCloudColor;
            arMap.renderMode = renderMode;
            arMap.LoadMap(mapData, map.metadata.id);
            return arMap;
		}

        public static void RegisterSpace(Transform tr, ARMap map, Vector3 offsetPosition, Quaternion offsetRotation, Vector3 offsetScale)
		{
            SpaceContainer sc;

            if (!transformToSpace.ContainsKey(tr))
            {
                sc = new SpaceContainer();
                transformToSpace[tr] = sc;
            }
            else
            {
                sc = transformToSpace[tr];
            }

            spaceToTransform[sc] = tr;

            sc.mapCount++;

            MapOffset mo = new MapOffset();
            mo.position = offsetPosition;
            mo.rotation = offsetRotation;
            mo.scale = offsetScale;
            mo.space = sc;

            mapIdToOffset[map.mapId] = mo;
            mapIdToMap[map.mapId] = map;
		}

        public static void RegisterSpace(Transform tr, ARMap map)
        {
            RegisterSpace(tr, map, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        public static void UnregisterSpace(Transform tr, int mapId)
		{
			if (transformToSpace.ContainsKey(tr))
			{
				SpaceContainer sc = transformToSpace[tr];
				if (--sc.mapCount == 0)
                {
					transformToSpace.Remove(tr);
                    spaceToTransform.Remove(sc);
                }
				if (mapIdToOffset.ContainsKey(mapId))
					mapIdToOffset.Remove(mapId);
                if (mapIdToMap.ContainsKey(mapId))
                    mapIdToMap.Remove(mapId);
			}
		}

		public static void UpdateSpace(SpaceContainer space, Vector3 pos, Quaternion rot)
        {
            if (spaceToTransform.ContainsKey(space))
            {
                Transform tr = spaceToTransform[space];
        		tr.SetPositionAndRotation(pos, rot);
            }
		}
    }
}