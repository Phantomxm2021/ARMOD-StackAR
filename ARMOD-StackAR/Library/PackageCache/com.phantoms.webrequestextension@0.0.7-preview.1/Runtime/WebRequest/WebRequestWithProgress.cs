using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.Phantoms.WebRequestExtension.Runtime.WebRequest
{
    public class WebRequestWithProgress : AbstractWebRequest, IDisposable
    {
        private Action<float> updateProgress;
        private readonly int timeout;

        public WebRequestWithProgress(int _timeout = 30, Action<float> _updateProgress = null)
        {
            timeout = _timeout;
            updateProgress = _updateProgress;
        }

        public override async Task<string> SendRequest(Uri _uri, string _method)
        {
            WebRequest = new UnityWebRequest
            {
                uri = _uri,
                timeout = this.timeout,
                downloadHandler = new DownloadHandlerBuffer(),
                method = _method
            };

            if (updateProgress == null)
            {
                await WebRequest.SendWebRequest();
            }
            else
            {
                await WebRequest.SendWebRequest().ConfigureAwait(new Progress<float>(updateProgress));
            }

            if (WebRequest.error != null || WebRequest.isHttpError)
            {
                throw new Exception(WebRequest.error);
            }

            return WebRequest.downloadHandler.text;
        }

        public override async Task<string> SendRequest(Uri _uri, string _method,
            IDictionary<string, string> _header, WWWForm _dataForm)
        {
            WebRequest = new UnityWebRequest
            {
                uri = _uri,
                timeout = this.timeout,
                downloadHandler = new DownloadHandlerBuffer(),
                method = _method
            };

            byte[] tmp_Data = null;
            if (_dataForm != null)
            {
                tmp_Data = _dataForm.data;
                if (tmp_Data.Length == 0)
                    tmp_Data = null;
            }

            WebRequest.uploadHandler = new UploadHandlerRaw(tmp_Data);
            WebRequest.downloadHandler = new DownloadHandlerBuffer();
            if (_dataForm == null)
                return null;

            if (_header != null)
            {
                foreach (KeyValuePair<string, string> tmp_KeyValuePair in _header)
                {
                    WebRequest.SetRequestHeader(tmp_KeyValuePair.Key, tmp_KeyValuePair.Value);
                }
            }

            if (updateProgress == null)
            {
                await WebRequest.SendWebRequest();
            }
            else
            {
                await WebRequest.SendWebRequest().ConfigureAwait(new Progress<float>(updateProgress));
            }

            if (WebRequest.error != null || WebRequest.isHttpError)
            {
                throw new Exception(WebRequest.error);
            }

            return WebRequest.downloadHandler.text;
        }

        public override async Task<string> SendRequest(Uri _uri, string _method, IDictionary<string, string> _header,
            byte[] _jsonData)
        {
            WebRequest = new UnityWebRequest
            {
                uri = _uri,
                timeout = this.timeout,
                downloadHandler = new DownloadHandlerBuffer(),
                method = _method
            };


            if (_header != null)
            {
                foreach (KeyValuePair<string, string> tmp_KeyValuePair in _header)
                {
                    WebRequest.SetRequestHeader(tmp_KeyValuePair.Key, tmp_KeyValuePair.Value);
                }
            }

            WebRequest.uploadHandler = new UploadHandlerRaw(_jsonData);


            if (updateProgress == null)
            {
                await WebRequest.SendWebRequest();
            }
            else
            {
                await WebRequest.SendWebRequest().ConfigureAwait(new Progress<float>(updateProgress));
            }

            if (WebRequest.error != null || WebRequest.isHttpError)
            {
                throw new Exception(WebRequest.error);
            }

            return WebRequest.downloadHandler.text;
        }

        public void Dispose()
        {
            updateProgress = null;
            WebRequest?.Dispose();
            WebRequest = null;
        }
    }
}