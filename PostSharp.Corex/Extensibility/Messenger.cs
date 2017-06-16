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
using System.Collections.Generic;
using System.Threading;
using PostSharp.Collections;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Publish-subcribe channel for error messages (<see cref="Message"/>).
    /// </summary>
    /// <remarks>
    /// Each thread has its own instance of the <see cref="Messenger"/> class.
    /// Message emitters should use the <see cref="Messenger.Write"/> method, 
    /// consumers should susbcribe to the <see cref="Messenger.Message"/> event or
    /// implement the <see cref="IMessageSink"/> interface and
    /// register it using the <see cref="AddRemoteSink"/> method.
    /// </remarks>
    [Serializable]
    public sealed class Messenger : MarshalByRefObject, IMessageSink, IDisposable
    {
        #region Fields

        /// <summary>
        /// Instance attached to the AppDomain.
        /// </summary>
        private static Messenger instance;

        /// <summary>
        /// Current number of emitted errors.
        /// </summary>
        private int errorCount;

        /// <summary>
        /// Current number of emitted warnings.
        /// </summary>
        private int warningCount;

        /// <summary>
        /// Current number of emitted informations.
        /// </summary>
        private int infoCount;

        private int fatalCount;

        /// <summary>
        /// Maximal number of errors before a fatal error is emitted.
        /// </summary>
        private int maxErrorCount = 50;

        private readonly List<MessageSinkAccessor> remoteSinks = new List<MessageSinkAccessor>();

#if TRACE
        private readonly Guid guid = Guid.NewGuid();
#endif

        private readonly Set<string> disabledMessages = new Set<string>( 4, StringComparer.InvariantCultureIgnoreCase );

        private readonly Set<string> escalatedMessages = new Set<string>( 4, StringComparer.InvariantCultureIgnoreCase );

        #endregion

        private Messenger( bool setMessageSource )
        {
            if ( setMessageSource )
            {
                MessageSource.SetCurrentSink( this );
            }
        }

        static Messenger()
        {
            Initialize();
        }

        /// <summary>
        /// Forces to initialize the static <see cref="Messenger"/> instance.
        /// </summary>
        public static void Initialize()
        {
            if ( instance == null )
            {
                instance = new Messenger( true );
            }
        }


        /// <summary>
        /// Initializes a new <see cref="Messenger"/>.
        /// </summary>
        public Messenger() : this( false )
        {
        }

        /// <summary>
        /// Event raised when a message is emitted.
        /// </summary>
        public event EventHandler<MessageEventArgs> Message;

        /// <summary>
        /// Gets the <see cref="Messenger"/> associated with the current thread.
        /// </summary>
        public static Messenger Current
        {
            get { return instance; }

            internal set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                Trace.PostSharpObject.WriteLine( "In AppDomain {0}, setting the current messenger " +
                                                 " to an object that belongs to the AppDomain {1}.",
                                                 AppDomain.CurrentDomain.Id, value.GetAppDomainId() );

                instance = value;
                MessageSource.SetCurrentSink( instance );
            }
        }

        /// <summary>
        /// Gets the number of emitted fatal error messages.
        /// </summary>
        public int FatalCount
        {
            get { return this.fatalCount; }
        }

        /// <summary>
        /// Gets the current number of emitted error messages.
        /// </summary>
        public int ErrorCount
        {
            get { return this.errorCount; }
        }

        /// <summary>
        /// Gets the current number of emitted warning messages.
        /// </summary>
        public int WarningCount
        {
            get { return this.warningCount; }
        }

        /// <summary>
        /// Gets the current number of emitted info messages.
        /// </summary>
        public int InfoCount
        {
            get { return this.infoCount; }
        }

        /// <summary>
        /// Gets or sets the maximal number of errors before a fatal error
        /// is emitted.
        /// </summary>
        public int MaxErrorCount
        {
            get { return this.maxErrorCount; }
            set { this.maxErrorCount = value; }
        }

        /// <summary>
        /// Adds a remote message sink to the current <see cref="Messenger"/>.
        /// </summary>
        /// <param name="sink">A message sink that resides in another application domain.</param>
        /// <param name="ownSink"><b>true</b> if the current <see cref="Messenger"/> should
        /// dispose <paramref name="sink"/> when the <see cref="Messenger"/> is disposed, otherwise
        /// <b>false</b>.</param>
        /// <remarks>
        /// <para>All message sinks registered by this method shall be called when a message
        /// will be written into the current <see cref="Messenger"/>
        /// </para> 
        /// </remarks>
        public void AddRemoteSink( IMessageSink sink, bool ownSink )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sink, "sink" );

            #endregion

            this.remoteSinks.Add( new MessageSinkAccessor( sink, ownSink ) );
        }

        /// <summary>
        /// Removes a remote message sink from the current <see cref="Messenger"/>.
        /// </summary>
        /// <param name="sink">A message sink that resides in another application domain.</param>
        /// <returns><b>true</b> if <paramref name="sink"/> has been removed, otherwise <b>false</b>.</returns>
        public bool RemoveRemoteSink( IMessageSink sink )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sink, "sink" );

            #endregion

            MessageSinkAccessor accessor = null;
            foreach ( MessageSinkAccessor candidate in this.remoteSinks )
            {
                if ( candidate.Value == sink )
                {
                    accessor = candidate;
                    break;
                }
            }

            if ( accessor != null )
            {
                this.remoteSinks.Remove( accessor );
                accessor.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disables a message given its identifier.
        /// </summary>
        /// <param name="messageId">Identifier of the message to be disabled.</param>
        /// <remarks>
        /// Disabling a message means ignoring it. You can disable only information and warning messages, 
        /// not errors.
        /// </remarks>
        public void DisableMessage( string messageId )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( messageId, "messageId" );

            #endregion

            this.disabledMessages.AddIfAbsent( messageId );
        }

        /// <summary>
        /// Escalates a message, given its identifier.
        /// </summary>
        /// <param name="messageId">Identifier of the message to be escalated.</param>
        /// <remarks>
        /// Disabling a message means changing its severify to <b>Error</b>.
        /// </remarks>
        public void EscalateMessage( string messageId )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( messageId, "messageId" );

            #endregion

            this.escalatedMessages.AddIfAbsent( messageId );
        }


        /// <summary>
        /// Emits a message.
        /// </summary>
        /// <param name="message">A <see cref="Message"/>.</param>
        /// <exception cref="MessageException">The message severity is <see cref="SeverityType.Fatal"/>.</exception>
        public void Write( Message message )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( message, "message" );

            #endregion

            if ( Trace.Messenger.Enabled )
            {
#if TRACE
                Trace.Messenger.WriteLine( "Messenger {0} received: {1}.",
                                           this.guid, message.MessageText );
#endif
            }

            // Skip the message if it was disabled.
            if ( message.Severity != SeverityType.Error &&
                 message.Severity != SeverityType.Fatal )
            {
                if ( this.disabledMessages.Contains( message.MessageId ) )
                    return;
            }

            // Escalate the message severity if requested.
            if ( this.escalatedMessages.Contains( message.MessageId ) &&
                 message.Severity != SeverityType.Fatal )
            {
                message.Severity = SeverityType.Error;
            }

            // Propagate the message to sinks.
            if ( this.Message != null || this.remoteSinks.Count > 0 )
            {
                MessageEventArgs args = new MessageEventArgs( message );
                if ( this.Message != null )
                {
                    this.Message( this, args );
                }

                foreach ( MessageSinkAccessor sink in this.remoteSinks )
                {
                    sink.Write( message );
                }
            }

            // Increment message counters.
            switch ( message.Severity )
            {
                case SeverityType.Info:
                    Interlocked.Increment( ref this.infoCount );
                    break;

                case SeverityType.Warning:
                    Interlocked.Increment( ref this.warningCount );
                    break;

                case SeverityType.Error:
                    Interlocked.Increment( ref this.errorCount );
                    if ( this.errorCount >= this.maxErrorCount )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0016", new object[] {this.errorCount} );
                    }
                    break;

                case SeverityType.Fatal:
                    Interlocked.Increment( ref this.fatalCount );
                    break;
            }
        }

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            // Dispose the message sink accessors.
            foreach ( MessageSinkAccessor sink in this.remoteSinks )
            {
                sink.Dispose();
            }
        }

        #endregion

        internal int GetAppDomainId()
        {
            return AppDomain.CurrentDomain.Id;
        }
    }

    /// <summary>
    /// Arguments of the <see cref="Messenger.Message"/> event.
    /// </summary>
    [Serializable]
    public sealed class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Message.
        /// </summary>
        private readonly Message message;

        /// <summary>
        /// Initializes a new <see cref="MessageEventArgs"/>.
        /// </summary>
        /// <param name="message">A <see cref="Message"/>.</param>
        public MessageEventArgs( Message message )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( message, "message" );

            #endregion

            this.message = message;
        }

        /// <summary>
        /// Gets the <see cref="Message"/> signaled by the event.
        /// </summary>
        public Message Message
        {
            get { return message; }
        }
    }
}