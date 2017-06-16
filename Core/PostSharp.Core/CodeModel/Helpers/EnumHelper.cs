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
using System.Reflection;
using PostSharp.CodeModel.TypeSignatures;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Helps to work with enumerations.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the underlying type of an enumeration.
        /// </summary>
        /// <param name="type">A type derived from <see cref="Enum"/>.</param>
        /// <returns>The underlying type of the enumeration.</returns>
        public static IntrinsicTypeSignature GetUnderlyingType( IType type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            TypeDefDeclaration typeDef = type.GetTypeDefinition();
            foreach ( FieldDefDeclaration field in typeDef.Fields )
            {
                if ( ( field.Attributes & FieldAttributes.HasDefault ) == 0 )
                {
                    IntrinsicTypeSignature intrinsicType = field.FieldType as IntrinsicTypeSignature;

                    ExceptionHelper.Core.AssertValidArgument( intrinsicType != null, "type", "NotAnEnumeration" );

                    return intrinsicType;
                }
            }

            throw ExceptionHelper.Core.CreateArgumentException( "type", "NotAnEnumeration" );
        }
    }
}