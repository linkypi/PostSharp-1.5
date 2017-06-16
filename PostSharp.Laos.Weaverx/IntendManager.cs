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
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Managed the <i>intends</i>, i.e. what aspect <i>intend</i> to do in their 
    /// implementation phase. Intends are typically added during the initialization
    /// phase and are verified by the <see cref="LaosAspectWeaver.ValidateInteractions"/>
    /// method.
    /// </summary>
    public static class IntendManager
    {
        private static readonly Guid intendTag = new Guid( "{DE4FD6B4-0AAC-47b0-B0A6-9FA65C56F7E5}" );


        /// <summary>
        /// Adds an intend.
        /// </summary>
        /// <param name="target">Declaration to which the intend applies.</param>
        /// <param name="intend">The intend.</param>
        public static void AddIntend( MetadataDeclaration target, string intend )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( target, "target" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( intend, "intend" );

            #endregion

            Set<string> intends = (Set<string>) target.GetTag( intendTag );
            if ( intends == null )
            {
                intends = new Set<string>();
                target.SetTag( intendTag, intends );
                intends.Add( intend );
            }
            else
            {
                if ( !intends.Contains( intend ) )
                {
                    intends.Add( intend );
                }
            }
        }

        /// <summary>
        /// Determines whether a given intend has been applied to a given declaration.
        /// </summary>
        /// <param name="target">Declaration to which the intend would apply.</param>
        /// <param name="intend">The intend.</param>
        /// <returns><b>true</b> if <paramref name="intend"/> applies on <paramref name="target"/>,
        /// otherwise <b>false</b>.</returns>
        public static bool HasIntend( MetadataDeclaration target, string intend )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( target, "target" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( intend, "intend" );

            #endregion

            Set<string> intends = (Set<string>) target.GetTag( intendTag );
            return intends == null ? false : intends.Contains( intend );
        }

        /// <summary>
        /// Determines whether a given intend has been applied to a given type
        /// or to one of its parent types.
        /// </summary>
        /// <param name="targetType">Type to which the intend would apply.</param>
        /// <param name="intend">The intend.</param>
        /// <returns><b>true</b> if <paramref name="intend"/> applies on <paramref name="targetType"/>
        /// or one of its base types, otherwise <b>false</b>.</returns>
        public static bool HasInheritedIntend( IType targetType, string intend )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetType, "targetType" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( intend, "intend" );

            #endregion

            TypeDefDeclaration targetTypeDef = targetType.GetTypeDefinition();
            if ( targetTypeDef.Module != targetType.Module )
            {
                return false;
            }

            if ( HasIntend( targetTypeDef, intend ) )
            {
                return true;
            }
            else
            {
                IType baseType = targetTypeDef.BaseType;

                if ( baseType != null )
                {
                    return HasInheritedIntend( baseType, intend );
                }
                else
                {
                    // We look only in the current module.
                    return false;
                }
            }
        }
    }
}