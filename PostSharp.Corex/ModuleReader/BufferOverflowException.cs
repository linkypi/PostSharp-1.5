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

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Exception thrown by the <see cref="BufferReader"/> class
    /// when one tries to read more bytes than the buffer 
    /// actually has.
    /// </summary>
    [Serializable]
    public class BufferOverflowException : ApplicationException
    {
        /// <summary>
        /// Initializes a new <see cref="BufferOverflowException"/>
        /// with the default message.
        /// </summary>
        public BufferOverflowException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BufferOverflowException"/> and
        /// specifies a message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public BufferOverflowException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BufferOverflowException"/> and specifies
        /// a message and an inner <see cref="Exception"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public BufferOverflowException( string message, Exception inner ) : base( message, inner )
        {
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        protected BufferOverflowException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
        }
    }
}