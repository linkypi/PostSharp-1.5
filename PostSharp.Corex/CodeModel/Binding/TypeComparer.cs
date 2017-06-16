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
using System.Collections;
using System.Collections.Generic;

namespace PostSharp.CodeModel.Binding
{
    /// <summary>
    /// Compares types (<see cref="IType"/>) using equivalence rules.
    /// </summary>
    public sealed class TypeComparer : IEqualityComparer<ITypeSignature>, IEqualityComparer<TypeSignature>,
                                       IEqualityComparer
    {
        private TypeComparer()
        {
        }

        private static readonly TypeComparer instance = new TypeComparer();

        /// <summary>
        /// Gets a singleton instance of <see cref="TypeComparer"/>.
        /// </summary>
        /// <returns></returns>
        public static TypeComparer GetInstance()
        {
            return instance;
        }


        /// <inheritdoc />
        public new bool Equals( object x, object y )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( x, "x" );
            ExceptionHelper.AssertArgumentNotNull( y, "y" );

            #endregion

            ITypeSignature leftType = x as ITypeSignature;
            ITypeSignature rightType = y as ITypeSignature;

            #region Preconditions

            if ( leftType == null )
            {
                throw new ArgumentException( "This argument should be of type IType.", "x" );
            }
            if ( leftType == null )
            {
                throw new ArgumentException( "This argument should be of type IType.", "y" );
            }

            #endregion

            return leftType.Equals( rightType );
        }


        /// <inheritdoc />
        public int GetHashCode( object obj )
        {
            ITypeSignatureInternal internalType = obj as ITypeSignatureInternal;

            if ( internalType == null )
            {
                throw new ArgumentException( "The object is either null either of the wrong type.", "obj" );
            }

            return this.GetHashCode( internalType );
        }


        /// <inheritdoc />
        public bool Equals( ITypeSignature x, ITypeSignature y )
        {
            if ( x == null || y == null )
                return false;

            return x.Equals( y );
        }


        /// <inheritdoc />
        public int GetHashCode( ITypeSignature obj )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( obj, "obj" );

            #endregion

            return obj.GetCanonicalHashCode();
        }


        /// <inheritdoc />
        public bool Equals( TypeSignature x, TypeSignature y )
        {
            if ( x == null || y == null )
                return false;

            return x.Equals( y );
        }


        /// <inheritdoc />
        public int GetHashCode( TypeSignature obj )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( obj, "obj" );

            #endregion

            return this.GetHashCode( (ITypeSignature) obj );
        }
    }
}