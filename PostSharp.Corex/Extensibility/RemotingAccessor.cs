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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Automatically manages the lifetime of a remote object.
    /// </summary>
    /// <typeparam name="T">Type of the remove object.</typeparam>
    /// <remarks>
    /// This type automates the use of <see cref="ClientSponsor"/> for
    /// remote objects. It starts sponsoring the remove object when the
    /// <see cref="RemotingAccessor{T}"/> instance is created, and stops 
    /// sponsoring when the <see cref="RemotingAccessor{T}"/> is disposed or
    /// finalized. It helps implementing consistent remote sponsoring
    /// without effort.
    /// </remarks>
    public class RemotingAccessor<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Sponsor of all instances.
        /// </summary>
        private static readonly ClientSponsor sponsor = new ClientSponsor( TimeSpan.FromMinutes( 5 ) );

        /// <summary>
        /// The remote object, or <b>null</b> if the instance has been disposed.
        /// </summary>
        private T remoteObject;

        private readonly int hashCode;

        private readonly Guid guid;

        private readonly bool ownsRemoteObject;

        /// <summary>
        /// Initializes a new <see cref="RemotingAccessor{T}"/>.
        /// </summary>
        /// <param name="remoteObject">An existing remote object (marshalle by reference).</param>
        /// <param name="ownsRemoteObject"><b>true</b> if the remote object should be disposed
        /// when the accessor is disposed, otherwise <b>false</b>.</param>
        public RemotingAccessor( T remoteObject, bool ownsRemoteObject )
        {
            MarshalByRefObject marshalByRefObject = remoteObject as MarshalByRefObject;

            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( remoteObject, "remoteObject" );
            if ( marshalByRefObject == null )
            {
                throw new ArgumentException( "The object should be a MarshalByRefObject.", "remoteObject" );
            }

            #endregion

            this.guid = Guid.NewGuid();

            Trace.Remoting.WriteLine( "Creating the {0} with guid {1}.",
                                      this.GetType(), guid );

            sponsor.Register( marshalByRefObject );
            this.remoteObject = remoteObject;
            this.hashCode = remoteObject.GetHashCode();
            this.ownsRemoteObject = ownsRemoteObject;
        }

        /// <summary>
        /// Gets the remote object.
        /// </summary>
        public T Value
        {
            get
            {
                this.AssertNotDisposed();
                return this.remoteObject;
            }
        }

        /// <summary>
        /// Determines whether the current instance has been disposed.
        /// </summary>
        public bool IsDisposed { get { return this.remoteObject == null; } }

        /// <summary>
        /// Throws an exception if the current instance has already been disposed.
        /// </summary>
        protected void AssertNotDisposed()
        {
            if ( remoteObject == null )
            {
                throw new ObjectDisposedException( this.GetType().FullName );
            }
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        /// <param name="disposing"><b>false</b> if the method is called by the
        /// destructor, otherwise <b>true</b>.</param>
        protected virtual void Dispose( bool disposing )
        {
#if TRACE
            if ( disposing )
            {
                Trace.Remoting.WriteLine( "Disposing the {0} with guid {1}.", this.GetType(), this.guid );
            }
            else
            {
                Trace.Remoting.WriteLine( "Finalizing the {0} with guid {1}. Did you forget to keep a reference " +
                                          "of the accessor and/or to dispose it?", this.GetType(), this.guid );
            }
#endif
            // Dispose the remote object.
            if ( this.ownsRemoteObject )
            {
                IDisposable remoteDisposable = this.Value as IDisposable;
                if ( remoteDisposable != null )
                {
                    remoteDisposable.Dispose();
                }
            }

            try
            {
                sponsor.Unregister( (MarshalByRefObject) (object) remoteObject );
            }
            catch ( RemotingException )
            {
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.remoteObject != null )
            {
                this.Dispose( true );
                this.remoteObject = null;
                GC.SuppressFinalize( this );
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~RemotingAccessor()
        {
            this.Dispose( false );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            this.AssertNotDisposed();
            return this.hashCode;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            this.AssertNotDisposed();
            return this.Value.ToString();
        }
    }
}
