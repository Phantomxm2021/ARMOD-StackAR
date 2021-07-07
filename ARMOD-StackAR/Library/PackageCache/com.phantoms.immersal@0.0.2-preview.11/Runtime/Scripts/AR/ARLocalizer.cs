/*===============================================================================
Copyright (C) 2020 Immersal Ltd. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sdk@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Threading.Tasks;
using Immersal.REST;

namespace Immersal.AR
{
    public class ARLocalizer : LocalizerBase
	{
		private static ARLocalizer instance = null;

		private void ARSessionStateChanged(ARSessionStateChangedEventArgs args)
		{
			CheckTrackingState(args.state);
		}

		private void CheckTrackingState(ARSessionState newState)
		{
			isTracking = newState == ARSessionState.SessionTracking;

			if (!isTracking)
			{
				foreach (KeyValuePair<Transform, SpaceContainer> item in ARSpace.transformToSpace)
					item.Value.filter.InvalidateHistory();
			}
		}

		public static ARLocalizer Instance
		{
			get
			{
#if UNITY_EDITOR
				if (instance == null && !Application.isPlaying)
				{
					instance = UnityEngine.Object.FindObjectOfType<ARLocalizer>();
				}
#endif
				if (instance == null)
				{
					Debug.LogError("No ARLocalizer instance found. Ensure one exists in the scene.");
				}
				return instance;
			}
		}

		void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			if (instance != this)
			{
				Debug.LogError("There must be only one ARLocalizer object in a scene.");
				UnityEngine.Object.DestroyImmediate(this);
				return;
			}
		}

        public override void Start()
        {
            base.Start();
			m_Sdk.Localizer = instance;
        }

		public override void OnEnable()
		{
			base.OnEnable();
#if !UNITY_EDITOR
			CheckTrackingState(ARSession.state);
			ARSession.stateChanged += ARSessionStateChanged;
#endif
		}

		public override void OnDisable()
		{
#if !UNITY_EDITOR
			ARSession.stateChanged -= ARSessionStateChanged;
#endif
			base.OnDisable();
		}

        public override async void LocalizeServer(SDKMapId[] mapIds)
        {
#if PLATFORM_LUMIN && UNITY_2020_1
            XRCameraImage image;
#else
            XRCpuImage image;
#endif
            ARCameraManager cameraManager = m_Sdk.cameraManager;
            var cameraSubsystem = cameraManager.subsystem;

#if PLATFORM_LUMIN && UNITY_2020_1
            if (cameraSubsystem.TryGetLatestImage(out image))
#else
            if (cameraSubsystem.TryAcquireLatestCpuImage(out image))
#endif
            {
				stats.localizationAttemptCount++;

                JobLocalizeServerAsync j = new JobLocalizeServerAsync();

                byte[] pixels;
                Vector3 camPos = m_Cam.transform.position;
                Quaternion camRot = m_Cam.transform.rotation;
				Vector4 intrinsics;
                int channels = 1;
                int width = image.width;
                int height = image.height;

                ARHelper.GetIntrinsics(out intrinsics);
				ARHelper.GetPlaneData(out pixels, image);

				float startTime = Time.realtimeSinceStartup;

                Task<(byte[], icvCaptureInfo)> t = Task.Run(() =>
                {
                    byte[] capture = new byte[channels * width * height + 1024];
                    icvCaptureInfo info = Immersal.Core.CaptureImage(capture, capture.Length, pixels, width, height, channels);
                    Array.Resize(ref capture, info.captureSize);
                    return (capture, info);
                });

                await t;

                j.image = t.Result.Item1;
				j.position = camPos;
				j.rotation = camRot;
				j.intrinsics = intrinsics;
				j.mapIds = mapIds;

                j.OnResult += async (SDKLocalizeResult result) =>
                {
					float elapsedTime = Time.realtimeSinceStartup - startTime;

					if (result.success)
					{
						Debug.Log("*************************** On-Server Localization Succeeded ***************************");
						Debug.Log(string.Format("Relocalized in {0} seconds", elapsedTime));

						int mapId = result.map;

						if (mapId > 0 && ARSpace.mapIdToOffset.ContainsKey(mapId))
						{
							if (mapId != lastLocalizedMapId)
							{
								if (resetOnMapChange)
								{
									Reset();
								}
								
								lastLocalizedMapId = mapId;

								OnMapChanged?.Invoke(mapId);
							}

							MapOffset mo = ARSpace.mapIdToOffset[mapId];
							stats.localizationSuccessCount++;
							
							Matrix4x4 responseMatrix = Matrix4x4.identity;
							responseMatrix.m00 = result.r00; responseMatrix.m01 = result.r01; responseMatrix.m02 = result.r02; responseMatrix.m03 = result.px;
							responseMatrix.m10 = result.r10; responseMatrix.m11 = result.r11; responseMatrix.m12 = result.r12; responseMatrix.m13 = result.py;
							responseMatrix.m20 = result.r20; responseMatrix.m21 = result.r21; responseMatrix.m22 = result.r22; responseMatrix.m23 = result.pz;
							
							Vector3 pos = responseMatrix.GetColumn(3);
							Quaternion rot = responseMatrix.rotation;
							ARHelper.GetRotation(ref rot);

							Matrix4x4 offsetNoScale = Matrix4x4.TRS(mo.position, mo.rotation, Vector3.one);
							Vector3 scaledPos = Vector3.Scale(pos, mo.scale);
							Matrix4x4 cloudSpace = offsetNoScale * Matrix4x4.TRS(scaledPos, rot, Vector3.one);
							Matrix4x4 trackerSpace = Matrix4x4.TRS(camPos, camRot, Vector3.one);
							Matrix4x4 m = trackerSpace * (cloudSpace.inverse);

							if (useFiltering)
								mo.space.filter.RefinePose(m);
							else
								ARSpace.UpdateSpace(mo.space, m.GetColumn(3), m.rotation);

							JobEcefAsync je = new JobEcefAsync();
							je.id = mapId;
							je.OnResult += (SDKEcefResult result2) =>
							{
								LocalizerPose localizerPose;
								LocalizerBase.GetLocalizerPose(out localizerPose, mapId, pos, rot, m.inverse, result2.ecef);
								this.lastLocalizedPose = localizerPose;
								OnPoseFound?.Invoke(localizerPose);
							};

							await je.RunJobAsync();

							if (ARSpace.mapIdToMap.ContainsKey(mapId))
							{
								ARMap map = ARSpace.mapIdToMap[mapId];
								map.NotifySuccessfulLocalization(mapId);
							}
						}
						else
						{
							Debug.Log(string.Format("Localization attempt failed after {0} seconds", elapsedTime));
						}
					}
					else
					{
						Debug.Log("*************************** On-Server Localization Failed ***************************");
					}
                };

				await j.RunJobAsync();

                image.Dispose();
            }

			base.LocalizeServer(mapIds);
        }

        public override async void LocalizeGeoPose(SDKMapId[] mapIds)
        {
            ARCameraManager cameraManager = m_Sdk.cameraManager;
            var cameraSubsystem = cameraManager.subsystem;

#if PLATFORM_LUMIN && UNITY_2020_1
            XRCameraImage image;
            if (cameraSubsystem.TryGetLatestImage(out image))
#else
            XRCpuImage image;
            if (cameraSubsystem.TryAcquireLatestCpuImage(out image))
#endif
            {
				stats.localizationAttemptCount++;

                JobGeoPoseAsync j = new JobGeoPoseAsync();

                byte[] pixels;
                Vector3 camPos = m_Cam.transform.position;
                Quaternion camRot = m_Cam.transform.rotation;
                int channels = 1;
                int width = image.width;
                int height = image.height;

                j.mapIds = mapIds;

                ARHelper.GetIntrinsics(out j.intrinsics);
                ARHelper.GetPlaneData(out pixels, image);

				float startTime = Time.realtimeSinceStartup;

                Task<(byte[], icvCaptureInfo)> t = Task.Run(() =>
                {
                    byte[] capture = new byte[channels * width * height + 1024];
                    icvCaptureInfo info = Immersal.Core.CaptureImage(capture, capture.Length, pixels, width, height, channels);
                    Array.Resize(ref capture, info.captureSize);
                    return (capture, info);
                });

                await t;

                j.image = t.Result.Item1;

                j.OnResult += async (SDKGeoPoseResult result) =>
                {
                    if (result.success)
                    {
                        Debug.Log("*************************** GeoPose Localization Succeeded ***************************");

                        int mapId = result.map;
						                        
                        double latitude = result.latitude;
                        double longitude = result.longitude;
                        double ellipsoidHeight = result.ellipsoidHeight;
                        Quaternion rot = new Quaternion(result.quaternion[1], result.quaternion[2], result.quaternion[3], result.quaternion[0]);
                        Debug.Log(string.Format("GeoPose returned latitude: {0}, longitude: {1}, ellipsoidHeight: {2}, quaternion: {3}", latitude, longitude, ellipsoidHeight, rot));

                        double[] ecef = new double[3];
                        double[] wgs84 = new double[3] { latitude, longitude, ellipsoidHeight };
                        Core.PosWgs84ToEcef(ecef, wgs84);

                        JobEcefAsync je = new JobEcefAsync();
                        je.id = mapId;

						ARMap map = null;
						if (ARSpace.mapIdToMap.ContainsKey(mapId))
						{
							map = ARSpace.mapIdToMap[mapId];
                        	je.useToken = map.privacy == "0" ? true : false;
						}

                        je.OnResult += (SDKEcefResult result2) =>
                        {
							float elapsedTime = Time.realtimeSinceStartup - startTime;

							if (mapId > 0 && ARSpace.mapIdToOffset.ContainsKey(mapId))
							{
								if (mapId != lastLocalizedMapId)
								{
									if (resetOnMapChange)
									{
										Reset();
									}
									
									lastLocalizedMapId = mapId;

									OnMapChanged?.Invoke(mapId);
								}

								MapOffset mo = ARSpace.mapIdToOffset[mapId];
								stats.localizationSuccessCount++;

								double[] mapToEcef = result2.ecef;
								Vector3 mapPos;
								Quaternion mapRot;
								Core.PosEcefToMap(out mapPos, ecef, mapToEcef);
								Core.RotEcefToMap(out mapRot, rot, mapToEcef);

								Matrix4x4 offsetNoScale = Matrix4x4.TRS(mo.position, mo.rotation, Vector3.one);
								Vector3 scaledPos = Vector3.Scale(mapPos, mo.scale);
								Matrix4x4 cloudSpace = offsetNoScale * Matrix4x4.TRS(scaledPos, mapRot, Vector3.one);
								Matrix4x4 trackerSpace = Matrix4x4.TRS(camPos, camRot, Vector3.one);
								Matrix4x4 m = trackerSpace*(cloudSpace.inverse);
								
								if (useFiltering)
									mo.space.filter.RefinePose(m);
								else
									ARSpace.UpdateSpace(mo.space, m.GetColumn(3), m.rotation);

								LocalizerPose localizerPose;
								LocalizerBase.GetLocalizerPose(out localizerPose, mapId, cloudSpace.GetColumn(3), cloudSpace.rotation, m.inverse, mapToEcef);
								this.lastLocalizedPose = localizerPose;
								OnPoseFound?.Invoke(localizerPose);

								if (map != null)
								{
									map.NotifySuccessfulLocalization(mapId);
								}
							}
							else
							{
								Debug.Log(string.Format("GeoPose localization attempt failed after {0} seconds", elapsedTime));
							}
                        };

                        await je.RunJobAsync();
                    }
                    else
                    {
                        Debug.Log("*************************** GeoPose Localization Failed ***************************");
                    }
                };

                await j.RunJobAsync();
                image.Dispose();
            }

			base.LocalizeGeoPose(mapIds);
        }
 
        public override async void Localize()
		{
#if PLATFORM_LUMIN && UNITY_2020_1
			XRCameraImage image;
#else
			XRCpuImage image;
#endif
			ARCameraManager cameraManager = m_Sdk.cameraManager;
			var cameraSubsystem = cameraManager.subsystem;

#if PLATFORM_LUMIN && UNITY_2020_1
			if (cameraSubsystem != null && cameraSubsystem.TryGetLatestImage(out image))
#else
			if (cameraSubsystem != null && cameraSubsystem.TryAcquireLatestCpuImage(out image))
#endif
			{
				stats.localizationAttemptCount++;
				Vector4 intrinsics;
				Vector3 camPos = m_Cam.transform.position;
				Quaternion camRot = m_Cam.transform.rotation;
				ARHelper.GetIntrinsics(out intrinsics);
				ARHelper.GetPlaneDataFast(ref m_PixelBuffer, image);

				if (m_PixelBuffer != IntPtr.Zero)
				{
					Vector3 pos = Vector3.zero;
					Quaternion rot = Quaternion.identity;

					float startTime = Time.realtimeSinceStartup;

					Task<int> t = Task.Run(() =>
					{
						return Immersal.Core.LocalizeImage(out pos, out rot, image.width, image.height, ref intrinsics, m_PixelBuffer);
					});

					await t;

					int mapHandle = t.Result;
					int mapId = ARMap.MapHandleToId(mapHandle);
					float elapsedTime = Time.realtimeSinceStartup - startTime;

					if (mapId > 0 && ARSpace.mapIdToOffset.ContainsKey(mapId))
					{
						Debug.Log(string.Format("Relocalized in {0} seconds", elapsedTime));
						stats.localizationSuccessCount++;

						if (mapId != lastLocalizedMapId)
						{
							if (resetOnMapChange)
							{
								Reset();
							}
							
							lastLocalizedMapId = mapId;

							OnMapChanged?.Invoke(mapId);
						}

						ARHelper.GetRotation(ref rot);
						MapOffset mo = ARSpace.mapIdToOffset[mapId];
						Matrix4x4 offsetNoScale = Matrix4x4.TRS(mo.position, mo.rotation, Vector3.one);
						Vector3 scaledPos = Vector3.Scale(pos, mo.scale);
						Matrix4x4 cloudSpace = offsetNoScale * Matrix4x4.TRS(scaledPos, rot, Vector3.one);
						Matrix4x4 trackerSpace = Matrix4x4.TRS(camPos, camRot, Vector3.one);
						Matrix4x4 m = trackerSpace * (cloudSpace.inverse);

						if (useFiltering)
							mo.space.filter.RefinePose(m);
						else
							ARSpace.UpdateSpace(mo.space, m.GetColumn(3), m.rotation);

						LocalizerPose localizerPose;
						GetLocalizerPose(out localizerPose, mapId, pos, rot, m.inverse);
						this.lastLocalizedPose = localizerPose;
						OnPoseFound?.Invoke(localizerPose);

						ARMap map = ARSpace.mapIdToMap[mapId];
						map.NotifySuccessfulLocalization(mapId);
					}
					else
					{
						Debug.Log(string.Format("Localization attempt failed after {0} seconds", elapsedTime));
					}
				}

				image.Dispose();
			}

			base.Localize();
		}
	}
}