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
using System.Reflection;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Describes the semantics of a PostSharp Host. The host is conceptually the component that
    /// instantiates and uses <see cref="IPostSharpObject"/>, the principal entry point for
    /// the Platform Infrastructure. The <see cref="IPostSharpObject"/> implementation uses this
    /// interface when it needs to get information from the host.
    /// </summary>
    /// <remarks>
    /// <para>Classes implementing <see cref="IPostSharpHost"/> should be derived from 
    /// <see cref="MarshalByRefObject"/> if the <see cref="IPostSharpObject"/> is set up to
    /// use its own application domain.
    /// </para>
    /// <para>
    /// If a host has default behavior, it is not required to implement its own <see cref="IPostSharpHost"/>.
    /// The default behavior is typically the compile-time use case, when all modules to be transformed are 
    /// passed to the <see cref="IPostSharpObject.InvokeProjects"/> method. Modules that are discovered 
    /// at runtime (like dependencies) are not processed.
    /// </para>
    /// </remarks>
    public interface IPostSharpHost
    {
        /// <summary>
        /// Returns the location and the loag arguments of an assembly known by name.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <returns>The full path of the assembly, or <b>null</b> if the host does not want
        /// to intervene in the default binding mechanism of the PostSharp Platform.</returns>
        /// <remarks>
        /// This method is called whenever an assembly reference has to be resolved inside the
        /// PostSharp Code Object Model, and this assembly has neither been resolved yet neither
        /// been passed to the <see cref="IPostSharpObject.InvokeProjects"/> method.
        /// </remarks>
        string ResolveAssemblyReference( AssemblyName assemblyName );

        /// <summary>
        /// Determines whether and how a module should be processed by PostSharp.
        /// </summary>
        /// <param name="assemblyFileName">Location of the considered assembly. This is merely the location
        /// returned by the <see cref="ResolveAssemblyReference"/> method.</param>
        /// <param name="moduleName">Name of the considered module.</param>
        /// <param name="assemblyName">Complete name of the assembly.</param>
        /// <returns>A <see cref="ProjectInvocationParameters"/> object if this module has to be
        /// processed, or <b>null</b> if the module should not be processed.</returns>
        ProjectInvocationParameters GetProjectInvocationParameters( string assemblyFileName, string moduleName,
                                                                    AssemblyName assemblyName );

        /// <summary>
        /// Notifies the host that an assembly has been renamed. This happens if the 
        /// <see cref="PostSharpObjectSettings"/>.<see cref="PostSharpObjectSettings.OverwriteAssemblyNames"/>
        /// flag has been set.
        /// </summary>
        /// <param name="oldAssemblyName">Original assembly name.</param>
        /// <param name="newAssemblyName">New assembly name.</param>
        /// <remarks>
        /// The assembly renaming is used in the runtime usage scenario in order to (1) remove
        /// strong names and (2) make sure that the unprocessed assembly is not loaded 'accidentally'
        /// by the default system binder (for instance because the transformed assembly lays in the GAC).
        /// See <see cref="PostSharpObjectSettings.OverwriteAssemblyNames"/> for details.
        /// </remarks>
        void RenameAssembly( AssemblyName oldAssemblyName, AssemblyName newAssemblyName );
    }
}