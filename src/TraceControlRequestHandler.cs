using System;
using System.Net;
using NETCore.Tracing.EventPipe;

namespace NETCore.Tracing
{
    public sealed class TraceControlRequestHandler : IRequestHandler
    {
        public string Prefix
        {
            get { return "TraceControl"; }
        }

        public void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if(!EventPipeSession.IsEnabled)
            {
                TraceControl.EnableDefault();
            }
            else
            {
                TraceControl.Disable();
            }
            string responseJson = $"{{\"tracingEnabled\": \"{EventPipeSession.IsEnabled}\"}}";

            byte[] responseBuffer = System.Text.Encoding.UTF8.GetBytes(responseJson);
            System.IO.Stream outputStream = response.OutputStream;
            response.ContentLength64 = responseBuffer.Length;
            response.ContentType = "application/json";
            outputStream.Write(responseBuffer, 0, responseBuffer.Length);
            outputStream.Close();
        }
    }
}
