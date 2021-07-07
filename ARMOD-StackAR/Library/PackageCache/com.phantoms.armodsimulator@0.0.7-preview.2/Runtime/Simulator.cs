using System;
using System.IO;
using System.Text;
using com.Phantoms.SDKConfigures.Runtime;
using com.Phantoms.SDKEntry.Runtime;
using com.Phantoms.ActionNotification.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace com.Phantoms.ARMODSimulator.Runtime
{
    public class Simulator : MonoBehaviour
    {
        private TestSimpleHttpServer httpServer;
        public string ProjectName;

        public BaseNotificationDataConfig config;
        private string serverPath;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            CreateServer();
        }


        private void OnDisable()
        {
            StopServing();
        }

        private void CreateServer()
        {
            serverPath = Application.dataPath.Replace("Assets", "ServerData");
#if UNITY_IOS
            serverPath = Path.Combine(serverPath, "ios");
#else
            serverPath = Path.Combine(serverPath, "android");
#endif
            httpServer = new TestSimpleHttpServer(serverPath, 8084);
        }


        public void StopServing()
        {
            if (httpServer.IsServing)
            {
                httpServer?.Stop();
            }
            else
            {
                CreateServer();
            }
        }

        public void StartEmulating()
        {
            SDKEntryPoint tmp_Entry = FindObjectOfType<SDKEntryPoint>();
            SDKConfiguration tmp_Configs = new SDKConfiguration
            {
                dashboardConfig = new DashboardConfig {token = "", dashboardGateway = $"http://localhost:8084"},
                customConfig = new CustomConfig(),
                imageCloudRecognizerConfig = new ImageCloudRecognizerConfig()
            };
            tmp_Entry.InitSDK(JsonUtility.ToJson(tmp_Configs));
            StringBuilder tmp_UrlBuilder = new StringBuilder();
            tmp_UrlBuilder.Append("http://localhost:8084/");
            tmp_UrlBuilder.Append(ProjectName.ToLower());
            tmp_UrlBuilder.Append("/");
            tmp_UrlBuilder.Append(ProjectName.ToLower());
            tmp_Entry.FetchByUidForSimulator(tmp_UrlBuilder.ToString());
        }

        public void SendEvent()
        {
            try
            {
                var tmp_Notify = ActionNotificationCenter.DefaultCenter;
                tmp_Notify.PostNotification(nameof(ActionParameterDataType.OnEvent), ConfigConvertToData());
            }
            catch (Exception tmp_Exception)
            {
                Debug.LogError(tmp_Exception);
                StopServing();
#if UNITY_EDITOR
                EditorApplication.ExecuteMenuItem("Edit/Play");
#endif
            }
        }

        private BaseNotificationData ConfigConvertToData()
        {
            BaseNotificationData tmp_Data;

            switch (config)
            {
                case FocusResultNotificationDataConfig tmp_DataConfig:
                {
                    var tmp_FocusResult = new FocusResultNotificationData()
                    {
                        ActionName = tmp_DataConfig.ActionName,
                        FocusPos = tmp_DataConfig.FocusPos,
                        FocusState = tmp_DataConfig.FocusState,
                        FocusRot = tmp_DataConfig.FocusRot,
                    };
                    tmp_Data = tmp_FocusResult;
                    break;
                }

                case AnchorNotificationDataConfig tmp_AnchorConfig:
                {
                    var tmp_AnchorNotificationData = new AnchorNotificationData()
                    {
                        ActionName = tmp_AnchorConfig.ActionName,
                        ControllerTargetNode = tmp_AnchorConfig.ControllerTargetNode,

                        NotificationAct = tmp_AnchorConfig.notificationAct,
                        Offset = tmp_AnchorConfig.Offset,
                        Rotation = tmp_AnchorConfig.Rotation,
                        Position = tmp_AnchorConfig.Position,
                        StickType = tmp_AnchorConfig.StickType,
                        TrackableType = tmp_AnchorConfig.TrackableType
                    };
                    tmp_Data = tmp_AnchorNotificationData;
                    break;
                }

                case MarkerNotificationDataConfig tmp_MarkerConfig:
                    var tmp_MarkerNotificationData = new MarkerNotificationData
                    {
                        MarkerName = tmp_MarkerConfig.MarkerName,
                        MarkerState = tmp_MarkerConfig.MarkerState,
                        MarkerTrackable = tmp_MarkerConfig.MarkerTrackable,
                        ActionName = tmp_MarkerConfig.ActionName,

                        NotificationAct = tmp_MarkerConfig.notificationAct
                    };
                    tmp_Data = tmp_MarkerNotificationData;
                    break;
                default:
                    tmp_Data = new BaseNotificationData
                    {
                        ActionName = config.ActionName
                    };
                    break;
            }

            return tmp_Data;
        }

        private void OnGUI()
        {
            ProjectName = GUILayout.TextField(ProjectName);
            if (GUILayout.Button("Fetch data"))
            {
                StartEmulating();
            }
        }
    }
}