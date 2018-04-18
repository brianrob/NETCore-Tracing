using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NETCore.Tracing.EventPipe;

namespace NETCore.Tracing
{
    public sealed class TraceControlRequestHandler : IRequestHandler
    {
        private bool s_TracingEnabled;
        private string s_TraceFilePath = string.Empty;

        public string[] Prefixes
        {
            get
            {
                return new string[]
                {
                    "TraceControl",
                    "TraceControl/Enable",
                    "TraceControl/Disable"
                };
            }
        }

        public void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string responseJson = null;

            if(!s_TracingEnabled && request.Url.AbsolutePath.StartsWith("/TraceControl/Enable", StringComparison.OrdinalIgnoreCase))
            {
                // The query string contains the configuration.
                // Valid fields are:
                //  - OutputFileName
                //  - CircularBufferMB
                //  - SamplingRateMS
                //  - Providers

                string outputFileName = request.QueryString.Get("OutputFileName");
                if(string.IsNullOrEmpty(outputFileName))
                {
                    outputFileName = "default.netperf";
                }

                uint circularBufferMB = 1024;
                string strCircularBufferMB = request.QueryString.Get("CircularBufferMB");
                if(!string.IsNullOrEmpty(strCircularBufferMB))
                {
                    circularBufferMB = Convert.ToUInt32(strCircularBufferMB);
                }

                uint samplingRateMS = 1;
                string strSamplingRateMS = request.QueryString.Get("SamplingRateMS");
                if(!string.IsNullOrEmpty(strSamplingRateMS))
                {
                    samplingRateMS = Convert.ToUInt32(strSamplingRateMS);
                }

                ProviderConfiguration[] configs = DefaultProviders;
                string strProviders = request.QueryString.Get("Providers");
                if(!string.IsNullOrEmpty(strProviders))
                {
                    configs = ParseProviderConfiguration(strProviders);
                }

                s_TraceFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

                TraceConfiguration traceConfig = new TraceConfiguration(s_TraceFilePath, circularBufferMB);
                traceConfig.SetSamplingRate(TimeSpan.FromMilliseconds(samplingRateMS));
                foreach(ProviderConfiguration provConfig in configs)
                {
                    traceConfig.EnableProvider(provConfig.Name, provConfig.Keywords, provConfig.Level);
                }

                TraceControl.Enable(traceConfig);
                s_TracingEnabled = true;
                responseJson = $"{{\"TracingEnabled\" : \"{s_TracingEnabled}\", \"OutputFilePath\" : \"{s_TraceFilePath}\", \"CircularBufferMB\" : {circularBufferMB}, \"SamplingRateMS\" : {samplingRateMS}}}";
            }
            else if(s_TracingEnabled && request.Url.AbsolutePath.StartsWith("/TraceControl/Disable", StringComparison.OrdinalIgnoreCase))
            {
                TraceControl.Disable();
                s_TracingEnabled = false;
                responseJson = $"{{\"TracingEnabled\" : \"{s_TracingEnabled}\", \"OutputFilePath\" : \"{s_TraceFilePath}\"}}";
                s_TraceFilePath = string.Empty;
            }
            else
            {
                responseJson = $"{{\"TracingEnabled\" : \"{s_TracingEnabled}\", \"OutputFilePath\" : \"{s_TraceFilePath}\"}}";
            }

            // Write the response payload.
            byte[] responseBuffer = System.Text.Encoding.UTF8.GetBytes(responseJson);
            System.IO.Stream outputStream = response.OutputStream;
            response.ContentLength64 = responseBuffer.Length;
            response.ContentType = "application/json";
            outputStream.Write(responseBuffer, 0, responseBuffer.Length);
            outputStream.Close();
        }

        private ProviderConfiguration[] ParseProviderConfiguration(string strProviders)
        {
            if(string.IsNullOrEmpty(strProviders))
            {
                return new ProviderConfiguration[0];
            }

            List<ProviderConfiguration> providerConfigs = new List<ProviderConfiguration>();

            // Configuration is of the format:
            //  Provider-Name:Keywords:Level,Provider-Name:Keywords:Level...

            Console.WriteLine($"Processing string: {strProviders}");

            // Split by ','.
            string[] strProviderList = strProviders.Split(new char[] { ',' });
            for(int i=0; i<strProviderList.Length; i++)
            {
                Console.WriteLine($"Processing token: {strProviderList[i]}");

                // Split by ':'.
                string[] strProviderComponents = strProviderList[i].Split(new char[] { ':' });
                if(strProviderComponents.Length == 3)
                {
                    ProviderConfiguration configuration = new ProviderConfiguration()
                    {
                        Name = strProviderComponents[0],
                        Keywords = Convert.ToUInt64(strProviderComponents[1], fromBase: 16),
                        Level = Convert.ToUInt32(strProviderComponents[2], fromBase: 10)
                    };
                    providerConfigs.Add(configuration);

                    Console.WriteLine($"Created provider: {configuration.ToString()}");
                }
                else
                {
                    Console.WriteLine($"Skipping {strProviderList[i]}.");
                }
            }

            return providerConfigs.ToArray();
        }

        private static ProviderConfiguration[] DefaultProviders = new ProviderConfiguration[]
        {
            new ProviderConfiguration()
            {
                Name = "Microsoft-Windows-DotNETRuntime",
                Keywords = 0x4c14fccbd,
                Level = 5
            },
            new ProviderConfiguration()
            {
                Name = "e13c0d23-ccbc-4e12-931b-d9cc2eee27e4",
                Keywords = 0x4c14fccbd,
                Level = 5
            },
            new ProviderConfiguration()
            {
                Name = "Microsoft-Windows-DotNETRuntimePrivate",
                Keywords = 0x4002000b,
                Level = 5
            },
            new ProviderConfiguration()
            {
                Name = "763fd754-7086-4dfe-95eb-c01a46faf4ca",
                Keywords = 0x4002000b,
                Level = 5
            },
            new ProviderConfiguration()
            {
                Name = "Microsoft-DotNETCore-SampleProfiler",
                Keywords = 0x0,
                Level = 5
            },
            new ProviderConfiguration()
            {
                Name = "3c530d44-97ae-513a-1e6d-783e8f8e03a9",
                Keywords = 0x0,
                Level = 5
            }
        };
    }

    public sealed class ProviderConfiguration
    {
        public string Name;
        public UInt64 Keywords;
        public uint Level;

        public override string ToString()
        {
            return $"Name = {Name}, Keywords = {Keywords.ToString("X")}, Level = {Level}";
        }
    }
}
