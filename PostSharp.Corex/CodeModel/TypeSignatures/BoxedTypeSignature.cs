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

#region Using directives

using System;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents the type of a boxed value.
    /// </summary>
    public sealed class BoxedTypeSignature : TypeSignature
    {
        /// <summary>
        /// Boxed type.
        /// </summary>
        private readonly ITypeSignatureInternal innerType;

        /// <summary>
        /// Initializes a new <see cref="BoxedTypeSignature"/>.
        /// </summary>
        /// <param name="innerType">The type of values being boxed.</param>
        public BoxedTypeSignature( ITypeSignature innerType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( innerType, "innerType" );

            #endregion

            this.innerType = (ITypeSignatureInternal) innerType;
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            this.ElementType.WriteReflectionTypeName( stringBuilder, options );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Concat( "box(", this.innerType.ToString(), ")" );
        }

        #region TypeSignature implementation

        internal override ITypeSignature GetNakedType(TypeNakingOptions options)
        {
            return this;
        }

        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.ReferenceType:
                case TypeClassifications.Any:
                case TypeClassifications.Boxed:
                case TypeClassifications.Signature:
                    return true;

                default:
                    return false;
            }
        }


        /// <inheritdoc />
        public override bool ContainsGenericArguments()
        {
            return this.innerType.ContainsGenericArguments();
        }

        /// <inheritdoc />
        public override ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            if ( this.ContainsGenericArguments() )
            {
                return new BoxedTypeSignature( this.innerType.MapGenericArguments( genericMap ) );
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the type of values being boxed.
        /// </summary>
        public override ITypeSignature ElementType { get { return this.innerType; } }


        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            writer.WriteKeyword( "boxed" );
            this.innerType.WriteILReference( writer, genericMap, options | WriteTypeReferenceOptions.WriteTypeKind );
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.innerType.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }


        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.innerType.GetSystemType( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public override ModuleDeclaration Module { get { return this.innerType.Module; } }

        /// <inheritdoc />
        public override ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            this.AssertSameDomain( targetModule );

            #endregion

            if ( targetModule == this.Module )
            {
                return this;
            }
            else
            {
                return new BoxedTypeSignature( this.innerType.Translate( targetModule ) );
            }

            #endregion
        }

        #endregion

        /// <inheritdoc />
        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            BoxedTypeSignature referenceAsBox = reference as BoxedTypeSignature;
            if ( referenceAsBox == null )
                return false;

            return ((ITypeSignatureInternal)this.innerType.GetNakedType(TypeNakingOptions.IgnoreOptionalCustomModifiers | TypeNakingOptions.IgnorePinned).GetNakedType(TypeNakingOptions.IgnoreAllCustomModifiers)).Equals(referenceAsBox.innerType.GetNakedType(TypeNakingOptions.IgnoreOptionalCustomModifiers | TypeNakingOptions.IgnorePinned).GetNakedType(TypeNakingOptions.IgnoreAllCustomModifiers), isReference);
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            return HashCodeHelper.CombineHashCodes( 74, this.innerType.GetCanonicalHashCode() );
        }
    }
}
