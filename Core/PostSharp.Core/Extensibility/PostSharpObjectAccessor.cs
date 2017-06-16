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

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Accessor of remote <see cref="PostSharpObject"/> instances. Manage timelife.
    /// </summary>
    internal class PostSharpObjectAccessor : RemotingAccessor<IPostSharpObject>, IPostSharpObject
    {
        /// <summary>
        /// Initializes a new <see cref="PostSharpObjectAccessor"/>.
        /// </summary>
        /// <param name="postSharpObject">The remote <see cref="PostSharpObject"/>.</param>
        public PostSharpObjectAccessor(IPostSharpObject postSharpObject)
            : base( postSharpObject, true )
        {
        }

        /// <inheritdoc />
        public void InvokeProject(ProjectInvocation projectInvocation)
        {
            this.Value.InvokeProject(projectInvocation);
        }

        /// <inheritdoc />
        public void InvokeProjects( ProjectInvocation[] projectInvocations )
        {
            this.Value.InvokeProjects( projectInvocations );
        }

        /// <inheritdoc />
        public void ProcessAssemblies(ModuleLoadStrategy[] modules)
        {
            this.Value.ProcessAssemblies( modules);
        }

        public AppDomain AppDomain { get { return this.Value.AppDomain; } }

        /// <inheritdoc />
        protected override void Dispose( bool disposing )
        {
            AppDomain appDomain = this.AppDomain;

            base.Dispose( disposing );

            if ( disposing )
            {
                Trace.PostSharpObject.WriteLine( "Unloading the AppDomain {0}.", appDomain.Id);
                AppDomain.Unload(appDomain);
            }
        }
    }
}