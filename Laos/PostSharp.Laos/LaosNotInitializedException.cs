#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

using System;
using System.Diagnostics;
using System.Text;
#if !SMALL
using System.Runtime.Serialization;

#endif

namespace PostSharp.Laos
{
    /// <summary>
    /// Exception raised typically when a woven program requires PostSharp Laos to be
    /// initialized, but it was not yet initialized. This situation typically occurs
    /// when an aspect has a static constructor that has aspects on it or calls (directly
    /// or not) an aspected method.
    /// </summary>
#if !SMALL
    [Serializable]
#endif
    public class LaosNotInitializedException : Exception
    {
        /// <summary>
        /// Initializes a new <see cref="LaosNotInitializedException"/>.
        /// </summary>
        private LaosNotInitializedException() :
            base( "PostSharp Laos aspects are not yet initialized. " +
                  "Your assembly probably contains aspects whose static constructor or other " +
                  "methods called by the static constructor are themselves woven." )

        {
        }

        /// <summary>
        /// Throws a <see cref="LaosNotInitializedException"/> and write some
        /// warnings to the debugger.
        /// </summary>
        public static void Throw()
        {
            LaosNotInitializedException e = new LaosNotInitializedException();

#if !CF
            if ( Debugger.IsLogging() )
            {
                StringBuilder message = new StringBuilder();
                message.Append( "PostSharp Laos Is Not Initialized" + Environment.NewLine );
                message.Append( "---------------------------------" + Environment.NewLine );
                message.Append( e.Message + Environment.NewLine );
                Debugger.Log( 1, "PostSharp", message.ToString() );
            }
#endif

            throw e;
        }


#if !SMALL
        /// <summary>
        /// Initializes a <see cref="LaosNotInitializedException"/> from serialized
        /// data.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected LaosNotInitializedException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
        }
#endif
    }
}