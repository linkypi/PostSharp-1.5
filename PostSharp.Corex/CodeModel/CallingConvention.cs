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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Determines the calling convention of methods.
    /// </summary>
    [Flags]
    public enum CallingConvention
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0x0,

        /// <summary>
        /// Unmanaged <b>cdecl</b>.
        /// </summary>
        UnmanagedCdecl = 0x1,

        /// <summary>
        /// Unmanaged <b>StdCall</b>.
        /// </summary>
        UnmanagedStdCall = 0x2,

        /// <summary>
        /// Unmanaged <b>ThisCall</b>.
        /// </summary>
        UnmanagedThisCall = 0x3,

        /// <summary>
        /// Unmanaged <b>FastCall</b>.
        /// </summary>
        UnmanagedFastCall = 0x4,

        /// <summary>
        /// Managed <b>VarArg</b>.
        /// </summary>
        VarArg = 0x5,

        /// <internal/>
        /// <summary>
        /// The signature is a field signature.
        /// </summary>
        Field = 0x6,

        /// <internal/>
        /// <summary>
        /// The signature is a local variable signature.
        /// </summary>
        LocalSig = 0x7,

        /// <internal/>
        /// <summary>
        /// The signature is a property signature,
        /// </summary>
        Property = 0x8,

        /// <internal/>
        /// <summary>
        ///  Generic method instantiation.
        /// </summary>
        GenericInstance = 0xa,

        /// <summary>
        /// NativeVarArg (Used ONLY for 64bit vararg PInvoke calls).
        /// </summary>
        NativeVarArg = 0xb,

        /// <internal />
        /// <summary>
        /// Indicates that any calling convention is acceptable.
        /// </summary>
        Any = 0xc,

        // The high bits of the calling convention convey additional info

        /// <summary>
        /// Bit mask for the calling convention.
        /// </summary>
        CallingConventionMask = 0x0f, // Calling convention is bottom 4 bits

        /// <summary>
        /// There is a <b>this</b> parameter.
        /// </summary>
        HasThis = 0x20, // Top bit indicates a 'this' parameter

        /// <summary>
        /// The <b>this</b> parameter is explicitely in the signature.
        /// </summary>
        ExplicitThis = 0x40,

        /// <internal/>
        /// <summary>
        /// Generic method signature with explicit number of type arguments.
        /// </summary>
        Generic = 0x10
    }
}