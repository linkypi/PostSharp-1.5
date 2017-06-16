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

using System.Reflection;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Provides methods that determines the visibility of classes, fields and methods.
    /// </summary>
    public static class VisibilityHelper
    {
        /// <summary>
        /// Determines whether a type is visible outside its assembly.
        /// </summary>
        /// <param name="typeDef">A type.</param>
        /// <returns><b>true</b> if <paramref name="typeDef"/> is visible outside its
        /// assembly, otherwise <b>false</b>.</returns>
        public static bool IsPublic( TypeDefDeclaration typeDef )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( typeDef, "typeDef" );

            #endregion

            switch ( typeDef.Attributes & TypeAttributes.VisibilityMask )
            {
                case TypeAttributes.Public:
                    return true;

                case TypeAttributes.NotPublic:
                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                case TypeAttributes.NestedPrivate:
                    return false;

                case TypeAttributes.NestedFamORAssem:
                case TypeAttributes.NestedFamily:
                case TypeAttributes.NestedPublic:
                    return IsPublic( typeDef.DeclaringType );

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException(
                        typeDef.Attributes & TypeAttributes.VisibilityMask,
                        "typeDef.Attributes" );
            }
        }

        /// <summary>
        /// Determines whether a field is visible outside its assembly.
        /// </summary>
        /// <param name="fieldDef">A field.</param>
        /// <returns><b>true</b> if <paramref name="fieldDef"/> is visible outside its
        /// assembly, otherwise <b>false</b>.</returns>
        public static bool IsPublic( FieldDefDeclaration fieldDef )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( fieldDef, "fieldDef" );

            #endregion

            switch ( fieldDef.Attributes & FieldAttributes.FieldAccessMask )
            {
                case FieldAttributes.Assembly:
                case FieldAttributes.FamANDAssem:
                case FieldAttributes.Private:
                case FieldAttributes.PrivateScope:
                    return false;

                case FieldAttributes.Public:
                case FieldAttributes.Family:
                case FieldAttributes.FamORAssem:
                    return IsPublic( fieldDef.DeclaringType );

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException(
                        fieldDef.Attributes & FieldAttributes.FieldAccessMask,
                        "fieldDef.Attributes" );
            }
        }

        /// <summary>
        /// Determines whether a method is visible outside its assembly.
        /// </summary>
        /// <param name="methodDef">A method.</param>
        /// <returns><b>true</b> if <paramref name="methodDef"/> is visible outside its
        /// assembly, otherwise <b>false</b>.</returns>
        public static bool IsPublic( MethodDefDeclaration methodDef )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( methodDef, "methodDef" );

            #endregion

            switch ( methodDef.Attributes & MethodAttributes.MemberAccessMask )
            {
                case MethodAttributes.Assembly:
                case MethodAttributes.FamANDAssem:
                case MethodAttributes.Private:
                case MethodAttributes.PrivateScope:
                    return false;

                case MethodAttributes.Public:
                case MethodAttributes.Family:
                case MethodAttributes.FamORAssem:
                    return IsPublic( methodDef.DeclaringType );

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException(
                        methodDef.Attributes & MethodAttributes.MemberAccessMask,
                        "methodDef.Attributes" );
            }
        }
    }
}