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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Enumeration of primitive types of the .NET runtime.
    /// This includes intrincic types.
    /// </summary>
    public enum IntrinsicType
    {
        /// <summary>
        /// <see cref="System.Void"/>.
        /// </summary>
        Void = 0,
        /// <summary>
        /// <see cref="System.Object"/>.
        /// </summary>
        Object = 1,
        /// <summary>
        /// <see cref="System.Boolean"/>.
        /// </summary>
        Boolean = 3,
        /// <summary>
        /// <see cref="System.Char"/>.
        /// </summary>
        Char = 4,
        /// <summary>
        /// <see cref="System.SByte"/>.
        /// </summary>
        SByte = 5,
        /// <summary>
        /// <see cref="System.Byte"/>.
        /// </summary>
        Byte = 6,
        /// <summary>
        /// <see cref="System.Int16"/>.
        /// </summary>
        Int16 = 7,
        /// <summary>
        /// <see cref="System.UInt16"/>.
        /// </summary>
        UInt16 = 8,
        /// <summary>
        /// <see cref="System.Int32"/>.
        /// </summary>
        Int32 = 9,
        /// <summary>
        /// <see cref="System.UInt32"/>.
        /// </summary>
        UInt32 = 10,
        /// <summary>
        /// <see cref="System.Int64"/>.
        /// </summary>
        Int64 = 11,
        /// <summary>
        /// <see cref="System.UInt64"/>.
        /// </summary>
        UInt64 = 12,
        /// <summary>
        /// <see cref="System.Single"/>.
        /// </summary>
        Single = 13,
        /// <summary>
        /// <see cref="System.Double"/>.
        /// </summary>
        Double = 14,
        /// <summary>
        /// <see cref="System.String"/>.
        /// </summary>
        String = 18,
        /// <summary>
        /// <see cref="System.IntPtr"/> (not serializable).
        /// </summary>
        IntPtr,
        /// <summary>
        /// <see cref="System.UIntPtr"/>  (not serializable).
        /// </summary>
        UIntPtr,
        /// <summary>
        /// Real number with platform-specific precision  (not serializable).
        /// </summary>
        NativeReal,
        /// <summary>
        /// <see cref="System.TypedReference"/> (not serializable).
        /// </summary>
        TypedReference,
        /// <summary>
        /// Null value. 
        /// </summary>
        /// <remarks>
        /// This is not a valid type of the CLR! This is only usefull
        /// for static analysis.
        /// </remarks>
        Null,
        /// <summary>
        /// Metadata token. 
        /// </summary>
        /// <remarks>
        /// This is not a valid type of the CLR! This is only usefull
        /// for static analysis.
        /// </remarks>
        Token
    }
}