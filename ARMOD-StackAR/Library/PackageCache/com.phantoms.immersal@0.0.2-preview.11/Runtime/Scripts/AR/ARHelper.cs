/*===============================================================================
Copyright (C) 2020 Immersal Ltd. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sdk@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Immersal.AR
{
	public class ARHelper {
		public static void SwitchHandedness(out Matrix4x4 a, Matrix4x4 b)
		{
			a = Matrix4x4.identity;
			a.m00 = b.m11;
			a.m01 = b.m10;
			a.m02 = -b.m12;
			a.m03 = -b.m13;
			a.m10 = b.m01;
			a.m11 = b.m00;
			a.m12 = -b.m02;
			a.m13 = -b.m03;
			a.m20 = -b.m21;
			a.m21 = -b.m20;
			a.m22 = b.m22;
			a.m23 = b.m23;
			a.m30 = a.m31 = a.m32 = 0;
			a.m33 = 1;
		}
		
		public static void SwitchHandedness(out Quaternion a, Quaternion b)
		{
			Matrix4x4 m;
			SwitchHandedness(out m, Matrix4x4.Rotate(b));
			a = m.rotation;
		}

		public static void SwitchHandedness(out Vector3 a, Vector3 b)
		{
			Matrix4x4 m;
			SwitchHandedness(out m, Matrix4x4.TRS(b, Quaternion.identity, Vector3.one));
			a = m.GetColumn(3);
		}

		public static void GetIntrinsics(out Vector4 intrinsics)
        {
            intrinsics = Vector4.zero;
			XRCameraIntrinsics intr;
			ARCameraManager manager = ImmersalSDK.Instance?.cameraManager;

			if (manager != null && manager.TryGetIntrinsics(out intr))
			{
				intrinsics.x = intr.focalLength.x;
				intrinsics.y = intr.focalLength.y;
				intrinsics.z = intr.principalPoint.x;
				intrinsics.w = intr.principalPoint.y;
            }
        }

		public static void GetRotation(ref Quaternion rot)
		{
			float angle = 0f;
			switch (Screen.orientation)
			{
				case ScreenOrientation.Portrait:
					angle = 0f;
					break;
				case ScreenOrientation.LandscapeLeft:
					angle = -90f;
					break;
				case ScreenOrientation.LandscapeRight:
					angle = 90f;
					break;
				case ScreenOrientation.PortraitUpsideDown:
					angle = 180f;
					break;
				default:
					angle = 0f;
					break;
			}

			rot *= Quaternion.Euler(0f, 0f, angle);
		}

#if PLATFORM_LUMIN && UNITY_2020_1
		public static void GetPlaneDataFast(ref IntPtr pixels, XRCameraImage image)
		{
			XRCameraImagePlane plane = image.GetPlane(0);	// use the Y plane
#else
		public static void GetPlaneDataFast(ref IntPtr pixels, XRCpuImage image)
		{
			XRCpuImage.Plane plane = image.GetPlane(0);	// use the Y plane
#endif
			int width = image.width, height = image.height;

			if (width == plane.rowStride)
			{
				unsafe
				{
					pixels = (IntPtr)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(plane.data);
				}
			}
			else
			{
				unsafe
				{
					ulong handle;
					byte[] data = new byte[width * height];
					byte* srcPtr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(plane.data);
					byte* dstPtr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(data, out handle);
					if (width > 0 && height > 0) {
						UnsafeUtility.MemCpyStride(dstPtr, width, srcPtr, plane.rowStride, width, height);
					}
					pixels = (IntPtr)dstPtr;
					UnsafeUtility.ReleaseGCObject(handle);
				}
			}
		}
		
#if PLATFORM_LUMIN && UNITY_2020_1
		public static void GetPlaneData(out byte[] pixels, XRCameraImage image)
		{
			XRCameraImagePlane plane = image.GetPlane(0);	// use the Y plane
#else
		public static void GetPlaneData(out byte[] pixels, XRCpuImage image)
		{
			XRCpuImage.Plane plane = image.GetPlane(0);	// use the Y plane
#endif
			int width = image.width, height = image.height;
			pixels = new byte[width * height];

			if (width == plane.rowStride)
			{
				plane.data.CopyTo(pixels);
			}
			else
			{
				unsafe
				{
					ulong handle;
					byte* srcPtr = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(plane.data);
					byte* dstPtr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(pixels, out handle);
					if (width > 0 && height > 0) {
						UnsafeUtility.MemCpyStride(dstPtr, width, srcPtr, plane.rowStride, width, height);
					}
					UnsafeUtility.ReleaseGCObject(handle);
				}
			}
		}

#if PLATFORM_LUMIN && UNITY_2020_1
		public static void GetPlaneDataRGB(out byte[] pixels, XRCameraImage image)
		{
			var conversionParams = new XRCameraImageConversionParams
#else
		public static void GetPlaneDataRGB(out byte[] pixels, XRCpuImage image)
		{
			var conversionParams = new XRCpuImage.ConversionParams
#endif
			{
				inputRect = new RectInt(0, 0, image.width, image.height),
				outputDimensions = new Vector2Int(image.width, image.height),
				outputFormat = TextureFormat.RGB24,
#if PLATFORM_LUMIN && UNITY_2020_1
				transformation = CameraImageTransformation.None
#else
				transformation = XRCpuImage.Transformation.None
#endif
			};

			int size = image.GetConvertedDataSize(conversionParams);
			pixels = new byte[size];
			GCHandle bufferHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			image.Convert(conversionParams, bufferHandle.AddrOfPinnedObject(), pixels.Length);
			bufferHandle.Free();
		}

		public static bool TryGetTrackingQuality(out int quality)
		{
			quality = default;

			if (ImmersalSDK.Instance?.arSession == null)
				return false;
			
			var arSubsystem = ImmersalSDK.Instance?.arSession.subsystem;
			
			if (arSubsystem != null && arSubsystem.running)
			{
				switch (arSubsystem.trackingState)
				{
					case TrackingState.Tracking:
						quality = 4;
						break;
					case TrackingState.Limited:
						quality = 1;
						break;
					case TrackingState.None:
						quality = 0;
						break;
				}
			}

			return true;
		}
	}
}
