/*===============================================================================
Copyright (C) 2020 Immersal Ltd. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sdk@immersal.com for licensing requests.
===============================================================================*/

using System;
using UnityEngine;

namespace Immersal.REST
{
    public struct SDKJobState
    {
        public const string Done = "done";
        public const string Sparse = "sparse";
        public const string Processing = "processing";
        public const string Failed = "failed";
        public const string Pending = "pending";
    }

    public enum SDKJobType { Map, Stitch, Alignment };

    [Serializable]
    public struct SDKJob
    {
        public int id;
        public int type;
        public string version;
        public string creator;
        public int size;
        public int bank;
        public string work;
        public string status;
        public string privacy;
        public string server;
        public string name;
        public double latitude;
        public double longitude;
        public double altitude;
        public string created;
        public string modified;
        public string sha256_al;
        public string sha256_sparse;
        public string sha256_dense;
        public string sha256_tex;
    }

    [Serializable]
    public struct SDKMapId
    {
        public int id;
    }

    [Serializable]
    public struct SDKLoginRequest
    {
        public static string endpoint = "login";
        public string login;
        public string password;
    }

    [Serializable]
    public struct SDKLoginResult
    {
        public string error;
        public string token;
        public int banks;
    }

    [Serializable]
    public struct SDKClearRequest
    {
        public static string endpoint = "clear";
        public string token;
        public int bank;
        public bool anchor;
    }

    [Serializable]
    public struct SDKClearResult
    {
        public string error;
    }

    [Serializable]
    public struct SDKConstructRequest
    {
        public static string endpoint = "construct";
        public string token;
        public int bank;
        public string name;
        public int featureCount;
        public bool preservePoses;
        public int windowSize;
    }

    [Serializable]
    public struct SDKConstructResult
    {
        public string error;
        public int id;
        public int size;
    }

    [Serializable]
    public struct SDKStatusRequest
    {
        public static string endpoint = "status";
        public string token;
        public int bank;
    }

    [Serializable]
    public struct SDKStatusResult
    {
        public string error;
        public int imageCount;
        public int bankMax;
        public int imageMax;
        public bool eulaAccepted;
    }

    [Serializable]
    public struct SDKJobsRequest
    {
        public static string endpoint = "list";
        public string token;
        public int bank;
    }

    [Serializable]
    public struct SDKGeoJobsRequest
    {
        public static string endpoint = "geolist";
        public string token;
        public int bank;
        public double latitude;
        public double longitude;
        public double radius;
    }

    [Serializable]
    public struct SDKJobsResult
    {
        public string error;
        public int count;
        public SDKJob[] jobs;
    }

    [Serializable]
    public struct SDKImageRequest
    {
        public static string endpoint = "capture";
        public string token;
        public int bank;
        public int run;
        public int index;
        public bool anchor;
        public double px;
        public double py;
        public double pz;
        public double r00;
        public double r01;
        public double r02;
        public double r10;
        public double r11;
        public double r12;
        public double r20;
        public double r21;
        public double r22;
        public double fx;
        public double fy;
        public double ox;
        public double oy;
        public double latitude;
        public double longitude;
        public double altitude;
    }

    [Serializable]
    public struct SDKImageResult
    {
        public string error;
        public string path;
    }
    
    [Serializable]
    public struct SDKGeoLocalizeRequest
    {
        public static string endpoint = "geolocalize";
        public string token;
        public int bank;
        public double fx;
        public double fy;
        public double ox;
        public double oy;
        public int param1;
        public int param2;
        public double param3;
        public double param4;
        public double latitude;
        public double longitude;
        public double radius;
    }

    [Serializable]
    public struct SDKLocalizeRequest
    {
        public static string endpoint = "localize";
        public string token;
        public int bank;
        public double fx;
        public double fy;
        public double ox;
        public double oy;
        public int param1;
        public int param2;
        public double param3;
        public double param4;
        public SDKMapId[] mapIds;
    }

    [Serializable]
    public struct SDKGeoPoseRequest
    {
        public static string endpoint = "geopose";
        public string token;
        public int bank;
        public double fx;
        public double fy;
        public double ox;
        public double oy;
        public int param1;
        public int param2;
        public double param3;
        public double param4;
        public SDKMapId[] mapIds;
    }

    [Serializable]
    public struct SDKLocalizeResult
    {
        public string error;
        public bool success;
        public int map;
        public float px;
        public float py;
        public float pz;
        public float r00;
        public float r01;
        public float r02;
        public float r10;
        public float r11;
        public float r12;
        public float r20;
        public float r21;
        public float r22;
    }

    [Serializable]
    public struct SDKGeoPoseResult
    {
        public string error;
        public bool success;
        public int map;
        public double latitude;
        public double longitude;
        public double ellipsoidHeight;
        public float[] quaternion;
    }

    [Serializable]
    public struct SDKEcefRequest
    {
        public static string endpoint = "ecef";
        public string token;
        public int bank;
        public int id;
    }

    [Serializable]
    public struct SDKEcefResult
    {
        public string error;
        public double[] ecef;
    }

    [Serializable]
    public struct SDKMapRequest
    {
        public static string endpoint = "mapb64";
        public string token;
        public int bank;
        public int id;
    }

    [Serializable]
    public struct SDKMapResult
    {
        public string error;
        public string sha256_al;
        public string b64;
        public SDKMapMetadataGetResult metadata;
    }

    [Serializable]
    public struct SDKDeleteMapRequest
    {
        public static string endpoint = "delete";
        public string token;
        public int bank;
        public int id;
    }

    [Serializable]
    public struct SDKDeleteMapResult
    {
        public string error;
    }

    [Serializable]
    public struct SDKRestoreMapImagesRequest
    {
        public static string endpoint = "restore";
        public string token;
        public int bank;
        public int id;
    }

    [Serializable]
    public struct SDKRestoreMapImagesResult
    {
        public string error;
    }

    [Serializable]
    public struct SDKMapPrivacyRequest
    {
        public static string endpoint = "privacy";
        public string token;
        public int bank;
        public int id;
        public int privacy;
    }

    [Serializable]
    public struct SDKMapPrivacyResult
    {
        public string error;
    }

    [Serializable]
    public struct SDKMapDownloadRequest
    {
        public static string endpoint = "mapb64";
        public string token;
        public int id;
    }

    [Serializable]
    public struct SDKMapDownloadResult
    {
        public string error;
        public string sha256_al;
        public string b64;
    }

    [Serializable]
    public struct SDKMapMetadataGetRequest
    {
        public static string endpoint = "metadataget";
        public string token;
        public int id;
    }

    [Serializable]
    public struct SDKMapMetadataGetResult
    {
        public string error;
        public int id;
        public int type;
        public string created;
        public string version;
        public int user;
        public int creator;
        public string name;
        public int size;
        public string status;
        public int privacy;
        public double latitude;
        public double longitude;
        public double altitude;
        public double tx;
        public double ty;
        public double tz;
        public double qw;
        public double qx;
        public double qy;
        public double qz;
        public double scale;
        public string sha256_al;
        public string sha256_sparse;
        public string sha256_dense;
        public string sha256_tex;
    }

    [Serializable]
    public struct SDKMapAlignmentSetRequest
    {
        public static string endpoint = "metadataset";
        public string token;
        public int id;
        public double tx;
        public double ty;
        public double tz;
        public double qw;
        public double qx;
        public double qy;
        public double qz;
        public double scale;
    }

    [Serializable]
    public struct SDKMapAlignmentSetResult
    {
        public string error;
    }

    [Serializable]
    public struct SDKMapAlignmentResetRequest
    {
        public static string endpoint = "reset";
        public string token;
        public int id;
    }

    [Serializable]
    public struct SDKMapAlignmentResetResult
    {
        public string error;
    }
}