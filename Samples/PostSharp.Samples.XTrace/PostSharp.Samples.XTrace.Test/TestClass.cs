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
using System.Threading;
using PostSharp.Samples.XTrace;

[assembly: XTraceMethodInvocation(
    AttributeTargetAssemblies = "mscorlib",
    AttributeTargetTypes = "System.Threading.*" )]
[assembly: XTraceMethodBoundary(
    AttributeTargetAssemblies = "PostSharp.Samples.XTrace.Test",
    AttributeTargetTypes = "PostSharp.Samples.XTrace.Test.*" )]

namespace PostSharp.Samples.XTrace.Test
{
    internal class TestClass<TType>
    {
        [XTraceFieldAccess] private static int field1 = 10;
        [XTraceFieldAccess] private TType field2;

        public void TestWithInsteadOf( TType someValue )
        {
            for ( int i = 0 ; i < 5 ; i++ )
            {
                TestInsteadOfCall<int>( i, someValue );

                switch ( i )
                {
                    case 1:
                        Console.WriteLine( "A" );
                        break;

                    case 2:
                        Console.WriteLine( "B" );
                        break;

                    case 3:
                        Console.WriteLine( "C" );
                        break;
                }
            }
        }


        public class SomeClass
        {
            [XTraceMethodBoundary( AttributeReplace = true )]
            public event EventHandler SomeEvent;
        }

        public TType Field2 { get { return field2; } set { field2 = value; } }


        public void MethodWithGenericParameter( TType someValue )
        {
            field1 = field1 + 5;
            TType loc = field2;
            field2 = someValue;
            Console.WriteLine( "field2 = {0}", this.field2 );

            try
            {
                MyMethodWithAround( "Hello, world.", true );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }


            MyMethodWithAround( "Hello, world.", false );


            try
            {
                MyMethodWithFail( "Hello, world.", true );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }

            MyMethodWithAroundAndFail( "Hello, world.", false );

            try
            {
                MyMethodWithAroundAndFail( "Hello, world.", true );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }

            MyMethodWithFail( "Hello, world.", false );

            this.TestWithInsteadOf( someValue );

            Thread.Sleep( 0 );
        }

        [XTraceMethodBoundary( AttributeReplace=true )]
        private static string MyMethodWithAround( string arg, bool fail )
        {
            if ( fail )
                throw new Exception();
            return arg;
        }

        [XTraceException]
        private static string MyMethodWithFail( string arg, bool fail )
        {
            Console.WriteLine( "MyMethodWithFail( fail = {0} )", fail );
            if ( fail )
                throw new Exception();

            return arg;
        }

        [XTraceMethodBoundary( AttributeReplace = true )]
        private int MyPropertyWithAround
        {
            get
            {
                Console.WriteLine( "get MyPropertyWithAround" );
                return 0;
            }
            set { Console.WriteLine( "set MyPropertyWithAround = {0}.", value ); }
        }


        [XTraceMethodBoundary( AttributeReplace = true )]
        [XTraceException( typeof(InvalidProgramException) )]
        private static string MyMethodWithAroundAndFail( string arg, bool fail )
        {
            if ( fail )
                throw new InvalidProgramException( "You have an invalid program, dude!" );
            return arg;
        }

        private void MethodWithOutParameters( ref PlatformID a, out PlatformID b )
        {
            b = a;
        }

        [XTraceMethodInvocation]
        public static void TestInsteadOfCall<TMethod>( TMethod arg0, TType arg1 )
        {
            Console.WriteLine( "Inside of TestInsteadOfCall( {{{0}}}, {{{1}}} ).", arg0, arg1 );
        }
    }

    internal delegate void TestInsteadOfCallDelegate<TType, TMethod>( TMethod arg0, TType arg1 );

    internal class A
    {
        public void Test<T0, T1>()
        {
            TestInsteadOfCallDelegate<T0, T1> del =
                new TestInsteadOfCallDelegate<T0, T1>( TestClass<T0>.TestInsteadOfCall<T1> );
        }
    }
}