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
    /// Enumeration of types used for binary serialization.
    /// </summary>
    internal enum CorSerializationType : byte
    {
        /// <summary>
        /// Undefined.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Boolean.
        /// </summary>
        Boolean = CorElementType.Boolean,

        /// <summary>
        /// Char.
        /// </summary>
        Char = CorElementType.Char,

        /// <summary>
        /// SByte.
        /// </summary>
        SByte = CorElementType.SByte,

        /// <summary>
        /// Byte.
        /// </summary>
        Byte = CorElementType.Byte,

        /// <summary>
        /// Int16.
        /// </summary>
        Int16 = CorElementType.Int16,

        /// <summary>
        /// UInt16.
        /// </summary>
        UInt16 = CorElementType.UInt16,

        /// <summary>
        /// Int32.
        /// </summary>
        Int32 = CorElementType.Int32,

        /// <summary>
        /// UInt32.
        /// </summary>
        UInt32 = CorElementType.UInt32,

        /// <summary>
        /// Int64.
        /// </summary>
        Int64 = CorElementType.Int64,

        /// <summary>
        /// UInt64.
        /// </summary>
        UInt64 = CorElementType.UInt64,

        /// <summary>
        /// Single.
        /// </summary>
        Single = CorElementType.Single,

        /// <summary>
        /// Double.
        /// </summary>
        Double = CorElementType.Double,

        /// <summary>
        /// String.
        /// </summary>
        String = CorElementType.String,

        /// <summary>
        ///  Single dimension zero lower bound array.
        /// </summary>
        Array = CorElementType.SzArray,

        /// <summary>
        /// Type.
        /// </summary>
        Type = 0x50,

        /// <summary>
        /// Tagged object (boxed value).
        /// </summary>
        TaggedObject = 0x51,

        /// <summary>
        /// Specifies that the member if a field.
        /// </summary>
        TypeField = 0x53,

        /// <summary>
        /// Specifies that the member is a property.
        /// </summary>
        TypeProperty = 0x54,

        /// <summary>
        /// Specify that the type is an enumeration.
        /// </summary>
        Enum = 0x55
    }
}