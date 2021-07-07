using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.Phantoms.WebRequestExtension.Runtime.WebRequest
{
    public abstract class AbstractWebRequest
    {
        protected UnityWebRequest WebRequest { get; set; }

        public abstract Task<string> SendRequest(Uri _uri, string _method);
        public abstract Task<string> SendRequest(Uri _uri, string _method, IDictionary<string, string> _header,WWWForm _dataForm);
        public abstract Task<string> SendRequest(Uri _uri, string _method, IDictionary<string, string> _header,byte[] _jsonData);
    }
}