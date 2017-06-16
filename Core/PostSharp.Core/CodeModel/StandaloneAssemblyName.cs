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
using PostSharp.CodeModel.Helpers;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Standalone implementation  of <see cref="IAssemblyName"/>.
    /// </summary>
    public sealed class StandaloneAssemblyName : IAssemblyName
    {
        private readonly string name;
        private readonly Version version;
        private readonly string culture;
        private readonly byte[] publicKeyToken;

        /// <summary>
        /// Initializes a new <see cref="StandaloneAssemblyName"/>.
        /// </summary>
        /// <param name="name">Short assembly name.</param>
        /// <param name="version">Assembly version.</param>
        /// <param name="culture">Assembly culture.</param>
        /// <param name="publicKeyToken">Assembly public key token/</param>
        public StandaloneAssemblyName( string name, Version version, string culture, byte[] publicKeyToken )
        {
            #region Preconditions
            ExceptionHelper.AssertArgumentNotEmptyOrNull(name, "name");
            #endregion

            this.name = name;
            this.publicKeyToken = publicKeyToken;
            this.culture = culture;
            this.version = version;
        }

        #region IAssemblyName Members

        /// <inheritdoc />
        public string Name { get { return name; } }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                return
                    AssemblyNameHelper.FormatAssemblyFullName( this.name, this.version, this.culture,
                                                               this.publicKeyToken );
            }
        }

        /// <inheritdoc />
        public Version Version { get { return version; } }

        /// <inheritdoc />
        public byte[] GetPublicKey()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public byte[] GetPublicKeyToken()
        {
            return this.publicKeyToken;
        }

        /// <inheritdoc />
        public string Culture { get { return culture; } }

        /// <inheritdoc />
        public bool IsMscorlib { get { return StringComparer.InvariantCultureIgnoreCase.Equals( this.name, "mscorlib" ); } }

        /// <inheritdoc />
        public byte[] PublicKeyToken { get { return publicKeyToken; } }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return this.FullName;
        }
    }
}
