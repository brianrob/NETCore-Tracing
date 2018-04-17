using System;
using System.Net;

namespace NETCore.Tracing
{
    public static class RequestHandlerList
    {
        public static IRequestHandler[] Handlers =
        {
            new TraceControlRequestHandler()
        };
    }

    public interface IRequestHandler
    {
        /// <summary>
        /// The URL prefixes to register for the handler.
        /// </summary>
        string[] Prefixes { get; }

        /// <summary>
        /// Handle a request.
        /// </summary>
        void HandleRequest(HttpListenerRequest request, HttpListenerResponse response);
    }
}
