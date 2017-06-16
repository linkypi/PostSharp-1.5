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


using System.Collections.Generic;
using PostSharp.Reflection;

namespace PostSharp.Samples.Composition
{
    /// <summary>
    /// Simple list implemented by aggregation. We have added a <see cref="Name"/>
    /// property to demonstrate that we can of course add other semantics.
    /// </summary>
    [SimpleComposition( typeof(IList<GenericTypeArg0>), typeof(List<GenericTypeArg0>) )]
    internal class SimpleList<T>
    {
        private string collectionName;

        public SimpleList( string collectionName )
        {
            this.collectionName = collectionName;
        }

        public SimpleList()
            : this( null )
        {
        }

        public string Name { get { return collectionName; } set { collectionName = value; } }
    }
}