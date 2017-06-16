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
using PostSharp.CodeModel;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Defines the semantics of the <b>PostSharp Object</b>, the entry point of the
    /// Platform Infrastructure. Use the <see cref="PostSharpObject.CreateInstance(PostSharpObjectSettings)"/> method
    /// to create an instance of the PostSharp Object.
    /// </summary>
    /// <remarks>
    /// <para>This object is disposable. Seriously! Be sure to call the <see cref="IDisposable.Dispose"/> method
    /// when you do not need PostSharp any more, otherwise the private application domain
    /// won't be unloaded.
    /// </para>
    /// <para>
    /// All assemblies loaded in the scope of one instance of <see cref="IPostSharpObject"/>
    /// and all projects will share a unique instance of the <see cref="Domain"/> class. If this
    /// behavior is not wished, you should use many instances of <see cref="IPostSharpObject"/>.
    /// </para>
    /// </remarks>
    public interface IPostSharpObject : IDisposable
    {
        /// <summary>
        /// Requests the processing of a single PostSharp project.
        /// </summary>
        /// <param name="projectInvocation">Parameters for the PostSharp invocation.</param>
        void InvokeProject( ProjectInvocation projectInvocation );

        /// <summary>
        /// Requests the processing of PostSharp projects. Each project and its parameters
        /// are described in a <see cref="ProjectInvocation"/> object.
        /// </summary>
        /// <param name="projectInvocations">An array in which each element is a request
        /// to execute a PostSharp project.</param>
        /// <remarks>
        /// One source assembly may be processed only once in the lifetime of a <see cref="IPostSharpObject"/>.
        /// </remarks>
        void InvokeProjects( ProjectInvocation[] projectInvocations );

        /// <summary>
        /// Requests the processing of assemblies by PostSharp. The projects to be executed and their
        /// paramaters are not yet known. They will be requested through the 
        /// <see cref="IPostSharpHost.GetProjectInvocationParameters"/> method of the
        /// <see cref="IPostSharpHost"/> interface.
        /// </summary>
        /// <param name="modules">Array of assemblies to be processed.</param>
        void ProcessAssemblies( ModuleLoadStrategy[] modules );

        /// <summary>
        /// Gets the <see cref="AppDomain"/> in which the PostSharp Object lives.
        /// </summary>
        AppDomain AppDomain { get; }
    }
}