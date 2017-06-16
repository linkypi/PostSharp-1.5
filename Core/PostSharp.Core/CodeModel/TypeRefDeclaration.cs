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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a type reference (<see cref="TokenType.TypeRef"/>). 
    /// </summary>
    /// <remarks>
    /// Type references are
    /// owned by types implementing the <see cref="ITypeRefResolutionScope"/>
    /// interface, i.e.
    /// <see cref="TypeRefDeclaration"/> (in case of nested types),
    /// <see cref="AssemblyRefDeclaration"/>, <see cref="ModuleRefDeclaration"/>
    /// and <see cref="ModuleDeclaration"/> (although the latest case should
    /// never occur).
    /// </remarks>
    public sealed class TypeRefDeclaration : NamedDeclaration, IMemberRefResolutionScope, INamedType,
                                             ITypeRefResolutionScope, ITypeSignatureInternal, IGenericDefinition,
                                             IWeakReferenceable
    {
        #region Fields

        /// <summary>
        /// Collection of type references.
        /// </summary>
        private readonly TypeRefDeclarationCollection typeRefs;

        /// <summary>
        /// Collection of field references.
        /// </summary>
        private readonly FieldRefDeclarationCollection fieldRefs;

        /// <summary>
        /// Collection of method references.
        /// </summary>
        private readonly MethodRefDeclarationCollection methodRefs;

        private TypeDefDeclaration cachedTypeDef;

        /// <summary>
        /// Runtime type corresponding to this type.
        /// </summary>
        /// <value>
        /// A <see cref="Type"/>, or <b>null</b> if the reflection type has not yet
        /// been resolved.
        /// </value>
        private Type cachedReflectionType;

        private TypeClassifications typeClassifications;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="TypeRefDeclaration"/>.
        /// </summary>
        public TypeRefDeclaration()
        {
            this.typeRefs = new TypeRefDeclarationCollection( this, "externalTypes" );
            this.fieldRefs = new FieldRefDeclarationCollection( this, "externalFields" );
            this.methodRefs = new MethodRefDeclarationCollection( this, "externalMethods" );
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.TypeRef;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if ( this.ResolutionScope is TypeRefDeclaration )
            {
                return this.ResolutionScope.ToString() + "::" + this.Name;
            }
            else
            {
                if ( this.ResolutionScope is ModuleDeclaration )
                {
                    return this.Name;
                }
                else
                {
                    return "[" + this.ResolutionScope.ToString() + "]" + this.Name;
                }
            }
        }

        #region Properties

        /// <summary>
        /// Gets the scope in which the current <see cref="TypeRefDeclaration"/>
        /// should be resolved.
        /// </summary>
        [Browsable( false )]
        public ITypeRefResolutionScope ResolutionScope
        {
            get { return (ITypeRefResolutionScope) this.Parent; }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public TypeRefDeclarationCollection TypeRefs
        {
            get
            {
                this.AssertNotDisposed();
                return this.typeRefs;
            }
        }

        INamedTypeCollection INamedType.NestedTypes
        {
            get { return this.typeRefs; }
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


        /// <inheritdoc />
        [ReadOnly( true )]
        public override IAssembly DeclaringAssembly
        {
            get
            {
                IAssembly parentAssembly = this.ResolutionScope as IAssembly;
                if ( parentAssembly != null )
                {
                    return parentAssembly;
                }

                ITypeSignature parentType = this.ResolutionScope as ITypeSignature;
                if ( parentType != null )
                {
                    return parentType.DeclaringAssembly;
                }

                IModuleInternal parentModule = this.ResolutionScope as IModuleInternal;
                if ( parentModule != null )
                {
                    return parentModule.Assembly;
                }

                throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidType",
                                                                           this.ResolutionScope.GetType(), "this.Parent",
                                                                           "IAssembly, ITypeSignature" );
            }
        }

        #endregion

        #region IType Members

        /// <summary>
        /// Gets the declaring type (i.e. the type in which the current type
        /// is nested), or <b>null</b> if the current type is not nested.
        /// </summary>
        public TypeRefDeclaration DeclaringType
        {
            get { return this.ResolutionScope as TypeRefDeclaration; }
        }

        /// <inheritdoc />
        IType IType.DeclaringType
        {
            get { return this.DeclaringType; }
        }


        /// <inheritdoc />
        bool ITypeSignatureInternal.IsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                    IsAssignableToOptions options )
        {
            ITypeSignatureInternal typeDef = this.GetTypeDefinition();
            ExceptionHelper.Core.AssertValidOperation( typeDef != null, "CannotResolveTypeRef", this );

            return typeDef.IsAssignableTo( signature, genericMap, options );
        }


        /// <inheritdoc />
        bool ITypeSignatureInternal.Equals( ITypeSignature reference, bool strict )
        {
            if ( this == reference )
                return true;

            INamedType namedReference = reference as INamedType;
            if ( namedReference == null )
                return false;

            return CompareHelper.Equals( this, namedReference, strict );
        }

        /// <inheritdoc />
        public bool Equals( ITypeSignature other )
        {
            if ( this == other ) return true;

            return
                ( (ITypeSignatureInternal) this ).Equals(
                    other.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), true );
        }

        /// <inheritdoc />
        public bool MatchesReference( ITypeSignature reference )
        {
            if ( this == reference ) return true;

            return
                ( (ITypeSignatureInternal) this ).Equals(
                    reference.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ), false );
        }

        /// <inheritdoc />
        public bool IsAssignableTo( ITypeSignature type, GenericMap genericMap )
        {
            return
                ( (ITypeSignatureInternal) this ).IsAssignableTo(
                    type.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers |
                                            TypeNakingOptions.IgnorePinned ), genericMap, IsAssignableToOptions.None );
        }

        /// <inheritdoc />
        public bool IsAssignableTo(ITypeSignature type)
        {
            return this.IsAssignableTo(type, this.Module.Cache.IdentityGenericMap);
        }


        /// <inheritdoc />
        public TypeDefDeclaration GetTypeDefinition()
        {
            return this.GetTypeDefinition( BindingOptions.Default );
        }

        /// <inheritdoc />
        public int GetCanonicalHashCode()
        {
            return this.Name.GetHashCode();
        }

        
        /// <inheritdoc />
        public TypeDefDeclaration GetTypeDefinition( BindingOptions bindingOptions )
        {
            if ( cachedTypeDef != null )
                return cachedTypeDef;

            TypeDefDeclaration typeDef;
            if ( this.DeclaringType != null )
            {
                TypeDefDeclaration enclosingType = this.DeclaringType.GetTypeDefinition( bindingOptions );
                if ( enclosingType == null )
                    return null;

                typeDef = enclosingType.Types.GetByName( this.Name );

                if ( typeDef == null )
                {
                    if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                    {
                        throw new BindingException( string.Format( "Cannot find the nested type '{0}' in type '{1}'.",
                                                                   this.Name, enclosingType.ToString() ) );
                    }

                    return null;
                }
            }
            else
            {
                AssemblyEnvelope assemblyEnvelope = this.DeclaringAssembly.GetAssemblyEnvelope();
                typeDef = assemblyEnvelope.GetTypeDefinition( this.Name, bindingOptions );

                if ( typeDef == null )
                    return null;
            }

            cachedTypeDef = typeDef;
            return typeDef;
        }

        /// <inheritdoc />
        public bool ContainsGenericArguments()
        {
            return false;
        }

        /// <inheritdoc />
        public ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            //ITypeSignature unboundMap = UnboundGenericHelper.Map(this, genericMap);
            //return unboundMap != null ? unboundMap : this;
            return this;
        }

        /// <summary>
        /// Gets the full type name according the conventions of <see cref="System.Reflection"/>
        /// (with the namespace and nesting type but without the assembly name).
        /// </summary>
        /// <returns>The full type name (with the namespace and nesting type but without the assembly name).</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This method has a non-trivial cost." )]
        public void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            if ( this.IsGenericDefinition && ( options & ReflectionNameOptions.IgnoreGenericTypeDefParameters ) == 0 &&
                 ( ( options & ReflectionNameOptions.EncodingMask ) != ReflectionNameOptions.MethodParameterEncoding ) )
            {
                this.GetTypeDefinition().WriteReflectionTypeName( stringBuilder, options );
            }
            else
            {
                if ( ( options & ReflectionNameOptions.EncodingMask ) != ReflectionNameOptions.MethodParameterEncoding )
                {
                    TypeRefDeclaration parentType = this.ResolutionScope as TypeRefDeclaration;
                    if ( parentType != null )
                    {
                        parentType.WriteReflectionTypeName( stringBuilder,
                                                            options & ~ReflectionNameOptions.UseAssemblyName );
                        stringBuilder.Append( "+" );
                    }
                }

                if ( ( options & ReflectionNameOptions.SkipNamespace ) == 0 )
                {
                    stringBuilder.Append( BindingHelper.EscapeReflectionName( this.Name ) );
                }
                else
                {
                    stringBuilder.Append(
                        BindingHelper.EscapeReflectionName( BindingHelper.GetTypeNameWithoutNamespace( this.Name ) ) );
                }

                if ( ( options & ReflectionNameOptions.UseAssemblyName ) != 0 )
                {
                    stringBuilder.Append( ", " );
                    stringBuilder.Append( this.DeclaringAssembly.FullName );
                }
            }
        }

        /// <inheritdoc />
        public Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            if ( this.cachedReflectionType == null )
            {
                StringBuilder builder = new StringBuilder();
                this.WriteReflectionTypeName( builder, ReflectionNameOptions.IgnoreGenericTypeDefParameters );
                this.cachedReflectionType =
                    this.DeclaringAssembly.GetSystemAssembly().GetType( builder.ToString(), true, false );
            }

            return this.cachedReflectionType;
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemType( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new TypeWrapper( this );
        }

        /// <inheritdoc />
        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.Any:
                    return true;

                case TypeClassifications.Array:
                case TypeClassifications.Boxed:
                case TypeClassifications.GenericParameter:
                case TypeClassifications.GenericTypeInstance:
                case TypeClassifications.Intrinsic:
                case TypeClassifications.MethodPointer:
                case TypeClassifications.Signature:
                case TypeClassifications.Module:
                case TypeClassifications.Pointer:
                    return NullableBool.False;

                case TypeClassifications.ReferenceType:
                    switch (
                        this.typeClassifications & ( TypeClassifications.ReferenceType | TypeClassifications.ValueType )
                        )
                    {
                        case TypeClassifications.ReferenceType:
                            return NullableBool.True;

                        case TypeClassifications.ValueType:
                            return NullableBool.False;
                    }
                    break;

                case TypeClassifications.ValueType:
                    switch (
                        this.typeClassifications & ( TypeClassifications.ReferenceType | TypeClassifications.ValueType )
                        )
                    {
                        case TypeClassifications.ReferenceType:
                            return NullableBool.False;

                        case TypeClassifications.ValueType:
                            return NullableBool.True;
                    }
                    break;
            }

            return this.GetTypeDefinition().BelongsToClassification( typeClassification );
        }


        /// <notSupported />
        [SuppressMessage( "Microsoft.Design", "CA1033",
            Justification = "This method is not implemented." )]
        int ITypeSignature.GetValueSize( PlatformInfo platform )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        void IVisitable<ITypeSignature>.Visit( string role, Visitor<ITypeSignature> visitor )
        {
        }

        /// <inheritdoc />
        void ITypeSignatureInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                                      WriteTypeReferenceOptions options )
        {
            AssemblyRefDeclaration assemblyRef = this.DeclaringAssembly as AssemblyRefDeclaration;

            if ( ( assemblyRef != null && assemblyRef.IsWeaklyReferenced ) ||
                 ( ( options & WriteTypeReferenceOptions.SerializedTypeReference ) != 0 ) )
            {
                writer.WriteKeyword( "class" );

                StringBuilder builder = new StringBuilder();
                this.WriteReflectionTypeName( builder, ReflectionNameOptions.UseAssemblyName );
                writer.WriteQuotedString( builder.ToString() );
            }
            else
            {
                if ( ( options & WriteTypeReferenceOptions.WriteTypeKind ) != 0 )
                {
                    if ( this.BelongsToClassification( TypeClassifications.ReferenceType ) )
                    {
                        writer.WriteKeyword( "class" );
                    }
                    else if ( this.BelongsToClassification( TypeClassifications.ValueType ) )
                    {
                        writer.WriteKeyword( "valuetype" );
                    }
                    else
                    {
                        throw ExceptionHelper.Core.CreateAssertionFailedException( "UnknownTypeKindOnTypeRef",
                                                                                   this.ToString() );
                    }
                }

                IAssemblyInternal parentAssembly = this.ResolutionScope as IAssemblyInternal;
                if ( parentAssembly != null )
                {
                    parentAssembly.WriteILReference( writer );
                }
                else
                {
                    IModuleInternal parentModule = this.ResolutionScope as IModuleInternal;
                    if ( parentModule != null )
                    {
                        parentModule.WriteILReference( writer );
                    }
                    else
                    {
                        ( (ITypeSignatureInternal) this.ResolutionScope ).WriteILReference( writer, genericMap,
                                                                                            WriteTypeReferenceOptions.
                                                                                                None );
                        writer.WriteSymbol( '/' );
                    }
                }

                writer.WriteDottedName( this.Name );
            }
        }

        /// <inheritdoc />
        ITypeSignature ITypeSignature.GetNakedType( TypeNakingOptions options )
        {
            return this;
        }

        /// <inheritdoc />
        public ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            return BindingHelper.TranslateTypeDefOrRef( this, targetModule );
        }

        #endregion

        #region IGenericDefinition Members

        /// <inheritdoc />
        public bool IsGenericDefinition
        {
            get { return this.Name.IndexOf( '`' ) > 0; }
        }

        /// <inheritdoc />
        bool IGeneric.IsGenericInstance
        {
            get { return false; }
        }

        /// <inheritdoc />
        public IGenericParameter GetGenericParameter( int ordinal )
        {
            #region Preconditions

            if ( ordinal >= this.GenericParameterCount )
            {
                throw new ArgumentOutOfRangeException( "ordinal" );
            }

            #endregion

            return this.Module.Cache.GetGenericParameter( ordinal, GenericParameterKind.Type );
        }

        /// <inheritdoc />
        public int GenericParameterCount
        {
            get
            {
                int pos = this.Name.IndexOf( '`' );
                if ( pos < 0 )
                {
                    return 0;
                }
                return Convert.ToInt32( this.Name.Substring( pos + 1 ), CultureInfo.InvariantCulture );
            }
        }

        #endregion

        #region IGeneric Members

        /// <inheritdoc />
        public GenericMap GetGenericContext( GenericContextOptions options )
        {
            if ( this.IsGenericDefinition )
            {
                if ( ( options & GenericContextOptions.ResolveGenericParameterDefinitions ) != 0 )
                {
                    return this.GetTypeDefinition().GetGenericContext( options );
                }
                else
                {
                    ITypeSignature[] parameters = new ITypeSignature[this.GenericParameterCount];
                    for ( int i = 0; i < parameters.Length; i++ )
                    {
                        parameters[i] = this.Module.Cache.GetGenericParameter( i, GenericParameterKind.Type );
                    }
                    return new GenericMap( parameters, null );
                }
            }
            else
            {
                return GenericMap.Empty;
            }
        }

        #endregion

        #region IWeakReferenceable Members

        /// <inheritdoc />
        [ReadOnly( true )]
        public bool IsWeaklyReferenced
        {
            get { return this.InternalIsWeaklyReferenced; }
            set { this.InternalIsWeaklyReferenced = value; }
        }

        /// <summary>
        /// Classifications of the referred type. We use only <see cref="PostSharp.CodeModel.TypeClassifications.ValueType"/>
        /// and <see cref="PostSharp.CodeModel.TypeClassifications.ReferenceType"/>. In case of ambiguity, many conflicting
        /// bits may be set; in this case, the method <see cref="BelongsToClassification"/> will request
        /// the type definition for classifications, which has the expensive side effect to load the referrenced assembly.
        /// </summary>
        [ReadOnly( true )]
        public TypeClassifications TypeClassifications
        {
            get { return typeClassifications; }
            set { typeClassifications = value; }
        }


        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.typeRefs.Dispose();
                this.fieldRefs.Dispose();
                this.methodRefs.Dispose();
            }
        }

        #endregion

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedTypeDef = null;
            this.cachedReflectionType = null;
            this.typeRefs.ClearCache();
            this.fieldRefs.ClearCache();
            this.methodRefs.ClearCache();

        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of type references (<see cref="TypeRefDeclarationCollection"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class TypeRefDeclarationCollection :
            UniquelyNamedElementCollection<TypeRefDeclaration>,
            INamedTypeCollection
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TypeRefDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal TypeRefDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }


            INamedType INamedTypeCollection.GetByName( string name )
            {
                return this.GetByName( name );
            }


            IEnumerator<INamedType> IEnumerable<INamedType>.GetEnumerator()
            {
                return EnumeratorEnlarger.EnlargeEnumerator<TypeRefDeclaration, INamedType>( this.GetEnumerator() );
            }


            internal TypeRefDeclaration FindType( string name, BindingOptions bindingOptions )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotEmptyOrNull( name, "name" );

                #endregion

                string[] nestedTypes = name.Split( '+' );
                TypeRefDeclaration parentTypeRef = null;

                for ( int i = 0; i < nestedTypes.Length; i++ )
                {
                    TypeRefDeclaration typeRef;

                    if ( parentTypeRef == null )
                    {
                        typeRef = this.GetByName( nestedTypes[i] );
                    }
                    else
                    {
                        typeRef = parentTypeRef.TypeRefs.GetByName( nestedTypes[i] );
                    }

                    if ( typeRef == null )
                    {
                        if ( ( bindingOptions & BindingOptions.ExistenceMask ) != BindingOptions.OnlyExisting )
                        {
                            typeRef = new TypeRefDeclaration
                                          {
                                              Name = nestedTypes[i],
                                              MetadataToken = MetadataToken.Null,
                                              IsWeaklyReferenced =
                                                  ( ( bindingOptions & BindingOptions.ExistenceMask ) ==
                                                    BindingOptions.WeakReference )
                                          };

                            if ( parentTypeRef == null )
                            {
                                this.Add( typeRef );
                            }
                            else
                            {
                                parentTypeRef.TypeRefs.Add( typeRef );
                            }
                        }
                        else
                        {
                            if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                            {
                                return null;
                            }
                            else
                            {
                                throw ExceptionHelper.Core.CreateBindingException( "CannotFindTypeInAssemblyRef",
                                                                                   name, this.Owner );
                            }
                        }
                    }
                    else if ( typeRef.IsWeaklyReferenced &&
                              ( bindingOptions & BindingOptions.ExistenceMask ) == BindingOptions.Default )
                    {
                        typeRef.IsWeaklyReferenced = false;
                    }

                    parentTypeRef = typeRef;
                }

                return parentTypeRef;
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                ITypeRefResolutionScope scope = (ITypeRefResolutionScope) this.Owner;
                scope.Module.ModuleReader.ImportTypeRefs( scope );
            }
        }
    }
}