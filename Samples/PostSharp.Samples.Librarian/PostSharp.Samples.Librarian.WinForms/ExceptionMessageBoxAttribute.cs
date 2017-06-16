using System;
using System.Reflection;
using System.Windows.Forms;
using PostSharp.Laos;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.WinForms
{
    [Serializable]
    public sealed class ExceptionMessageBoxAttribute : OnExceptionAspect
    {
        public override Type GetExceptionType( MethodBase method )
        {
            return typeof(BusinessException);
        }

        public override void OnException( MethodExecutionEventArgs eventArgs )
        {
            MessageBox.Show( eventArgs.Instance as Form,
                             eventArgs.Exception.Message,
                             "Business Error",
                             MessageBoxButtons.OK,
                             MessageBoxIcon.Error );
            eventArgs.FlowBehavior = FlowBehavior.Continue;
        }
    }
}