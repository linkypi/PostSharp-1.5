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
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Helpers;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Specifies a mapping between an assembly reference
    /// and the assembly to which the reference is actually resolve.
    /// Assembly redirection policies allow to substitute other
    /// assemblies to the ones that are actually requested, and therefore
    /// support newer versions of these assemblies, or even different
    /// variants of the .NET Framework.
    /// </summary>
    public sealed class AssemblyRedirectionPolicy
    {
        private readonly string oldName;
        private readonly byte[] oldPublicKeyToken;
        private readonly string culture;
        private readonly Version oldVersionLowerBound;
        private readonly Version oldVersionUpperBound;
        private readonly byte[] newPublicKeyToken;
        private readonly Version newVersion;
        private readonly string newName;

        /// <summary>
        /// Initializes a new <see cref="AssemblyRedirectionPolicy"/>.
        /// </summary>
        /// <param name="oldShortName">Short name of the assembly reference, or '*' to match any assembly name.</param>
        /// <param name="oldPublicKeyToken">Public key token of the assembly reference, or <b>null</b> to match
        /// any public key token.</param>
        /// <param name="culture">Culture of the assembly reference, or <b>null</b> to match any culture.</param>
        /// <param name="oldVersionLowerBound">Lower bound of the version of the assembly reference, <b>null</b> not to 
        /// specify any version lower bound.</param>
        /// <param name="oldVersionUpperBound">Upper bound of the version of the assembly reference, <b>null</b> not to 
        /// specify any version upper bound.</param>
        /// <param name="newName">New short name of the resolved assembly reference, or <b>null</b> to copy the reference name.</param>
        /// <param name="newPublicKeyToken">New public key token of the resolved assembly reference, or <b>null</b> to copy the
        /// public key token of the unresolved reference.</param>
        /// <param name="newVersion">New version of the resolved assembly reference, or <b>null</b> to copy the version
        /// of the unresolved reference.</param>
        public AssemblyRedirectionPolicy(
            string oldShortName,
            byte[] oldPublicKeyToken,
            string culture,
            Version oldVersionLowerBound,
            Version oldVersionUpperBound,
            string newName,
            byte[] newPublicKeyToken,
            Version newVersion )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( oldShortName, "oldName" );

            #endregion

            this.oldName = oldShortName;
            this.newName = newName;
            this.oldPublicKeyToken = oldPublicKeyToken;
            this.culture = culture;
            this.oldVersionLowerBound = oldVersionLowerBound;
            this.oldVersionUpperBound = oldVersionUpperBound;
            this.newPublicKeyToken = newPublicKeyToken;
            this.newVersion = newVersion;
        }

        /// <summary>
        /// Gets or sets the short name of the assembly reference.
        /// </summary>
        /// <value>
        /// The short name of the assembly reference, or '*' to match any assembly name.
        /// </value>
        public string OldName { get { return oldName; }  }

        /// <summary>
        /// Gets or sets the public key token of the assembly reference.
        /// </summary>
        /// <value>
        /// The public key token of the assembly reference, or <b>null</b> to match
        /// any public key token.
        /// </value>
        public byte[] OldPublicKeyToken { get { return oldPublicKeyToken; } }

        /// <summary>
        /// Gets or sets the culture of the assembly reference.
        /// </summary>
        /// <value>
        /// Culture of the assembly reference, or <b>null</b> to match any culture.
        /// </value>
        public string Culture { get { return culture; } }

        /// <summary>
        /// Gets or sets the lower bound of the version of the assembly reference.
        /// </summary>
        /// <value>
        /// Lower bound of the version of the assembly reference, <b>null</b> not to 
        /// specify any version lower bound.
        /// </value>
        public Version OldVersionLowerBound { get { return oldVersionLowerBound; } }

        /// <summary>
        /// Gets or sets the upper bound of the version of the assembly reference.
        /// </summary>
        /// <value>
        /// Upper bound of the version of the assembly reference, <b>null</b> not to 
        /// specify any version upper bound.
        /// </value>
        public Version OldVersionUpperBound { get { return oldVersionUpperBound; } }

        /// <summary>
        /// Gets or sets the new public key token of the resolved assembly reference.
        /// </summary>
        /// <value>
        /// The new public key token of the resolved assembly reference, or <b>null</b> to copy the
        /// public key token of the unresolved reference.
        /// </value>
        public byte[] NewPublicKeyToken { get { return newPublicKeyToken; } }

        /// <summary>
        /// Gets or sets the new version of the resolved assembly reference.
        /// </summary>
        /// <value>
        /// New version of the resolved assembly reference, or <b>null</b> to copy the version
        /// of the unresolved reference.
        /// </value>
        public Version NewVersion { get { return newVersion; } }

        /// <summary>
        /// Determines whether the current policy applies to a given assembly name.
        /// </summary>
        /// <param name="assemblyName">An assembly name (typically an assembly reference).</param>
        /// <returns><b>true</b> if the current policy applies to <paramref name="assemblyName"/>,
        /// otherwise <b>false</b>.</returns>
        public bool Matches( IAssemblyName assemblyName )
        {
            if ( !StringComparer.InvariantCultureIgnoreCase.Equals( assemblyName.Name, this.oldName ) &&
                 !StringComparer.InvariantCultureIgnoreCase.Equals( "*", this.oldName ) )
                return false;

            if ( this.oldPublicKeyToken != null &&
                 !CompareHelper.CompareBytes( this.oldPublicKeyToken, assemblyName.GetPublicKeyToken() ) )
                return false;

            if ( !string.IsNullOrEmpty( this.culture ) &&
                 !StringComparer.InvariantCultureIgnoreCase.Equals( this.culture,
                                                                    string.IsNullOrEmpty( assemblyName.Culture )
                                                                        ? "neutral"
                                                                        : assemblyName.Culture ) )
                return false;

            if ((this.oldVersionLowerBound != null && (assemblyName.Version == null || assemblyName.Version.CompareTo(this.oldVersionLowerBound) < 0)) ||
                 (this.oldVersionUpperBound != null && (assemblyName.Version == null || assemblyName.Version.CompareTo(this.oldVersionUpperBound) > 0)))
                return false;

            return true;
        }

        /// <summary>
        /// Applies the current policy to an assembly reference and returns the
        /// transformed assembly reference.
        /// </summary>
        /// <param name="oldAssemblyName">Assembly name to be transformed.</param>
        /// <returns>The transformed, resolved assembly name, obtained by applying the current policy.</returns>
        /// <remarks>
        /// This method assumes that the current policy actually matches <paramref name="oldAssemblyName"/>, which
        /// can be determined using the <see cref="Matches"/> method.
        /// </remarks>
        public IAssemblyName Apply( IAssemblyName oldAssemblyName )
        {
            Version version = this.newVersion ?? oldAssemblyName.Version;
            byte[] publicKeyToken = this.newPublicKeyToken ?? oldAssemblyName.GetPublicKeyToken();

            return new StandaloneAssemblyName( newName ?? oldAssemblyName.Name,
                                           version, oldAssemblyName.Culture, publicKeyToken );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                string.Format(
                    "{0}, Versions={2}-{3}, Culture={4}, PublicKeyToken={1} --> {7}, Version={5}, Culture=<copy>, PublicKeyToken={6}",
                    oldName ?? "*",
                    AssemblyNameHelper.FormatBytes( oldPublicKeyToken ),
                    oldVersionLowerBound, oldVersionUpperBound,
                    culture,
                    this.newVersion != null ? this.newVersion.ToString() : "<copy>",
                    this.newPublicKeyToken != null
                        ?
                            AssemblyNameHelper.FormatBytes( this.newPublicKeyToken )
                        : "<copy>",
                        this.newName ?? "<copy>");
        }
    }
}
