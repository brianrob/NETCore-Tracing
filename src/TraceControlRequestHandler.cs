using System;
using System.Net;
using NETCore.Tracing.EventPipe;

namespace NETCore.Tracing
{
    public sealed class TraceControlRequestHandler : IRequestHandler
    {
        private bool s_TracingEnabled;

        public string Prefix
        {
            get { return "TraceControl"; }
        }

        public void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if(!s_TracingEnabled)
            {
                TraceControl.EnableDefault();
                s_TracingEnabled = true;
            }
            else
            {
                TraceControl.Disable();
                s_TracingEnabled = false;
            }
            string responseJson = $"{{\"tracingEnabled\": \"{s_TracingEnabled}\"}}";

            byte[] responseBuffer = System.Text.Encoding.UTF8.GetBytes(responseJson);
            System.IO.Stream outputStream = response.OutputStream;
            response.ContentLength64 = responseBuffer.Length;
            response.ContentType = "application/json";
            outputStream.Write(responseBuffer, 0, responseBuffer.Length);
            outputStream.Close();
        }
    }
}
