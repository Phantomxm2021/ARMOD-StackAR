namespace com.Phantoms.ARMODAPI.Runtime
{
    [System.Serializable]
    public class DeviceInfo
    {
        public string deviceModel; //（只读）设备的模型或模式。
        public string deviceName; //（只读）用户定义的设备名称。
        public string deviceUniqueIdentifier; //（只读）设备的唯一标识符。每一台设备都有唯一的标识符。
        public int graphicsDeviceID; //（只读）显卡的唯一标识符ID。
        public string graphicsDeviceName; //（只读）显卡的名称。
        public string graphicsDeviceType; //（只读）显卡的类型。
        public string graphicsDeviceVendor; //（只读）显卡的供应商。
        public int graphicsDeviceVendorID; //（只读）显卡供应商的唯一识别码ID。
        public string graphicsDeviceVersion; //（只读）显卡的类型和版本。
        public int graphicsMemorySize; //只读）显存大小。
        public bool graphicsMultiThreaded; //（只读）是否支持多线程渲染？
        public int graphicsShaderLevel; //（只读）显卡着色器的级别。
        public int maxTextureSize; //（只读）支持的最大纹理大小。
        public string npotSupport; //（只读）GPU支持的NPOT纹理。
        public string operatingSystem; //（只读）操作系统的版本名称。
        public int processorCount; //（只读）当前处理器的数量。
        public int processorFrequency; //（只读）处理器的频率。
        public string processorType; //（只读）处理器的名称。
        public int supportedRenderTargetCount; //（只读）支持渲染多少目标纹理。
        public bool supports2DArrayTextures; //（只读）是否支持2D数组纹理。
        public bool supports3DTextures; //（只读）是否支持3D（体积）纹理。
        public bool supportsAccelerometer; //（只读）是否支持获取加速度计。
        public bool supportsAudio; //（只读）是否支持获取用于回放的音频设备。
        public bool supportsComputeShaders; //（只读）是否支持计算着色器。
        public bool supportsGyroscope; //是否支持获取陀螺仪。
        public bool supportsImageEffects; //（只读）是否支持图形特效。
        public bool supportsLocationService; //是否支持定位功能。
        public bool supportsMotionVectors; //是否支持运动向量。
        public bool supportsVibration; //是否支持用户触摸震动反馈。
        public int systemMemorySize; //（只读）系统内存大小。
        public string unsupportedIdentifier; //不支持运行在当前设备的SystemInfo属性值。
    }
}