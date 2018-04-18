using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

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

            if(options.CircularMB.HasValue)
            {
                if (argPos++ == 0)
                {
                    relativeUrlBuilder.Append($"?CircularMB={options.CircularMB.Value}");
                }
                else
                {
                    relativeUrlBuilder.Append($"&CircularMB={options.CircularMB.Value}");
                }
            }

            if (options.SamplingRate.HasValue)
            {
                if (argPos++ == 0)
                {
                    relativeUrlBuilder.Append($"?SamplingRate={options.SamplingRate.Value}");
                }
                else
                {
                    relativeUrlBuilder.Append($"&SamplingRate={options.SamplingRate.Value}");
                }
            }

            string response = ExecuteWebRequest(relativeUrlBuilder.ToString());
            Console.WriteLine(response);

            // TODO: Get and display tracing session properties.
            return 0;
        }

        private static int DisableTracing(DisableTracingOptions options)
        {
            string relativeUrl = "/TraceControl/Disable";
            string response = ExecuteWebRequest(relativeUrl);
            Console.WriteLine(response);

            // TODO: Get and display the trace file location.
            return 0;
        }

        private static int TriggerGC(TriggerGCOptions options)
        {
            // Get the generation to collect.
            uint generation = options.Generation ?? 2;

            // Build the relative URL to hit.
            string relativeUrl = $"/TriggerGC/{generation}";
            string response = ExecuteWebRequest(relativeUrl);
            Console.WriteLine(response);

            return 0;
        }

        private static string ExecuteWebRequest(string relativeUrl)
        {
            string targetUrl = BaseUrl + relativeUrl;
            Console.WriteLine($"RequestURL = {targetUrl}");

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
