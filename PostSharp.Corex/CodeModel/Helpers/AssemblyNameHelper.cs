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
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Provides helper methods to work with assembly names.
    /// </summary>
    public sealed class AssemblyNameHelper
    {
        private static readonly SHA1 sha1 = new SHA1Managed();

        /// <summary>
        /// Parses an hexadecimal string into an array of bytes.
        /// </summary>
        /// <param name="str">A string containing hexadecimal characters. Should contain a peer number of characters.</param>
        /// <returns>The bytes represented by <paramref name="str"/>.</returns>
        public static byte[] ParseBytes( string str )
        {
            if ( str == null )
                return null;

            if ( StringComparer.InvariantCultureIgnoreCase.Equals( str, "null" ) )
                return null;

            if ( str.Length == 0 )
                return EmptyArray<byte>.GetInstance();

            if ( str.Length%2 != 0 )
                throw new ArgumentException();

            byte[] result = new byte[str.Length/2];

            for ( int i = 0; i < result.Length; i++ )
            {
                result[i] = byte.Parse( str.Substring( i*2, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            }

            return result;
        }

        /// <summary>
        /// Formats an array of bytes into an hexadecimal string, using a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="builder">The <see cref="StringBuilder"/> into which the string has to be written.</param>
        public static void FormatBytes( byte[] bytes, StringBuilder builder )
        {
            if ( bytes != null && bytes.Length > 0 )
            {
                for ( int i = 0; i < bytes.Length; i++ )
                {
                    builder.AppendFormat( "{0:x2}", (int) bytes[i] );
                }
            }
            else
            {
                builder.Append( "null" );
            }
        }

        /// <summary>
        /// Formats an array of bytes into a hexadecimal string.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <returns>The hexadecimal string corresponding to <paramref name="bytes"/>.</returns>
        public static string FormatBytes( byte[] bytes )
        {
            if ( bytes == null || bytes.Length == 0 )
            {
                return "null";
            }

            StringBuilder builder = new StringBuilder( 20 );
            FormatBytes( bytes, builder );
            return builder.ToString();
        }

        /// <summary>
        /// Composes a full assembly name.
        /// </summary>
        /// <param name="name">Friendly name.</param>
        /// <param name="version">Version (or <b>null</b>).</param>
        /// <param name="culture">Culture (or <b>null</b>).</param>
        /// <param name="publicKeyToken">Public key token (or <b>null</b>).</param>
        /// <returns>The full assembly name corresponding to parameters.</returns>
        public static string FormatAssemblyFullName(
            string name,
            Version version,
            string culture,
            byte[] publicKeyToken
            )
        {
            StringBuilder builder = new StringBuilder( 128 );
            builder.Append( name );

            if ( version != null )
            {
                builder.Append( ", Version=" );
                builder.Append( version.ToString() );
            }

            if ( ! string.IsNullOrEmpty( culture ) )
            {
                builder.Append( ", Culture=" );
                builder.Append( culture );
            }
            else
            {
                builder.Append( ", Culture=neutral" );
            }

            if ( publicKeyToken != null && publicKeyToken.Length > 0 )
            {
                builder.Append( ", PublicKeyToken=" );
                FormatBytes( publicKeyToken, builder );
            }

            return builder.ToString();
        }

        /// <summary>
        /// Compute the token of a public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns>The token (i.e. the first 8 bytes of the SHA1 hash) of <paramref name="publicKey"/>.</returns>
        public static byte[] ComputeKeyToken( byte[] publicKey )
        {
            byte[] token = new byte[8];
            byte[] hash = sha1.ComputeHash( publicKey );

            for ( int i = 0; i < 8; i++ )
            {
                token[i] = hash[hash.Length - i - 1];
            }

            return token;
        }

        /// <summary>
        /// Converts an <see cref="AssemblyName"/> into an <see cref="IAssemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">An <see cref="AssemblyName"/>.</param>
        /// <returns>An <see cref="IAssemblyName"/> corresponding to <paramref name="assemblyName"/>.</returns>
        public static IAssemblyName Convert( AssemblyName assemblyName )
        {
            return AssemblyNameWrapper.GetWrapper( assemblyName );
        }

        /// <summary>
        /// Converts an <see cref="IAssemblyName"/> into an <see cref="AssemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">An <see cref="IAssemblyName"/>.</param>
        /// <returns>An <see cref="AssemblyName"/> corresponding to <paramref name="assemblyName"/>.</returns>
        public static AssemblyName Convert( IAssemblyName assemblyName )
        {
            return new AssemblyName( assemblyName.FullName );
        }

        /// <summary>
        /// Gets the assembly name of a <see cref="Type"/> (works also with type wrappers).
        /// </summary>
        /// <param name="type">A <see cref="Type"/> (eventually a type wrapper).</param>
        /// <returns>The name of the assembly declaring <paramref name="type"/>.</returns>
        public static AssemblyNameWrapper GetAssemblyName( Type type )
        {
            IReflectionWrapper wrapper = type as IReflectionWrapper;

            if ( wrapper != null )
            {
                return AssemblyNameWrapper.GetWrapper( wrapper.DeclaringAssemblyName.FullName );
            }
            else
            {
                return AssemblyNameWrapper.GetWrapper( type.Assembly );
            }
        }
    }
}