using System;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Gives access to a remove <see cref="IMessageSink"/>.
    /// </summary>
    public sealed class MessageSinkAccessor : RemotingAccessor<IMessageSink>, IMessageSink
    {
        /// <summary>
        /// Initializes a new <see cref="MessageSinkAccessor"/>
        /// </summary>
        /// <param name="remoteSink">A message sink implementing <see cref="MarshalByRefObject"/>.</param>
        /// <param name="ownRemoteSink"><b>true</b> if <paramref name="remoteSink"/> should be disposed
        /// when the accessor is disposed, otherwise <b>false</b>.</param>
        public MessageSinkAccessor( IMessageSink remoteSink, bool ownRemoteSink )
            : base( remoteSink, ownRemoteSink )
        {
        }

        /// <inheritdoc />
        public void Write( Message message )
        {
            this.Value.Write( message );
        }
    }
}