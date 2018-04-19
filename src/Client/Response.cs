using System;

namespace NETCore.Tracing.Client
{
    public sealed class EnableTracingResponse
    {
        public bool TracingEnabled { get; set; }
        public string OutputFilePath { get; set; }
        public uint CircularBufferMB { get; set; }
        public uint SamplingRateMS { get; set; }
    }

    public sealed class DisableTracingResponse
    {
        public bool TracingEnabled { get; set; }
        public string OutputFilePath { get; set; }
    }

    public sealed class TriggerGCResponse
    {
        public uint GCGeneration { get; set; }
    }
}
