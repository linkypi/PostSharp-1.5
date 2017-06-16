#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

#if !SMALL

using System;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of an assembly name.
    /// </summary>
    public interface IAssemblyName
    {
        /// <summary>
        /// Gets the assembly friendly name.
        /// </summary>
        /// <value>
        /// The assembly friendly name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the assembly full name.
        /// </summary>
        /// <value>
        /// The assembly full name including version number, culture name
        /// and public key token.
        /// </value>
        string FullName { get; }

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        /// <value>
        /// A <see cref="Version"/>.
        /// </value>
        Version Version { get; }

        /// <summary>
        /// Gets the assemby public key.
        /// </summary>
        /// <value>
        /// An array of bytes containing the public key,
        /// or <b>null</b> if no public key is specified (for instance if
        /// only the public key token is given).
        /// </value>
        /// <returns>An array of bytes containing the public key,
        /// or <b>null</b> if no public key is specified.</returns>
        byte[] GetPublicKey();

        /// <summary>
        /// Gets the assembly public key token.
        /// </summary>
        /// <returns>An array of bytes containing the public key token,
        /// or <b>null</b> if no public key is specified.</returns>
        byte[] GetPublicKeyToken();


        /// <summary>
        /// Gets the assembly culture name.
        /// </summary>
        /// <value>
        /// The standard assembly culture name, or <b>null</b> if the assembly
        /// is culture-neutral.
        /// </value>
        string Culture { get; }

        /// <summary>
        /// Determines whether the current assembly is <b>mscorlib</b>.
        /// </summary>
        bool IsMscorlib { get; }
    }
}

#endif