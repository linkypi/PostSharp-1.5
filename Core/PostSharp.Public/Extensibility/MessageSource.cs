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
using System.Globalization;
using System.Resources;
using PostSharp.CodeModel;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Provides commodify methods to work with an <see cref="IMessageSink"/>.
    /// </summary>
    public class MessageSource : MarshalByRefObject, IMessageSink
    {
        private static IMessageSink sink;
        private readonly ResourceManager resourceManager;
        private readonly string source;

        /// <summary>
        /// Initializes a new <see cref="MessageSource"/>.
        /// </summary>
        /// <param name="source">Name of the component emitting. the messages.</param>
        /// <param name="resourceManager">The <see cref="ResourceManager"/> that will be used to
        /// retrieve message texts.</param>
        public MessageSource( string source, ResourceManager resourceManager )
        {
            #region Preconditions

            if ( resourceManager == null )
            {
                throw new ArgumentNullException( "resourceManager" );
            }
            if ( string.IsNullOrEmpty( source ) )
            {
                throw new ArgumentNullException( "source" );
            }

            #endregion

            this.resourceManager = resourceManager;
            this.source = source;
        }

        /// <summary>
        /// Creates a <see cref="Message"/> object.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="arguments">Array of arguments used to format the message text,
        /// or <b>null</b> if this message has no argument.</param>
        /// <param name="locationFile">File that caused the error, or <b>null</b> if
        /// the file is unknown or does not apply.</param>
        /// <param name="locationLine"> Position (line) in the file that caused the error,
        /// or <see cref="Message.NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="locationColumn">Position (column) in the file that caused the error,
        /// or <see cref="Message.NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this message,
        /// or <b>null</b> if this message was not caused by an
        /// exception.</param>
        /// <returns>A <see cref="Message"/> object.</returns>
        public Message CreateMessage( SeverityType severity, string messageId, object[] arguments, string locationFile,
                                      int locationLine, int locationColumn, Exception innerException )
        {
            string messageTextFormattingString = resourceManager.GetString( messageId );
            string messageText;

            if ( messageTextFormattingString == null )
            {
                messageText = string.Format( CultureInfo.CurrentUICulture, "[{0}]. No message text found!.", messageId );
            }
            else if ( arguments == null )
            {
                messageText = messageTextFormattingString;
            }
            else
            {
                try
                {
                    messageText = string.Format( CultureInfo.CurrentUICulture, messageTextFormattingString, arguments );
                }
                catch ( FormatException e )
                {
                    throw new FormatException( "Cannot format the MessageText with id " +
                                               messageId + ".", e );
                }
            }

            string helpLinkFormattingString = resourceManager.GetString( messageId + "?" );
            string helpLink;

            if ( helpLinkFormattingString != null )
            {
                try
                {
                    helpLink = string.Format( CultureInfo.CurrentUICulture, helpLinkFormattingString, arguments );
                }
                catch ( FormatException e )
                {
                    throw new FormatException( "Cannot format the HelpLink with id " + messageId + ".", e );
                }
            }
            else
            {
                helpLink = null;
            }

            return new Message( severity, messageId, messageText, helpLink, this.source,
                                locationFile, locationLine, locationColumn, innerException );
        }

        /// <summary>
        /// Emits a <see cref="Message"/> and specifies all its properties.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="arguments">Array of arguments used to format the message text,
        /// or <b>null</b> if this message has no argument.</param>
        /// <param name="locationFile">File that caused the error, or <b>null</b> if
        /// the file is unknown or does not apply.</param>
        /// <param name="locationLine"> Position (line) in the file that caused the error,
        /// or <see cref="Message.NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="locationColumn">Position (column) in the file that caused the error,
        /// or <see cref="Message.NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this message,
        /// or <b>null</b> if this message was not caused by an
        /// exception.</param>
        public void Write( SeverityType severity, string messageId, object[] arguments, string locationFile,
                           int locationLine, int locationColumn, Exception innerException )
        {
            this.Write( this.CreateMessage( severity, messageId, arguments, locationFile,
                                            locationLine, locationColumn, innerException ) );
        }

        /// <summary>
        /// Emits a <see cref="Message"/> and specifies the source file name,
        /// line and column.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="locationFile">File that caused the error, or <b>null</b> if
        /// the file is unknown or does not apply.</param>
        /// <param name="locationLine"> Position (line) in the file that caused the error,
        /// or <see cref="Message.NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="locationColumn">Position (column) in the file that caused the error,
        /// or <see cref="Message.NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="arguments">Message arguments.</param>
        public void Write( SeverityType severity, string messageId, object[] arguments, string locationFile,
                           int locationLine, int locationColumn )
        {
            Write( severity, messageId, arguments, locationFile, locationLine, locationColumn, null );
        }


        /// <summary>
        /// Emits a <see cref="Message"/> and specifies the source file name and
        /// line.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="arguments">Array of arguments used to format the message text,
        /// or <b>null</b> if this message has no argument.</param>
        /// <param name="locationFile">File that caused the error, or <b>null</b> if
        /// the file is unknown or does not apply.</param>
        /// <param name="locationLine"> Position (line) in the file that caused the error,
        /// or <see cref="Message.NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        public void Write( SeverityType severity, string messageId,
                           object[] arguments, string locationFile, int locationLine )
        {
            Write( severity, messageId, arguments, locationFile, locationLine, Message.NotAvailable );
        }

        /// <summary>
        /// Emits a <see cref="Message"/> and specifies the source file name..
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="arguments">Array of arguments used to format the message text,
        /// or <b>null</b> if this message has no argument.</param>
        /// <param name="locationFile">File that caused the error, or <b>null</b> if
        /// the file is unknown or does not apply.</param>
        public void Write( SeverityType severity, string messageId,
                           object[] arguments, string locationFile )
        {
            Write( severity, messageId, arguments, locationFile, Message.NotAvailable, Message.NotAvailable );
        }

        /// <summary>
        /// Emits a <see cref="Message"/> without specifying the location of the error.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="arguments">Array of arguments used to format the message text,
        /// or <b>null</b> if this message has no argument.</param>
        public void Write( SeverityType severity, string messageId, object[] arguments )
        {
            Write( severity, messageId, arguments, null, Message.NotAvailable, Message.NotAvailable );
        }

        /// <summary>
        /// Emits a <see cref="Message"/> and specifies the error location using a <see cref="SymbolSequencePoint"/>.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="arguments">Array of arguments used to format the message text,
        /// or <b>null</b> if this message has no argument.</param>
        /// <param name="symbolSequencePoint">Location of the problem in source code..</param>
        public void Write( SeverityType severity, string messageId, object[] arguments,
                           SymbolSequencePoint symbolSequencePoint )
        {
            string fileName;
            int line;
            int column;


            if ( symbolSequencePoint != null )
            {
                line = symbolSequencePoint.StartLine;
                column = symbolSequencePoint.StartColumn;

                if ( symbolSequencePoint.Document != null )
                {
                    fileName = symbolSequencePoint.Document.URL;
                }
                else
                {
                    fileName = null;
                }
            }
            else
            {
                line = Message.NotAvailable;
                column = Message.NotAvailable;
                fileName = null;
            }

            this.Write( severity, messageId,
                        arguments,
                        fileName, line, column );
        }

        #region IMessageSink Members

        /// <inheritdoc />
        public void Write( Message message )
        {
            if ( sink == null )
                return;

            sink.Write( message );

            // If we have a fatal error, throw an exception.
            if ( message.Severity == SeverityType.Fatal )
            {
                throw new MessageException( message );
            }
        }

        #endregion

        public static void SetCurrentSink( IMessageSink sink )
        {
            MessageSource.sink = sink;
        }

        /// <summary>
        /// Gets the current message sink.
        /// </summary>
        public static IMessageSink MessageSink
        {
            get { return sink; }
        }
    }
}

#endif