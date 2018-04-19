using System;
using CommandLine;

namespace NETCore.Tracing.Client
{
    [Verb("EnableTracing", HelpText = "Enable EventPipe tracing functionality.")]
    internal sealed class EnableTracingOptions
    {
        [Option("OutputFileName", HelpText = "The name of the destination file without the full path.")]
        public string OutputFileName { get; set; }

        [Option("Providers", HelpText = "A set of comma-delimited provider configurations.  For example: Microsoft-Windows-DotNETRuntime:0xFFFFFFFFFFFFFFFF:5,...")]
        public string Providers { get; set; }

        [Option("CircularBufferMB", HelpText = "The size in megabytes of the circular buffer.")]
        public uint? CircularBufferMB { get; set; }

        [Option("SamplingRateMS", HelpText = "The sampling rate of the sample-based profiler in milliseconds.")]
        public uint? SamplingRateMS { get; set; }
    }

    [Verb("DisableTracing", HelpText = "Disable EventPipe tracing functionality.")]
    internal sealed class DisableTracingOptions
    {
    }

    [Verb("TriggerGC", HelpText = "Trigger a GC in the target process.")]
    internal sealed class TriggerGCOptions
    {
        [Option("Generation", Required = false, HelpText = "The generation of the GC to trigger.")]
        public uint? Generation { get; set; }
    }
}
