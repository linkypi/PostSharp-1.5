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
using System.Runtime.InteropServices;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Represents a buffer in the unmanaged memory, i.e. an address and a buffer size.
    /// </summary>
    internal struct UnmanagedBuffer
    {
        #region Fields

        /// <summary>
        /// Pointer to the first byte of the segment.
        /// </summary>
        /// <value>
        /// A pointer, or <see cref="IntPtr.Zero"/> if the current
        /// <see cref="UnmanagedBuffer"/> is null.
        /// </value>
        private readonly IntPtr origin;

        /// <summary>
        /// Lenght of the heap segment in bytes.
        /// </summary>
        private readonly int length;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="UnmanagedBuffer"/> 
        /// and sets its address and size.
        /// </summary>
        /// <param name="origin">Location of the first byte of the <see cref="UnmanagedBuffer"/>.</param>
        /// <param name="length">Number of bytes of the <see cref="UnmanagedBuffer"/>.</param>
        public UnmanagedBuffer( IntPtr origin, int length )
        {
            this.origin = origin;
            this.length = length;
        }

        /// <summary>
        /// Gets a pointer to the first byte of the <see cref="UnmanagedBuffer"/>.
        /// </summary>
        public IntPtr Origin
        {
            get { return this.origin; }
        }

        /// <summary>
        /// Gets the number of bytes in the <see cref="UnmanagedBuffer"/>.
        /// </summary>
        public int Size
        {
            get { return this.length; }
        }

        /// <summary>
        /// Determines whether the current <see cref="UnmanagedBuffer"/> is void (empty).
        /// </summary>
        public bool IsVoid
        {
            get { return this.origin == IntPtr.Zero; }
        }

        /// <summary>
        /// Gets a void (empty) <see cref="UnmanagedBuffer"/>.
        /// </summary>
#pragma warning disable 649
        public static readonly UnmanagedBuffer Void;
#pragma warning restore 649

        /// <summary>
        /// Converts the current <see cref="UnmanagedBuffer"/> to an array of bytes.
        /// </summary>
        /// <returns>A new array of bytes that is a copy of the heap segment,
        /// or <b>null</b> if the current <see cref="UnmanagedBuffer"/> is void.</returns>
        public byte[] ToByteArray()
        {
            if ( this.IsVoid )
            {
                return null;
            }
            else
            {
                byte[] array = new byte[this.length];
                Marshal.Copy( origin, array, 0, length );
                return array;
            }
        }
    }
}