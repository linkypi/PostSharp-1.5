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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a type construction (pointer, array, generic type instance, intrinsic, ...).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A type signature is composed of chains of type signature elements. Elements are instances
    /// of types of the <see cref="PostSharp.CodeModel.TypeSignatures"/> namespace. All
    /// element types are derived from <see cref="TypeSignature"/>. The 
    /// <see cref="TypeSignature.ElementType"/> property gives the next element of the chain.
    /// </para>
    /// <para>
    /// For instance, the type signature <c>int * pinned</c> is
    /// a chain formed by <see cref="TypeSignatures.PinnedTypeSignature"/> ,
    /// <see cref="TypeSignatures.PointerTypeSignature"/> and 
    /// <see cref="TypeSignatures.IntrinsicTypeSignature"/>. This type signature
    /// would be instantiated by the following code:
    /// </para>
    /// <code>
    /// TypeSignature sig = TypeSignature.MakeIntrinsic(PrimitiveType.Int32).MakePointer(true).MakePinned();
    /// </code>
    /// <para>
    /// The <see cref="TypeSignature"/> type defines the common functionalities of
    /// all type signature elements.
    /// </para>
    /// <para>
    /// Instances of all types derived from <see cref="TypeSignature"/> are immutable.
    /// </para>
    /// </remarks>
    public abstract class TypeSignature : ITypeSignatureInternal
    {
        /// <summary>
        /// Sentinel type used to separate normal method parameters and
        /// variable method parameters.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104", Justification = "The type is immutable." )] internal static readonly TypeSignature Sentinel = new SpecialTypeSignature( "Sentinel" );

        /// <summary>
        /// Initializes a new <see cref="TypeSignature"/>.
        /// </summary>
        internal TypeSignature()
        {
        }

        /// <summary>
        /// Gets the textWriter <see cref="TypeSignature"/>, i.e. the next element of
        /// the chains of type signature element.
        /// </summary>
        /// <remarks>
        /// A <see cref="TypeSignature"/>, or <b>null</b> if there is no textWriter type.
        /// </remarks>
        public abstract ITypeSignature ElementType { get; }

        #region IType Members

        /// <inheritdoc />
        public abstract bool ContainsGenericArguments();

        /// <inheritdoc />
        public abstract ITypeSignature MapGenericArguments( GenericMap genericMap );

        /// <inheritdoc />
        public virtual string Name
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual string AssemblyQualifiedName
        {
            get { return this.GetSystemType( null, null ).AssemblyQualifiedName; }
        }

        /// <inheritdoc />
        public virtual IAssembly DeclaringAssembly
        {
            get { return null; }
        }

        /// <inheritdoc />
        public abstract NullableBool BelongsToClassification( TypeClassifications typeClassification );

        /// <inheritdoc />
        void ITypeSignatureInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                                      WriteTypeReferenceOptions options )
        {
            if ( ( options & WriteTypeReferenceOptions.SerializedTypeReference ) != 0 )
            {
                writer.WriteKeyword( "class" );
                writer.WriteQuotedString( this.AssemblyQualifiedName );
            }
            else
            {
                this.InternalWriteILReference( writer, genericMap, options );
            }
        }

        /// <inheritdoc />
        internal abstract void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options );

        /// <inheritdoc />
        public abstract Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <inheritdoc />
        public abstract Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <inheritdoc />
        public virtual int GetValueSize( PlatformInfo platform )
        {
            Trace.CodeModel.WriteLine(
                "Cannot determine the size of {0} because because it is not supported by this kind of type signature.",
                this );
            return -1;
        }

        /// <inheritdoc />
        public abstract ITypeSignature Translate( ModuleDeclaration targetModule );

        /// <inheritdoc />
        public abstract ModuleDeclaration Module { get; }

        /// <inheritdoc />
        public abstract void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options );

        /// <inheritdoc />
        public virtual void Visit( string role, Visitor<ITypeSignature> visitor )
        {
            ExceptionHelper.AssertArgumentNotNull( visitor, "visitor" );

            ITypeSignature innerType = this.ElementType;
            if ( innerType != null )
            {
                visitor( this, "ElementType", innerType );
                innerType.Visit( role, visitor );
            }
        }

        /// <inheritdoc />
        public bool IsAssignableTo( ITypeSignature type, GenericMap genericMap )
        {
            return
                ( (ITypeSignatureInternal)
                  this.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers | TypeNakingOptions.IgnorePinned ) )
                    .IsAssignableTo(
                    type.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers |
                                            TypeNakingOptions.IgnorePinned ), genericMap, IsAssignableToOptions.None );
        }

        /// <inheritdoc />
        public bool IsAssignableTo( ITypeSignature type )
        {
            return this.IsAssignableTo( type, this.Module.Cache.IdentityGenericMap );
        }


        bool ITypeSignatureInternal.IsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                    IsAssignableToOptions options )
        {
            return this.InternalIsAssignableTo( signature, genericMap, options );
        }

        internal abstract bool InternalEquals( ITypeSignature reference, bool isReference );

        bool ITypeSignatureInternal.Equals( ITypeSignature reference, bool strict )
        {
            return this.InternalEquals( reference, strict );
        }

        /// <inheritdoc />
        public abstract int GetCanonicalHashCode();


        internal virtual bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                      IsAssignableToOptions options )
        {
            if ( this.Equals( signature ) )
            {
                return true;
            }
            return false;
        }

     

        #endregion

        [Conditional( "ASSERT" )]
        internal void AssertSameDomain( ModuleDeclaration module )
        {
            ModuleDeclaration thisModule = this.Module;
            ExceptionHelper.Core.AssertValidArgument(
                thisModule == null || thisModule.Domain == module.Domain,
                "module",
                "ModuleInSameDomain" );
        }

        #region Equality

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
        ITypeSignature ITypeSignature.GetNakedType( TypeNakingOptions options )
        {
            return this.GetNakedType( options );
        }

        internal abstract ITypeSignature GetNakedType( TypeNakingOptions options );

        #endregion

        /// <inheritdoc />
        public TypeDefDeclaration GetTypeDefinition()
        {
            return this.InternalGetTypeDefinition( BindingOptions.Default );
        }

        /// <inheritdoc />
        public TypeDefDeclaration GetTypeDefinition( BindingOptions bindingOptions )
        {
            return this.InternalGetTypeDefinition( bindingOptions );
        }

        /// <inheritdoc />
        public bool Equals( ITypeSignature other )
        {
            return
                ( (ITypeSignatureInternal) this.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ) ).Equals
                    ( other.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), true );
        }

        /// <inheritdoc />
        public bool MatchesReference( ITypeSignature reference )
        {
            return
                ( (ITypeSignatureInternal) this.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ) ).Equals
                    ( reference.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), false );
        }

        /// <inheritdoc />
        protected virtual TypeDefDeclaration InternalGetTypeDefinition( BindingOptions bindingOptions )
        {
            if ((bindingOptions & BindingOptions.DontThrowException) == 0)
            {
                throw new NotSupportedException();
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Used exclusively to represent the sentinel <see cref="TypeSignature"/>.
        /// </summary>
        private sealed class SpecialTypeSignature : TypeSignature
        {
            /// <summary>
            /// Special type name.
            /// </summary>
            private readonly string name;

            /// <summary>
            /// Initializes a new <see cref="TypeSignature.SpecialTypeSignature"/>.
            /// </summary>
            /// <param name="name">NameWx`</param>
            public SpecialTypeSignature( string name )
            {
                this.name = name;
            }

            /// <inheritdoc />
            public override bool ContainsGenericArguments()
            {
                return false;
            }

            /// <inheritdoc />
            public override ITypeSignature MapGenericArguments( GenericMap genericMap )
            {
                return this;
            }

            public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
            {
                return NullableBool.False;
            }

            /// <inheritdoc />
            public override ITypeSignature ElementType
            {
                get { return null; }
            }

            /// <inheritdoc />
            public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
            {
                stringBuilder.Append( "#Sentinel" );
            }

            /// <inheritdoc />
            internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                             WriteTypeReferenceOptions options )
            {
                writer.WriteKeyword( "sentinel" );
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return this.name;
            }

            /// <inheritdoc />
            public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc />
            public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
            {
                throw new NotSupportedException();
            }


            /// <inheritdoc />
            public override ITypeSignature Translate( ModuleDeclaration targetModule )
            {
                return this;
            }

            /// <inheritdoc />
            public override ModuleDeclaration Module
            {
                get { return null; }
            }

            /// <inheritdoc />
            public override void Visit( string role, Visitor<ITypeSignature> visitor )
            {
            }

            internal override ITypeSignature GetNakedType( TypeNakingOptions options )
            {
                return this;
            }

            internal override bool InternalEquals( ITypeSignature reference, bool isReference )
            {
                return this == reference;
            }

            public override int GetCanonicalHashCode()
            {
                return this.name.GetHashCode();
            }
        }

        #region IGeneric Members

        /// <inheritdoc />
        public virtual GenericMap GetGenericContext( GenericContextOptions options )
        {
            return GenericMap.Empty;
        }

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
        bool IGeneric.IsGenericDefinition
        {
            get { return false; }
        }

        /// <inheritdoc />
        public virtual bool IsGenericInstance
        {
            get { return false; }
        }

        #endregion
    }
}