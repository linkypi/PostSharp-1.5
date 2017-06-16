using System.ComponentModel;

namespace PostSharp.Samples.Silverlight.Aspects.Impl
{
    internal class NotifyPropertyChangedImpl : INotifyPropertyChanged
    {
        private readonly object owner;

        public NotifyPropertyChangedImpl(object owner)
        {
            this.owner = owner;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(owner, new PropertyChangedEventArgs(propertyName));
        }
    }
}