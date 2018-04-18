using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using NETCore.Tracing.Service.EventPipe;

namespace NETCore.Tracing.Service
{
    internal sealed class Controller : IDisposable
    {
        private const string BaseURL = "http://localhost:8080/";
        private HttpListener m_Listener;
        private Dictionary<string, IRequestHandler> m_Handlers = new Dictionary<string, IRequestHandler>();

        internal Controller()
        {
            m_Listener = new HttpListener();
            Task.Run(new Action(Listen));

            Console.WriteLine("Created Controller!");
        }

        public void Dispose()
        {
            ((IDisposable)m_Listener).Dispose();

            Console.WriteLine("Disposed Controller!");
        }

        private void ConfigureListener(HttpListener listener)
        {
            // Add the base prefix.
            listener.Prefixes.Add(BaseURL);

            foreach (IRequestHandler handler in RequestHandlerList.Handlers)
            {
                foreach(string handlerPrefix in handler.Prefixes)
                {
                    // Build the prefix.
                    string httpListenerPrefix = BaseURL + handlerPrefix + "/";

                    // Check for duplicate handlers.
                    if(m_Handlers.ContainsKey(httpListenerPrefix))
                    {
                        Console.WriteLine($"Ignoring handler {handler.GetType().FullName} with duplicate prefix {httpListenerPrefix}.");
                        continue;
                    }

                    // Add the handler and prefix.
                    listener.Prefixes.Add(httpListenerPrefix);
                    m_Handlers.Add("/" + handlerPrefix, handler);
                    m_Handlers.Add("/" + handlerPrefix + "/", handler);
                    Console.WriteLine($"Added handler {handler.GetType().FullName} with prefix {httpListenerPrefix}.");
                }
            }
        }

        private void Listen()
        {
            if(!HttpListener.IsSupported)
            {
                Console.WriteLine("HttpListener is not supported.  Tracing controller will not be enabled.");
                return;
            }
            
            ConfigureListener(m_Listener);
            m_Listener.Start();
            Console.WriteLine("Listener started.");

            while(m_Listener.IsListening)
            {
                HttpListenerContext context = m_Listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Ignore non-loopback requests.
                if (!request.IsLocal)
                {
                    continue;
                }

                // Handle the request.
                FindHandlerAndExecuteRequest(request, response);                
            }
        }

        private void FindHandlerAndExecuteRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            IRequestHandler handler = null;
            if(m_Handlers.TryGetValue(request.Url.AbsolutePath, out handler))
            {
                Console.WriteLine($"Handling Request to {request.Url} with handler {handler.GetType().FullName}.");
                handler.HandleRequest(request, response);
            }
            else
            {
                Console.WriteLine($"No handler found for {request.Url} with AbsolutePath {request.Url.AbsolutePath}.");
            }
        }
    }
}
