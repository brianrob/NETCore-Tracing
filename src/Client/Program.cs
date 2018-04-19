using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace NETCore.Tracing.Client
{
    class Program
    {
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<EnableTracingOptions, DisableTracingOptions, TriggerGCOptions>(args)
                .MapResult(
                    (EnableTracingOptions opts) => EnableTracing(opts),
                    (DisableTracingOptions opts) => DisableTracing(opts),
                    (TriggerGCOptions opts) => TriggerGC(opts),
                    errs => 1);
        }

        private static int EnableTracing(EnableTracingOptions options)
        {
            StringBuilder relativeUrlBuilder = new StringBuilder("/TraceControl/Enable");
            int argPos = 0;
            if (!string.IsNullOrEmpty(options.Providers))
            {
                if(argPos++ == 0)
                {
                    relativeUrlBuilder.Append($"?Providers={options.Providers}");
                }
                else
                {
                    relativeUrlBuilder.Append($"&Providers={options.Providers}");
                }
            }

            if (!string.IsNullOrEmpty(options.OutputFileName))
            {
                if (argPos++ == 0)
                {
                    relativeUrlBuilder.Append($"?OutputFileName={options.OutputFileName}");
                }
                else
                {
                    relativeUrlBuilder.Append($"&OutputFileName={options.OutputFileName}");
                }
            }

            if(options.CircularBufferMB.HasValue)
            {
                if (argPos++ == 0)
                {
                    relativeUrlBuilder.Append($"?CircularBufferMB={options.CircularBufferMB.Value}");
                }
                else
                {
                    relativeUrlBuilder.Append($"&CircularBufferMB={options.CircularBufferMB.Value}");
                }
            }

            if (options.SamplingRateMS.HasValue)
            {
                if (argPos++ == 0)
                {
                    relativeUrlBuilder.Append($"?SamplingRateMS={options.SamplingRateMS.Value}");
                }
                else
                {
                    relativeUrlBuilder.Append($"&SamplingRateMS={options.SamplingRateMS.Value}");
                }
            }

            string strResponse = ExecuteWebRequest(relativeUrlBuilder.ToString());

            EnableTracingResponse response = JsonConvert.DeserializeObject<EnableTracingResponse>(strResponse);
            Console.WriteLine($"Tracing Enabled: {response.TracingEnabled}");
            Console.WriteLine($"Trace File Path: {response.OutputFilePath}");
            Console.WriteLine($"Circular Buffer Size: {response.CircularBufferMB} MB");
            Console.WriteLine($"Sampling Rate: {response.SamplingRateMS} ms");
            return 0;
        }

        private static int DisableTracing(DisableTracingOptions options)
        {
            string relativeUrl = "/TraceControl/Disable";
            string strResponse = ExecuteWebRequest(relativeUrl);

            DisableTracingResponse response = JsonConvert.DeserializeObject<DisableTracingResponse>(strResponse);
            Console.WriteLine($"Tracing Enabled: {response.TracingEnabled}");
            Console.WriteLine($"Trace File Path: {response.OutputFilePath}");
            return 0;
        }

        private static int TriggerGC(TriggerGCOptions options)
        {
            // Get the generation to collect.
            uint generation = options.Generation ?? 2;

            // Build the relative URL to hit.
            string relativeUrl = $"/TriggerGC/{generation}";
            string strResponse = ExecuteWebRequest(relativeUrl);

            TriggerGCResponse response = JsonConvert.DeserializeObject<TriggerGCResponse>(strResponse);
            Console.WriteLine($"Triggered Gen{response.GCGeneration} GC.");
            return 0;
        }

        private static string ExecuteWebRequest(string relativeUrl)
        {
            string targetUrl = BaseUrl + relativeUrl;
            using (HttpClient client = new HttpClient())
            {
                string response = null;
                try
                {
                    Task<HttpResponseMessage> responseMessageTask = client.GetAsync(targetUrl);
                    responseMessageTask.Wait();
                    HttpResponseMessage responseMessage = responseMessageTask.Result;
                    Task<string> responseTask = responseMessage.Content.ReadAsStringAsync();
                    responseTask.Wait();
                    response = responseTask.Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                return response;
            }
        }

        private const string BaseUrl = "http://localhost:8080";
    }
}
