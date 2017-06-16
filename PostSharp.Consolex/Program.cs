using PostSharp.Extensibility;
using System;

namespace PostSharp.Console
{
    class Program
    {
        [LoaderOptimization(LoaderOptimization.SingleDomain)]
        public static int Main(string[] args)
        {
            return (int)CommandLineProgram.Main(args);
        }
    }
}
