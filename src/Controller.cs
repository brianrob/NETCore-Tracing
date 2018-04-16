using System;
using System.Net;
using System.Threading.Tasks;

namespace NETCore.Tracing
{
    internal sealed class Controller : IDisposable
    {
        HttpListener m_Listener;

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
            listener.Prefixes.Add("http://localhost:8080/");
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

                string responseString = HandleRequest(request);
                if(responseString == null)
                {
                    responseString = string.Empty;
                }

                HttpListenerResponse response = context.Response;
                byte[] responseBuffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                System.IO.Stream outputStream = response.OutputStream;
                response.ContentLength64 = responseBuffer.Length;
                outputStream.Write(responseBuffer, 0, responseBuffer.Length);
                outputStream.Close();
            }
        }

        private static string HandleRequest(HttpListenerRequest request)
        {
            Console.WriteLine("Handling Request");
            return "<HTML><BODY>Hello World!</BODY><HTML>";
        }
    }
}
