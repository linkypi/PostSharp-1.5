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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Exception embedding a <see cref="Message"/>.
    /// </summary>
    [Serializable]
    [SuppressMessage( "Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors" )]
    public sealed class MessageException : Exception
    {
        /// <summary>
        /// The <see cref="Message"/>.
        /// </summary>
        private readonly Message message;

        /// <summary>
        /// Initializes a new <see cref="MessageException"/> from
        /// an existing <see cref="Message"/>.
        /// </summary>
        /// <param name="message">A <see cref="Message"/>.</param>
        [SuppressMessage( "Microsoft.Usage", "CA221:DoNotCallOverridableMethodsInConstructor" )]
        public MessageException( Message message ) : base( GetMessageText( message ) )
        {
            #region Preconditions

            if ( message == null )
            {
                throw new ArgumentNullException( "message" );
            }

            #endregion

            this.message = message;
            this.Source = message.Source;
            this.HelpLink = message.HelpLink;
        }

        /// <summary>
        /// Deserializes a <see cref="MessageException"/>.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        private MessageException(
            SerializationInfo info,
            StreamingContext context )
            : base( info, context )
        {
            this.message = (Message) info.GetValue( "mo", typeof(Message) );
        }


        /// <summary>
        /// Serializes the current object.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        [SecurityPermission( SecurityAction.Demand, SerializationFormatter = true )]
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            #region Preconditions

            if ( info == null )
            {
                throw new ArgumentNullException( "info" );
            }

            #endregion

            base.GetObjectData( info, context );
            info.AddValue( "mo", this.message );
        }

        /// <summary>
        /// Gets the <see cref="Message"/> em
        /// </summary>
        public Message MessageObject
        {
            get { return this.message; }
        }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        /// <param name="message">A <see cref="Message"/>.</param>
        /// <returns>The message text.</returns>
        private static string GetMessageText( Message message )
        {
            if ( message == null )
            {
                return null;
            }
            else
            {
                return message.MessageText;
            }
        }
    }
}

#endif