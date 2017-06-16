using System;
using System.Diagnostics;
using PostSharp.Laos;

namespace PostSharp.Samples.Simplest
{
[Serializable]
public sealed class NotSoTraceAttribute : OnMethodBoundaryAspect
{
    private string entryMessage;
    private string exitMessage;

    public override void CompileTimeInitialize(System.Reflection.MethodBase method)
    {
        this.entryMessage = string.Format("Entering {0}/{1}",
                                          method.DeclaringType, method);

        this.exitMessage = string.Format("Exit {0}/{1}",
                                    method.DeclaringType, method);
    }

    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    { Trace.TraceInformation(this.entryMessage); }

    public override void OnExit(MethodExecutionEventArgs eventArgs)
    { Trace.TraceInformation(this.exitMessage); }
}
}
