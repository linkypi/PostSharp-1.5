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

namespace PostSharp.Samples.Composition
{
    /// <summary>
    /// Main class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        private static void Main()
        {
            IList<string> list = Post.Cast<SimpleList<string>, IList<string>>(
                new SimpleList<string>() );

            // Now I can use the list completely normally.
            list.Add( "dog" );
            list.Add( "cat" );
            list.Add( "cow" );

            foreach ( string item in list )
            {
                Console.WriteLine( "The collection contains the item {{{0}}}.", item );
            }
        }
    }
}