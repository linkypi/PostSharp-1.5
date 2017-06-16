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
	[Trace( AttributeExclude=true) ]
	class ThirdClass
	{
		public static void NotTraced()
		{
			System.Diagnostics.Trace.WriteLine( "NotTraced() should NOT be traced." );
		}

		[Trace]
		public static void Traced()
		{
			System.Diagnostics.Trace.WriteLine( "Traced() should be traced." );
		}



	}
}
