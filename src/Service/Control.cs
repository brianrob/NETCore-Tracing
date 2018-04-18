using System;

namespace NETCore.Tracing.Service
{
    public static class Control
    {
        private static Controller s_Controller;
        private static object s_ControllerLock = new object();
        public static void Start()
        {
            if(s_Controller == null)
            {
                lock(s_ControllerLock)
                {
                    if(s_Controller == null)
                    {
                        s_Controller = new Controller();
                    }
                }
            }
        }

        public static void Stop()
        {
            if(s_Controller != null)
            {
                lock(s_ControllerLock)
                {
                    if(s_Controller != null)
                    {
                        ((IDisposable)s_Controller).Dispose();
                        s_Controller = null;
                    }
                }
            }
        }
    }
}
