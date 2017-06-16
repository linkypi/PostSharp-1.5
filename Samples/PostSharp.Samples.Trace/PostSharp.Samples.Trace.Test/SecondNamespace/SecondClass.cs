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
using PostSharp.Samples.Trace;

namespace PostSharp.Samples.Trace.Test.SecondNamespace
{
	class SecondClass
	{
		public static void SomeTracedMethod()
		{
			System.Diagnostics.Trace.WriteLine( "Should be traced." );
		}

		[Trace( AttributeExclude=true )]
		public static void SomeNonTracedMethod()
		{
			System.Diagnostics.Trace.WriteLine( "SomeNonTracedMethod should NOT be traced." );
		}

		[Trace( Category="OtherCategory" )]
		public static void SomeOtherCategory()
		{
			System.Diagnostics.Trace.WriteLine( "SomeOtherCategory should have the category 'OtherCategory'." );
		}

		public static void MethodNotTrace()
		{
			System.Diagnostics.Trace.WriteLine( "MethodNotTrace should NOT be traced." );
		}


	}
}
