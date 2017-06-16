using System;
using PostSharp.Laos;

namespace PostSharp.Samples.Silverlight.Aspects.Impl
{
    [CompositionAspectConfiguration(
        PublicInterface = "System.ComponentModel.INotifyPropertyChanged, System",
        Options = CompositionAspectOptions.GenerateImplementationAccessor)]
    public sealed class NotifyPropertyChangedCompositionAdvice : ICompositionAspect
    {
        public void RuntimeInitialize(Type type)
        {
        }

        public object CreateImplementationObject(InstanceBoundLaosEventArgs eventArgs)
        {
            return new NotifyPropertyChangedImpl(eventArgs.Instance);
        }
    }
}