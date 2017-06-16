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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Exception thrown when a binding exception occurs in PostSharp.
    /// </summary>
    [Serializable]
    public class BindingException : Exception
    {
        /// <summary>
        /// Initializes a new <see cref="BindingException"/>.
        /// </summary>
        public BindingException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BindingException"/> and defines the message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public BindingException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BindingException"/> and defines the message and the inner exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inner">Inner exception.</param>
        public BindingException( string message, Exception inner ) : base( message, inner )
        {
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        protected BindingException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
        }
    }
}