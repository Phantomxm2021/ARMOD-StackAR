using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace  com.Phantoms.ARMODSimulator.Runtime
{
    public class TestSimpleHttpServer
    {
        private readonly string[] indexFiles =
        {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm"
        };

        private static readonly IDictionary<string, string> mimeTypeMappings =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                #region extension to MIME type list

                {".asf", "video/x-ms-asf"},
                {".asx", "video/x-ms-asf"},
                {".avi", "video/x-msvideo"},
                {".bin", "application/octet-stream"},
                {".cco", "application/x-cocoa"},
                {".crt", "application/x-x509-ca-cert"},
                {".css", "text/css"},
                {".deb", "application/octet-stream"},
                {".der", "application/x-x509-ca-cert"},
                {".dll", "application/octet-stream"},
                {".dmg", "application/octet-stream"},
                {".ear", "application/java-archive"},
                {".eot", "application/octet-stream"},
                {".exe", "application/octet-stream"},
                {".arexperience", "application/zip"},
                {".flv", "video/x-flv"},
                {".gif", "image/gif"},
                {".hqx", "application/mac-binhex40"},
                {".htc", "text/x-component"},
                {".htm", "text/html"},
                {".html", "text/html"},
                {".ico", "image/x-icon"},
                {".img", "application/octet-stream"},
                {".iso", "application/octet-stream"},
                {".jar", "application/java-archive"},
                {".jardiff", "application/x-java-archive-diff"},
                {".jng", "image/x-jng"},
                {".jnlp", "application/x-java-jnlp-file"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".js", "application/x-javascript"},
                {".mml", "text/mathml"},
                {".mng", "video/x-mng"},
                {".mov", "video/quicktime"},
                {".mp3", "audio/mpeg"},
                {".mpeg", "video/mpeg"},
                {".mpg", "video/mpeg"},
                {".msi", "application/octet-stream"},
                {".msm", "application/octet-stream"},
                {".msp", "application/octet-stream"},
                {".pdb", "application/x-pilot"},
                {".pdf", "application/pdf"},
                {".pem", "application/x-x509-ca-cert"},
                {".pl", "application/x-perl"},
                {".pm", "application/x-perl"},
                {".png", "image/png"},
                {".prc", "application/x-pilot"},
                {".ra", "audio/x-realaudio"},
                {".rar", "application/x-rar-compressed"},
                {".rpm", "application/x-redhat-package-manager"},
                {".rss", "text/xml"},
                {".run", "application/x-makeself"},
                {".sea", "application/x-sea"},
                {".shtml", "text/html"},
                {".sit", "application/x-stuffit"},
                {".swf", "application/x-shockwave-flash"},
                {".tcl", "application/x-tcl"},
                {".tk", "application/x-tcl"},
                {".txt", "text/plain"},
                {".war", "application/java-archive"},
                {".wbmp", "image/vnd.wap.wbmp"},
                {".wmv", "video/x-ms-wmv"},
                {".xml", "text/xml"},
                {".xpi", "application/x-xpinstall"},
                {".zip", "application/zip"},

                #endregion
            };

        private Thread serverThread;
        private string rootDirectory;
        private HttpListener listener;
        private int port;

        public int Port => port;
        public bool IsServing => listener != null && listener.IsListening;

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="_path">Directory path to serve.</param>
        /// <param name="_port">Port of the server.</param>
        public TestSimpleHttpServer(string _path, int _port)
        {
            this.Initialize(_path, _port);
        }

        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        /// <param name="_path">Directory path to serve.</param>
        public TestSimpleHttpServer(string _path)
        {
            //get an empty port
            TcpListener tmp_L = new TcpListener(IPAddress.Loopback, 0);
            tmp_L.Start();
            int tmp_Port = ((IPEndPoint) tmp_L.LocalEndpoint).Port;
            tmp_L.Stop();
            this.Initialize(_path, tmp_Port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            serverThread.Abort();
            listener.Stop();
            Debug.Log("Server stop!");
        }

        private void Listen()
        {
            try
            {
                listener = new HttpListener {AuthenticationSchemes = AuthenticationSchemes.Anonymous};
                listener.Prefixes.Add($"http://*:{port}/");
                listener.Start();
                listener.BeginGetContext(GetContext, listener);
            }
            catch (Exception tmp_Exception)
            {
                Debug.LogError(tmp_Exception.Message);
            }
        }

        void GetContext(IAsyncResult ar)
        {
            HttpListener tmp_HttpListener = ar.AsyncState as HttpListener;
            if (tmp_HttpListener == null) return;
            HttpListenerContext tmp_Context = tmp_HttpListener.EndGetContext(ar);

            tmp_HttpListener.BeginGetContext(GetContext, tmp_HttpListener);

            HttpListenerRequest tmp_Request = tmp_Context.Request;
            HttpListenerResponse tmp_Response = tmp_Context.Response;

            tmp_Response.ContentType = "html";
            tmp_Response.ContentEncoding = Encoding.UTF8;
            Process(tmp_Context);
        }

        private void Process(HttpListenerContext _context)
        {
            string tmp_Filename = _context.Request.Url.AbsolutePath;
            tmp_Filename = tmp_Filename.Substring(1);

            if (string.IsNullOrEmpty(tmp_Filename))
            {
                foreach (string tmp_IndexFile in indexFiles)
                {
                    if (!File.Exists(Path.Combine(rootDirectory, tmp_IndexFile))) continue;
                    tmp_Filename = tmp_IndexFile;
                    break;
                }
            }

            tmp_Filename = Path.Combine(rootDirectory, tmp_Filename);

            if (File.Exists(tmp_Filename))
            {
                try
                {
                    Stream tmp_Input = new FileStream(tmp_Filename, FileMode.Open);

                    //Adding permanent http response headers    
                    _context.Response.ContentType =
                        mimeTypeMappings.TryGetValue(Path.GetExtension(tmp_Filename), out var tmp_Mime)
                            ? tmp_Mime
                            : "application/octet-stream";
                    _context.Response.ContentLength64 = tmp_Input.Length;

                    if (_context.Request.HttpMethod != "HEAD")
                    {
                        using (Stream tmp_Output = _context.Response.OutputStream)
                        {
                            byte[] tmp_Buffer = new byte[tmp_Input.Length];
                            int tmp_Nbytes;
                            while ((tmp_Nbytes = tmp_Input.Read(tmp_Buffer, 0, tmp_Buffer.Length)) > 0)
                            {
                                _context.Response.SendChunked = tmp_Input.Length > 1024 * 16;
                                tmp_Output.Write(tmp_Buffer, 0, tmp_Nbytes);
                            }
                        }
                    }


                    tmp_Input.Close();
                }
                catch
                {
                    _context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                _context.Response.StatusCode = (int) HttpStatusCode.NotFound;
            }

            _context.Response.OutputStream.Close();
        }

        private void Initialize(string _path, int _port)
        {
            this.rootDirectory = _path;
            this.port = _port;
            serverThread = new Thread(this.Listen);
            serverThread.Start();
            serverThread.IsBackground = true;
        }
    }
}