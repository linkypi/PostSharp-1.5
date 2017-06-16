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

#region Using directives

#endregion

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Elements composing the grammar of binary type signature.
    /// </summary>
    internal enum CorElementType : byte
    {
        /// <summary>
        /// Void.
        /// </summary>
        Void = 0x1,

        /// <summary>
        /// Boolean.
        /// </summary>
        Boolean = 0x2,

        /// <summary>
        /// Char.
        /// </summary>
        Char = 0x3,

        /// <summary>
        /// SByte.
        /// </summary>
        SByte = 0x4,

        /// <summary>
        /// Byte.
        /// </summary>
        Byte = 0x5,

        /// <summary>
        /// Int16.
        /// </summary>
        Int16 = 0x6,

        /// <summary>
        /// UInt16.
        /// </summary>
        UInt16 = 0x7,

        /// <summary>
        /// Int32.
        /// </summary>
        Int32 = 0x8,

        /// <summary>
        /// UInt32.
        /// </summary>
        UInt32 = 0x9,

        /// <summary>
        /// Int64.
        /// </summary>
        Int64 = 0xa,

        /// <summary>
        /// UInt64.
        /// </summary>
        UInt64 = 0xb,

        /// <summary>
        /// Single.
        /// </summary>
        Single = 0xc,

        /// <summary>
        /// Double.
        /// </summary>
        Double = 0xd,

        /// <summary>
        /// String.
        /// </summary>
        String = 0xe,

        /// <summary>
        /// Unmanaged pointer.
        /// </summary>
        Pointer = 0xf,

        /// <summary>
        /// Managed pointer.
        /// </summary>
        ByRef = 0x10,

        /// <summary>
        /// Value type.
        /// </summary>
        TValue = 0x11,

        /// <summary>
        /// Class.
        /// </summary>
        Class = 0x12,

        /// <summary>
        /// Generic type parameter.
        /// </summary>
        GenericTypeParameter = 0x13,

        /// <summary>
        /// Multidimensional array.
        /// </summary>
        Array = 0x14,

        /// <summary>
        /// Generic type instance.
        /// </summary>
        GenericInstance = 0x15,

        /// <summary>
        /// Typed reference.
        /// </summary>
        TypedByRef = 0x16,

        /// <summary>
        /// IntPtr.
        /// </summary>
        IntPtr = 0x18,

        /// <summary>
        /// UIntPtr.
        /// </summary>
        UIntPtr = 0x19,

        /// <summary>
        /// Pointer to a method.
        /// </summary>
        MethodPointer = 0x1B,

        /// <summary>
        /// Object.
        /// </summary>
        Object = 0x1C,

        /// <summary>
        /// Single dimension array with zero lower bound and no upper bound.
        /// </summary>
        SzArray = 0x1D,

        /// <summary>
        /// Generic method parameter.
        /// </summary>
        GenericMethodParameter = 0x1e,

        /// <summary>
        /// Required custom modifier.
        /// </summary>
        CustomModifierRequired = 0x1F,

        /// <summary>
        /// Optional custom modifier.
        /// </summary>
        CustomModifierOptional = 0x20,


        /// <summary>
        /// Sentinel for VarArgs.
        /// </summary>
        Sentinel = 0x41,

        /// <summary>
        /// Pinned pointer.
        /// </summary>
        Pinned = 0x45
    }
}