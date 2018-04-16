using System;
using NETCore.Tracing;

namespace DebugHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            Control.Start();

            Console.WriteLine("Started Tracing.  Press any key to exit.");
            Console.ReadKey();

            Control.Stop();
        }
    }
}
