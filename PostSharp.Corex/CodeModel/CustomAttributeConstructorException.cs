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
    /// Wraps any exception thrown during the construction of a custom attribute
    /// runtime instance.
    /// </summary>
    [Serializable]
    public class CustomAttributeConstructorException : ApplicationException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new <see cref="CustomAttributeConstructorException"/>
        /// </summary>
        public CustomAttributeConstructorException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CustomAttributeConstructorException"/> and specifies the exception message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public CustomAttributeConstructorException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CustomAttributeConstructorException"/> and specifies the error
        /// message and the inner exception.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public CustomAttributeConstructorException( string message, Exception inner ) : base( message, inner )
        {
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        protected CustomAttributeConstructorException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
        }
    }
}