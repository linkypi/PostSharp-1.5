using PostSharp.Extensibility;
using PostSharp.Laos;

namespace PostSharp.Samples.Silverlight.Aspects
{
    [ExternalAspectConfiguration(
        "PostSharp.Samples.Silverlight.Impl.NotifyPropertyChangedImpl, PostSharp.Samples.Silverlight.Impl")]
    [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Struct, PersistMetaData = false)]
    public class NotifyPropertyChangedAttribute : ExternalAspect
    {
    }
}