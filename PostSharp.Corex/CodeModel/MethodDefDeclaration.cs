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
using System.Globalization;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a method definition (<see cref="TokenType.MethodDef"/>).
    /// </summary>
    public sealed class MethodDefDeclaration : NamedDeclaration, IMethodInternal,
                                               IMemberRefResolutionScope, IWriteILDefinition, ISecurable,
                                               IGenericMethodDefinition, IGenericDefinitionDefinition,
                                               IVisitable<ITypeSignature>, IRemoveable
    {
        #region Fields

        /// <summary>
        /// Method attributes.
        /// </summary>
        private MethodAttributes attributes;

        /// <summary>
        /// Calling convention.
        /// </summary>
        private CallingConvention callingConvention;

        /// <summary>
        /// Collection of permission sets.
        /// </summary>
        private readonly PermissionSetDeclarationCollection permissionSets;

        /// <summary>
        /// implementation attributes.
        /// </summary>
        private MethodImplAttributes implAttributes = MethodImplAttributes.Managed;

        /// <summary>
        /// Method body.
        /// </summary>
        /// <value>
        /// A <see cref="MethodBodyDeclaration"/>, or <b>null</b> if the current
        /// method has no body (e.g. is abstract) or if the body has not yet
        /// been constructed.
        /// </value>
        private MethodBodyDeclaration body;

        /// <summary>
        /// Determines whether the body has been constructed.
        /// </summary>
        private bool bodyResolved;

        /// <summary>
        /// Specifies the return parameter.
        /// </summary>
        private ParameterDeclaration returnParameter;

        /// <summary>
        /// P-Invoke map.
        /// </summary>
        /// <value>
        /// A <see cref="PInvokeMap"/>, or <b>null</b> if the current method
        /// is not a P-Invoke declaration.
        /// </value>
        private PInvokeMap pinvokeMap;

        /// <summary>
        /// Collection of interface methods implemented by the current method.
        /// </summary>
        private readonly MethodImplementationDeclarationCollection methodImplementations;

        /// <summary>
        /// Runtime method corresponding to the current method.
        /// </summary>
        private MethodBase cachedReflectionMethod;

        /// <summary>
        /// Collection of parameters.
        /// </summary>
        private readonly ParameterDeclarationCollection parameters;

        /// <summary>
        /// Collection of generic method parameters.
        /// </summary>
        private readonly GenericParameterDeclarationCollection genericParameters;

        /// <summary>
        /// Collection of call-site signatures, in case the current
        /// method calling convention is <see cref="PostSharp.CodeModel.CallingConvention.VarArg"/>.
        /// </summary>
        /// <remarks>
        /// Call-site signatures determine the variable list of parameters.
        /// </remarks>
        private MethodRefDeclarationCollection callSiteSignatures;

        /// <summary>
        /// Collection of method specifications.
        /// </summary>
        private readonly MethodSpecDeclarationCollection methodSpecs;

        private uint rva = uint.MaxValue;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MethodDefDeclaration"/>.
        /// </summary>
        public MethodDefDeclaration()
        {
            this.permissionSets = new PermissionSetDeclarationCollection( this, "securityDeclaration" );
            this.parameters = new ParameterDeclarationCollection( this, "parameters" );
            this.genericParameters = new GenericParameterDeclarationCollection( this, "genericParameters" );
            this.methodSpecs = new MethodSpecDeclarationCollection( this, "methodSpecifications" );
            this.methodImplementations = new MethodImplementationDeclarationCollection( this, "methodImplementations" );
            this.callSiteSignatures = new MethodRefDeclarationCollection( this, "callSiteSignatures" );
        }

        /// <inheritdoc />
        protected override bool NotifyChildPropertyChanged( Element child, string property, object oldValue,
                                                            object newValue )
        {
            if ( base.NotifyChildPropertyChanged( child, property, oldValue, newValue ) )
            {
                return true;
            }

            switch ( child.Role )
            {
                case "parameters":
                    if ( property == "Ordinal " )
                    {
                        this.parameters.OnItemOrdinalChanged( (ParameterDeclaration) child, (int) oldValue );
                        return true;
                    }
                    break;

                case "genericParameters":
                    if ( property == "Ordinal" )
                    {
                        this.genericParameters.OnItemOrdinalChanged( (GenericParameterDeclaration) child,
                                                                     (int) oldValue );
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.MethodDef;
        }

        #region Properties

        /// <summary>
        /// Gets the visibility of the current method.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                switch ( this.attributes & MethodAttributes.MemberAccessMask )
                {
                    case MethodAttributes.Assembly:
                        return Visibility.Assembly;

                    case MethodAttributes.FamANDAssem:
                        return Visibility.FamilyAndAssembly;

                    case MethodAttributes.FamORAssem:
                        return Visibility.FamilyOrAssembly;

                    case MethodAttributes.Private:
                    case MethodAttributes.PrivateScope:
                        return Visibility.Private;

                    case MethodAttributes.Family:
                        return Visibility.Family;

                    case MethodAttributes.Public:
                        return Visibility.Public;


                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( this.attributes, "this.Attributes" );
                }
            }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public MethodSpecDeclarationCollection MethodSpecs
        {
            get
            {
                this.AssertNotDisposed();
                return this.methodSpecs;
            }
        }

        /// <summary>
        /// Gets the collection of generic formal parameters (<see cref="GenericParameterDeclaration"/>).
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

        /// <summary>
        /// Gets the P-Invoke map.
        /// </summary>
        /// <value>
        /// A <see cref="PInvokeMap"/>, or <b>null</b> if the current method
        /// is not a P-Invoke.
        /// </value>
        [ReadOnly( true )]
        public PInvokeMap PInvokeMap
        {
            get { return pinvokeMap; }
            set { pinvokeMap = value; }
        }

        /// <summary>
        /// Gets the return parameter.
        /// </summary>
        [ReadOnly( true )]
        public ParameterDeclaration ReturnParameter
        {
            get { return returnParameter; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                if ( this.returnParameter != value )
                {
                    if ( this.returnParameter != null )
                    {
                        this.returnParameter.OnRemovingFromParent();
                    }

                    this.returnParameter = value;

                    this.returnParameter.OnAddingToParent( this, "returnParameter" );
                }
            }
        }

        /// <summary>
        /// Gets the collection of parameters.
        /// </summary>
        [Browsable( false )]
        public ParameterDeclarationCollection Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// Gets or sets the method attributes.
        /// </summary>
        [ReadOnly( true )]
        public MethodAttributes Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        /// <summary>
        /// Determines whether the method is virtual.
        /// </summary>
        public bool IsVirtual
        {
            get { return ( this.attributes & MethodAttributes.Virtual ) != 0; }
        }

        /// <summary>
        /// Determines whether the method is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return ( this.attributes & MethodAttributes.Abstract ) != 0; }
        }

        /// <summary>
        /// Determines whether the method is sealed.
        /// </summary>
        public bool IsSealed
        {
            get { return ( this.attributes & MethodAttributes.Final ) != 0; }
        }

        /// <summary>
        /// Determines whether the method takes a new slot.
        /// </summary>
        public bool IsNew
        {
            get { return ( this.attributes & MethodAttributes.NewSlot ) != 0; }
        }


        /// <summary>
        /// Determines whether the method is static.
        /// </summary>
        [Browsable( false )]
        public bool IsStatic
        {
            get { return ( this.attributes & MethodAttributes.Static ) != 0; }
        }

        /// <summary>
        /// Gets or sets the method calling convention.
        /// </summary>
        [ReadOnly( true )]
        public CallingConvention CallingConvention
        {
            get { return callingConvention; }
            set
            {
                if ( callingConvention != value )
                {
                    if ( value == CallingConvention.VarArg )
                    {
                        this.callSiteSignatures = new MethodRefDeclarationCollection( this, "externalMethods" );
                    }
                    else
                    {
                        this.callSiteSignatures = null;
                    }

                    callingConvention = value;
                }
            }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public PermissionSetDeclarationCollection PermissionSets
        {
            get
            {
                this.AssertNotDisposed();
                return permissionSets;
            }
        }

        /// <summary>
        /// When the current method overrides a method in a parent type, returns
        /// the overriden method.
        /// </summary>
        /// <returns>The overridden method, or <b>null</b>if the current method
        /// does not override any method.</returns>
        public MethodDefDeclaration GetParentDefinition()
        {
            if ( !this.IsVirtual || this.DeclaringType == null ||
                 ( this.attributes & MethodAttributes.NewSlot ) == MethodAttributes.NewSlot )
                return null;

            IType baseType = this.DeclaringType.BaseType;
            IMethodSignature methodSignature = null;
            ModuleDeclaration module = null;

            while ( baseType != null )
            {
                TypeDefDeclaration typeDef = baseType.GetTypeDefinition();
                ModuleDeclaration newModule = typeDef.Module;

                if ( newModule != module )
                    methodSignature = BindingHelper.TranslateMethodSignature( this, typeDef.Module );

                MethodDefDeclaration method = (MethodDefDeclaration)
                                              typeDef.Methods.GetMethod( this.Name, methodSignature,
                                                                         BindingOptions.DontThrowException );

                if ( method != null )
                    return method;

                baseType = typeDef.BaseType;
                module = newModule;
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the method implementation attributes.
        /// </summary>
        [ReadOnly( true )]
        public MethodImplAttributes ImplementationAttributes
        {
            get { return implAttributes; }
            set { implAttributes = value; }
        }

        /// <summary>
        /// Determines whether the method currently has a body.
        /// </summary>
        /// <see cref="MayHaveBody"/>
        public bool HasBody
        {
            get
            {
                if ( this.body != null )
                {
                    return true;
                }

                return ( this.rva != 0 ) && this.MayHaveBody;
            }
        }

        /// <summary>
        /// Determines whether this method may have a body.
        /// </summary>
        /// <see cref="HasBody"/>
        /// <remarks>
        /// The difference with <see cref="HasBody"/> is that property
        /// returns <b>true</b> if the method body is <b>not yet</b>
        /// implemented, for instance because it was marked as <b>external</b>
        /// in C#. But the method would accept a method body. The current property
        /// returns <b>false</b> even if the current method has a body, but may not.
        /// </remarks>
        public bool MayHaveBody
        {
            get
            {
                return ( ( this.implAttributes & MethodImplAttributes.CodeTypeMask ) == MethodImplAttributes.IL ) &&
                       ( ( this.implAttributes & ( MethodImplAttributes.InternalCall | MethodImplAttributes.ForwardRef ) ) ==
                         0 ) &&
                       ( ( this.attributes &
                           ( MethodAttributes.Abstract | MethodAttributes.PinvokeImpl | MethodAttributes.UnmanagedExport ) ) ==
                         0 );
            }
        }

        /// <summary>
        /// Gets the method body.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		This method can have no body.
        /// </exception>
        [Browsable( false )]
        public MethodBodyDeclaration MethodBody
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.MayHaveBody, "MethodHasNoBody" );
                ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "MethodHasNoParent" );

                #endregion

                if ( !bodyResolved )
                {
                    this.bodyResolved = true;

                    if ( this.rva != 0 && this.rva != uint.MaxValue )
                    {
                        if ( Trace.ModuleReader.Enabled )
                        {
                            Trace.ModuleReader.WriteLine( "Decoding the method body " +
                                                          ( this.DeclaringType != null
                                                                ? this.DeclaringType.ToString()
                                                                : "<Module>" ) +
                                                          "::" + this.ToString() );
                        }

                        InstructionBlockBuilder.BuildInstructionBlockTree( this );
                    }
                    else
                    {
                        // This happens when the method WAS without body and method attributes have
                        // been changed at post-compilation, but the Body property has not been set.
                        this.body = new MethodBodyDeclaration();
                        this.body.OnAddingToParent( this, "body" );
                    }
                }

                return this.body;
            }
            set
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.MayHaveBody, "CannotHaveMethodBody", this );

                #endregion

                this.bodyResolved = true;


                if ( this.body != value )
                {
                    if ( this.body != null )
                    {
                        this.body.OnRemovingFromParent();
                    }
                    this.body = value;
                    if ( this.body != null )
                    {
                        this.body.OnAddingToParent( this, "body" );
                    }
                }
            }
        }


        /// <notSupported />
        FieldRefDeclarationCollection IMemberRefResolutionScope.FieldRefs
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc />
        MethodRefDeclarationCollection IMemberRefResolutionScope.MethodRefs
        {
            get { return this.CallSiteSignatures; }
        }

        /// <summary>
        /// Call-site signature of the current method, if 
        /// it calling convention is <see cref="PostSharp.CodeModel.CallingConvention.VarArg"/>.
        /// </summary>
        [Browsable( false )]
        public MethodRefDeclarationCollection CallSiteSignatures
        {
            get
            {
                this.AssertNotDisposed();
                return this.callSiteSignatures;
            }
        }

        /// <summary>
        /// Gets the collection of interface methods that are implemented by the current method.
        /// </summary>
        [Browsable( false )]
        public MethodImplementationDeclarationCollection InterfaceImplementations
        {
            get
            {
                this.AssertNotDisposed();
                return this.methodImplementations;
            }
        }

        /// <inheritdoc />
        int IGenericDefinition.GenericParameterCount
        {
            get { return this.genericParameters.Count; }
        }


        /// <inheritdoc />
        [ReadOnly( true )]
        public bool IsGenericDefinition
        {
            get { return this.genericParameters.Count > 0; }
        }

        MethodDefDeclaration IMethod.GetMethodDefinition()
        {
            return this;
        }

        MethodDefDeclaration IMethod.GetMethodDefinition( BindingOptions bindingOptions )
        {
            return this;
        }

        #endregion

        /// <summary>
        /// Gets the RVA of the method body.
        /// </summary>
        internal uint RVA
        {
            get { return this.rva; }
            set { this.rva = value; }
        }

        /// <inheritdoc />
        public bool Equals( IMethodSignature other )
        {
            return CompareHelper.Equals( this, other, true );
        }

        /// <inheritdoc />
        public bool Equals( IMethod other )
        {
            return CompareHelper.Equals( this, other, true );
        }

        /// <inheritdoc />
        public bool MatchesReference( IMethod reference )
        {
            return CompareHelper.Equals( this, reference, false );
        }

        /// <inheritdoc />
        public bool MatchesReference( IMethodSignature reference )
        {
            return CompareHelper.Equals( this, reference, false );
        }


        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            if ( this.DeclaringType != null )
            {
                strBuilder.Append( this.DeclaringType.ToString() );
                strBuilder.Append( "/" );
            }
            strBuilder.Append( this.Name );
            strBuilder.Append( '(' );
            for ( int i = 0; i < parameters.Count; i++ )
            {
                if ( i > 0 )
                {
                    strBuilder.Append( ", " );
                }
                strBuilder.Append( parameters[i].ParameterType.ToString() );
                if ( !string.IsNullOrEmpty( parameters[i].Name ) )
                {
                    strBuilder.Append( ' ' );
                    strBuilder.Append( parameters[i].Name );
                }
            }
            strBuilder.Append( ") : " );
            strBuilder.Append( this.returnParameter != null ? this.returnParameter.ParameterType.ToString() : "void" );

            return strBuilder.ToString();
        }

        /// <summary>
        /// Gets the declaring <see cref="TypeDefDeclaration"/>.
        /// </summary>
        [Browsable( false )]
        public TypeDefDeclaration DeclaringType
        {
            get { return this.Parent as TypeDefDeclaration; }
        }

        #region Private implementation of IMethod

        /// <inheritdoc />
        IType IMember.DeclaringType
        {
            get { return this.DeclaringType; }
        }

        /// <inheritdoc />
        int IMethodSignature.ParameterCount
        {
            get { return this.Parameters.Count; }
        }

        /// <inheritdoc />
        int IMethodSignature.GenericParameterCount
        {
            get { return this.genericParameters.Count; }
        }

        /// <inheritdoc />
        ITypeSignature IMethodSignature.GetParameterType( int index )
        {
            return this.Parameters[index].ParameterType;
        }

        /// <inheritdoc />
        ITypeSignature IMethodSignature.ReturnType
        {
            get { return this.returnParameter == null ? null : this.returnParameter.ParameterType; }
        }

        /// <inheritdoc />
        bool IGeneric.IsGenericInstance
        {
            get { return false; }
        }

        /// <inheritdoc />
        public MethodBase GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            if ( this.Name == ".ctor" || this.Name == ".cctor" )
            {
                return new MethodConstructorInfoWrapper( this, genericTypeArguments, genericMethodArguments );
            }
            else
            {
                return new MethodMethodInfoWrapper( this, genericTypeArguments, genericMethodArguments );
            }
        }

        /// <inheritdoc />
        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public void WriteReflectionMethodName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            GenericMap genericMap = this.GetGenericContext( GenericContextOptions.ResolveGenericParameterDefinitions );

            this.returnParameter.ParameterType.MapGenericArguments( genericMap ).WriteReflectionTypeName( stringBuilder,
                                                                                                          options |
                                                                                                          ReflectionNameOptions
                                                                                                              .
                                                                                                              MethodParameterEncoding );
            stringBuilder.Append( ' ' );
            stringBuilder.Append( this.Name );

            if ( this.IsGenericDefinition )
            {
                stringBuilder.Append( '[' );
                for ( int i = 0; i < this.genericParameters.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        stringBuilder.Append( ',' );
                    }
                    stringBuilder.Append( this.genericParameters[i].Name );
                }
                stringBuilder.Append( ']' );
            }


            stringBuilder.Append( '(' );
            for ( int i = 0; i < this.parameters.Count; i++ )
            {
                if ( i > 0 )
                {
                    stringBuilder.Append( ", " );
                }
                this.parameters[i].ParameterType.MapGenericArguments( genericMap ).WriteReflectionTypeName(
                    stringBuilder,
                    options |
                    ReflectionNameOptions
                        .
                        MethodParameterEncoding );
            }
            stringBuilder.Append( ')' );
        }

        /// <inheritdoc />
        public MethodBase GetSystemMethod( Type[] genericTypeArguments, Type[] genericMethodArguments,
                                           BindingOptions bindingOptions )
        {
            if ( this.cachedReflectionMethod == null )
            {
                try
                {
                    Module module = this.Module.GetSystemModule();
                    this.cachedReflectionMethod = module != null
                                                ? module.ResolveMethod(
                                                      (int) this.MetadataToken.Value, genericTypeArguments,
                                                      genericMethodArguments )
                                                : null;
                }
                catch ( NullReferenceException )
                {
                    // This method could throw a NullReferenceException without appearing reason.

                    ITypeSignature declaringType = this.DeclaringType;
                    if ( declaringType != null )
                    {
                        Type reflectionDeclaringType =
                            declaringType.GetSystemType( genericTypeArguments, genericMethodArguments );
                        if ( reflectionDeclaringType == null )
                        {
                            Trace.ReflectionBinding.WriteLine(
                                "MethodDefDeclaration.GetSystemMethod( {{{0}}} ) : cannot find " +
                                " the declaring type {1}.", this, declaringType );
                            return null;
                        }

                        return
                            BindingHelper.GetSystemMethod( reflectionDeclaringType, this, genericTypeArguments,
                                                           genericMethodArguments, bindingOptions );
                    }
                    else
                    {
                        Module module = this.Module.GetSystemModule();
                        return module != null
                                   ?
                                       BindingHelper.GetSystemMethod( module, this, genericTypeArguments,
                                                                      genericMethodArguments, bindingOptions )
                                   : null;
                    }
                }
            }

            return this.cachedReflectionMethod;
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemMethod( genericTypeArguments, genericMethodArguments, BindingOptions.Default );
        }


        /// <inheritdoc />
        public IMethod FindGenericInstance( IList<ITypeSignature> genericArguments, BindingOptions bindingOptions )
        {
            MethodSpecDeclaration method = this.methodSpecs.GetGenericInstance( genericArguments );

            if ( method == null )
            {
                if ( ( bindingOptions & BindingOptions.ExistenceMask ) != BindingOptions.OnlyExisting )
                {
                    // Not found but requested.
                    method = new MethodSpecDeclaration();
                    method.GenericArguments.AddRange( genericArguments );
                    this.methodSpecs.Add( method );
                }
                else if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                {
                    return null;
                }
                else
                {
                    StringBuilder argsString = new StringBuilder();
                    argsString.Append( '<' );
                    for ( int i = 0; i < genericArguments.Count; i++ )
                    {
                        if ( i > 0 )
                            argsString.Append( ", " );
                        argsString.Append( genericArguments[i].ToString() );
                    }
                    argsString.Append( '>' );

                    throw ExceptionHelper.Core.CreateBindingException( "CannotFindMethodSpecInMethodDef",
                                                                       argsString.ToString(), this.ToString() );
                }
            }

            return method;
        }

        /// <inheritdoc />
        public bool ReferencesAnyGenericArgument()
        {
            if ( this.returnParameter.ParameterType.ContainsGenericArguments() )
            {
                return true;
            }

            foreach ( ParameterDeclaration parameter in this.parameters )
            {
                if ( parameter.ParameterType.ContainsGenericArguments() )
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public IMethodSignature MapGenericArguments( GenericMap genericMap )
        {
            if ( this.ReferencesAnyGenericArgument() )
            {
                TypeSignatureCollection mappedParameters = new TypeSignatureCollection( this.parameters.Count );

                for ( int i = 0; i < this.parameters.Count; i++ )
                {
                    mappedParameters.Add( this.parameters[i].ParameterType.MapGenericArguments( genericMap ) );
                }

                return
                    new MethodSignature( this.Module,
                                         this.callingConvention,
                                         this.returnParameter.ParameterType.MapGenericArguments( genericMap ),
                                         mappedParameters, this.genericParameters.Count );
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc />
        public IMethod Translate( ModuleDeclaration targetModule )
        {
            return BindingHelper.TranslateMethodDefOrRef( this, targetModule );
        }

        /// <inheritdoc />
        IMethodSignature IMethodSignature.Translate( ModuleDeclaration targetModule )
        {
            return this.Translate( targetModule );
        }

        /// <inheritdoc />
        IGenericParameter IGenericDefinition.GetGenericParameter( int ordinal )
        {
            return this.genericParameters[ordinal];
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="GenericMap"/> of the current method.
        /// </summary>
        /// <returns>The <see cref="GenericMap"/> of the current method.</returns>
        public GenericMap GetGenericContext( GenericContextOptions options )
        {
            return new GenericMap( this.DeclaringType.GetGenericContext( options ),
                                   this.genericParameters );
        }

        /// <inheritdoc />
        public void Visit( string role, Visitor<ITypeSignature> visitor )
        {
            ExceptionHelper.AssertArgumentNotNull( visitor, "visitor" );
            this.returnParameter.ParameterType.Visit( role, visitor );

            visitor( this,
                     "ReturnType",
                     this.returnParameter.ParameterType );
            this.returnParameter.ParameterType.Visit( role, visitor );

            foreach ( ParameterDeclaration parameter in this.parameters )
            {
                visitor( this,
                         string.Format( CultureInfo.InvariantCulture, "ParameterType:{0}", parameter.Ordinal ),
                         parameter.ParameterType );
                parameter.ParameterType.Visit( role, visitor );
            }
        }

        #region writer IL

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            this.WriteILDefinition( writer, this.DeclaringType.GetGenericContext( GenericContextOptions.None ) );
        }

        /// <summary>
        /// Writes the IL definition of the current method.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of the
        /// declaring type.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap )
        {
            Trace.ModuleWriter.WriteLine( "Write method {{{0}}}.", this );

            GenericMap methodGenericContext = this.GetGenericContext( GenericContextOptions.None );

            writer.Options.InMethodDefinition = true;

            writer.WriteKeyword( ".method" );
            writer.MarkAutoIndentLocation();

            #region Method attributes

            switch ( this.attributes & MethodAttributes.MemberAccessMask )
            {
                case MethodAttributes.Assembly:
                    writer.WriteKeyword( "assembly" );
                    break;

                case MethodAttributes.FamANDAssem:
                    writer.WriteKeyword( "famandassem" );
                    break;

                case MethodAttributes.Family:
                    writer.WriteKeyword( "family" );
                    break;

                case MethodAttributes.FamORAssem:
                    writer.WriteKeyword( "famorassem" );
                    break;

                case MethodAttributes.Private:
                    writer.WriteKeyword( "private" );
                    break;

                case MethodAttributes.PrivateScope:
                    writer.WriteKeyword( "privatescope" );
                    break;

                case MethodAttributes.Public:
                    writer.WriteKeyword( "public" );
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "Unexpected member access." );
            }

            if ( ( this.attributes & MethodAttributes.HideBySig ) != 0 )
            {
                writer.WriteKeyword( "hidebysig" );
            }

            if ( ( this.attributes & MethodAttributes.NewSlot ) != 0 )
            {
                writer.WriteKeyword( "newslot" );
            }

            if ( ( this.attributes & MethodAttributes.SpecialName ) != 0 )
            {
                writer.WriteKeyword( "specialname" );
            }

            if ( ( this.attributes & MethodAttributes.RTSpecialName ) != 0 )
            {
                writer.WriteKeyword( "rtspecialname" );
            }


            if ( ( this.attributes & MethodAttributes.Abstract ) != 0 )
            {
                writer.WriteKeyword( "abstract" );
            }

            if ( ( this.attributes & MethodAttributes.CheckAccessOnOverride ) != 0 )
            {
                writer.WriteKeyword( "strict" );
            }

            if ( ( this.attributes & MethodAttributes.Virtual ) != 0 )
            {
                writer.WriteKeyword( "virtual" );
            }

            if ( ( this.attributes & MethodAttributes.Final ) != 0 )
            {
                writer.WriteKeyword( "final" );
            }

            if ( ( this.attributes & MethodAttributes.Static ) != 0 )
            {
                writer.WriteKeyword( "static" );
            }


            if ( ( this.attributes & MethodAttributes.RequireSecObject ) != 0 )
            {
                writer.WriteKeyword( "reqsecobj" );
            }

            writer.WriteConditionalLineBreak();

            if ( ( this.attributes & MethodAttributes.PinvokeImpl ) != 0 )
            {
                if ( this.pinvokeMap != null )
                {
                    this.pinvokeMap.WriteILDefinition( this, writer );
                }
                else
                {
                    writer.WriteKeyword( "pinvokeimpl( /* No map */ )" );
                }
                writer.WriteConditionalLineBreak();
            }

            #endregion

            writer.WriteCallConvention( this.callingConvention );

            if ( this.returnParameter != null )
            {
                this.returnParameter.WriteILDefinition( writer, methodGenericContext, true );
            }
            else
            {
                writer.WriteKeyword( "void" );
            }

            writer.WriteConditionalLineBreak();

            writer.WriteDottedName( this.Name );


            // Generic parameters
            if ( this.genericParameters.Count > 0 )
            {
                writer.WriteSymbol( '<' );
                for ( int i = 0; i < this.genericParameters.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        writer.WriteSymbol( ',' );
                    }
                    this.genericParameters[i].WriteILDefinition( writer, methodGenericContext );
                }

                writer.WriteSymbol( '>' );
            }

            // Parameters
            writer.WriteSymbol( '(' );
            writer.MarkAutoIndentLocation();
            for ( int i = 0; i < this.parameters.Count; i++ )
            {
                if ( i > 0 )
                {
                    writer.WriteSymbol( ',' );
                    writer.WriteLineBreak();
                }

                parameters[i].WriteILDefinition( writer );
            }
            writer.WriteSymbol( ')' );

            #region implementation attributes

            switch ( this.implAttributes & MethodImplAttributes.CodeTypeMask )
            {
                case MethodImplAttributes.IL:
                    writer.WriteKeyword( "cil" );
                    break;

                case MethodImplAttributes.Native:
                    writer.WriteKeyword( "native" );
                    break;

                case MethodImplAttributes.OPTIL:
                    writer.WriteKeyword( "optil" );
                    break;

                case MethodImplAttributes.Runtime:
                    writer.WriteKeyword( "runtime" );
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "Unexpected implementation attribute." );
            }


            switch ( this.implAttributes & MethodImplAttributes.ManagedMask )
            {
                case MethodImplAttributes.Managed:
                    writer.WriteKeyword( "managed" );
                    break;

                case MethodImplAttributes.Unmanaged:
                    writer.WriteKeyword( "unmanaged" );
                    break;
            }

            if ( ( this.implAttributes & MethodImplAttributes.PreserveSig ) != 0 )
            {
                writer.WriteKeyword( "preservesig" );
            }


            if ( ( this.implAttributes & MethodImplAttributes.ForwardRef ) != 0 )
            {
                writer.WriteKeyword( "forwardref" );
            }

            if ( ( this.implAttributes & MethodImplAttributes.InternalCall ) != 0 )
            {
                writer.WriteKeyword( "internalcall" );
            }

            if ( ( this.implAttributes & MethodImplAttributes.Synchronized ) != 0 )
            {
                writer.WriteKeyword( "synchronized" );
            }

            if ( ( this.implAttributes & MethodImplAttributes.NoInlining ) != 0 )
            {
                writer.WriteKeyword( "noinlining" );
            }

            #endregion

            writer.ResetIndentLocation();
            writer.WriteLineBreak();
            writer.BeginBlock();


            // Entry point
            if ( this == this.Module.EntryPoint )
            {
                writer.WriteKeyword( ".entrypoint" );
                writer.WriteLineBreak();
            }

            // Custom attributes on the method
            this.CustomAttributes.WriteILDefinition( writer );


            // Custom attributes on the return value
            if ( this.returnParameter != null && this.returnParameter.CustomAttributes.Count > 0 )
            {
                writer.WriteKeyword( ".param" );
                writer.WriteSymbol( '[' );
                writer.WriteInteger( 0, IntegerFormat.Decimal );
                writer.WriteSymbol( ']' );
                writer.WriteLineBreak();
                this.returnParameter.CustomAttributes.WriteILDefinition( writer );
            }


            // Metadata on the parameters
            for ( int i = 0; i < this.parameters.Count; i++ )
            {
                if ( ( this.parameters[i].Attributes & ParameterAttributes.HasDefault ) != 0 ||
                     this.parameters[i].CustomAttributes.Count > 0 )
                {
                    writer.WriteKeyword( ".param" );
                    writer.WriteSymbol( '[' );
                    writer.WriteInteger( i + 1, IntegerFormat.Decimal );
                    writer.WriteSymbol( ']' );
                    if ( ( this.parameters[i].Attributes & ParameterAttributes.HasDefault ) != 0 )
                    {
                        writer.WriteSymbol( '=' );

                        if ( this.parameters[i].DefaultValue == null )
                        {
                            writer.WriteKeyword( "nullref" );
                        }
                        else
                        {
                            this.parameters[i].DefaultValue.WriteILValue( writer, WriteSerializedValueMode.FieldValue );
                        }
                    }
                    writer.WriteLineBreak();
                    this.parameters[i].CustomAttributes.WriteILDefinition( writer );
                }
            }


            this.permissionSets.WriteILDefinition( writer );


            // Overrides
            foreach ( MethodImplementationDeclaration methodImpl in this.InterfaceImplementations )
            {
                IMethod overridenMethod = methodImpl.ImplementedMethod;

                writer.WriteKeyword( ".override" );

                IGenericInstance genericType = overridenMethod.DeclaringType as IGenericInstance;

                if ( genericType != null && genericType.IsGenericInstance )
                {
                    writer.WriteKeyword( "method" );
                    ( (IMethodInternal) overridenMethod ).WriteILReference( writer, genericMap,
                                                                            WriteMethodReferenceOptions.Override );
                }
                else
                {
                    ( (ITypeSignatureInternal) overridenMethod.DeclaringType ).WriteILReference( writer, genericMap,
                                                                                                 WriteTypeReferenceOptions
                                                                                                     .
                                                                                                     None );
                    writer.WriteSymbol( "::" );
                    writer.WriteDottedName( overridenMethod.Name );
                }
                writer.WriteLineBreak();
            }


            if ( this.HasBody )
            {
                this.MethodBody.WriteILDefinition( writer );
                if ( writer.Options.ReleaseBodyAfterWrite )
                {
                    this.ReleaseBody();
                }
            }

            writer.EndBlock( false );
            try
            {
                writer.WriteCommentLine("End of method: " + this.ToString());
            }
            catch ( Exception )
            {
            }
            
            writer.Options.InMethodDefinition = false;
        }

        /// <inheritdoc />
        void IMethodInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                               WriteMethodReferenceOptions options )
        {
            writer.WriteCallConvention( this.callingConvention );
            ( (ITypeSignatureInternal) this.ReturnParameter.ParameterType ).WriteILReference( writer,
                                                                                              writer.Options.
                                                                                                  InMethodBody
                                                                                                  ? GenericMap.Empty
                                                                                                  : genericMap,
                                                                                              WriteTypeReferenceOptions.
                                                                                                  WriteTypeKind );

            ITypeSignature declaringType = this.DeclaringType;

            if ( declaringType != null )
            {
                ( (ITypeSignatureInternal) declaringType ).WriteILReference( writer, GenericMap.Empty,
                                                                             WriteTypeReferenceOptions.None );
                writer.WriteSymbol( "::" );
            }

            writer.WriteDottedName( this.Name );

            if ( ( options & WriteMethodReferenceOptions.Override ) != 0 && this.genericParameters.Count > 0 )
            {
                writer.WriteSymbol( "<[" );
                writer.WriteInteger( this.genericParameters.Count, IntegerFormat.Decimal );
                writer.WriteSymbol( "]>" );
            }

            writer.WriteSymbol( '(' );
            writer.MarkAutoIndentLocation();

            for ( int i = 0; i < this.parameters.Count; i++ )
            {
                if ( i > 0 )
                {
                    writer.WriteSymbol( ',' );
                    writer.WriteLineBreak();
                }

                ( (ITypeSignatureInternal) this.parameters[i].ParameterType ).WriteILReference( writer,
                                                                                                writer.Options.
                                                                                                    InMethodDefinition
                                                                                                    ? GenericMap.Empty
                                                                                                    : genericMap,
                                                                                                WriteTypeReferenceOptions
                                                                                                    .
                                                                                                    WriteTypeKind );

                // todo: marshal
            }
            writer.WriteSymbol( ')' );
            writer.ResetIndentLocation();
        }

        #endregion

        /// <summary>
        /// Indicates that the body is no more needed in memory, so that
        /// it is marked for garbage collection. 
        /// </summary>
        /// <remarks>
        /// The body will be rebuilt
        /// from the PE image the next time it will be requested. All changes
        /// will be lost.
        /// </remarks>
        public void ReleaseBody()
        {
            this.MethodBody = null;
            this.bodyResolved = false;
        }

        /// <inheritdoc />
        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.methodSpecs.Dispose();
                this.genericParameters.Dispose();
                this.callSiteSignatures.Dispose();
                this.permissionSets.Dispose();
                this.parameters.Dispose();
                if ( this.body != null )
                {
                    this.body.Dispose();
                }
            }
        }

        #region IRemoveable Members

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "CannotRemoveBecauseNoParent" );

            #endregion

            this.DeclaringType.Methods.Remove( this );
        }

        #endregion

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedReflectionMethod = null;
        }
    }


    namespace Collections
    {
        /// <summary>
        /// Collection of methods (<see cref="MethodDefDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class MethodDefDeclarationCollection :
            OrderedEmitAndByNonUniqueNameDeclarationCollection<MethodDefDeclaration>,
            IMethodCollection

        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MethodDefDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal MethodDefDeclarationCollection( Declaration parent, string role )
                :
                    base( parent, role )
            {
            }

            /// <inheritdoc />
            protected override bool RequiresEmitOrdering
            {
                get { return true; }
            }

            /// <summary>
            /// Finds a method in the type given its name and signature.
            /// </summary>
            /// <param name="name">Method name.</param>
            /// <param name="signature">Method signature.</param>
            /// <param name="bindingOptions">Determines the behavior in case the method is not
            /// found.</param>
            /// <returns>The method, or <b>null</b> if the method could not be found.</returns>
            public IMethod GetMethod( string name, IMethodSignature signature, BindingOptions bindingOptions )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( name, "name" );
                ExceptionHelper.AssertArgumentNotNull( signature, "signature" );

                #endregion

                MethodDefDeclaration method = (MethodDefDeclaration)
                                              BindingHelper.FindMethod(
                                                  ( (TypeDefDeclaration) this.Owner ).GetGenericContext(
                                                      GenericContextOptions.None ), signature,
                                                  this.GetByName( name ) );

                if ( method == null )
                {
                    if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                    {
                        throw ExceptionHelper.Core.CreateBindingException( "BindingCannotFindMethod",
                                                                           name, signature, this.Owner,
                                                                           ( (IDeclaration) this.Owner ).Module );
                    }
                    else
                    {
                        return null;
                    }
                }

                return method;
            }

            /// <inheritdoc />
            IEnumerable<IMethod> IMethodCollection.GetByName( string name )
            {
                return EnumeratorEnlarger.EnlargeEnumerable<MethodDefDeclaration, IMethod>( this.GetByName( name ) );
            }

            #region IEnumerable<IMethod> Members

            IEnumerator<IMethod> IEnumerable<IMethod>.GetEnumerator()
            {
                return EnumeratorEnlarger.EnlargeEnumerator<MethodDefDeclaration, IMethod>( this.GetEnumerator() );
            }

            #endregion

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                TypeDefDeclaration typeDef = (TypeDefDeclaration) this.Owner;
                typeDef.Module.ModuleReader.ImportMethodDefs( typeDef );
            }
        }
    }
}