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
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a type specification (<see cref="TokenType.TypeSpec"/>).
    /// </summary>
    /// <remarks>
    /// Type specifications are owned by <see cref="ModuleDeclaration"/>.
    /// </remarks>
    public sealed class TypeSpecDeclaration : MetadataDeclaration, IType,
                                              IMemberRefResolutionScope,
                                              ITypeSignatureInternal, IGenericInstance

    {
        #region Fields

        /// <summary>
        /// Type signature.
        /// </summary>
        private ITypeSignatureInternal signature;

        /// <summary>
        /// Collection of field references defined on this type specification.
        /// </summary>
        private readonly FieldRefDeclarationCollection fieldRefs;

        /// <summary>
        /// Collection of method references defined on this type specification.
        /// </summary>
        private readonly MethodRefDeclarationCollection methodRefs;

        private TypeDefDeclaration typeDef;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="TypeSpecDeclaration"/>.
        /// </summary>
        public TypeSpecDeclaration()
        {
            this.fieldRefs = new FieldRefDeclarationCollection( this, "externalFields" );
            this.methodRefs = new MethodRefDeclarationCollection( this, "externalMethods" );
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.TypeSpec;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return signature.ToString();
        }

        /// <inheritdoc />
        [Browsable( false )]
        public FieldRefDeclarationCollection FieldRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.fieldRefs;
            }
        }

        /// <inheritdoc />
        IFieldCollection IType.Fields
        {
            get { return this.FieldRefs; }
        }

        /// <inheritdoc />
        public TypeAttributes Attributes
        {
            get { return this.GetTypeDefinition().Attributes; }
        }

        /// <inheritdoc />
        public bool IsSealed
        {
            get { return this.GetTypeDefinition().IsSealed; }
        }

        /// <inheritdoc />
        public bool IsInterface
        {
            get { return this.GetTypeDefinition().IsInterface; }
        }

        /// <inheritdoc />
        public bool IsAbstract
        {
            get { return this.GetTypeDefinition().IsAbstract; }
        }

        /// <inheritdoc />
        IType IType.DeclaringType
        {
            get { return null; }
        }

        TypeDefDeclaration ITypeSignature.GetTypeDefinition()
        {
            return this.signature.GetTypeDefinition();
        }

        /// <inheritdoc />
        [Browsable( false )]
        public MethodRefDeclarationCollection MethodRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.methodRefs;
            }
        }

        /// <inheritdoc />
        IMethodCollection IType.Methods
        {
            get { return this.MethodRefs; }
        }

        /// <summary>
        /// Gets or sets the type signature specifying the current type specification.
        /// </summary>
        [ReadOnly( true )]
        public ITypeSignature Signature
        {
            get { return signature; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );
                ExceptionHelper.Core.AssertValidOperation( this.Parent == null, "PropertyCannotBeChangedAfterParentSet" );

                #endregion

                signature = (ITypeSignatureInternal) value;
            }
        }

        #region IType Members

        /// <inheritdoc />
        public NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            return this.signature.BelongsToClassification( typeClassification );
        }

        /// <inheritdoc />
        public bool ContainsGenericArguments()
        {
            return this.signature.ContainsGenericArguments();
        }

        /// <inheritdoc />
        public ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            return this.signature.MapGenericArguments( genericMap );
        }

        /// <inheritdoc />
        int IGenericInstance.GenericArgumentCount
        {
            get
            {
                IGenericInstance inner = this.signature as IGenericInstance;
                if ( inner != null )
                {
                    return inner.GenericArgumentCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <inheritdoc />
        public int GetValueSize( PlatformInfo platform )
        {
            return this.signature.GetValueSize( platform );
        }

        /// <inheritdoc />
        public Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.signature.GetSystemType( genericTypeArguments, genericMethodArguments );
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemType( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.signature.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public ITypeSignature GetGenericArgument( int ordinal )
        {
            IGenericInstance inner = this.signature as IGenericInstance;
            if ( inner != null )
            {
                return inner.GetGenericArgument( ordinal );
            }
            else
            {
                throw ExceptionHelper.Core.CreateInvalidOperationException( "NotGenericDeclaration" );
            }
        }

        /// <inheritdoc />
        public bool IsGenericInstance
        {
            get
            {
                IGenericInstance inner = this.signature as IGenericInstance;
                if ( inner != null )
                {
                    return inner.IsGenericInstance;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <inheritdoc />
        public GenericMap GetGenericContext( GenericContextOptions options )
        {
            IGenericInstance inner = this.signature as IGenericInstance;
            if ( inner != null )
            {
                return inner.GetGenericContext( options );
            }
            else
            {
                return GenericMap.Empty;
            }
        }

        /// <inheritdoc />
        void ITypeSignatureInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                                      WriteTypeReferenceOptions options )
        {
            this.signature.WriteILReference( writer, genericMap, options );
        }

        /// <inheritdoc />
        ITypeSignature ITypeSignature.GetNakedType( TypeNakingOptions options )
        {
            return this.signature.GetNakedType( options );
        }

        /// <inheritdoc />
        public bool IsAssignableTo( ITypeSignature type, GenericMap genericMap )
        {
            return ( (ITypeSignatureInternal) this ).IsAssignableTo( type, genericMap, IsAssignableToOptions.None );
        }

        /// <inheritdoc />
        public bool IsAssignableTo(ITypeSignature type)
        {
            return this.IsAssignableTo(type, this.Module.Cache.IdentityGenericMap);
        }


        /// <inheritdoc />
        bool ITypeSignatureInternal.IsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                    IsAssignableToOptions options )
        {
            return this.signature.IsAssignableTo( signature, genericMap, options );
        }

        /// <inheritdoc />
        bool ITypeSignatureInternal.Equals( ITypeSignature reference, bool strict )
        {
            return this.signature.Equals( reference, true );
        }

        /// <inheritdoc />
        public int GetCanonicalHashCode()
        {
            return this.signature.GetCanonicalHashCode();
        }

        /// <inheritdoc />
        public ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            ExceptionHelper.Core.AssertValidArgument(
                targetModule.Domain == this.Domain, "module", "ModuleInSameDomain" );

            #endregion

            if ( targetModule == this.Module )
            {
                return this;
            }
            else
            {
                return this.signature.Translate( targetModule );
            }
        }

        /// <inheritdoc />
        public void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            this.signature.WriteReflectionTypeName( stringBuilder, options );
        }

        /// <inheritdoc />
        public TypeDefDeclaration GetTypeDefinition()
        {
            if ( this.typeDef == null )
            {
                this.typeDef = this.GetTypeDefinition( BindingOptions.Default );
            }
            return this.typeDef;
        }

        /// <inheritdoc />
        public TypeDefDeclaration GetTypeDefinition( BindingOptions bindingOptions )
        {
            GenericTypeInstanceTypeSignature genericTypeInstance =
                this.signature as GenericTypeInstanceTypeSignature;

            if ( genericTypeInstance != null )
            {
                INamedType genericDeclaration = genericTypeInstance.GenericDefinition;
                if ( genericDeclaration != null )
                {
                    return genericDeclaration.GetTypeDefinition( bindingOptions );
                }
                else
                {
                    return null;
                }
            }

            ArrayTypeSignature arrayTypeSignature = this.signature as ArrayTypeSignature;
            if ( arrayTypeSignature != null )
            {
                return this.Domain.FindTypeDefinition( typeof(Array) );
            }

            if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                throw new InvalidOperationException();
            else
                return null;
        }

        /// <inheritdoc />
        bool IGeneric.IsGenericDefinition
        {
            get { return false; }
        }

        #endregion

        /// <summary>
        /// Conditionally unwraps the <see cref="ITypeSignature"/> contained in a <see cref="TypeSpecDeclaration"/>.
        /// </summary>
        /// <param name="signature">A signature (eventually a <see cref="TypeSpecDeclaration"/>).</param>
        /// <returns>If <paramref name="signature"/> is a <see cref="TypeSpecDeclaration"/>, the current
        /// method returns the signature contained in the <see cref="TypeSpecDeclaration"/>. Otherwise,
        /// <paramref name="signature"/> is returned.</returns>
        public static ITypeSignature Unwrap( ITypeSignature signature )
        {
            TypeSpecDeclaration typeSpec = signature as TypeSpecDeclaration;
            return typeSpec != null ? typeSpec.signature : signature;
        }

        #region Equality

        /// <inheritdoc />
        public void Visit( string role, Visitor<ITypeSignature> visitor )
        {
            ExceptionHelper.AssertArgumentNotNull( visitor, "visitor" );
            this.signature.Visit( role, visitor );
        }

        /// <inheritdoc />
        public bool Equals( ITypeSignature other )
        {
            if ( this == other ) return true;

            return
                ( (ITypeSignatureInternal)
                  ( (ITypeSignatureInternal) this ).GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ) ).
                    Equals( other.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), true );
        }

        /// <inheritdoc />
        public bool MatchesReference( ITypeSignature reference )
        {
            if ( this == reference ) return true;

            return ( (ITypeSignatureInternal) this ).Equals( reference, false );
        }

        #endregion

        /// <inheritdoc />
        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.fieldRefs.Dispose();
                this.methodRefs.Dispose();
            }
        }

        /// <summary>
        /// Gets all instances of <see cref="TypeSpecDeclaration"/> that have the same signature.
        /// </summary>
        /// <returns>The set of all instances of <see cref="TypeSpecDeclaration"/> 
        /// that have the same signature.</returns>
        /// <remarks>
        /// Compilers are supposed to generate a unique <see cref="TypeSpecDeclaration"/> for
        /// each signature. However, this rule is not always respected.
        /// </remarks>
        public IEnumerable<TypeSpecDeclaration> GetSiblings()
        {
            return this.Module.TypeSpecs.GetSiblingsBySignature( this );
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of type specifications (<see cref="TypeSpecDeclarationCollection"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class TypeSpecDeclarationCollection :
            ElementCollection<TypeSpecDeclaration>
        {
            private static readonly TypeSpecDeclarationCollectionImplFactory factory =
                new TypeSpecDeclarationCollectionImplFactory();


            /// <summary>
            /// Initializes a new instance of the <see cref="TypeSpecDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal TypeSpecDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }

            internal override ICollectionFactory<TypeSpecDeclaration> GetCollectionFactory()
            {
                return factory;
            }

            /// <summary>
            /// Gets the type specification corresponding to a given <see cref="IType"/>
            /// but does not create it if it is not found.
            /// </summary>
            /// <param name="signature">A <see cref="IType"/>.</param>
            /// <returns>The <see cref="TypeSpecDeclaration"/> corresponding to
            /// <paramref name="signature"/>, or <b>null</b> if no type specification
            /// in the collection corresponds to the current signature.</returns>
            public TypeSpecDeclaration GetBySignature( ITypeSignature signature )
            {
                return this.GetBySignature( signature, false );
            }

            /// <summary>
            /// Gets the type specification corresponding to a given <see cref="IType"/>
            /// and specifies whether to create it if it is not found.
            /// </summary>
            /// <param name="signature">A <see cref="IType"/>.</param>
            /// <param name="create">Whether the <see cref="TypeSpecDeclaration"/> should be created if not found.</param>
            /// <returns>The <see cref="TypeSpecDeclaration"/> corresponding to
            /// <paramref name="signature"/>, or <b>null</b> if no type specification
            /// in the collection corresponds to the current signature and the <paramref name="create"/> parameter is <b>false</b>.</returns>
            public TypeSpecDeclaration GetBySignature( ITypeSignature signature, bool create )
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidArgument(
                    signature.Module == null || signature.Module == ( (MetadataDeclaration) this.Owner ).Module,
                    "signature", "SignatureDoesNotBelongToCurrentModule" );

                #endregion

                TypeSpecDeclaration typeSpec;

                if ( this.Implementation == null )
                {
                    typeSpec = null;
                }
                else
                {
                    typeSpec =
                        ( (TypeSpecDeclarationCollectionImpl) this.Implementation ).GetFirstBySignature( signature );
                }

                if ( typeSpec == null && create )
                {
                    typeSpec = new TypeSpecDeclaration {Signature = signature};
                    this.Add( typeSpec );
                }

                return typeSpec;
            }

            /// <summary>
            /// Gets all type specifications corresponding to a given <see cref="IType"/>.
            /// </summary>
            /// <param name="signature">A <see cref="IType"/>.</param>
            /// <returns>The set of <see cref="TypeSpecDeclaration"/> instances corresponding to
            /// <paramref name="signature"/>, or an empty collection if no type specification
            /// in the collection corresponds to <paramref name="signature"/>.</returns>
            public IEnumerable<TypeSpecDeclaration> GetSiblingsBySignature( ITypeSignature signature )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( signature, "signature" );
                ExceptionHelper.Core.AssertValidArgument(
                    signature.Module == null || signature.Module == ( (MetadataDeclaration) this.Owner ).Module,
                    "signature", "SignatureDoesNotBelongToCurrentModule" );

                #endregion

                if ( this.Implementation != null )
                {
                    return
                        ( (TypeSpecDeclarationCollectionImpl) this.Implementation ).GetAllBySignature( signature );
                }
                else
                {
                    return EmptyCollection<TypeSpecDeclaration>.GetInstance();
                }
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                ( (IModuleScoped) this.Owner ).Module.ModuleReader.ImportTypeSpecs();
            }

            private class SignatureIndex : Index<ITypeSignature, TypeSpecDeclaration>
            {
                public SignatureIndex()
                    : base(
                        new MultiDictionaryFactory<ITypeSignature, TypeSpecDeclaration>( TypeComparer.GetInstance() ) )
                {
                }

                protected override ITypeSignature GetItemKey( TypeSpecDeclaration value )
                {
                    return value.Signature;
                }
            }

            private class SignatureIndexFactory : IndexFactory<ITypeSignature, TypeSpecDeclaration>
            {
                public override ICollection<TypeSpecDeclaration> CreateCollection()
                {
                    return new SignatureIndex();
                }
            }

            private class TypeSpecDeclarationCollectionImpl : IndexedCollection<TypeSpecDeclaration>
            {
                private static readonly ICollectionFactory<TypeSpecDeclaration>[] collectionFactory =
                    new ICollectionFactory<TypeSpecDeclaration>[]
                        {
#if ORDERED_EMIT
                            new MetadataTokenIndexFactory<TypeSpecDeclaration>(),
#else
                            ListFactory<TypeSpecDeclaration>.Default,
#endif
                            new SignatureIndexFactory()
                        };

                public TypeSpecDeclarationCollectionImpl( int capacity )
                    : base( collectionFactory, capacity )
                {
                }

                public TypeSpecDeclaration GetFirstBySignature( ITypeSignature signature )
                {
                    TypeSpecDeclaration typeSpec;
                    this.TryGetFirstValueByKey( 1, signature, out typeSpec );
                    return typeSpec;
                }

                public IEnumerable<TypeSpecDeclaration> GetAllBySignature( ITypeSignature signature )
                {
                    return
                        this.GetValuesByKey( 1, signature );
                }
            }

            private class TypeSpecDeclarationCollectionImplFactory : IndexedCollectionFactory<TypeSpecDeclaration>
            {
                protected override IndexedCollection<TypeSpecDeclaration> CreateCollection( int capacity )
                {
                    return new TypeSpecDeclarationCollectionImpl( capacity );
                }
            }
        }
    }
}