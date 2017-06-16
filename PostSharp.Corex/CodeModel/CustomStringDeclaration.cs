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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a user string that <i>not</i> originally defined in the module 
    /// (<see cref="TokenType.CustomString"/>).
    /// </summary>
    /// <seealso cref="LiteralString"/>.
    /// <remarks>
    /// <para>
    /// It is not possible to create new user strings because the token
    /// representing user strings (with token type <see cref="TokenType.String"/>)
    /// are not index in a metadata table, but RVA pointing to the first
    /// string character.
    /// </para>
    /// <para>
    /// However, we need to address user strings using tokens (and not simply
    /// references), because custom IL code is as a byte stream.
    /// </para>
    /// <para>
    /// In order to overcome the current issue, we define a new table of custom strings,
    /// which behaves like a normal metadata table. 
    /// </para>
    /// <para>
    /// <see cref="CustomStringDeclaration"/> is an immutable type.
    /// </para>
    /// </remarks>
    internal class CustomStringDeclaration : MetadataDeclaration
    {
        /// <summary>
        /// The custom string value.
        /// </summary>
        private readonly LiteralString value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStringDeclaration"/> type.
        /// </summary>
        /// <param name="value">The custom string value (the string in itself).</param>
        internal CustomStringDeclaration( LiteralString value )
        {
            this.value = value;
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.CustomString;
        }

        /// <summary>
        /// Gets the value of the custom string.
        /// </summary>
        public LiteralString Value { get { return this.value; } }

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.value.ToString();
        }

        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.value.ToString();
        }
    }
}