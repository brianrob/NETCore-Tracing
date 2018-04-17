using System;
using System.IO;
using System.Net;
using NETCore.Tracing.EventPipe;

namespace NETCore.Tracing
{
    public sealed class TraceControlRequestHandler : IRequestHandler
    {
        private bool s_TracingEnabled;
        private string s_TraceFilePath = string.Empty;

        public string[] Prefixes
        {
            get
            {
                return new string[]
                {
                    "TraceControl",
                    "TraceControl/Enable",
                    "TraceControl/Disable"
                };
            }
        }

        public void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string responseJson = null;

            if(!s_TracingEnabled && request.Url.AbsolutePath.StartsWith("/TraceControl/Enable", StringComparison.OrdinalIgnoreCase))
            {
                s_TraceFilePath = Path.Combine(Directory.GetCurrentDirectory(), "default.netperf");
                TraceControl.EnableDefault();
                s_TracingEnabled = true;
                responseJson = $"{{\"tracingEnabled\" : \"{s_TracingEnabled}\", \"outputFilePath\" : \"{s_TraceFilePath}\"}}";
            }
            else if(s_TracingEnabled && request.Url.AbsolutePath.StartsWith("/TraceControl/Disable", StringComparison.OrdinalIgnoreCase))
            {
                TraceControl.Disable();
                s_TracingEnabled = false;
                responseJson = $"{{\"tracingEnabled\" : \"{s_TracingEnabled}\", \"outputFilePath\" : \"{s_TraceFilePath}\"}}";
                s_TraceFilePath = string.Empty;
            }
            else
            {
                responseJson = $"{{\"tracingEnabled\" : \"{s_TracingEnabled}\", \"outputFilePath\" : \"{s_TraceFilePath}\"}}";
            }

            // Write the response payload.
            byte[] responseBuffer = System.Text.Encoding.UTF8.GetBytes(responseJson);
            System.IO.Stream outputStream = response.OutputStream;
            response.ContentLength64 = responseBuffer.Length;
            response.ContentType = "application/json";
            outputStream.Write(responseBuffer, 0, responseBuffer.Length);
            outputStream.Close();
        }
    }
}
