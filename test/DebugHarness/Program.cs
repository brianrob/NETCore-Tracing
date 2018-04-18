using System;
using System.Threading;
using System.Threading.Tasks;
using NETCore.Tracing.Service;

namespace DebugHarness
{
    class Program
    {
        private const int NumTasks = 2;

        static void Main(string[] args)
        {
            Control.Start();

            for(int i=0; i<NumTasks; i++)
            {
                Task.Run(new Action(SpinTask));
            }

            Console.WriteLine("Started Tracing.  Press any key to exit.");
            Console.ReadKey();

            Control.Stop();
        }

        private static void SpinTask()
        {
            while(true)
            {
                GC.KeepAlive(new object());
                Thread.Sleep(1);
            }
        }
    }
}
