using System;
using System.Net;

namespace NETCore.Tracing
{
    public sealed class TriggerGCRequestHandler : IRequestHandler
    {
        public string[] Prefixes
        {
            get
            {
                return new string[]
                {
                    "TriggerGC/0",
                    "TriggerGC/1",
                    "TriggerGC/2"
                };
            }
        }

        public void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            int depth = 0;
            if(request.Url.AbsolutePath.StartsWith("/TriggerGC/0", StringComparison.OrdinalIgnoreCase))
            {
                depth = 0;
            }
            else if(request.Url.AbsolutePath.StartsWith("/TriggerGC/1", StringComparison.OrdinalIgnoreCase))
            {
                depth = 1;
            }
            else if(request.Url.AbsolutePath.StartsWith("/TriggerGC/2", StringComparison.OrdinalIgnoreCase))
            {
                depth = 2;
            }

            // Trigger the GC.
            GC.Collect(depth, GCCollectionMode.Forced, blocking: true, compacting: true);

            string responseJson = $"{{\"GCGeneration\" : {depth}}}";

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
