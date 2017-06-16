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
using PostSharp.Extensibility;

namespace PostSharp.Samples.Trace
{
    /// <summary>
    /// When applied on a method, specifies that the method entry and exit should be traced.
    /// </summary>
    /// <remarks>
    /// <para>If you use this attribute, be sure to import the <b>PostSharp.Samples.Trace</b>
    /// plug-in in your PostSharp project.</para>
    /// <para>This attribute will trace using the <see cref="System.Diagnostics.Trace"/> object.</para>
    /// </remarks>
	[AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Module |
		AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method 
		| AttributeTargets.Constructor,
		AllowMultiple = true, Inherited = false )]
    [MulticastAttributeUsage( MulticastTargets.Method | MulticastTargets.InstanceConstructor | MulticastTargets.StaticConstructor, AllowMultiple=true )]
    [RequirePostSharp("PostSharp.Samples.Trace", "PostSharp.Samples.Trace")]
	public sealed class TraceAttribute : MulticastAttribute
	{
		string category = null;

        /// <summary>
        /// Message category.
        /// </summary>
		public string Category
		{
			get { return category; }
			set { category = value; }
		}


        
    }
}
