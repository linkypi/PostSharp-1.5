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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents the type of an array.
    /// </summary>
    /// <remarks>
    /// Arrays with a single dimension, no fixed size and zero lower bound are called
    /// <b>Vectors</b>. Normal arrays in C#, for instance, are vectors. Use the 
    /// <see cref="IsVector"/> property to determine whether an <see cref="ArrayTypeSignature"/>
    /// is a vector.
    /// </remarks>
    public sealed class ArrayTypeSignature : TypeSignature
    {
        #region Fields

        /// <summary>
        /// Array of dimension bounds.
        /// </summary>
        private readonly ArrayDimension[] dimensions;

        /// <summary>
        /// Type of array elements.
        /// </summary>
        private readonly ITypeSignatureInternal innerType;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ArrayTypeSignature"/>.
        /// </summary>
        /// <param name="innerType">Type of array elements.</param>
        /// <param name="dimensions">Array dimensions.</param>
        public ArrayTypeSignature( ITypeSignature innerType, ArrayDimension[] dimensions )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( innerType, "innerType" );
            ExceptionHelper.AssertArgumentNotNull( dimensions, "dimensions" );

            #endregion

            this.dimensions = dimensions;
            this.innerType = (ITypeSignatureInternal) innerType;
        }

        /// <summary>
        /// Initializes a new <see cref="ArrayTypeSignature"/> with default dimensions ([0..]).
        /// </summary>
        /// <param name="innerType">Type of array elements.</param>
        public ArrayTypeSignature( ITypeSignature innerType )
            : this( innerType, ArrayDimension.VectorDimensions )
        {
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            this.ElementType.WriteReflectionTypeName( stringBuilder, options );
            stringBuilder.Append( '[' );
            for ( int i = 0; i < this.dimensions.Length; i++ )
            {
                if ( i > 0 )
                {
                    stringBuilder.Append( "," );
                }

                ArrayDimension dimension = this.dimensions[i];
                if ( dimension.LowerBound == 0 )
                {
                    // Do nothing.
                }
                else if ( dimension.LowerBound == ArrayDimension.Unlimited )
                {
                    stringBuilder.Append( "*" );
                }
                else
                {
                    stringBuilder.Append( "?" );
                }
            }
            stringBuilder.Append( "]" );
        }

        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.ReferenceType:
                case TypeClassifications.Any:
                case TypeClassifications.Array:
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
                return new ArrayTypeSignature( this.innerType.MapGenericArguments( genericMap ), this.dimensions );
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.innerType.ToString() + "[" + new string( ',', this.dimensions.Length - 1 ) + "]";
        }

        /// <summary>
        /// Gets the array rank (number of dimensions).
        /// </summary>
        public int Rank
        {
            get { return this.dimensions.Length; }
        }

        /// <summary>
        /// Gets a dimension given its index.
        /// </summary>
        /// <param name="index">Dimension index.</param>
        /// <returns>The dimension at index <paramref name="index"/>.</returns>
        public ArrayDimension GetDimension( int index )
        {
            return this.dimensions[index];
        }

        /// <summary>
        /// Determines whether the current array is a vector.
        /// </summary>
        public bool IsVector
        {
            get
            {
                return this.dimensions.Length == 1 &&
                       this.dimensions[0].LowerBound == 0 &&
                       !this.dimensions[0].HasSize;
            }
        }

        #region TypeSignature implementation

        internal override bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                       IsAssignableToOptions options )
        {
            if ( this.Equals( signature ) )
            {
                return true;
            }

            if ( IntrinsicTypeSignature.Is( signature, IntrinsicType.Object ) )
            {
                return true;
            }

            ArrayTypeSignature arrayType = signature as ArrayTypeSignature;

            if ( arrayType != null )
            {
                if ( arrayType.Rank == this.Rank )
                {
                    bool thisElementIsValueType =
                        this.ElementType.BelongsToClassification( TypeClassifications.ValueType );
                    bool otherElementIsValueType =
                        arrayType.ElementType.BelongsToClassification( TypeClassifications.ValueType );

                    return ( thisElementIsValueType == otherElementIsValueType ) &&
                           ( (ITypeSignatureInternal)
                             this.ElementType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers |
                                                            TypeNakingOptions.IgnorePinned ) ).IsAssignableTo(
                               arrayType.ElementType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers |
                                                                   TypeNakingOptions.IgnorePinned ),
                               genericMap, options );
                }
                else
                {
                    return false;
                }
            }
            else
            {
                IType arrayClass =
                    (IType) this.Module.FindType( "System.Array, mscorlib", BindingOptions.WeakReference );
                if ( ( (ITypeSignatureInternal) arrayClass ).IsAssignableTo( signature, genericMap, options ) )
                {
                    return true;
                }

                INamedType listInterfaceDef =
                    (INamedType) this.Module.FindType( "System.Collections.Generic.IList`1, mscorlib",
                                                       BindingOptions.WeakReference |
                                                       BindingOptions.RequireGenericDefinition );
                GenericTypeInstanceTypeSignature listInterfaceSpec =
                    new GenericTypeInstanceTypeSignature( listInterfaceDef,
                                                          new[] {this.ElementType} );

                return ( (ITypeSignatureInternal) listInterfaceSpec ).IsAssignableTo( signature, genericMap, options );
            }
        }

        /// <summary>
        /// Gets the type of array elements.
        /// </summary>
        public override ITypeSignature ElementType
        {
            get { return this.innerType; }
        }

        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            Type innerreflectionType = this.innerType.GetSystemType( genericTypeArguments, genericMethodArguments );
            if ( innerreflectionType == null )
            {
                return null;
            }
            else
            {
                if ( this.IsVector )
                {
                    return innerreflectionType.MakeArrayType();
                }
                else
                {
                    return innerreflectionType.MakeArrayType( this.dimensions.Length );
                }
            }
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new ArrayTypeSignatureWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            this.innerType.WriteILReference( writer, genericMap, options | WriteTypeReferenceOptions.WriteTypeKind );
            writer.WriteSymbol( '[' );
            if ( !this.IsVector )
            {
                for ( int i = 0; i < this.dimensions.Length; i++ )
                {
                    if ( i > 0 )
                    {
                        writer.WriteSymbol( ", " );
                    }

                    if ( this.dimensions[i].HasLowerBound )
                    {
                        writer.WriteInteger( this.dimensions[i].LowerBound, IntegerFormat.Decimal );
                        writer.WriteSymbol( "..." );
                    }

                    if ( this.dimensions[i].HasSize )
                    {
                        writer.WriteInteger( this.dimensions[i].Size, IntegerFormat.Decimal );
                    }
                }
            }
            writer.WriteSymbol( ']' );
            writer.WriteSpace();
        }

        /// <inheritdoc />
        public override ModuleDeclaration Module
        {
            get { return this.innerType.Module; }
        }

        /// <inheritdoc />
        public override ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );

            #endregion

            if ( targetModule == this.Module )
            {
                return this;
            }
            else
            {
                return new ArrayTypeSignature( this.innerType.Translate( targetModule ), this.dimensions );
            }

            #endregion
        }

        internal override ITypeSignature GetNakedType( TypeNakingOptions options )
        {
            return this;
        }

        #endregion

        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            ArrayTypeSignature referenceAsArray = reference as ArrayTypeSignature;
            if ( referenceAsArray == null )
                return false;

            if (
                !( (ITypeSignatureInternal)
                   this.innerType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ) ).Equals(
                     referenceAsArray.innerType.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ),
                     isReference ) )
                return false;

            if ( this.dimensions.Length != referenceAsArray.dimensions.Length )
                return false;

            for ( int i = 0; i < this.dimensions.Length; i++ )
            {
                if ( !this.dimensions[i].Equals( referenceAsArray.dimensions[i] ) )
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            return HashCodeHelper.CombineHashCodes( this.innerType.GetCanonicalHashCode(), this.dimensions.Length );
        }
    }

    /// <summary>
    /// Specifies the bounds of a single dimension of an <see cref="ArrayTypeSignature"/>.
    /// </summary>
    [SuppressMessage( "Microsoft.Performance", "CA1815",
        Justification = "The Equals method will not be used." )]
    public struct ArrayDimension : IEquatable<ArrayDimension>
    {
        #region Fields

        /// <summary>
        /// When set to <see cref="LowerBound"/> or <see cref="Size"/>, specifies
        /// that the value is unbounded.
        /// </summary>
        public const int Unlimited = int.MaxValue;

        /// <summary>
        /// Lower bound, or <see cref="Unlimited"/>.
        /// </summary>
        private readonly int lowerBound;

        /// <summary>
        /// Number of elements, or <see cref="Unlimited"/>.
        /// </summary>
        private readonly int size;

        /// <summary>
        /// Dimensions of a vector.
        /// </summary>
        internal static readonly ArrayDimension[] VectorDimensions =
            new[] {new ArrayDimension( 0, Unlimited )};

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ArrayDimension"/>.
        /// </summary>
        /// <param name="lowerBound">Lower bound, or <see cref="Unlimited"/>.</param>
        /// <param name="size">Number of elements, or <see cref="Unlimited"/>.</param>
        public ArrayDimension( int lowerBound, int size )
        {
            this.lowerBound = lowerBound;
            this.size = size;
        }

        /// <summary>
        /// Gets the lower bound.
        /// </summary>
        /// <remarks>
        /// A signed interger, or <see cref="Unlimited"/> so specify that this
        /// dimension has no lower bound.
        /// </remarks>
        public int LowerBound
        {
            get { return lowerBound; }
        }

        /// <summary>
        /// Determines whether the dimension has a lower bound.
        /// </summary>
        public bool HasLowerBound
        {
            get { return lowerBound != Unlimited; }
        }

        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        /// <remarks>
        /// A singed integer, or <see cref="Unlimited"/> to specify that this
        /// dimension can have an unlimited number of elements.
        /// </remarks>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Determines whether the dimension has a limited number of elements.
        /// </summary>
        public bool HasSize
        {
            get { return size != Unlimited; }
        }

        #region Equality

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if ( obj is ArrayDimension )
            {
                return this.Equals( (ArrayDimension) obj );
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool Equals( ArrayDimension other )
        {
            return other.lowerBound == this.lowerBound && this.size == other.size;
        }

        /// <summary>
        /// Determines whether two instances are equal.
        /// </summary>
        /// <param name="left">An <see cref="ArrayDimension"/>.</param>
        /// <param name="right">An <see cref="ArrayDimension"/>.</param>
        /// <returns><b>true</b> if both instances are equal, otherwise <b>false</b>.</returns>
        public static bool operator ==( ArrayDimension left, ArrayDimension right )
        {
            return left.Equals( right );
        }


        /// <summary>
        /// Determines whether two instances are different.
        /// </summary>
        /// <param name="left">An <see cref="ArrayDimension"/>.</param>
        /// <param name="right">An <see cref="ArrayDimension"/>.</param>
        /// <returns><b>true</b> if both instances are different, otherwise <b>false</b>.</returns>
        public static bool operator !=( ArrayDimension left, ArrayDimension right )
        {
            return !left.Equals( right );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.size + this.lowerBound;
        }

        #endregion
    }
}