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
using System.Collections.Generic;
using System.Text;

namespace PostSharp.Samples.Trace.Test.FirstNamespace
{
	class FirstClass
	{
        public int someField;

		public FirstClass()
		{
			System.Diagnostics.Trace.WriteLine( "Constructor 1." );
		}

		public FirstClass( int a )
			: this()
		{
			try
			{
				System.Diagnostics.Trace.WriteLine( "Constructor 2." );
			}
			catch
			{
				System.Diagnostics.Trace.WriteLine( "Catch in Constructor 2." );
			}
		}

		public FirstClass( int a, int b )
			: this( a + b )
		{
			try
			{
				System.Diagnostics.Trace.WriteLine( "Constructor 3." );
			}
			catch
			{
				System.Diagnostics.Trace.WriteLine( "Catch in Constructor 3." );
			}
		}

		public void TestVoid()
		{
			System.Diagnostics.Trace.WriteLine( "TestVoid()" );
		}

		public int TestInt32()
		{
			System.Diagnostics.Trace.WriteLine( "TestInt32()" );
			return 50;
		}

		public void TestException()
		{
			System.Diagnostics.Trace.WriteLine( "TestException()" );
			throw new ApplicationException();
		}

        public void TestField()
        {
            this.someField++;
        }
	}
}
