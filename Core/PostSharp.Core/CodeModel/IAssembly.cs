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
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of an assembly 
    /// (<see cref="AssemblyRefDeclaration"/>
    /// <see cref="AssemblyEnvelope"/>).
    /// </summary>
    public interface IAssembly : IAssemblyName, ITaggable, IEquatable<IAssembly>
    {
        /// <summary>
        /// Gets the reflection <see cref="System.Reflection.Assembly"/> corresponding 
        /// to the current instance.
        /// </summary>
        /// <returns>The <see cref="System.Reflection.Assembly"/> corresponding to this
        /// instance, or <b>null</b> if the assembly could not be found.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This method has a non-trivial cost." )]
        Assembly GetSystemAssembly();

        /// <summary>
        /// Gets the <see cref="AssemblyEnvelope"/> corresponding to the current instance
        /// in the current domain.
        /// </summary>
        /// <returns>An <see cref="AssemblyEnvelope"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This method has a non-trivial cost." )]
        AssemblyEnvelope GetAssemblyEnvelope();

        /// <summary>
        /// Determines whether the current <see cref="AssemblyEnvelope"/> matches a given assembly reference.
        /// </summary>
        /// <param name="assemblyName">The assembly reference.</param>
        /// <returns><b>true</b> if the current <see cref="AssemblyEnvelope"/> matches <paramref name="assemblyName"/>,
        /// otherwise <b>false</b>.</returns>
        /// <remarks>Matching an assembly reference is a looser requirement than matching an assembly name exactly;
        /// an assembly reference may set no requirement on the public key token or the version, for instance.</remarks>
        bool MatchesReference( IAssemblyName assemblyName );
    }

    internal interface IAssemblyInternal : IAssembly
    {
        /// <summary>
        /// Writes in IL a reference to the current instance.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        void WriteILReference( ILWriter writer );

        bool Equals( IAssembly assembly, bool strict );
    }
}