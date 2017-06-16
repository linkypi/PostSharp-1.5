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
    /// Represents the type of a pinned pointer.
    /// </summary>
    public sealed class PinnedTypeSignature : TypeSignature
    {
        /// <summary>
        /// Pointer type.
        /// </summary>
        private readonly ITypeSignatureInternal pointerType;

        /// <summary>
        /// Initializes a new <see cref="PinnedTypeSignature"/>.
        /// </summary>
        /// <param name="pointerType">Type of the underlying pointer.</param>
        public PinnedTypeSignature(ITypeSignature pointerType)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(pointerType, "pointerType");

            #endregion

            this.pointerType = (ITypeSignatureInternal) pointerType;
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return string.Concat(this.pointerType.ToString(), " pinned");
        }

        internal override bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap, IsAssignableToOptions options )
        {
            // Should never be called.
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        internal override ITypeSignature GetNakedType(TypeNakingOptions options)
        {
            ITypeSignature nakedPointerType = this.pointerType.GetNakedType( options );

            if ((options & TypeNakingOptions.IgnorePinned) != 0)
            {
                return nakedPointerType;
            }

            return this;
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName(StringBuilder stringBuilder, ReflectionNameOptions options)
        {
            this.ElementType.WriteReflectionTypeName(stringBuilder, options);
        }

        #region TypeSignature implementation

        /// <inheritdoc />
        public override NullableBool BelongsToClassification(TypeClassifications typeClassification)
        {
            return this.pointerType.BelongsToClassification(typeClassification);
        }

        /// <inheritdoc />
        public override bool ContainsGenericArguments()
        {
            return this.pointerType.ContainsGenericArguments();
        }


        /// <inheritdoc />
        public override ITypeSignature MapGenericArguments(GenericMap genericMap)
        {
            if (this.ContainsGenericArguments())

            {
                return new PinnedTypeSignature(this.pointerType.MapGenericArguments(genericMap));
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the underlying pointer type.
        /// </summary>
        public override ITypeSignature ElementType
        {
            get { return this.pointerType; }
        }


        /// <inheritdoc />
        internal override void InternalWriteILReference(ILWriter writer, GenericMap genericMap,
                                                        WriteTypeReferenceOptions options)
        {
            this.pointerType.WriteILReference(writer, writer.Options.InMethodBody ? GenericMap.Empty : genericMap,
                                              options);
            writer.WriteKeyword("pinned");
        }

        /// <inheritdoc />
        public override int GetValueSize(PlatformInfo platform)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(platform, "platform");

            #endregion

            return platform.NativePointerSize;
        }

        /// <inheritdoc />
        public override Type GetSystemType(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.pointerType.GetSystemType(genericTypeArguments, genericMethodArguments);
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.pointerType.GetReflectionWrapper(genericTypeArguments, genericMethodArguments);
        }

        /// <inheritdoc />
        public override ModuleDeclaration Module
        {
            get { return this.pointerType.Module; }
        }


        /// <inheritdoc />
        public override ITypeSignature Translate(ModuleDeclaration targetModule)
        {
            #region Preconditions

            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(targetModule, "targetModule");
            this.AssertSameDomain(targetModule);

            #endregion

            if (targetModule == this.Module)
            {
                return this;
            }
            else
            {
                return new PinnedTypeSignature(this.pointerType.Translate(targetModule));
            }

            #endregion
        }

        #endregion

        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            PinnedTypeSignature referenceAsPinned = reference as PinnedTypeSignature;
            if (referenceAsPinned == null)
                return false;

            return ((ITypeSignatureInternal)this.pointerType.GetNakedType(TypeNakingOptions.IgnoreOptionalCustomModifiers)).Equals(referenceAsPinned.pointerType.GetNakedType(TypeNakingOptions.IgnoreOptionalCustomModifiers), isReference);
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            return HashCodeHelper.CombineHashCodes( 5, this.pointerType.GetCanonicalHashCode() );
        }
    }
}
