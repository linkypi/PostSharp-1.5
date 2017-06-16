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

using System.Globalization;
using System.Text;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Defines constants identifying the different variants of the
    /// .NET Framework (<see cref="Full"/>, <see cref="Compact"/>, <see cref="Silverlight"/>).
    /// </summary>
    public static class FrameworkVariants
    {
        /// <summary>
        /// Normal, full .NET Framework.
        /// </summary>
        public const string Full = "b77a5c561934e089";

        /// <summary>
        /// .NET Compact Framework.
        /// </summary>
        public const string Compact = "969db8053d3322ac";

        /// <summary>
        /// Silverlight.
        /// </summary>
        public const string Silverlight = "7cec85d7bea7798e";

        /// <summary>
        /// Micro Framework.
        /// </summary>
        /// <remarks>
        /// There is strong name in the Micro Framework.
        /// </remarks>
        public const string Micro = null;

#if !MF
        /// <summary>
        /// Convert a byte array (typically containing the public key token of <b>mscorlib</b>)
        /// into a string that can be compared to one of the constants defined in this class.
        /// </summary>
        /// <param name="bytes">A byte array (typically containing the public key token of <b>mscorlib</b>)</param>
        /// <returns>A string that can be compared to one of the constants defined in this class</returns>
        public static string FromBytes( byte[] bytes )
        {
            if ( bytes == null ) return null;
            StringBuilder builder = new StringBuilder( bytes.Length*2 );
            for ( int i = 0; i < bytes.Length; i++ )
            {
                builder.AppendFormat( CultureInfo.InvariantCulture, "{0:x2}", bytes[i] );
            }

            return builder.ToString();
        }
#endif
    }
}