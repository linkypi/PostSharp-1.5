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
using PostSharp.Samples.Trace;
using PostSharp.Samples.Trace.Test.FirstNamespace;
using PostSharp.Samples.Trace.Test.SecondNamespace;

[assembly: Trace( Category = "BaseCategory" )]
[assembly: Trace( AttributeTargetTypes = "PostSharp.Samples.Trace.Test.FirstNamespace.*",
    Category = "FirstCategory" )]
[assembly: Trace( AttributeTargetTypes = "PostSharp.Samples.Trace.Test.SecondNamespace.*",
    Category = "SecondNamespace", AttributePriority = 10 )]
[assembly: Trace( AttributeTargetTypes = "PostSharp.Samples.Trace.Test.SecondNamespace.*",
    AttributeTargetMembers = "*NotTrace", AttributeExclude = true, AttributePriority = 20 )]

namespace PostSharp.Samples.Trace.Test
{
    public static class Program
    {
        public static int Main( string[] args )
        {
            System.Diagnostics.Trace.Listeners.Add( new TextWriterTraceListener( Console.Out ) );
            FirstClass firstClass = new FirstClass();
            firstClass.TestVoid();
            firstClass.TestInt32();
            new FirstClass( 1 );
            new FirstClass( 1, 2 );
            try
            {
                firstClass.TestException();
            }
            catch
            {
                Console.WriteLine( "Catch." );
            }

            SecondClass.MethodNotTrace();
            SecondClass.SomeNonTracedMethod();
            SecondClass.SomeOtherCategory();
            SecondClass.SomeTracedMethod();
            ThirdClass.NotTraced();
            ThirdClass.Traced();
            return 0;
        }
    }
}