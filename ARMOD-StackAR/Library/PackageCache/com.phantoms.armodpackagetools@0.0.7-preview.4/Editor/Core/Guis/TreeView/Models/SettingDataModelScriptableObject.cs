using System;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class SettingDataModelScriptableObject:ScriptableObject
    {
        [NonSerialized] public string Gateway;
        [NonSerialized] public AliyunCloudStorageModel AliyunCloudStorageModel = new AliyunCloudStorageModel();
    }
}