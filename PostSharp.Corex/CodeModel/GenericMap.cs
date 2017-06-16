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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Map generic arguments to their value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Generic arguments are always given by ordinal. The current object stores
    /// a map between ordinals and a type signature to which the ordinal is
    /// mapped.
    /// </para>
    /// <para>Do not rely on the <see cref="object.Equals(object)"/> method to test for equality.</para>
    /// </remarks>
    [SuppressMessage( "Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes" )]
    public struct GenericMap
    {
        /// <summary>
        /// List of generic type parameters.
        /// </summary>
        /// <value>
        /// A list, or <b>null</b> if there are no generic type parameters.
        /// </value>
        private readonly IList<ITypeSignature> genericTypeParameters;

        /// <summary>
        /// List of generic method parameters.
        /// </summary>
        /// <value>
        /// A list, or <b>null</b> if there are no generic method parameters.
        /// </value>
        private readonly IList<ITypeSignature> genericMethodParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMap"/> type with
        /// explicit values.
        /// </summary>
        /// <param name="genericTypeParameters">Generic type parameters, or <b>null</b>.</param>
        /// <param name="genericMethodParameters">Generic method parameters, or <b>null</b>.</param>
        public GenericMap( IList<ITypeSignature> genericTypeParameters, IList<ITypeSignature> genericMethodParameters )
        {
            this.genericTypeParameters = genericTypeParameters;
            this.genericMethodParameters = genericMethodParameters;
        }

        /// <summary>
        /// Initializes a new <see cref="GenericMap"/> and copies the type parameters
        /// of another <see cref="GenericMap"/>.
        /// </summary>
        /// <param name="typeGenericMap"><see cref="GenericMap"/> from which type
        /// parameters have to be copied.</param>
        /// <param name="genericMethodParameters">Generic method parameters, or <b>null</b>.</param>
        /// <remarks>
        /// This is typically used to complete a type-level context with generic
        /// method parameters.
        /// </remarks>
        public GenericMap( GenericMap typeGenericMap, IList<ITypeSignature> genericMethodParameters )
            : this( typeGenericMap.genericTypeParameters, genericMethodParameters )
        {
        }


        /// <summary>
        /// Gets an empty <see cref="GenericMap"/> (a context without generic parameters).
        /// </summary>
#pragma warning disable 649
        public static readonly GenericMap Empty;
#pragma warning restore 649

        /// <summary>
        /// Determines whether the current generic context is empty, i.e. has no generic argument
        /// or parameter at all.
        /// </summary>
        public bool IsEmpty
        {
            get { return this.genericMethodParameters == null && this.genericTypeParameters == null; }
        }

        /// <summary>
        /// Gets the number of generic type parameters.
        /// </summary>
        public int GenericTypeParameterCount
        {
            get { return this.genericTypeParameters == null ? 0 : this.genericTypeParameters.Count; }
        }

        /// <summary>
        /// Gets the type mapped to a given type generic parameter.
        /// </summary>
        /// <param name="ordinal">The parameter position.</param>
        /// <returns>The <see cref="ITypeSignature"/> mapped to this generic parameter.</returns>
        public ITypeSignature GetGenericTypeParameter( int ordinal )
        {
            if ( this.genericTypeParameters == null || this.genericTypeParameters.Count <= ordinal )
            {
                throw new ArgumentOutOfRangeException( "ordinal" );
            }
            else
            {
                return this.genericTypeParameters[ordinal];
            }
        }


        /// <summary>
        /// Gets the array of type generic arguments in the current <see cref="GenericMap"/>.
        /// </summary>
        /// <returns>An array of <see cref="ITypeSignature"/>, or <b>null</b> if the current
        /// <see cref="GenericMap"/> has no type generic argument.</returns>
        public ITypeSignature[] GetGenericTypeParameters()
        {
            if ( this.genericTypeParameters != null )
            {
                ITypeSignature[] array = new ITypeSignature[this.genericTypeParameters.Count];
                this.genericTypeParameters.CopyTo( array, 0 );
                return array;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the array of method generic arguments in the current <see cref="GenericMap"/>.
        /// </summary>
        /// <returns>An array of <see cref="ITypeSignature"/>, or <b>null</b> if the current
        /// <see cref="GenericMap"/> has no method generic argument.</returns>
        public ITypeSignature[] GetGenericMethodParameters()
        {
            if ( this.genericMethodParameters != null )
            {
                ITypeSignature[] array = new ITypeSignature[this.genericMethodParameters.Count];
                this.genericMethodParameters.CopyTo( array, 0 );
                return array;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the number of generic method parameters.
        /// </summary>
        public int GenericMethodParameterCount
        {
            get { return this.genericMethodParameters == null ? 0 : this.genericMethodParameters.Count; }
        }

        /// <summary>
        ///  Gets the type mapped to a given method generic parameter.
        /// </summary>
        /// <param name="ordinal">The parameter position.</param>
        /// <returns>The <see cref="ITypeSignature"/> mapped to this generic parameter.</returns>
        public ITypeSignature GetGenericMethodParameter( int ordinal )
        {
            if ( this.genericMethodParameters == null || this.genericMethodParameters.Count <= ordinal )
            {
                throw new ArgumentOutOfRangeException( "ordinal" );
            }

            return this.genericMethodParameters[ordinal];
        }

        /// <summary>
        /// Gets the type mapped to a generic parameter given its kind and ordinal.
        /// </summary>
        /// <param name="kind">The kind of generic parameter (type or method).</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns>The <see cref="ITypeSignature"/> mapped to this generic parameter.</returns>
        public ITypeSignature GetGenericParameter( GenericParameterKind kind, int ordinal )
        {
            switch ( kind )
            {
                case GenericParameterKind.Method:
                    return GetGenericMethodParameter( ordinal );

                case GenericParameterKind.Type:
                    return GetGenericTypeParameter( ordinal );

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( kind, "kind" );
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="GenericMap"/> contains a mapping for
        /// a given generic parameter.
        /// </summary>
        /// <param name="kind">Kind of generic parameter.</param>
        /// <param name="ordinal">Ordinal of the generic parameter.</param>
        /// <returns><b>true</b> if the current <see cref="GenericMap"/> contains a mapping for
        /// the generic parameter of kind <paramref name="kind"/> and ordinal <paramref name="ordinal"/>,
        /// otherwise <b>false</b>.</returns>
        public bool ContainsGenericParameter( GenericParameterKind kind, int ordinal )
        {
            switch ( kind )
            {
                case GenericParameterKind.Method:
                    return this.genericMethodParameters != null && this.genericMethodParameters.Count > ordinal;

                case GenericParameterKind.Type:
                    return this.genericTypeParameters != null && this.genericTypeParameters.Count > ordinal;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( kind, "kind" );
            }
        }

        /// <summary>
        /// Gets the type mapped to a given generic parameter given an <see cref="IGenericParameter"/>.
        /// </summary>
        /// <param name="genericParameter">A generic parameter.</param>
        /// <returns>The <see cref="ITypeSignature"/> mapped to this generic parameter.</returns>
        public ITypeSignature GetGenericParameter( IGenericParameter genericParameter )
        {
            ExceptionHelper.AssertArgumentNotNull( genericParameter, "genericParameter" );
            return this.GetGenericParameter( genericParameter.Kind, genericParameter.Ordinal );
        }

        /// <summary>
        /// Gets a <see cref="GenericMap"/> based on the current context,
        /// but remove method generic parameters.
        /// </summary>
        /// <returns>A <see cref="GenericMap"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        public GenericMap GetTypeContext()
        {
            return new GenericMap( this.genericTypeParameters, null );
        }

        /// <summary>
        /// Transform the current <see cref="GenericMap"/> using a second <see cref="GenericMap"/>.
        /// </summary>
        /// <param name="map">The <see cref="GenericMap"/> that should be applied on the
        /// current map.</param>
        /// <returns>A <see cref="GenericMap"/> where all generic arguments where mapped
        /// by <paramref name="map"/>.</returns>
        public GenericMap Apply( GenericMap map )
        {
            ITypeSignature[] typeArgs, methodArgs;

            if ( this.genericTypeParameters != null )
            {
                typeArgs = new ITypeSignature[this.genericTypeParameters.Count];
                for ( int i = 0; i < typeArgs.Length; i++ )
                {
                    typeArgs[i] = this.genericTypeParameters[i].MapGenericArguments( map );
                }
            }
            else
            {
                typeArgs = null;
            }

            if ( this.genericMethodParameters != null )
            {
                methodArgs = new ITypeSignature[this.genericMethodParameters.Count];
                for ( int i = 0; i < methodArgs.Length; i++ )
                {
                    methodArgs[i] = this.genericMethodParameters[i].MapGenericArguments( map );
                }
            }
            else
            {
                methodArgs = null;
            }

            return new GenericMap( typeArgs, methodArgs );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( "<" );
            if ( this.genericTypeParameters != null )
            {
                for ( int i = 0; i < this.genericTypeParameters.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        builder.Append( ", " );
                    }
                    if ( this.genericTypeParameters[i] != null )
                    {
                        builder.Append( this.genericTypeParameters[i].ToString() );
                    }
                    else
                    {
                        builder.Append( "null" );
                    }
                }
            }
            builder.Append( ">;<" );
            if ( this.genericMethodParameters != null )
            {
                for ( int i = 0; i < this.genericMethodParameters.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        builder.Append( ", " );
                    }
                    if ( this.genericMethodParameters[i] != null )
                    {
                        builder.Append( this.genericMethodParameters[i].ToString() );
                    }
                    else
                    {
                        builder.Append( "null" );
                    }
                }
            }
            builder.Append( ">" );

            return builder.ToString();
        }
    }
}