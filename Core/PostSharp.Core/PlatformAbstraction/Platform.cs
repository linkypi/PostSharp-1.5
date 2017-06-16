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
using System.Diagnostics.SymbolStore;
using System.Security;
using System.Security.Policy;
using PostSharp.CodeModel;
using PostSharp.ModuleReader;
using PostSharp.PlatformAbstraction.DotNet;
using PostSharp.PlatformAbstraction.Mono;

namespace PostSharp.PlatformAbstraction
{
    /// <summary>
    /// Abstraction of the platform (.NET, Mono) on which PostSharp currently run.
    /// </summary>
    /// <remarks>
    /// This class is not to be confused with <see cref="TargetPlatformAdapter"/>, which
    /// is an abstraction of the platform to which PostSharp should compile. It is theoretically
    /// possible to build transformed assemblies for a different platform than the current one
    /// (for instance .NET 1.1 on .NET 2.0), although this feature is not used and not
    /// supported.
    /// </remarks>
    /// <seealso cref="DotNet20Platform"/>
    /// <seealso cref="Mono20Platform"/>
    public abstract class Platform
    {
        /// <summary>
        /// Gets the current <see cref="Platform"/>.
        /// </summary>
        public static readonly Platform Current;

        static Platform()
        {
            try
            {
                bool isMono = Type.GetType( "System.MonoType", false ) != null;

                Current = isMono
                              ? new Mono20Platform()
                              : (Platform)
                                Activator.CreateInstance( typeof(Platform).Assembly.FullName,
                                                          "PostSharp.PlatformAbstraction.DotNet.DotNet20Platform" ).
                                    Unwrap
                                    ();
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.ToString() );
                throw;
            }
        }

        internal Platform()
        {
        }

        /// <summary>
        /// Determines whether intrinsics of opposite sign (for instance <see cref="int"/>
        /// and <see cref="uint"/>) are assignable.
        /// </summary>
        public bool IntrinsicOfOppositeSignAssignable { get; protected set; }

        /// <summary>
        /// Gets the <see cref="PlatformIdentity"/> for the current platform.
        /// </summary>
        public PlatformIdentity Identity { get; protected set; }

        /// <summary>
        /// Gets the default <see cref="TargetPlatformAdapter"/>
        /// </summary>
        /// <value>
        /// The name of the default <see cref="TargetPlatformAdapter"/>
        /// in <b>PostSharp-Platform.config</b>.
        /// </value>
        public string DefaultTargetPlatformName { get; protected set; }

        /// <summary>
        /// Gets the strategy used to read a module (from disk or from memory).
        /// </summary>
        public ReadModuleStrategy ReadModuleStrategy { get; protected set; }

        /// <summary>
        /// Normalizes a CIL identifier so that it is accepted by ILASM without quoting.
        /// </summary>
        /// <param name="name">The identifier name.</param>
        /// <returns>The normalized identifier name.</returns>
        public abstract string NormalizeCilIdentifier( string name );

        /// <summary>
        /// Gets an <see cref="ISymbolReader"/> for a module.
        /// </summary>
        /// <param name="moduleReader">A <see cref="ModuleReader"/>.</param>
        /// <returns>The <see cref="ISymbolReader"/> for <paramref name="moduleReader"/>,
        /// or <b>null</b> if none could be returned (feature not supported or no PDB found).</returns>
        internal abstract ISymbolReader GetSymbolReader( ModuleReader.ModuleReader moduleReader );

        /// <summary>
        /// Creates an <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="evidence">Evidence.</param>
        /// <param name="setup">Setup information.</param>
        /// <param name="permissions">Permission.</param>
        /// <returns>A new <see cref="AppDomain"/>.</returns>
        public abstract AppDomain CreateAppDomain( string name, Evidence evidence, AppDomainSetup setup,
                                                   PermissionSet permissions );

        /// <summary>
        /// Finds an assembly in GAC.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <returns>The full path of the assembly in GAC (i.e., on file system), or <b>null</b> if the
        /// assembly was not found.</returns>
        public abstract string FindAssemblyInCache( IAssemblyName assemblyName );
    }

    /// <summary>
    /// Simple identity of a <see cref="Platform"/>.
    /// </summary>
    public enum PlatformIdentity
    {
        /// <summary>
        /// Microsoft implementation.
        /// </summary>
        Microsoft = 0,

        /// <summary>
        /// Mono.
        /// </summary>
        Mono
    }
}