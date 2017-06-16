using System;
using System.Collections.Generic;
using System.Text;
using PostSharp.Laos;

namespace PostSharp.Samples.Compact
{
    public class RecordAspect : OnMethodBoundaryAspect
    {
        private readonly string message;

        // Constructor used during deserialization.
        public RecordAspect()
        {
            
        }

        public RecordAspect( string message )
        {
            this.message = message;
            
        }
        public override void OnSuccess(MethodExecutionEventArgs eventArgs)
        {
            ((MainForm) eventArgs.Instance).AddRecordToHistory(message);
        }

       
    }
}
