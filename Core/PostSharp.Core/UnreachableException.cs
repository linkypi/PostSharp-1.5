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

using System;
using System.Runtime.Serialization;

namespace PostSharp
{
    /// <summary>
    /// Formal exception thrown after calling a method that always results in an exception,
    /// so that static analyzers do not complain.
    /// </summary>
    [Serializable]
    public class UnreachableException : Exception
    {
        /// <summary>
        /// Initializes an <see cref="UnreachableException"/> with default message.
        /// </summary>
        public UnreachableException()
        {
        }

        /// <summary>
        /// Initializes an <see cref="UnreachableException"/> with a given message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public UnreachableException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes an <see cref="UnreachableException"/> with a given message and inner <see cref="Exception"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public UnreachableException( string message, Exception inner ) : base( message, inner )
        {
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected UnreachableException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
        }
    }
}