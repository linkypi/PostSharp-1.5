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
    /// Attributes of a .NET PE module.
    /// </summary>
    [Flags]
    public enum ImageAttributes
    {
        /// <summary>
        /// Always 1 (§23.1).
        /// </summary>
        ILOnly = 0x00000001,

        /// <summary>
        /// Image may only be loaded into a 32-bit process, 
        /// for instance if there are 32-bit vtablefixups, 
        /// or casts from native integers to int32. CLI implementations that have 64-bit 
        /// native integers shall refuse loading binaries with the current flag set.
        /// </summary>
        Requires32Bits = 0x00000002,

        /// <summary>
        /// Image has a strong name signature.
        /// </summary>
        StrongNameRequired = 0x00000008,

        /// <summary>
        ///  Always 0 (§23.1).
        /// </summary>
        TrackDebugData = 0x00010000
    }
}