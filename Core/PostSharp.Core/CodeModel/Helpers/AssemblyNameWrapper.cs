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
using System.Globalization;
using System.Reflection;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Wraps an <see cref="AssemblyName"/> into an <see cref="IAssemblyName"/>.
    /// </summary>
    public sealed class AssemblyNameWrapper : IAssemblyName
    {
        private AssemblyName assemblyName;
        private string fullName;

        private static readonly Dictionary<string, AssemblyNameWrapper> fullNameInstances =
            new Dictionary<string, AssemblyNameWrapper>();

        private static readonly Dictionary<Assembly, AssemblyNameWrapper> assemblyInstances =
            new Dictionary<Assembly, AssemblyNameWrapper>();

        /// <summary>
        /// Gets an <see cref="AssemblyName"/> for an assembly given as a <see cref="string"/> containing its full name.
        /// </summary>
        /// <param name="assemblyFullName">Full name of the assembly.</param>
        /// <returns>An <see cref="AssemblyNameWrapper"/> instance (eventually cached) correspondig
        /// to <paramref name="assemblyFullName"/>.</returns>
        public static AssemblyNameWrapper GetWrapper( string assemblyFullName )
        {
            assemblyFullName = assemblyFullName.Trim();

            AssemblyNameWrapper assemblyNameWrapper;
            if ( !fullNameInstances.TryGetValue( assemblyFullName, out assemblyNameWrapper ) )
            {
                assemblyNameWrapper = new AssemblyNameWrapper( null, assemblyFullName );
                fullNameInstances.Add( assemblyFullName, assemblyNameWrapper );
            }

            return assemblyNameWrapper;
        }

        /// <summary>
        /// Gets an <see cref="AssemblyName"/> for an assembly given as an <see cref="AssemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <returns>An <see cref="AssemblyNameWrapper"/> instance (eventually cached) correspondig
        /// to <paramref name="assemblyName"/>.</returns>
        public static AssemblyNameWrapper GetWrapper(AssemblyName assemblyName)
        {
            return new AssemblyNameWrapper( assemblyName, null );
        }

        /// <summary>
        /// Gets an <see cref="AssemblyName"/> for an <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly">Assembly.</param>
        /// <returns>An <see cref="AssemblyNameWrapper"/> instance (eventually cached) correspondig
        /// to <paramref name="assembly"/>.</returns>
        public static AssemblyNameWrapper GetWrapper(Assembly assembly)
        {
            AssemblyNameWrapper assemblyNameWrapper;
            if ( !assemblyInstances.TryGetValue( assembly, out assemblyNameWrapper ) )
            {
                assemblyNameWrapper = GetWrapper( assembly.GetName() );
                assemblyInstances.Add( assembly, assemblyNameWrapper );
            }

            return assemblyNameWrapper;
        }

        private AssemblyNameWrapper( AssemblyName assemblyName, string fullName )
        {
            this.assemblyName = assemblyName;
            this.fullName = fullName;
        }

        #region IAssemblyName Members

        /// <inheritdoc />
        public string Name
        {
            get
            {
                if ( this.assemblyName == null )
                    this.assemblyName = new AssemblyName( this.fullName );

                return this.assemblyName.Name;
            }
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                if ( this.fullName == null )
                    this.fullName = this.assemblyName.FullName;
                return this.fullName;
            }
        }

        /// <inheritdoc />
        public Version Version
        {
            get
            {
                if ( this.assemblyName == null )
                    this.assemblyName = new AssemblyName( this.fullName );

                return this.assemblyName.Version;
            }
        }

        /// <inheritdoc />
        public byte[] GetPublicKey()
        {
            if ( this.assemblyName == null )
                this.assemblyName = new AssemblyName( this.fullName );

            return this.assemblyName.GetPublicKey();
        }

        /// <inheritdoc />
        public byte[] GetPublicKeyToken()
        {
            if ( this.assemblyName == null )
                this.assemblyName = new AssemblyName( this.fullName );

            return this.assemblyName.GetPublicKeyToken();
        }

        /// <inheritdoc />
        public string Culture
        {
            get
            {
                if ( this.assemblyName == null )
                    this.assemblyName = new AssemblyName( this.fullName );

                CultureInfo cultureInfo = this.assemblyName.CultureInfo;
                return cultureInfo == null ? null : cultureInfo.Name;
            }
        }

        /// <inheritdoc />
        public bool IsMscorlib
        {
            get
            {
                if ( this.assemblyName == null )
                    this.assemblyName = new AssemblyName( this.fullName );

                return this.assemblyName.Name == "mscorlib";
            }
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return this.FullName;
        }
    }
}