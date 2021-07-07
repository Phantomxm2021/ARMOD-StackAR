/*===============================================================================
Copyright (C) 2020 Immersal Ltd. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sdk@immersal.com for licensing requests.
===============================================================================*/

using System;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Immersal.REST
{
    public class ImmersalHttp
    {
        public static readonly string URL_FORMAT = "{0}/{1}";

        public static async Task<U> Request<T, U>(T request, IProgress<float> progress)
        {
            U result = default(U);
            string jsonString = JsonUtility.ToJson(request);
            HttpRequestMessage r = new HttpRequestMessage(HttpMethod.Post, string.Format(URL_FORMAT, ImmersalSDK.Instance.localizationServer, (string)typeof(T).GetField("endpoint").GetValue(null)));
            r.Content = new StringContent(jsonString);

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (var response = await ImmersalSDK.client.DownloadAsync(r, stream, progress, CancellationToken.None))
                    {
                        string responseBody = Encoding.ASCII.GetString(stream.GetBuffer());
                        //Debug.Log(responseBody);
                        result = JsonUtility.FromJson<U>(responseBody);
                        if (!response.IsSuccessStatusCode)
                        {
                            Debug.Log(string.Format("ImmersalHttp error: {0} ({1}), {2}", (int)response.StatusCode, response.ReasonPhrase, response.RequestMessage));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("ImmersalHttp connection error: " + e);
            }

            return result;
        }

        public static async Task<U> RequestUpload<T, U>(T request, byte[] data, IProgress<float> progress)
        {
            U result = default(U);
            string jsonString = JsonUtility.ToJson(request);
            byte[] jsonBytes = Encoding.ASCII.GetBytes(jsonString);
            byte[] body = new byte[jsonBytes.Length + 1 + data.Length];
            Array.Copy(jsonBytes, 0, body, 0, jsonBytes.Length);
            body[jsonBytes.Length] = 0;
            Array.Copy(data, 0, body, jsonBytes.Length + 1, data.Length);
            HttpRequestMessage r = new HttpRequestMessage(HttpMethod.Post, string.Format(URL_FORMAT, ImmersalSDK.Instance.localizationServer, (string)typeof(T).GetField("endpoint").GetValue(null)));
            var byteStream = new ProgressMemoryStream(body, progress);
            r.Content = new StreamContent(byteStream);

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (var response = await ImmersalSDK.client.DownloadAsync(r, stream, null, CancellationToken.None))
                    {
                        string responseBody = Encoding.ASCII.GetString(stream.GetBuffer());
                        //Debug.Log(responseBody);
                        result = JsonUtility.FromJson<U>(responseBody);
                        if (!response.IsSuccessStatusCode)
                        {
                            Debug.Log(string.Format("ImmersalHttp error: {0} ({1}), {2}", (int)response.StatusCode, response.ReasonPhrase, response.RequestMessage));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("ImmersalHttp connection error: " + e);
            }

            return result;
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> DownloadAsync(this HttpClient client, HttpRequestMessage request, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default) {
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                request.Dispose();
                
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync())
                {
                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination);
                        return response;
                    }

                    var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                    await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                }

                return response;
            }
        }
    }

    public class ProgressMemoryStream : MemoryStream
    {
        IProgress<float> progress;
        private int length;

        public ProgressMemoryStream(byte[] buffer, IProgress<float> progress = null)
            : base(buffer, true) {
            
            this.length = buffer.Length;
            this.progress = progress;
        }

        public override int Read([In, Out] byte[] buffer, int offset, int count) {
            int n = base.Read(buffer, offset, count);
            progress?.Report((float)this.Position / this.length);
            return n;
        }
    }

    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }

    public class JobAsync
    {
        public string token = ImmersalSDK.Instance.developerToken;
        public Action OnStart;
        public Action<string> OnError;
        public Progress<float> Progress = new Progress<float>();

        public virtual async Task RunJobAsync()
        {
            await Task.Yield();
        }

        protected void HandleError(string e)
        {
            OnError?.Invoke(e ?? "conn");
        }
    }

    public class JobClearAsync : JobAsync
    {
        public bool anchor;
        public Action<SDKClearResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobClearAsync ***************************");
            this.OnStart?.Invoke();

            SDKClearRequest r = new SDKClearRequest();
            r.token = this.token;
            r.anchor = this.anchor;
            SDKClearResult result = await ImmersalHttp.Request<SDKClearRequest, SDKClearResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobConstructAsync : JobAsync
    {
        public string name;
        public int featureCount = 1024;
        public int windowSize = 0;
        public bool preservePoses = false;
        public Action<SDKConstructResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobConstructAsync ***************************");
            this.OnStart?.Invoke();

            SDKConstructRequest r = new SDKConstructRequest();
            r.token = this.token;
            r.name = this.name;
            r.featureCount = this.featureCount;
            r.preservePoses = this.preservePoses;
            SDKConstructResult result = await ImmersalHttp.Request<SDKConstructRequest, SDKConstructResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobRestoreMapImagesAsync : JobAsync
    {
        public int id;
        public Action<SDKRestoreMapImagesResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobRestoreMapImagesAsync ***************************");
            this.OnStart?.Invoke();

            SDKRestoreMapImagesRequest r = new SDKRestoreMapImagesRequest();
            r.token = this.token;
            r.id = this.id;
            SDKRestoreMapImagesResult result = await ImmersalHttp.Request<SDKRestoreMapImagesRequest, SDKRestoreMapImagesResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobDeleteMapAsync : JobAsync
    {
        public int id;
        public Action<SDKDeleteMapResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobDeleteMapAsync ***************************");
            this.OnStart?.Invoke();

            SDKDeleteMapRequest r = new SDKDeleteMapRequest();
            r.token = this.token;
            r.id = this.id;
            SDKDeleteMapResult result = await ImmersalHttp.Request<SDKDeleteMapRequest, SDKDeleteMapResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobStatusAsync : JobAsync
    {
        public Action<SDKStatusResult> OnResult;

        public override async Task RunJobAsync()
        {
//            Debug.Log("*************************** JobStatusAsync ***************************");
            this.OnStart?.Invoke();

            SDKStatusRequest r = new SDKStatusRequest();
            r.token = this.token;
            SDKStatusResult result = await ImmersalHttp.Request<SDKStatusRequest, SDKStatusResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobCaptureAsync : JobAsync
    {
        public int run;
        public int index;
        public bool anchor;
        public Vector4 intrinsics;
        public Matrix4x4 rotation;
        public Vector3 position;
        public double latitude;
        public double longitude;
        public double altitude;
        public string encodedImage;
        public string imagePath;
        public Action<SDKImageResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobCaptureAsync ***************************");
            this.OnStart?.Invoke();

            SDKImageRequest r = new SDKImageRequest();
            r.token = this.token;
            r.run = this.run;
            r.index = this.index;
            r.anchor = this.anchor;
            r.px = position.x;
            r.py = position.y;
            r.pz = position.z;
            r.r00 = rotation.m00;
            r.r01 = rotation.m01;
            r.r02 = rotation.m02;
            r.r10 = rotation.m10;
            r.r11 = rotation.m11;
            r.r12 = rotation.m12;
            r.r20 = rotation.m20;
            r.r21 = rotation.m21;
            r.r22 = rotation.m22;
            r.fx = intrinsics.x;
            r.fy = intrinsics.y;
            r.ox = intrinsics.z;
            r.oy = intrinsics.w;
            r.latitude = latitude;
            r.longitude = longitude;
            r.altitude = altitude;

            byte[] image = File.ReadAllBytes(imagePath);

            SDKImageResult result = await ImmersalHttp.RequestUpload<SDKImageRequest, SDKImageResult>(r, image, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobLocalizeServerAsync : JobAsync
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector4 intrinsics;
        public int param1 = 0;
        public int param2 = 12;
        public float param3 = 0.0f;
        public float param4 = 2.0f;
        public double latitude = 0.0;
        public double longitude = 0.0;
        public double radius = 0.0;
        public bool useGPS = false;
        public SDKMapId[] mapIds;
        public byte[] image;
        public Action<SDKLocalizeResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobLocalizeServerAsync ***************************");
            this.OnStart?.Invoke();

            SDKLocalizeResult result = default;

            if (this.useGPS)
            {
                SDKGeoLocalizeRequest r = new SDKGeoLocalizeRequest();
                r.token = this.token;
                r.fx = intrinsics.x;
                r.fy = intrinsics.y;
                r.ox = intrinsics.z;
                r.oy = intrinsics.w;
                r.param1 = param1;
                r.param2 = param2;
                r.param3 = param3;
                r.param4 = param4;
                r.latitude = this.latitude;
                r.longitude = this.longitude;
                r.radius = this.radius;
                result = await ImmersalHttp.RequestUpload<SDKGeoLocalizeRequest, SDKLocalizeResult>(r, this.image, this.Progress);
            }
            else
            {
                SDKLocalizeRequest r = new SDKLocalizeRequest();
                r.token = this.token;
                r.fx = intrinsics.x;
                r.fy = intrinsics.y;
                r.ox = intrinsics.z;
                r.oy = intrinsics.w;
                r.param1 = param1;
                r.param2 = param2;
                r.param3 = param3;
                r.param4 = param4;
                r.mapIds = this.mapIds;
                result = await ImmersalHttp.RequestUpload<SDKLocalizeRequest, SDKLocalizeResult>(r, this.image, this.Progress);

            }

            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobGeoPoseAsync : JobAsync
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector4 intrinsics;
        public int param1 = 0;
        public int param2 = 12;
        public float param3 = 0.0f;
        public float param4 = 2.0f;
        public SDKMapId[] mapIds;
        public byte[] image;
        public Action<SDKGeoPoseResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobGeoPoseAsync ***************************");
            this.OnStart?.Invoke();

            SDKGeoPoseRequest r = new SDKGeoPoseRequest();
            r.token = this.token;
            r.fx = intrinsics.x;
            r.fy = intrinsics.y;
            r.ox = intrinsics.z;
            r.oy = intrinsics.w;
            r.param1 = param1;
            r.param2 = param2;
            r.param3 = param3;
            r.param4 = param4;
            r.mapIds = this.mapIds;

            SDKGeoPoseResult result = await ImmersalHttp.RequestUpload<SDKGeoPoseRequest, SDKGeoPoseResult>(r, this.image, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobEcefAsync : JobAsync
    {
        public int id;
        public bool useToken = true;
        public Action<SDKEcefResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobEcefAsync ***************************");
            this.OnStart?.Invoke();

            SDKEcefRequest r = new SDKEcefRequest();
            r.token = useToken ? this.token : "";
            r.id = this.id;
            SDKEcefResult result = await ImmersalHttp.Request<SDKEcefRequest, SDKEcefResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobListJobsAsync : JobAsync
    {
        public double latitude = 0.0;
        public double longitude = 0.0;
        public double radius = 0.0;
        public bool useGPS = false;
        public bool useToken = true;
        public Action<SDKJobsResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobListJobsAsync ***************************");
            this.OnStart?.Invoke();

            SDKJobsResult result = default;

            if (this.useGPS)
            {
                SDKGeoJobsRequest r = new SDKGeoJobsRequest();
                r.token = this.useToken ? this.token : "";
                r.latitude = this.latitude;
                r.longitude = this.longitude;
                r.radius = this.radius;
                result = await ImmersalHttp.Request<SDKGeoJobsRequest, SDKJobsResult>(r, this.Progress);
            }
            else
            {
                SDKJobsRequest r = new SDKJobsRequest();
                r.token = this.useToken ? this.token : "";
                result = await ImmersalHttp.Request<SDKJobsRequest, SDKJobsResult>(r, this.Progress);
            }

            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobLoadMapAsync : JobAsync
    {
        public int id;
        public bool useToken = true;
        public Action<SDKMapResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobLoadMapAsync ***************************");
            this.OnStart?.Invoke();

            SDKMapRequest r = new SDKMapRequest();
            r.token = this.useToken ? this.token : "";
            r.id = this.id;
            SDKMapResult result = await ImmersalHttp.Request<SDKMapRequest, SDKMapResult>(r, this.Progress);
            if (result.error == "none")
            {
                JobMapMetadataGetAsync j = new JobMapMetadataGetAsync();
                j.id = this.id;
                j.token = r.token;
                j.OnResult += (SDKMapMetadataGetResult metadata) => 
                {
                    if (metadata.error == "none")
                    {
                        result.metadata = metadata;
                        this.OnResult?.Invoke(result);
                    }
                    else
                    {
                        HandleError(metadata.error);
                    }
                };

                await j.RunJobAsync();
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobSetPrivacyAsync : JobAsync
    {
        public int id;
        public int privacy;
        public Action<SDKMapPrivacyResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobSetPrivacyAsync ***************************");
            this.OnStart?.Invoke();

            SDKMapPrivacyRequest r = new SDKMapPrivacyRequest();
            r.token = this.token;
            r.id = this.id;
            r.privacy = this.privacy;
            SDKMapPrivacyResult result = await ImmersalHttp.Request<SDKMapPrivacyRequest, SDKMapPrivacyResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobLoginAsync : JobAsync
    {
        public string username;
        public string password;
        public Action<SDKLoginResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobLoginAsync ***************************");
            this.OnStart?.Invoke();

            SDKLoginRequest r = new SDKLoginRequest();
            r.login = this.username;
            r.password = this.password;
            SDKLoginResult result = await ImmersalHttp.Request<SDKLoginRequest, SDKLoginResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobMapDownloadAsync : JobAsync
    {
        public int id;
        public Action<SDKMapDownloadResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobMapDownloadAsyc ***************************");
            this.OnStart?.Invoke();

            SDKMapDownloadRequest r = new SDKMapDownloadRequest();
            r.token = this.token;
            r.id = this.id;
            SDKMapDownloadResult result = await ImmersalHttp.Request<SDKMapDownloadRequest, SDKMapDownloadResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobMapMetadataGetAsync : JobAsync
    {
        public int id;
        public Action<SDKMapMetadataGetResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobMapMetadataGetAsync ***************************");
            this.OnStart?.Invoke();

            SDKMapMetadataGetRequest r = new SDKMapMetadataGetRequest();
            r.token = this.token;
            r.id = this.id;
            SDKMapMetadataGetResult result = await ImmersalHttp.Request<SDKMapMetadataGetRequest, SDKMapMetadataGetResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }

    public class JobMapAlignmentSetAsync : JobAsync
    {
        public int id;
        public double tx;
        public double ty;
        public double tz;
        public double qw;
        public double qx;
        public double qy;
        public double qz;
        public double scale;
        public Action<SDKMapAlignmentSetResult> OnResult;

        public override async Task RunJobAsync()
        {
            Debug.Log("*************************** JobMapAlignmentSetAsync ***************************");
            this.OnStart?.Invoke();

            SDKMapAlignmentSetRequest r = new SDKMapAlignmentSetRequest();
            r.token = this.token;
            r.id = this.id;
            r.tx = this.tx;
            r.ty = this.ty;
            r.tz = this.tz;
            r.qw = this.qw;
            r.qx = this.qx;
            r.qy = this.qy;
            r.qz = this.qz;
            r.scale = this.scale;
            SDKMapAlignmentSetResult result = await ImmersalHttp.Request<SDKMapAlignmentSetRequest, SDKMapAlignmentSetResult>(r, this.Progress);
            if (result.error == "none")
            {
                this.OnResult?.Invoke(result);
            }
            else
            {
                HandleError(result.error);
            }
        }
    }
}
