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
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents the type of a pointer (managed or unmanaged).
    /// </summary>
    public sealed class PointerTypeSignature : TypeSignature
    {
        #region Fields

        /// <summary>
        /// Determines wether the pointer is managed.
        /// </summary>
        private readonly bool managed;

        /// <summary>
        /// Type of the instance referenced to by the pointer.
        /// </summary>
        private readonly ITypeSignatureInternal innerType;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="PointerTypeSignature"/>.
        /// </summary>
        /// <param name="managed">Determines whether the pointer is managed.</param>
        /// <param name="innerType">Type of the instance referenced to by the pointer.</param>
        public PointerTypeSignature( ITypeSignature innerType, bool managed )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( innerType, "innerType" );

            #endregion

            this.managed = managed;
            this.innerType = (ITypeSignatureInternal) innerType;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Concat( this.innerType.ToString(), this.IsManaged ? "&" : "*" );
        }


        /// <summary>
        /// Determines whether the pointer is managed.
        /// </summary>
        public bool IsManaged { get { return this.managed; } }

        #region TypeSignature implementation

        /// <inheritdoc />
        internal override bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap, IsAssignableToOptions options )
        {
            if ( this.Equals( signature ) )
            {
                return true;
            }

            if ( (options & IsAssignableToOptions.DisallowUnconditionalObjectAssignability) == 0 & 
                IntrinsicTypeSignature.Is( signature, IntrinsicType.Object ) )
            {
                return !this.managed;
            }

            PointerTypeSignature pointerType = signature as PointerTypeSignature;
            if ( pointerType != null )
            {
                if ( pointerType.managed == this.managed )
                {
                    if ( this.ElementType.BelongsToClassification( TypeClassifications.ValueType ) ==
                         pointerType.ElementType.BelongsToClassification( TypeClassifications.ValueType ) )
                    {
                        return
                            ((ITypeSignatureInternal)this.ElementType.GetNakedType(TypeNakingOptions.IgnoreOptionalCustomModifiers | TypeNakingOptions.IgnorePinned)).IsAssignableTo(pointerType.ElementType.GetNakedType(TypeNakingOptions.IgnoreOptionalCustomModifiers | TypeNakingOptions.IgnorePinned),
                                                                                          genericMap, options );
                    }
                }
            }

            return false;
        }

        internal override ITypeSignature GetNakedType(TypeNakingOptions options)
        {
            if ( (options & TypeNakingOptions.IgnoreManagedPointers) != 0 && this.managed )
            {
                return this.ElementType.GetNakedType(options);
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            this.ElementType.WriteReflectionTypeName( stringBuilder, options );

            if ( ( options & ReflectionNameOptions.MethodParameterEncoding ) != 0 )
            {
                if ( this.managed )
                {
                    stringBuilder.Append( " ByRef" );
                }
                else
                {
                    stringBuilder.Append( "*" );
                }
            }
            else
            {
                stringBuilder.Append( this.managed ? '&' : '*' );
            }
        }

        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.Any:
                case TypeClassifications.Pointer:
                case TypeClassifications.Signature:
                case TypeClassifications.ValueType:
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
                return new PointerTypeSignature( this.innerType.MapGenericArguments( genericMap ), this.managed );
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the type of the instance referenced to by pointers of this type.
        /// </summary>
        public override ITypeSignature ElementType { get { return this.innerType; } }

        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            this.innerType.WriteILReference( writer,
                                             writer.Options.InMethodBody ? GenericMap.Empty : genericMap, options );

            if ( this.IsManaged )
            {
                writer.WriteSymbol( '&' );
            }
            else
            {
                writer.WriteSymbol( '*' );
            }
        }

        /// <inheritdoc />
        public override int GetValueSize( PlatformInfo platform )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( platform, "platform" );

            #endregion

            return platform.NativePointerSize;
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new PointerTypeSignatureWrapper( this, genericTypeArguments, genericMethodArguments );
        }


        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            Type innerreflectionType = this.innerType.GetSystemType( genericTypeArguments, genericMethodArguments );

            if ( innerreflectionType == null )
            {
                return null;
            }
            else if ( this.managed )
            {
                return innerreflectionType.MakeByRefType();
            }
            else
            {
                return innerreflectionType.MakePointerType();
            }
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
                return new PointerTypeSignature( this.innerType.Translate( targetModule ), this.managed );
            }

            #endregion
        }

        #endregion

        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            PointerTypeSignature referenceAsPointer = reference as PointerTypeSignature;
            if (referenceAsPointer == null)
                return false;

            return this.managed == referenceAsPointer.managed &&
                   ((ITypeSignatureInternal) this.innerType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers )).Equals( referenceAsPointer.innerType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), isReference );
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            return HashCodeHelper.CombineHashCodes(this.managed ? 1 : 2, this.innerType.GetCanonicalHashCode());
        }
    }
}
