#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

#region Using directives

using System;
using System.Runtime.Serialization;

#endregion

namespace PostSharp
{
    /// <summary>
    /// This exception is thrown when an internal assertion failed.
    /// </summary>
    [Serializable]
    public class AssertionFailedException : Exception
    {
        /// <summary>
        /// Initializes a new <see cref="AssertionFailedException"/> with no message.
        /// </summary>
        public AssertionFailedException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="AssertionFailedException"/> exception with a message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public AssertionFailedException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="AssertionFailedException"/> exception with
        /// a message and an inner exception.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public AssertionFailedException( string message, Exception inner ) : base( message, inner )
        {
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Context.</param>
        protected AssertionFailedException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
        }
    }
}