using System.Threading;
using PostSharp.Samples.Silverlight.Aspects;

namespace PostSharp.Samples.Silverlight.Test
{
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();
            this.DataContext = new Person ("Mobutu", "Sese Seko", "Zaire");
        }

        [OnWorkerThread]
        private void somethingLongButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Thread.Sleep(5000);
        }
    }
}