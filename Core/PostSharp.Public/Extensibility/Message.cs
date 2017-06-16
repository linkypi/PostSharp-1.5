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
using System.Text;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Encapsulates a message (error, warning, info, ...).
    /// </summary>
    [Serializable]
    public sealed class Message
    {
        #region Fields

        /// <summary>
        /// When set to the <see cref="LocationLine"/> or the
        /// <see cref="LocationColumn"/> property, means that
        /// the value of this property is unknown.
        /// </summary>
        public const int NotAvailable = 0;

        /// <summary>
        /// Message severity.
        /// </summary>
        private SeverityType severity;

        /// <summary>
        /// Identifier of the message type. Key to retrieve
        /// the full text of the message in the resource file.
        /// </summary>
        private readonly string messageId;

        /// <summary>
        /// File that caused the error, or <b>null</b> if
        /// the file is unknown or does not apply.
        /// </summary>
        private readonly string locationFile;

        /// <summary>
        /// Position (line) in the file that caused the error,
        /// or <see cref="NotAvailable"/> if the line is 
        /// unknown or does not apply.
        /// </summary>
        private readonly int locationLine;

        /// <summary>
        /// Position (column) in the file that caused the error,
        /// or <see cref="NotAvailable"/> if the line is 
        /// unknown or does not apply.
        /// </summary>
        private readonly int locationColumn;

        /// <summary>
        /// The <see cref="Exception"/> that caused this message,
        /// or <b>null</b> if this message was not caused by an
        /// exception.
        /// </summary>
        private readonly Exception innerException;

        private readonly string source;

        private readonly string messageText;

        private readonly string helpLink;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="Message"/> and specifies only required parameters.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="source">Name of the component emitting the message.</param>
        /// <param name="messageText">Fully formatted message text.</param>
        public Message( SeverityType severity, string messageId,
                        string messageText, string source )
            : this( severity, messageId, messageText, null, source, null,
                    NotAvailable, NotAvailable, null )
        {
        }


        /// <summary>
        /// Initializes a new <see cref="Message"/> and specifies all its properties.
        /// </summary>
        /// <param name="severity">Message severify (fatal error, error, info, debug).</param>
        /// <param name="messageId">Identifier of the message type.</param>
        /// <param name="locationFile">File that caused the error, or <b>null</b> if
        /// the file is unknown or does not apply.</param>
        /// <param name="locationLine"> Position (line) in the file that caused the error,
        /// or <see cref="NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="locationColumn">Position (column) in the file that caused the error,
        /// or <see cref="NotAvailable"/> if the line is 
        /// unknown or does not apply.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this message,
        /// or <b>null</b> if this message was not caused by an
        /// exception.</param>
        /// <param name="source">Name of the component emitting the message.</param>
        /// <param name="helpLink">Link to the help file page associated to this message.</param>
        /// <param name="messageText">Fully formatted message text.</param>
        public Message( SeverityType severity, string messageId,
                        string messageText, string helpLink, string source,
                        string locationFile, int locationLine, int locationColumn, Exception innerException )
        {
            #region Preconditions

            if ( string.IsNullOrEmpty( messageId ) )
            {
                throw new ArgumentNullException( "messageId" );
            }

            if ( string.IsNullOrEmpty( messageText ) )
            {
                throw new ArgumentNullException( "messageText" );
            }

            if ( string.IsNullOrEmpty( source ) )
            {
                throw new ArgumentNullException( "source" );
            }

            #endregion

            this.source = source;
            this.severity = severity;
            this.messageId = messageId;
            this.locationFile = locationFile;
            this.locationLine = locationLine;
            this.locationColumn = locationColumn;
            this.innerException = innerException;
            this.helpLink = helpLink;
            this.messageText = messageText;
        }

        #region Properties

        /// <summary>
        /// Gets the message severity.
        /// </summary>
        public SeverityType Severity
        {
            get { return severity; }
            set { this.severity = value; }
        }

        /// <summary>
        /// Gets the message type identifier.
        /// </summary>
        public string MessageId
        {
            get { return messageId; }
        }

        /// <summary>
        /// Gets the name of the file that caused
        /// the message.
        /// </summary>
        /// <value>
        /// A full file name, or <b>null</b> if the file name
        /// is unknown or not applicable.
        /// </value>
        public string LocationFile
        {
            get { return locationFile; }
        }

        /// <summary>
        /// Gets the line in the file that caused the
        /// message.
        /// </summary>
        /// <summary>
        /// An integer greater or equal to 1, or <see cref="NotAvailable"/>
        /// if the line is unknown or does not apply.
        /// </summary>
        public int LocationLine
        {
            get { return locationLine; }
        }

        /// <summary>
        /// Gets the column in the file that caused the
        /// message.
        /// </summary>
        /// <summary>
        /// An integer greater or equal to 1, or <see cref="NotAvailable"/>
        /// if the column is unknown or does not apply.
        /// </summary>
        public int LocationColumn
        {
            get { return locationColumn; }
        }

        /// <summary>
        /// Gets the
        /// </summary>
        public Exception InnerException
        {
            get { return this.innerException; }
        }

        /// <summary>
        /// Gets or sets the name of the source component.
        /// </summary>
        public string Source
        {
            get { return this.source; }
        }

        /// <summary>
        /// Gets the message formatted text.
        /// </summary>
        public string MessageText
        {
            get { return this.messageText; }
        }

        /// <summary>
        /// Gets the help link.
        /// </summary>
        public string HelpLink
        {
            get { return this.helpLink; }
        }

        #endregion

        /// <summary>
        /// Returns a string composed of the messages of
        /// all inner exceptions.
        /// </summary>
        /// <param name="outerException">The outer exception.</param>
        /// <returns>A string composed of the mesages of all
        /// <paramref name="outerException"/> and all inner exceptions,
        /// concatenated by the string <c>--&gt;</c>.</returns>
        public static string GetExceptionStackMessage( Exception outerException )
        {
            #region Preconditions

            if ( outerException == null )
            {
                throw new ArgumentNullException( "outerException" );
            }

            #endregion

            Exception cursor = outerException;
            StringBuilder builder = new StringBuilder();
            while ( cursor != null )
            {
                if ( builder.Length > 0 )
                {
                    builder.Append( " --> " );
                }
                builder.Append( cursor.Message );
                cursor = cursor.InnerException;
            }

            return builder.ToString();
        }
    }


    /// <summary>
    /// Types of message severities.
    /// </summary>
    public enum SeverityType
    {
        /// <summary>
        /// Debugging information (typically trace).
        /// </summary>
        Debug,

        /// <summary>
        /// Verbose (lowly important information).
        /// </summary>
        Verbose,

        /// <summary>
        /// Information.
        /// </summary>
        Info,

        /// <summary>
        /// Important information.
        /// </summary>
        ImportantInfo,

        /// <summary>
        /// Command line.
        /// </summary>
        CommandLine,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Error.
        /// </summary>
        Error,

        /// <summary>
        /// Fatal error.
        /// </summary>
        Fatal
    }
}

#endif