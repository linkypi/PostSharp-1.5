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
    /// Contains the characteristics of a platform, typically
    /// the target platform of the module.
    /// </summary>
    public sealed class PlatformInfo
    {
        /// <summary>
        /// Size of a native pointer in bytes.
        /// </summary>
        private readonly int nativePointerSize;

        /// <summary>
        /// Gets the size of a native pointer in bytes.
        /// </summary>
        public int NativePointerSize { get { return this.nativePointerSize; } }

        /// <summary>
        /// Initializes a new <see cref="PlatformInfo"/>.
        /// </summary>
        /// <param name="nativePointerSize">Size of a native pointer in bytes.</param>
        internal PlatformInfo( int nativePointerSize )
        {
            this.nativePointerSize = nativePointerSize;
        }
    }
}