using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.Tracing.EventPipe
{
    public static class EventPipeSession
    {
        private static bool s_IsEnabled;

        public static void Enable()
        {
            s_IsEnabled = true;
        }

        public static void Disable()
        {
            s_IsEnabled = false;
        }

        public static bool IsEnabled
        {
            get { return s_IsEnabled; }
        }
    }
}
