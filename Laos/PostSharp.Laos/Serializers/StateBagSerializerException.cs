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

#if !SMALL
using System;
using System.Runtime.Serialization;

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// Exception thrown by the <see cref="StateBagSerializer"/>.
    /// </summary>
    [Serializable]
    public class StateBagSerializerException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new <see cref="StateBagSerializerException"/> with the default message.
        /// </summary>
        public StateBagSerializerException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="StateBagSerializerException"/> with a given message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public StateBagSerializerException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="StateBagSerializerException"/> with a given message and an inner exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inner">Inner exception.</param>
        public StateBagSerializerException( string message, Exception inner ) : base( message, inner )
        {
        }

        /// <summary>
        /// Deserializes a <see cref="StateBagSerializerException"/>.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        protected StateBagSerializerException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
        }
    }
}

#endif