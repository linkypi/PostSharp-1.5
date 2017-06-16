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

using System;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Enumeration of IL prefixes.
    /// </summary>
    [Flags]
    public enum InstructionPrefixes
    {
        /// <summary>
        /// No prefix.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that a pointer instruction might be unaligned
        /// to the natural size of the following instruction operand.
        /// The runtime should assume that the pointer instruction
        /// is aligned at <b>byte</b> boundary.
        /// </summary>
        Unaligned1 = 1,

        /// <summary>
        /// Indicates that a pointer instruction might be unaligned
        /// to the natural size of the following instruction operand.
        /// The runtime should assume that the pointer instruction
        /// is aligned at <b>2 bytes</b> boundary.
        /// </summary>
        Unaligned2 = 2,

        /// <summary>
        /// Indicates that a pointer instruction might be unaligned
        /// to the natural size of the following instruction operand.
        /// The runtime should assume that the pointer instruction
        /// is aligned at <b>4 bytes</b> boundary.
        /// </summary>
        Unaligned4 = 3,

        /// <summary>
        /// Masks isolating the unaligned prefixes.
        /// </summary>
        UnalignedMask = 3,

        /// <summary>
        /// Indicates that the content of the read location is volatile
        /// and cannot be cached for later access.
        /// </summary>
        Volatile = 4,

        /// <summary>
        /// Indicates that the method could be terminated after the
        /// next call instruction.
        /// </summary>
        Tail = 8,

        /// <summary>
        /// Indicates that the virtual call of the next instruction
        /// is constrained to a given type.
        /// </summary>
        Constrained = 16,

        /// <summary>
        /// Tndicates that the subsequent array address operation performs no
        /// type check at runtime, and that it returns a controlled-mutability
        /// managed pointer
        /// </summary>
        ReadOnly = 32
    }
}