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
using PostSharp.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents a type modified by a custom modifier.
    /// </summary>
    public sealed class CustomModifierTypeSignature : TypeSignature
    {
        #region Fields

        /// <summary>
        /// The custom modifier.
        /// </summary>
        private readonly CustomModifier customModifier;

        /// <summary>
        /// The modified type.
        /// </summary>
        private readonly ITypeSignatureInternal innerType;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="CustomModifierTypeSignature"/>.
        /// </summary>
        /// <param name="customModifier">The custom modifier.</param>
        /// <param name="innerType">The modified type.</param>
        public CustomModifierTypeSignature( CustomModifier customModifier, ITypeSignature innerType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( innerType, "innerType" );

            #endregion

            this.customModifier = customModifier;
            this.innerType = (ITypeSignatureInternal) innerType;
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            this.ElementType.WriteReflectionTypeName( stringBuilder, options );
        }

        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            return this.innerType.BelongsToClassification( typeClassification );
        }

        /// <inheritdoc />
        public override bool ContainsGenericArguments()
        {
            return this.innerType.ContainsGenericArguments();
        }

        /// <inheritdoc />
        public override ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                string.Concat( this.innerType.ToString(), customModifier.Required ? "reqmod(" : "optmod(",
                               customModifier.Type.ToString(), ") " );
        }

        /// <summary>
        /// Gets the custom modifier.
        /// </summary>
        public CustomModifier Modifier { get { return this.customModifier; } }

        #region TypeSignature implementation

        /// <inheritdoc />
        public override void Visit( string role, Visitor<ITypeSignature> visitor )
        {
            base.Visit( role, visitor );
            ExceptionHelper.AssertArgumentNotNull( visitor, "visitor" );

            this.customModifier.Type.Visit( role, visitor );
            this.innerType.Visit( role, visitor );
        }


        /// <summary>
        /// Gets the modified type.
        /// </summary>
        public override ITypeSignature ElementType { get { return this.innerType; } }

        /// <inheritdoc />
        public override int GetValueSize( PlatformInfo platform )
        {
            return this.innerType.GetValueSize( platform );
        }

        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            this.innerType.WriteILReference( writer, genericMap, options );

            if ( this.customModifier.Required )
            {
                writer.WriteKeyword( "modreq" );
                writer.WriteSymbol( '(' );
            }
            else
            {
                writer.WriteKeyword( "modopt" );
                writer.WriteSymbol( '(' );
            }

            ( (ITypeSignatureInternal) this.customModifier.Type ).WriteILReference( writer, genericMap,
                                                                                    WriteTypeReferenceOptions.None );

            writer.WriteSymbol( ')' );
        }

        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.innerType.GetSystemType( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.innerType.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }


        /// <inheritdoc />
        internal override ITypeSignature GetNakedType( TypeNakingOptions options )
        {
            if ( this.customModifier.Required )
            {
                if ( ( options & TypeNakingOptions.IgnoreRequiredCustomModifiers ) != 0 )
                {
                    return this.innerType.GetNakedType(options);
                }
            }
            else
            {
                if ( ( options & TypeNakingOptions.IgnoreOptionalCustomModifiers ) != 0 )
                {
                    return this.innerType.GetNakedType(options);
                }
            }

            return this;

        }

        /// <inheritdoc />
        public override ModuleDeclaration Module { get { return this.customModifier.Type.Module; } }

        /// <inheritdoc />
        public override ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            ExceptionHelper.Core.AssertValidArgument( targetModule.Domain == this.Module.Domain,
                                                      "targetModule", "ModuleInSameDomain" );

            #endregion

            if ( targetModule == this.Module )
            {
                return this;
            }
            else
            {
                return
                    new CustomModifierTypeSignature( this.customModifier.Translate( targetModule ),
                                                     this.innerType.Translate( targetModule ) );
            }
        }

        #endregion

        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            CustomModifierTypeSignature referenceAsModifier = reference as CustomModifierTypeSignature;
            if (referenceAsModifier == null)
                return false;

            if (!((ITypeSignatureInternal) referenceAsModifier.innerType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers )).Equals( referenceAsModifier.innerType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), isReference ))
                return false;

            return ((ITypeSignatureInternal) this.Modifier.Type).Equals( referenceAsModifier.Modifier.Type, isReference );
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            return HashCodeHelper.CombineHashCodes(this.innerType.GetCanonicalHashCode(), 
                this.Modifier.Type.GetCanonicalHashCode());
        }
    }
}
