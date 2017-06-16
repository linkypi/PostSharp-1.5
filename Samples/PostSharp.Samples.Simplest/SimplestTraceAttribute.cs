using System;
using System.Diagnostics;
using PostSharp.Laos;

namespace PostSharp.Samples.Simplest
{
    public sealed class SimplestTraceAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionEventArgs eventArgs)
        {
            Trace.TraceInformation("Entering {0}.", eventArgs.Method);
            Trace.Indent();
        }

        public override void OnExit(MethodExecutionEventArgs eventArgs)
        {
            Trace.Unindent();
            Trace.TraceInformation("Leaving {0}.", eventArgs.Method);
        }
    }
}
