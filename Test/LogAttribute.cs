using System;
using PostSharp.Laos;

namespace Test
{
    [Serializable]
    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public class LogAttribute  : OnMethodBoundaryAspect
    {
        /* PostSharp 2.0 里可用的方法
        public override void OnSuccess(MethodExecutionArgs args)
        {
            Console.WriteLine("...{0}...onsuccess......", DateTime.Now);
            base.OnSuccess(args);
            Log.Write(args);
        }
        */

        public LogAttribute() {
        }

        public override void OnEntry(MethodExecutionEventArgs args)
        {
            Console.WriteLine("start log ... ");
            base.OnEntry(args);
        }

        public override void OnExit(MethodExecutionEventArgs args)
        {
            Console.WriteLine("end log ...");
            base.OnExit(args);
        }
    }
}
