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
using System.Diagnostics;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Object that stands between the <see cref="PostSharpObject"/> and the remote host 
    /// (<see cref="IPostSharpHost"/>). Lays in the PostSharp AppDomain.
    /// Typically derived by a host when some processings have to be done in the PostSharp AppDomain
    /// instead of the host AppDomain.
    /// </summary>
    /// <remarks>
    /// The default implementation of all methods just calls the remote host.
    /// </remarks>
    /// <note>
    /// Derived classes should have a default constructor.
    /// </note>
    public class PostSharpLocalHost : MarshalByRefObject, IDisposable
    {
        private RemotingAccessor<IPostSharpHost> remoteHost;
        private PostSharpObject postSharpObject;

        /// <summary>
        /// Gets the remote host (i.e. the <see cref="IPostSharpHost"/> located in the
        /// host AppDomain) associated to to the current instance.
        /// </summary>
        public IPostSharpHost RemoteHost { get { return this.remoteHost.Value; } }

        /// <summary>
        /// Gets the <see cref="PostSharpObject"/> associated to the current instance.
        /// </summary>
        public PostSharpObject PostSharpObject { get { return this.postSharpObject; } }

        /// <summary>
        /// Initializes the current object and call the initialization method of derived
        /// classes,
        /// </summary>
        /// <param name="postSharpObject">The <see cref="PostSharpObject"/> to which the
        /// current instance is associated.</param>
        /// <param name="remoteHost">The <see cref="IPostSharpHost"/> to which the
        /// current instance is associated.</param>
        internal void InternalInitialize( PostSharpObject postSharpObject, IPostSharpHost remoteHost )
        {
            this.remoteHost = new RemotingAccessor<IPostSharpHost>( remoteHost, false );
            this.postSharpObject = postSharpObject;

            this.Initialize();
        }

        /// <summary>
        /// Initializes the current instance. 
        /// </summary>
        /// <remarks>
        /// This method is called just after the object is instantiated and the properties
        /// <see cref="RemoteHost"/> and <see cref="PostSharpObject"/> have been initialized.
        /// </remarks>
        public virtual void Initialize()
        {
        }


        /// <summary>
        /// Returns the location and the loag arguments of an assembly known by name.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <returns>An <see cref="ModuleLoadStrategy"/> object containing principally the
        /// location of the assembly to load, or <b>null</b> if the host does not want
        /// to intervene in the default binding mechanism of the PostSharp Platform.</returns>
        /// <remarks>
        /// This method is called whenever an assembly reference has to be resolved inside the
        /// PostSharp Code Object Model, and this assembly has neither been resolved yet neither
        /// been passed to the <see cref="IPostSharpObject.InvokeProjects"/> method.
        /// </remarks>
        public virtual string ResolveAssemblyReference( IAssemblyName assemblyName )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( assemblyName, "assemblyName" );
            this.AssertNotDisposed();

            #endregion

            return this.RemoteHost.ResolveAssemblyReference( new AssemblyName( assemblyName.FullName ) );
        }

        /// <summary>
        /// Determines whether and how a module should be processed by PostSharp.
        /// </summary>
        /// <param name="module">Module that is being considered.</param>
        /// <returns>A <see cref="ProjectInvocationParameters"/> object if this module has to be
        /// processed, or <b>null</b> if the module should not be processed.</returns>
        public virtual ProjectInvocationParameters GetProjectInvocationParameters( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );
            this.AssertNotDisposed();

            #endregion

            return this.RemoteHost.GetProjectInvocationParameters(
                module.Assembly.GetSystemAssembly().Location,
                module.Name,
                new AssemblyName( module.Assembly.FullName ) );
        }

        /// <summary>
        /// Notifies the host that an assembly has been renamed. This happens if the 
        /// <see cref="PostSharpObjectSettings"/>.<see cref="PostSharpObjectSettings.OverwriteAssemblyNames"/>
        /// flag has been set.
        /// </summary>
        /// <param name="assemblyManifest">The assembly manifest where the 
        /// <see cref="AssemblyManifestDeclaration.OverwrittenName"/> is different than
        /// the <b>Name</b> property.</param>
        /// <remarks>
        /// The assembly renaming is used in the runtime usage scenario in order to (1) remove
        /// strong names and (2) make sure that the unprocessed assembly is not loaded 'accidentally'
        /// by the default system binder (for instance because the transformed assembly lays in the GAC).
        /// See <see cref="PostSharpObjectSettings.OverwriteAssemblyNames"/> for details.
        /// </remarks>
        public virtual void RenameAssembly( AssemblyManifestDeclaration assemblyManifest )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( assemblyManifest, "assemblyManifest" );
            this.AssertNotDisposed();

            #endregion

            this.RemoteHost.RenameAssembly( new AssemblyName( assemblyManifest.FullName ),
                                            new AssemblyName( assemblyManifest.OverwrittenFullName ) );
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Determines whether the current instance has been disposed.
        /// </summary>
        public bool IsDisposed { get { return this.remoteHost.IsDisposed; } }

        /// <summary>
        /// Throws an exception if the current object has been disposed.
        /// </summary>
        [Conditional( "ASSERT" )]
        private void AssertNotDisposed()
        {
            if ( this.IsDisposed )
            {
                throw new ObjectDisposedException( "PostSharpLocalHost" );
            }
        }

        /// <summary>
        /// Disposes the current object.
        /// </summary>
        /// <param name="disposing"><b>true</b> if the current method is called because the object
        /// is being disposed explicitely, <b>false</b> if it is called because of the
        /// destructor.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this.remoteHost.Dispose();
            }
        }

        /// <summary>
        /// Frees the resources consumed by the current object.
        /// </summary>
        public void Dispose()
        {
            if ( !this.IsDisposed )
            {
                this.Dispose( true );
                GC.SuppressFinalize( this );
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~PostSharpLocalHost()
        {
            if ( !this.IsDisposed )
            {
                this.Dispose( false );
            }
        }

        #endregion
    }
}