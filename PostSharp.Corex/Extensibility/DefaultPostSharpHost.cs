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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Default implementation of <see cref="IPostSharpHost"/> for the runtime usage scenario.
    /// </summary>
    public class DefaultPostSharpHost : MarshalByRefObject, IPostSharpHost
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public static readonly DefaultPostSharpHost Instance = new DefaultPostSharpHost();

        /// <summary>
        /// Protected constructor. 
        /// </summary>
        /// <remarks>
        /// It makes no sense to have many instances of the <see cref="DefaultPostSharpHost"/>,
        /// but we want to make it possible to inherit this class.
        /// </remarks>
        protected DefaultPostSharpHost()
        {
        }

        /// <inheritdoc />
        public string ResolveAssemblyReference( AssemblyName assemblyName )
        {
            return null;
        }

        /// <inheritdoc />
        public ProjectInvocationParameters GetProjectInvocationParameters( string assemblyFileName, string moduleName,
                                                                           AssemblyName assemblyName )
        {
            return null;
        }

        /// <inheritdoc />
        public void RenameAssembly( AssemblyName oldAssemblyName, AssemblyName newAssemblyName )
        {
        }
    }
}