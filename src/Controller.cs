using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using NETCore.Tracing.EventPipe;

namespace NETCore.Tracing
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
                    string prefix = BaseURL + handlerPrefix;
                    string prefixWithTrailingSlash = prefix + "/";

                    // Check for duplicate handlers.
                    if(m_Handlers.ContainsKey(prefix))
                    {
                        Console.WriteLine($"Ignoring handler {handler.GetType().FullName} with duplicate prefix {prefix}.");
                        continue;
                    }

                    // Add the handler and prefix.
                    listener.Prefixes.Add(prefixWithTrailingSlash);
                    m_Handlers.Add(prefix, handler);
                    m_Handlers.Add(prefixWithTrailingSlash, handler);
                    Console.WriteLine($"Added handler {handler.GetType().FullName} with prefix {prefix}.");
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
            if(m_Handlers.TryGetValue(request.Url.AbsoluteUri, out handler))
            {
                Console.WriteLine($"Handling Request to {request.Url} with handler {handler.GetType().FullName}.");
                handler.HandleRequest(request, response);
            }
            else
            {
                Console.WriteLine($"No handler found for {request.Url}.");
            }
        }
    }
}
