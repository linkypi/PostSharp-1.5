using System;
using PostSharp.Laos;

namespace Test
{
    [Serializable]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ExceptionAttribute : OnMethodBoundaryAspect
    {
        private string _name = string.Empty;
        public ExceptionAttribute(string methodname) {
            _name = methodname;
        }

        public override void OnException(MethodExecutionEventArgs eventArgs)
        {
            Console.WriteLine("something take error, method : {0} , instance : {1}. addition msg :{2}",
                eventArgs.Method.Name, eventArgs.Instance, eventArgs.Exception.Message);

            //base.OnException(eventArgs);

        }
    }
}
