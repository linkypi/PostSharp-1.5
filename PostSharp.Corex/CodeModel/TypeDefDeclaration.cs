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
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a type.
    /// </summary>
    /// <remarks>
    ///  Types may be contained by other types (<see cref="TypeDefDeclaration"/>)
    /// or modules (<see cref="ModuleDeclaration"/>).
    /// </remarks>
    public sealed class TypeDefDeclaration : NamedDeclaration, INamedType, IGenericDefinitionDefinition,
                                             IWriteILDefinition, ISecurable, ITypeSignatureInternal, ITypeContainer,
                                             IRemoveable, IMemberRefResolutionScope
    {
        /// <summary>
        /// Method name for instance constructors.
        /// </summary>
        public const string InstanceConstructorName = ".ctor";

        /// <summary>
        /// Method name for type constructors.
        /// </summary>
        public const string TypeConstructorName = ".cctor";


        /// <summary>
        /// When applied to the <see cref="TypeDefDeclaration.ExplicitTypeSize"/> 
        /// or <see cref="TypeDefDeclaration.ExplicitAlignment"/> property,
        /// specifies that the type size or alignment (packing size) is determined automatically 
        /// by the runtime according to the current platform.
        /// </summary>
        public const int Auto = 0;

        #region Fields

        /// <summary>
        /// Collection of methods.
        /// </summary>
        private readonly MethodDefDeclarationCollection methods;

        /// <summary>
        /// Collection of types.
        /// </summary>
        private readonly TypeDefDeclarationCollection nestedTypes;

        /// <summary>
        /// Collection of fields.
        /// </summary>
        private readonly FieldDefDeclarationCollection fields;

        /// <summary>
        /// Type attributes.
        /// </summary>
        private TypeAttributes attributes;

        /// <summary>
        /// Explicit field alignment, or <see cref="Auto"/>.
        /// </summary>
        private int explicitAlignment;

        /// <summary>
        /// Explicit type size, or <see cref="Auto"/>.
        /// </summary>
        private int explicitTypeSize;

        /// <summary>
        /// Base type.
        /// </summary>
        private IType baseType;

        /// <summary>
        /// Collection of permission sets.
        /// </summary>
        private readonly PermissionSetDeclarationCollection permissionSets;

        /// <summary>
        /// Collection of interfaces implemented by this type.
        /// </summary>
        private readonly InterfaceImplementationDeclarationCollection interfaceImplementations;

        /// <summary>
        /// Collection of properties.
        /// </summary>
        private readonly PropertyDeclarationCollection properties;

        /// <summary>
        /// Collection of events.
        /// </summary>
        private readonly EventDeclarationCollection events;

        /// <summary>
        /// Collection of generic parameters.
        /// </summary>
        private readonly GenericParameterDeclarationCollection genericParameters;

        /// <summary>
        /// Runtime type corresponding to this type.
        /// </summary>
        /// <value>
        /// A <see cref="Type"/>, or <b>null</b> if the reflection type has not yet
        /// been resolved.
        /// </value>
        private Type cachedReflectionType;

        private readonly FieldRefDeclarationCollection fieldRefs;
        private readonly MethodRefDeclarationCollection methodRefs;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="TypeDefDeclaration"/>.
        /// </summary>
        public TypeDefDeclaration()
        {
            this.methods = new MethodDefDeclarationCollection( this, "methods" );
            this.nestedTypes = new TypeDefDeclarationCollection( this, "nestedTypes" );
            this.fields = new FieldDefDeclarationCollection( this, "fields" );
            this.permissionSets = new PermissionSetDeclarationCollection( this, "permissionSets" );
            this.interfaceImplementations = new InterfaceImplementationDeclarationCollection( this,
                                                                                              "interfaceImplementations" );
            this.properties = new PropertyDeclarationCollection( this, "properties" );
            this.events = new EventDeclarationCollection( this, "events" );
            this.genericParameters = new GenericParameterDeclarationCollection( this, "genericParameters" );
            this.fieldRefs = new FieldRefDeclarationCollection( this, "fieldRefs" );
            this.methodRefs = new MethodRefDeclarationCollection( this, "methodRefs" );
        }

        /// <inheritdoc />
        protected override bool NotifyChildPropertyChanged( Element child, string property, object oldValue,
                                                            object newValue )
        {
            if ( base.NotifyChildPropertyChanged( child, property, oldValue, newValue ) )
                return true;

            if ( property == "Name" )
            {
                switch ( child.Role )
                {
                    case "methods":
                        if ( property == "Name" )
                            this.methods.OnItemNameChanged( (MethodDefDeclaration) child, (string) oldValue );
                        return true;

                    case "nestedTypes":
                        if ( property == "Name" )
                            this.nestedTypes.OnItemNameChanged( (TypeDefDeclaration) child, (string) oldValue );
                        return true;

                    case "fields":
                        if ( property == "Name" )
                            this.fields.OnItemNameChanged( (FieldDefDeclaration) child, (string) oldValue );
                        return true;

                    case "properties":
                        if ( property == "Name" )
                            this.properties.OnItemNameChanged( (PropertyDeclaration) child, (string) oldValue );
                        return true;

                    case "events":
                        if ( property == "Name" )
                            this.events.OnItemNameChanged( (EventDeclaration) child, (string) oldValue );
                        return true;

                    case "genericParameters":
                        if ( property == "Ordinal" )
                            this.genericParameters.OnItemOrdinalChanged( (GenericParameterDeclaration) child,
                                                                         (int) oldValue );
                        return true;

                    case "fieldRefs":
                        if ( property == "Name" )
                            this.fieldRefs.OnItemNameChanged( (FieldRefDeclaration) child, (string) oldValue );
                        return true;

                    case "methodRefs":
                        if ( property == "Name" )
                            this.methodRefs.OnItemNameChanged( (MethodRefDeclaration) child, (string) oldValue );
                        return true;
                }
            }

            return false;
        }


        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.TypeDef;
        }

        /// <summary>
        /// Gets the list of interfaces implemented directly or indirectly by the current type.
        /// </summary>
        /// <returns>The of interfaces implemented directly or indirectly by the current type.</returns>
        public Set<ITypeSignature> GetInterfacesRecursive()
        {
            Set<ITypeSignature> interfaces = new Set<ITypeSignature>( 4, TypeComparer.GetInstance() );

            this.InternalGetInterfacesRecurvise( interfaces, this.GetGenericContext( GenericContextOptions.None ) );
            return interfaces;
        }

        private void InternalGetInterfacesRecurvise( Set<ITypeSignature> interfaces, GenericMap genericMap )
        {
            ITypeSignature typeSpec = this.MapGenericArguments( genericMap );


            // Process the base type.
            if ( this.baseType != null )
            {
                GenericMap baseTypeGenericMap;
                TypeDefDeclaration baseTypeDef = this.baseType.GetTypeDefinition();

                GenericTypeInstanceTypeSignature baseGenericTypeInstance =
                    this.baseType.GetNakedType( TypeNakingOptions.None ) as GenericTypeInstanceTypeSignature;

                if ( baseGenericTypeInstance != null )
                {
                    baseTypeGenericMap =
                        new GenericMap( baseGenericTypeInstance.GenericArguments, null ).Apply( genericMap );
                }
                else
                {
                    baseTypeGenericMap = GenericMap.Empty;
                }

                baseTypeDef.InternalGetInterfacesRecurvise( interfaces, baseTypeGenericMap );
            }

            // Process the interfaces.
            if ( ( this.attributes & TypeAttributes.Interface ) == 0 ||
                 interfaces.AddIfAbsent( typeSpec ) )
            {
                foreach ( InterfaceImplementationDeclaration interf in this.InterfaceImplementations )
                {
                    GenericMap interfGenericMap;
                    TypeDefDeclaration interfTypeDef = interf.ImplementedInterface.GetTypeDefinition();

                    GenericTypeInstanceTypeSignature interfGenericTypeInstance =
                        interf.ImplementedInterface.GetNakedType( TypeNakingOptions.None ) as
                        GenericTypeInstanceTypeSignature;

                    if ( interfGenericTypeInstance != null )
                    {
                        interfGenericMap =
                            new GenericMap( interfGenericTypeInstance.GenericArguments, null ).Apply( genericMap );
                    }
                    else
                    {
                        interfGenericMap = GenericMap.Empty;
                    }

                    interfTypeDef.InternalGetInterfacesRecurvise( interfaces, interfGenericMap );
                }
            }
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
        public override string ToString()
        {
            if ( this.Parent is TypeDefDeclaration )
            {
                return this.Parent.ToString() + "::" + this.Name;
            }
            else
            {
                if ( this.Parent is ModuleDeclaration )
                {
                    return this.Name;
                }
                else
                {
                    return "[" + this.Parent.ToString() + "]" + this.Name;
                }
            }
        }

        #region Properties

        /// <summary>
        /// Gets the type visibility.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                switch ( this.attributes & TypeAttributes.VisibilityMask )
                {
                    case TypeAttributes.NestedAssembly:
                    case TypeAttributes.NotPublic:
                        return Visibility.Assembly;

                    case TypeAttributes.NestedFamANDAssem:
                        return Visibility.FamilyAndAssembly;

                    case TypeAttributes.NestedFamily:
                        return Visibility.Family;

                    case TypeAttributes.NestedFamORAssem:
                        return Visibility.FamilyOrAssembly;

                    case TypeAttributes.NestedPrivate:
                        return Visibility.Private;

                    case TypeAttributes.NestedPublic:
                    case TypeAttributes.Public:
                        return Visibility.Public;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( this.attributes, "this.Attributes" );
                }
            }
        }

        /// <summary>
        /// Gets the declaring type (i.e. the type in which the current type
        /// is nested), or <b>null</b> if the current type is not nested.
        /// </summary>
        public TypeDefDeclaration DeclaringType
        {
            get { return this.Parent as TypeDefDeclaration; }
        }

        /// <inheritdoc />
        IType IType.DeclaringType
        {
            get { return this.DeclaringType; }
        }

        /// <summary>
        /// Gets or sets the type attributes.
        /// </summary>
        [ReadOnly( true )]
        public TypeAttributes Attributes
        {
            get { return this.attributes; }
            set { this.attributes = value; }
        }

        /// <summary>
        /// Determines whether the type is sealed.
        /// </summary>
        public bool IsSealed
        {
            get { return ( this.attributes & TypeAttributes.Sealed ) != 0; }
        }

        /// <summary>
        /// Determines whether the type is an interface.
        /// </summary>
        public bool IsInterface
        {
            get { return ( this.attributes & TypeAttributes.Interface ) != 0; }
        }

        /// <summary>
        /// Determines whether the class is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return ( this.attributes & TypeAttributes.Abstract ) != 0; }
        }

        /// <summary>
        /// Gets or sets the field alignment (packing size).
        /// </summary>
        /// <value>
        /// Of of {1, 2, 4, 8, 16, 32, 64, 128}, or <see cref="Auto"/>
        /// to specify that the alignment shall be determined by the runtime.
        /// </value>
        [ReadOnly( true )]
        public int ExplicitAlignment
        {
            get { return this.explicitAlignment; }
            set { this.explicitAlignment = value; }
        }

        /// <summary>
        /// Gets or sets the type size.
        /// </summary>
        /// <value>
        /// A strictly positive integer, or <see cref="TypeDefDeclaration.Auto"/>
        /// to specify that the size shall be computed by the runtime.
        /// </value>
        [ReadOnly( true )]
        public int ExplicitTypeSize
        {
            get { return this.explicitTypeSize; }
            set { this.explicitTypeSize = value; }
        }

        /// <summary>
        /// Gets or sets the base type, from which the current type derives.
        /// </summary>
        [ReadOnly( true )]
        public IType BaseType
        {
            get { return baseType; }
            set { baseType = value; }
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
        bool ITypeSignatureInternal.IsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                    IsAssignableToOptions options )
        {
            if ( this.Equals( signature ) )
            {
                return true;
            }

            if ( ( options & IsAssignableToOptions.DisallowUnconditionalObjectAssignability ) == 0 &&
                 IntrinsicTypeSignature.Is( signature, IntrinsicType.Object ) )
            {
                return true;
            }

            if ( this.baseType != null &&
                 ( (ITypeSignatureInternal) this.baseType ).IsAssignableTo( signature, genericMap, options ) )
            {
                return true;
            }

            foreach ( InterfaceImplementationDeclaration interfaceImpl in this.InterfaceImplementations )
            {
                if ( ( (ITypeSignatureInternal) interfaceImpl.ImplementedInterface ).IsAssignableTo( signature,
                                                                                                     genericMap, options ) )
                {
                    return true;
                }
            }

            if ( this.BelongsToClassification( TypeClassifications.Enum ) )
            {
                // We have an enumeration.
                return ( (ITypeSignatureInternal) EnumHelper.GetUnderlyingType( this ) ).IsAssignableTo( signature,
                                                                                                         genericMap,
                                                                                                         options );
            }

            return false;
        }

        bool ITypeSignatureInternal.Equals( ITypeSignature reference, bool strict )
        {
            if ( this == reference ) return true;

            INamedType namedReference = reference as INamedType;
            if ( namedReference == null )
                return false;

            return CompareHelper.Equals( this, namedReference, strict );
        }

        /// <inheritdoc />
        public int GetCanonicalHashCode()
        {
            return this.Name.GetHashCode();
        }


        /// <summary>
        /// Gets the collection of methods (<see cref="MethodDefDeclaration"/>).
        /// </summary>
        [Browsable( false )]
        public MethodDefDeclarationCollection Methods
        {
            get
            {
                this.AssertNotDisposed();
                return this.methods;
            }
        }

        /// <inheritdoc />
        IMethodCollection IType.Methods
        {
            get { return this.Methods; }
        }

        /// <summary>
        /// Gets the collection of nested types (<see cref="TypeDefDeclaration"/>).
        /// </summary>
        [Browsable( false )]
        public TypeDefDeclarationCollection Types
        {
            get
            {
                this.AssertNotDisposed();
                return this.nestedTypes;
            }
        }

        INamedTypeCollection INamedType.NestedTypes
        {
            get { return this.nestedTypes; }
        }


        /// <summary>
        /// Gets the collection of fields (<see cref="FieldDefDeclaration"/>).
        /// </summary>
        [Browsable( false )]
        public FieldDefDeclarationCollection Fields
        {
            get
            {
                this.AssertNotDisposed();
                return this.fields;
            }
        }

        /// <inheritdoc />
        IFieldCollection IType.Fields
        {
            get { return this.Fields; }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public PermissionSetDeclarationCollection PermissionSets
        {
            get
            {
                this.AssertNotDisposed();
                return this.permissionSets;
            }
        }

        /// <summary>
        /// Gets the collection of interfaces implemented by the current type.
        /// </summary>
        [Browsable( false )]
        public InterfaceImplementationDeclarationCollection InterfaceImplementations
        {
            get { return this.interfaceImplementations; }
        }

        /// <summary>
        /// Gets the collection of properties (<see cref="PropertyDeclaration"/>) defined
        /// in the current type.
        /// </summary>
        [Browsable( false )]
        public PropertyDeclarationCollection Properties
        {
            get
            {
                this.AssertNotDisposed();
                return this.properties;
            }
        }

        /// <summary>
        /// Gets the collection of events (<see cref="EventDeclaration"/>) defined
        /// in the current type.
        /// </summary>
        [Browsable( false )]
        public EventDeclarationCollection Events
        {
            get
            {
                this.AssertNotDisposed();
                return this.events;
            }
        }

        /// <summary>
        /// Gets the collection of formal generic parameters of the current type.
        /// </summary>
        [Browsable( false )]
        public GenericParameterDeclarationCollection GenericParameters
        {
            get
            {
                this.AssertNotDisposed();
                return this.genericParameters;
            }
        }

        #endregion

        #region IType

        /// <inheritdoc />
        TypeDefDeclaration ITypeSignature.GetTypeDefinition()
        {
            return this;
        }

        /// <inheritdoc />
        TypeDefDeclaration ITypeSignature.GetTypeDefinition( BindingOptions bindingOptions )
        {
            return this;
        }


        /// <inheritdoc />
        public NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.Array:
                case TypeClassifications.Boxed:
                case TypeClassifications.GenericParameter:
                case TypeClassifications.GenericTypeInstance:
                case TypeClassifications.Intrinsic:
                case TypeClassifications.MethodPointer:
                case TypeClassifications.Signature:
                    return false;

                case TypeClassifications.Module:
                    return this.Name == "<Module>";
            }


            if ( this.baseType == null )
            {
                // The type is not derived. Use type attributes.
                TypeClassifications thisTypeClassification;
                if ( ( this.attributes & TypeAttributes.Interface ) != 0 )
                {
                    thisTypeClassification = TypeClassifications.Interface;
                }
                else
                {
                    thisTypeClassification = TypeClassifications.Class;
                }

                return TypeClassificationHelper.BelongsToClassification( thisTypeClassification, typeClassification );
            }
            else
            {
                // At this point, the type is not a base type, i.e. it has the
                // type of its base type. But if the base type is a root type,
                // then the type is different.


                if ( this.baseType.DeclaringAssembly.IsMscorlib )
                {
                    INamedType baseTypeNamed = this.baseType as INamedType;
                    if ( baseTypeNamed != null )
                    {
                        TypeClassifications rootTypeClassification =
                            TypeClassificationHelper.GetRootTypeClassification( baseTypeNamed.Name );
                        if ( rootTypeClassification != TypeClassifications.Any )
                        {
                            return
                                TypeClassificationHelper.BelongsToClassification( rootTypeClassification,
                                                                                  typeClassification );
                        }
                    }
                }


                return this.baseType.BelongsToClassification( typeClassification );
            }
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


        /// <inheritdoc />
        public int GetValueSize( PlatformInfo platform )
        {
            if ( this.ExplicitTypeSize != Auto )
            {
                return this.explicitTypeSize;
            }

            // We have to compute the type as the sum of the size of its fields.
            int size = 0;
            foreach ( FieldDefDeclaration field in this.fields )
            {
                if ( ( field.Attributes & ( FieldAttributes.Literal | FieldAttributes.Static ) ) != 0 )
                    continue;

                if ( field.FieldType.BelongsToClassification( TypeClassifications.ReferenceType ) )
                    size += platform.NativePointerSize;
                else
                    size += field.FieldType.GetValueSize( platform );
            }

            return size;
        }

        /// <summary>
        /// Determines whether the current type is the special type representing the module.
        /// </summary>
        public bool IsModuleSpecialType
        {
            get { return this.Name == "<Module>"; }
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
            if ( ( ( options & ReflectionNameOptions.EncodingMask ) != ReflectionNameOptions.MethodParameterEncoding )
                )
            {
                TypeDefDeclaration parentType = this.Parent as TypeDefDeclaration;
                if ( parentType != null )
                {
                    parentType.WriteReflectionTypeName( stringBuilder,
                                                        ( options | ReflectionNameOptions.IgnoreGenericTypeDefParameters )
                                                        & ~ReflectionNameOptions.UseAssemblyName );
                    stringBuilder.Append( '+' );
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

            if ( ( options & ReflectionNameOptions.UseAssemblyName ) != 0 && 
                 (options & ReflectionNameOptions.SerializedValue) == 0)
            {
                stringBuilder.Append( ", " );
                stringBuilder.Append( this.DeclaringAssembly.FullName );
            }

            if ( this.IsGenericDefinition && ( ( options & ReflectionNameOptions.IgnoreGenericTypeDefParameters ) == 0 ) &&
                 this.IsGenericDefinition &&
                 ( ( options & ReflectionNameOptions.EncodingMask ) != ReflectionNameOptions.MethodParameterEncoding ) )
            {
                bool useBrackets = ( options & ReflectionNameOptions.UseBracketsForGenerics ) != 0;
                stringBuilder.Append( useBrackets ? '[' : '<' );
                for ( int i = 0; i < this.genericParameters.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        stringBuilder.Append( ',' );
                    }
                    stringBuilder.Append( this.genericParameters[i].Name );
                }
                stringBuilder.Append( useBrackets ? ']' : '>' );
            }
        }

        /// <inheritdoc />
        public Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            if ( this.cachedReflectionType == null )
            {
                if ( this.IsModuleSpecialType )
                    return null;

                try
                {
                    Module module = this.Module.GetSystemModule();
                    this.cachedReflectionType = module != null
                                              ?
                                                  module.ResolveType( (int) this.MetadataToken.Value, null, null )
                                              : null;
                }
                catch ( ArgumentException )
                {
                    // Try to recover.
                    StringBuilder stringBuilder = new StringBuilder();
                    this.WriteReflectionTypeName( stringBuilder, ReflectionNameOptions.None );
                    this.cachedReflectionType =
                        this.DeclaringAssembly.GetSystemAssembly().GetType( stringBuilder.ToString(), true, false );
                }
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
        int IGenericDefinition.GenericParameterCount
        {
            get { return this.genericParameters.Count; }
        }

        /// <inheritdoc />
        IGenericParameter IGenericDefinition.GetGenericParameter( int ordinal )
        {
            return this.genericParameters[ordinal];
        }

        /// <inheritdoc />
        public bool IsGenericDefinition
        {
            get { return this.genericParameters.Count > 0; }
        }

        bool IGeneric.IsGenericInstance
        {
            get { return false; }
        }

        #region IMemberRefResolutionScope Members

        /// <inheritdoc />
        FieldRefDeclarationCollection IMemberRefResolutionScope.FieldRefs
        {
            get { return fieldRefs; }
        }

        /// <inheritdoc />
        MethodRefDeclarationCollection IMemberRefResolutionScope.MethodRefs
        {
            get { return methodRefs; }
        }

        #endregion

        /// <inheritdoc />
        ITypeSignature ITypeSignature.GetNakedType( TypeNakingOptions options )
        {
            return this;
        }

        /// <inheritdoc />
        void IVisitable<ITypeSignature>.Visit( string role, Visitor<ITypeSignature> visitor )
        {
        }

        /// <inheritdoc />
        public ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            return BindingHelper.TranslateTypeDefOrRef( this, targetModule );
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        public GenericMap GetGenericContext( GenericContextOptions options )
        {
            this.AssertNotDisposed();
            return new GenericMap( this.genericParameters, null );
        }


        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            this.AssertNotDisposed();

            Trace.ModuleWriter.WriteLine( "Writing type {{{0}}}.", this );

            GenericMap genericMap = this.GetGenericContext( GenericContextOptions.None );

            // Rename generic parameters if we have duplicates.
            if ( this.genericParameters.Count > 0 )
            {
                Set<string> genericParameterNames = new Set<string>( this.genericParameters.Count );
                foreach ( GenericParameterDeclaration genericParameter in this.genericParameters )
                {
                    if ( genericParameter.Name != null && !genericParameterNames.AddIfAbsent( genericParameter.Name ) )
                    {
                        genericParameter.Name = genericParameter.Name +
                                                string.Format( CultureInfo.InvariantCulture, "~{0}",
                                                               genericParameter.Ordinal );
                    }
                }
            }

            #region Class head

            writer.WriteKeyword( ".class" );

            #region Attributes

            if ( ( this.attributes & TypeAttributes.Interface ) != 0 )
            {
                writer.WriteKeyword( "interface" );
            }

            if ( ( this.attributes & TypeAttributes.VisibilityMask ) == TypeAttributes.Public )
            {
                writer.WriteKeyword( "public" );
            }

            if ( ( this.attributes & TypeAttributes.VisibilityMask ) == TypeAttributes.NotPublic )
            {
                writer.WriteKeyword( "private" );
            }

            if ( ( this.attributes & TypeAttributes.Abstract ) != 0 )
            {
                writer.WriteKeyword( "abstract" );
            }


            switch ( this.attributes & TypeAttributes.LayoutMask )
            {
                case TypeAttributes.AutoLayout:
                    writer.WriteKeyword( "auto" );
                    break;

                case TypeAttributes.SequentialLayout:
                    writer.WriteKeyword( "sequential" );
                    break;

                case TypeAttributes.ExplicitLayout:
                    writer.WriteKeyword( "explicit" );
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnknownPosition type layout." );
            }


            switch ( this.attributes & TypeAttributes.StringFormatMask )
            {
                case TypeAttributes.AnsiClass:
                    writer.WriteKeyword( "ansi" );
                    break;

                case TypeAttributes.UnicodeClass:
                    writer.WriteKeyword( "unicode" );
                    break;

                case TypeAttributes.AutoClass:
                    writer.WriteKeyword( "autochar" );
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "UnknownPosition class string format." );
            }


            if ( ( this.attributes & TypeAttributes.Import ) != 0 )
            {
                writer.WriteKeyword( "import" );
            }


            if ( ( this.attributes & TypeAttributes.Serializable ) != 0 )
            {
                writer.WriteKeyword( "serializable" );
            }


            if ( ( this.attributes & TypeAttributes.Sealed ) != 0 )
            {
                writer.WriteKeyword( "sealed" );
            }


            switch ( this.attributes & TypeAttributes.VisibilityMask )
            {
                case TypeAttributes.NestedAssembly:
                    writer.WriteKeyword( "nested assembly" );
                    break;

                case TypeAttributes.NestedPrivate:
                    writer.WriteKeyword( "nested private" );
                    break;

                case TypeAttributes.NestedFamily:
                    writer.WriteKeyword( "nested family" );
                    break;

                case TypeAttributes.NestedFamANDAssem:
                    writer.WriteKeyword( "nested famandassem" );
                    break;

                case TypeAttributes.NestedFamORAssem:
                    writer.WriteKeyword( "nested famorassem" );
                    break;

                case TypeAttributes.NestedPublic:
                    writer.WriteKeyword( "nested public" );
                    break;
            }

            if ( ( this.attributes & TypeAttributes.BeforeFieldInit ) != 0 )
            {
                writer.WriteKeyword( "beforefieldinit" );
            }

            if ( ( this.attributes & TypeAttributes.SpecialName ) == TypeAttributes.SpecialName )
            {
                writer.WriteKeyword( "specialname" );
            }

            if ( ( this.attributes & TypeAttributes.RTSpecialName ) == TypeAttributes.RTSpecialName )
            {
                writer.WriteKeyword( "rtspecialname" );
            }

            #endregion

            writer.WriteDottedName( this.Name );


            if ( this.genericParameters.Count > 0 )
            {
                writer.WriteSymbol( '<' );
                for ( int i = 0; i < this.genericParameters.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        writer.WriteSymbol( ',' );
                    }
                    this.genericParameters[i].WriteILDefinition( writer, genericMap );
                }

                writer.WriteSymbol( '>' );
            }


            if ( this.baseType != null )
            {
                writer.WriteLineBreak();
                writer.Indent += 3;
                writer.WriteKeyword( "extends" );

                ( (ITypeSignatureInternal) this.baseType ).WriteILReference( writer, genericMap,
                                                                             WriteTypeReferenceOptions.None );
                writer.Indent -= 3;
            }

            if ( this.interfaceImplementations.Count > 0 )
            {
                writer.WriteLineBreak();
                writer.Indent += 3;
                writer.WriteKeyword( "implements" );
                writer.MarkAutoIndentLocation();
                int i = 0;
                foreach ( InterfaceImplementationDeclaration interfaceImplementation in this.interfaceImplementations )
                {
                    if ( i > 0 )
                    {
                        writer.WriteSymbol( ", " );
                        writer.WriteLineBreak();
                    }
                    ( (ITypeSignatureInternal) interfaceImplementation.ImplementedInterface ).WriteILReference( writer,
                                                                                                                genericMap,
                                                                                                                WriteTypeReferenceOptions
                                                                                                                    .
                                                                                                                    None );
                    i++;
                }
                writer.Indent -= 3;
            }
            writer.ResetIndentLocation();
            writer.WriteLineBreak();

            #endregion

            writer.BeginBlock();

            if ( !writer.Options.InForwardDeclaration )
            {
                if ( this.explicitAlignment != Auto ||
                     ( ( writer.Options.Compatibility & ILWriterCompatibility.ForceWriteTypePacking ) != 0 &&
                       this.explicitTypeSize != Auto ) )
                {
                    writer.WriteKeyword( ".pack" );
                    writer.WriteInteger( this.explicitAlignment, IntegerFormat.Decimal );
                    writer.WriteLineBreak();
                }

                if ( this.explicitTypeSize != Auto ||
                     ( ( writer.Options.Compatibility & ILWriterCompatibility.ForceWriteTypePacking ) != 0 &&
                       this.explicitAlignment != Auto ) )
                {
                    writer.WriteKeyword( ".size" );
                    writer.WriteInteger( this.explicitTypeSize, IntegerFormat.Decimal );
                    writer.WriteLineBreak();
                }


                this.CustomAttributes.WriteILDefinition( writer );
                this.permissionSets.WriteILDefinition( writer );
            }


            writer.Indent--;
            foreach ( TypeDefDeclaration nestedType in this.Types )
            {
                nestedType.WriteILDefinition( writer );
                writer.WriteLineBreak();
            }
            writer.Indent++;

            if ( !writer.Options.InForwardDeclaration )
            {
                foreach ( FieldDefDeclaration field in this.Fields )
                {
                    field.WriteILDefinition( writer, genericMap );
                    writer.WriteLineBreak();
                }

                foreach ( MethodDefDeclaration method in this.Methods )
                {
                    method.WriteILDefinition( writer, genericMap );
                    writer.WriteLineBreak();
                }

                foreach ( EventDeclaration @event in this.events )
                {
                    @event.WriteILDefinition( writer, genericMap );
                    writer.WriteLineBreak();
                }

                foreach ( PropertyDeclaration property in this.properties )
                {
                    property.WriteILDefinition( writer, genericMap );
                    writer.WriteLineBreak();
                }


                // TODO: dataDecl, secDecl, extSourceSpec, exportHead, languageDecl
            }

            writer.EndBlock( false );
            writer.WriteCommentLine( "End of type: " + this.ToString() );
            writer.WriteLineBreak();
            writer.WriteLineBreak();
        }


        /// <inheritdoc />
        void ITypeSignatureInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                                      WriteTypeReferenceOptions options )
        {
            this.AssertNotDisposed();

            if ( ( options & WriteTypeReferenceOptions.SerializedTypeReference ) != 0 )
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


                TypeDefDeclaration parentType = this.Parent as TypeDefDeclaration;
                if ( parentType != null )
                {
                    ( (ITypeSignatureInternal) parentType ).WriteILReference( writer, genericMap,
                                                                              options &
                                                                              ~WriteTypeReferenceOptions.WriteTypeKind );
                    writer.WriteSymbol( '/' );
                }

                writer.WriteDottedName( this.Name );
            }
        }

        #endregion

        /// <summary>
        /// Finds a field in the current type or in its parents.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <returns>The field named <paramref name="fieldName"/>,
        /// or <b>null</b> if the field was not found.</returns>
        public FieldDefDeclaration FindField( string fieldName )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( fieldName, "fieldName" );

            #endregion

            TypeDefDeclaration typeDef = this;

            while ( true )
            {
                FieldDefDeclaration fieldDef = typeDef.fields.GetByName( fieldName );

                if ( fieldDef != null )
                    return fieldDef;

                if ( typeDef.baseType == null )
                    return null;

                typeDef = typeDef.baseType.GetTypeDefinition();
            }
        }

        /// <summary>
        /// Finds a property in the current type or in its parents.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>The property named <paramref name="propertyName"/>,
        /// or <b>null</b> if that property could not be found.</returns>
        /// <remarks>Only parameterless properties are considered.</remarks>
        public PropertyDeclaration FindProperty( string propertyName )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( propertyName, "propertyName" );

            #endregion

            TypeDefDeclaration typeDef = this;

            while ( true )
            {
                foreach ( PropertyDeclaration property in typeDef.properties.GetByName( propertyName ) )
                {
                    if ( property.Parameters.Count == 0 )
                        return property;
                }

                if ( typeDef.baseType == null )
                    return null;

                typeDef = typeDef.baseType.GetTypeDefinition();
            }
        }

        /// <inheritdoc />
        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.events.Dispose();
                this.properties.Dispose();
                this.fields.Dispose();
                this.methods.Dispose();
                this.permissionSets.Dispose();
                this.nestedTypes.Dispose();
                this.genericParameters.Dispose();
            }
        }

        #region IRemoveable Members

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "CannotRemoveBecauseNoParent" );

            #endregion

            TypeDefDeclaration declaringType = this.DeclaringType;
            if ( declaringType != null )
            {
                declaringType.Types.Remove( this );
            }
            else
            {
                this.Module.Types.Remove( this );
            }
        }

        #endregion

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedReflectionType = null;
            this.events.ClearCache();
            this.properties.ClearCache();
            this.fields.ClearCache();
            this.methods.ClearCache();
            this.permissionSets.ClearCache();
            this.nestedTypes.ClearCache();
            this.genericParameters.ClearCache();
        }
    }

   

    namespace Collections
    {
        /// <summary>
        /// Collection of types (<see cref="TypeDefDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class TypeDefDeclarationCollection :
            OrderedEmitAndByUniqueNameDeclarationCollection<TypeDefDeclaration>,
            INamedTypeCollection
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TypeDefDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal TypeDefDeclarationCollection( Declaration parent, string role ) : base( parent, role )
            {
            }

            /// <inheritdoc />
            protected override bool RequiresEmitOrdering
            {
                get
                {
#if ORDERED_EMIT
                    return true;
#else
                    return false;
#endif
                }
            }

            /// <inheritdoc />
            INamedType INamedTypeCollection.GetByName( string name )
            {
                return this.GetByName( name );
            }

            /// <inheritdoc />
            IEnumerator<INamedType> IEnumerable<INamedType>.GetEnumerator()
            {
                return EnumeratorEnlarger.EnlargeEnumerator<TypeDefDeclaration, INamedType>( this.GetEnumerator() );
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                ITypeContainer owner = (ITypeContainer) this.Owner;
                owner.Module.ModuleReader.ImportTypeDefs( owner );
            }
        }
    }
}