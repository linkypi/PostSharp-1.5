#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics;

namespace PostSharp.Samples.XTrace.Test
{
    internal class Program
    {
        private static void Main( string[] args )
        {
            TextWriterTraceListener traceListener = new TextWriterTraceListener( Console.Out );
            traceListener.IndentSize = 3;

            Trace.Listeners.Add(traceListener);


            TestClass<string> test = new TestClass<string>();
            test.MethodWithGenericParameter( "ahoj" );
        }
    }
}
