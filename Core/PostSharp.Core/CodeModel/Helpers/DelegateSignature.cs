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
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Collections;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Represents a delegate signature.
    /// </summary>
    /// <remarks>
    /// The types used to define a signature can be valid inside a different
    /// module than the one into which the delegate shall be generated. The
    /// <see cref="DelegateBuilder"/> utility translates types automatically.
    /// </remarks>
    public sealed class DelegateSignature : Declaration, IEquatable<DelegateSignature>
    {
        private readonly ParameterDeclarationCollection parameters;
        private readonly GenericParameterDeclarationCollection genericParameters;
        private ITypeSignature returnType;


        /// <summary>
        /// Initializes a new <see cref="DelegateSignature"/>.
        /// </summary>
        /// <param name="module">Module in which the signature should be valid.</param>
        public DelegateSignature( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            this.Parent = module;
            this.Module = module;

            this.parameters = new ParameterDeclarationCollection( this, "parameters" );
            this.genericParameters = new GenericParameterDeclarationCollection( this, "genericParameters" );
            this.returnType = module.Cache.GetIntrinsic( IntrinsicType.Void );
        }

        /// <summary>
        /// Gets or sets the delegate return type.
        /// </summary>
        public ITypeSignature ReturnType
        {
            get { return returnType; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                returnType = value;
            }
        }

        /// <summary>
        /// Gets the list of parameters.
        /// </summary>
        /// <remarks>
        /// Parameters present in this list may be owned by other entities.
        /// It is not necessary to clone them before adding it to this collection.
        /// </remarks>
        public ParameterDeclarationCollection Parameters
        {
            get { return this.parameters; }
        }

        /// <summary>
        /// Gets the list of generic parameters.
        /// </summary>
        /// <remarks>
        /// Generic parameters present in this list may be owned by other entities.
        /// It is not necessary to clone them before adding it to this collection.
        /// </remarks>
        public GenericParameterDeclarationCollection GenericParameters
        {
            get { return this.genericParameters; }
        }

        #region Equality

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hashCode = HashCodeHelper.CombineHashCodes( this.parameters.Count, this.genericParameters.Count );

            for ( int i = 0; i < parameters.Count; i++ )
            {
                HashCodeHelper.CombineHashCodes( ref hashCode, this.parameters[i].ParameterType.GetCanonicalHashCode() );
                HashCodeHelper.CombineHashCodes( ref hashCode, (int) this.parameters[i].Attributes );
            }

            return hashCode;
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            return this.Equals( (DelegateSignature) obj );
        }

        /// <inheritdoc />
        public bool Equals( DelegateSignature other )
        {
            if ( other == null )
            {
                return false;
            }

            // Compare number of parameters.
            if ( this.parameters.Count != other.parameters.Count )
            {
                return false;
            }

            // Compare number of generic arguments.
            if ( this.genericParameters.Count != other.genericParameters.Count )
            {
                return false;
            }

            TypeComparer typeComparer = TypeComparer.GetInstance();


            // Compare return types.
            if ( !typeComparer.Equals( this.returnType, other.returnType ) )
            {
                return false;
            }

            // Compare parameter types.
            for ( int i = 0; i < this.parameters.Count; i++ )
            {
                if (
                    !typeComparer.Equals( this.parameters[i].ParameterType,
                                          other.parameters[i].ParameterType ) ||
                    this.parameters[i].Attributes != other.parameters[i].Attributes)
                {
                    return false;
                }
            }

            // Compare generic parameters.
            for ( int i = 0; i < this.genericParameters.Count; i++ )
            {
                if ( !this.genericParameters[i].Equals( other.genericParameters[i], true ) )
                {
                    return false;
                }
            }

            // They are equal.
            return true;
        }

        #endregion
    }
}