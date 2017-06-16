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

using System;

namespace PostSharp.Laos
{
    ///<summary>
    /// Wraps a <see cref="Type"/> or a type name.
    ///</summary>
    public class TypeIdentity
    {
#if !SMALL
        private readonly Type type;

        private TypeIdentity( Type type )
        {
            this.type = type;
        }

        /// <summary>
        /// Wraps a <see cref="Type"/> into a <see cref="TypeIdentity"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="TypeIdentity"/> wrapping <paramref cref="type"/>.</returns>
        public static TypeIdentity Wrap( Type type )
        {
            return type != null ? new TypeIdentity( type ) : null;
        }

        /// <summary>
        /// Wraps an array of <see cref="Type"/> into an array of <see cref="TypeIdentity"/>.
        /// </summary>
        /// <param name="types">An array of <see cref="Type"/>.</param>
        /// <returns>An array of <see cref="TypeIdentity"/> wrapping <paramref name="types"/>.</returns>
        public static TypeIdentity[] Wrap( Type[] types )
        {
            if ( types == null ) return null;

            TypeIdentity[] typeIdentities = new TypeIdentity[types.Length];

            for ( int i = 0; i < types.Length; i++ )
                typeIdentities[i] = new TypeIdentity( types[i] );

            return typeIdentities;
        }


        /// <summary>
        /// Gets the wrapped <see cref="Type"/>, or <b>null</b> it the <see cref="TypeName"/> property is set.
        /// </summary>
        public Type Type
        {
            get { return this.type; }
        }
#endif

        private readonly string typeName;

        private TypeIdentity( string typeName )
        {
            this.typeName = typeName;
        }

        /// <summary>
        /// Wraps a type name into a <see cref="TypeIdentity"/>.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <returns>A <see cref="TypeIdentity"/> wrapping the type name.</returns>
        public static TypeIdentity Wrap( string typeName )
        {
            return typeName != null ? new TypeIdentity( typeName ) : null;
        }

        /// <summary>
        /// Wraps an array of type names into an array of <see cref="TypeIdentity"/>.
        /// </summary>
        /// <param name="typeNames">An array of type names.</param>
        /// <returns>An array of <see cref="TypeIdentity"/> wrapping <paramref name="typeNames"/>.</returns>
        public static TypeIdentity[] Wrap( string[] typeNames )
        {
            if ( typeNames == null ) return null;

            TypeIdentity[] typeIdentities = new TypeIdentity[typeNames.Length];

            for ( int i = 0; i < typeNames.Length; i++ )
                typeIdentities[i] = new TypeIdentity( typeNames[i] );

            return typeIdentities;
        }


        /// <summary>
        /// Gets the wrapped type name, or <b>null</b> it the <see cref="Type"/> property is set.
        /// </summary>
        public string TypeName
        {
            get { return this.typeName; }
        }
    }
}