using System.ComponentModel;
using System.Reflection;
using PostSharp.Laos;

namespace PostSharp.Samples.Silverlight.Aspects.Impl
{
    public class NotifyPropertyChangedSetterAdvice : IOnMethodBoundaryAspect
    {
        private readonly string propertyName;

        public NotifyPropertyChangedSetterAdvice(string propertyName)
        {
            this.propertyName = propertyName;
        }


        public void RuntimeInitialize(MethodBase method)
        {
        }

        public void OnException(MethodExecutionEventArgs eventArgs)
        {
        }

        public void OnEntry(MethodExecutionEventArgs eventArgs)
        {
        }

        public void OnExit(MethodExecutionEventArgs eventArgs)
        {
        }

        public void OnSuccess(MethodExecutionEventArgs eventArgs)
        {
            ((NotifyPropertyChangedImpl)
             ((IComposed<INotifyPropertyChanged>) eventArgs.Instance).GetImplementation(eventArgs.InstanceCredentials)).
                RaisePropertyChanged(this.propertyName);
        }
    }
}