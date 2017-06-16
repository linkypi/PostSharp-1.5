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
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a method reference.
    /// </summary>
    /// <remarks>
    ///  Method references are
    /// owned by <see cref="IMemberRefResolutionScope"/>.
    /// </remarks>
    public sealed class MethodRefDeclaration : MemberRefDeclaration, IMethodInternal,
                                               IGenericMethodDefinition, INamed
    {
        /// <summary>
        /// Method signature.
        /// </summary>
        private MethodSignature signature;

        /// <summary>
        /// Collection of method specifications.
        /// </summary>
        private readonly MethodSpecDeclarationCollection methodSpecs;

        private MethodDefDeclaration cachedMethodDef;


        /// <summary>
        /// Initializes a new <see cref="MethodRefDeclaration"/>.
        /// </summary>
        public MethodRefDeclaration()
        {
            this.methodSpecs = new MethodSpecDeclarationCollection( this, "methodSpecifications" );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ResolutionScope.ToString() + "/" + this.Name + this.signature.ToString();
        }

        /// <inheritdoc />
        public void WriteReflectionMethodName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            GenericMap genericContext =
                this.GetGenericContext( GenericContextOptions.ResolveGenericParameterDefinitions );

            if ( this.IsGenericDefinition )
            {
                this.GetMethodDefinition().WriteReflectionMethodName( stringBuilder, options );
            }
            else
            {
                this.Signature.ReturnType.MapGenericArguments( genericContext ).WriteReflectionTypeName( stringBuilder,
                                                                                                         options |
                                                                                                         ReflectionNameOptions
                                                                                                             .
                                                                                                             MethodParameterEncoding );
                stringBuilder.Append( ' ' );
                stringBuilder.Append( this.Name );

                stringBuilder.Append( '(' );
                for ( int i = 0; i < this.signature.ParameterTypes.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        stringBuilder.Append( ", " );
                    }
                    this.signature.ParameterTypes[i].MapGenericArguments( genericContext ).WriteReflectionTypeName(
                        stringBuilder, options | ReflectionNameOptions.MethodParameterEncoding );
                }
                stringBuilder.Append( ')' );
            }
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
        public MethodBase GetSystemMethod( Type[] genericTypeArguments, Type[] genericMethodArguments,
                                           BindingOptions bindingOptions )
        {
            ITypeSignature declaringType = this.DeclaringType;
            if ( declaringType != null )
            {
                Type reflectionDeclaringType =
                    declaringType.GetSystemType( genericTypeArguments, genericMethodArguments );
                if ( reflectionDeclaringType == null )
                {
                    Trace.ReflectionBinding.WriteLine(
                        "MethodRefDeclaration.GetSystemMethod( {{{0}}} ) : cannot find " +
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

                    throw ExceptionHelper.Core.CreateBindingException( "CannotFindMethodSpecInMethodRef",
                                                                       argsString.ToString(), this.ToString() );
                }
            }

            return method;
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

        #region Properties

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
        /// Gets or sets the method signature.
        /// </summary>
        [ReadOnly( true )]
        public MethodSignature Signature
        {
            get { return signature; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                signature = value;
            }
        }

        #endregion

        #region implementation of IMethod

        /// <inheritdoc />
        int IMethodSignature.ParameterCount
        {
            get { return this.signature.ParameterTypes.Count; }
        }

        /// <inheritdoc />
        ITypeSignature IMethodSignature.GetParameterType( int index )
        {
            return this.signature.ParameterTypes[index];
        }

        /// <inheritdoc />
        bool IMethodSignature.ReferencesAnyGenericArgument()
        {
            return this.signature.ReferencesAnyGenericArgument();
        }

        /// <inheritdoc />
        IMethodSignature IMethodSignature.MapGenericArguments( GenericMap genericMap )
        {
            return this.signature.MapGenericArguments( genericMap );
        }

        /// <inheritdoc />
        CallingConvention IMethodSignature.CallingConvention
        {
            get { return this.signature.CallingConvention; }
        }

        /// <inheritdoc />
        ITypeSignature IMethodSignature.ReturnType
        {
            get { return this.signature.ReturnType; }
        }

        /// <inheritdoc />
        int IGenericDefinition.GenericParameterCount
        {
            get { return this.signature.GenericParameterCount; }
        }

        /// <inheritdoc />
        public bool IsGenericDefinition
        {
            get { return this.signature.GenericParameterCount > 0; }
        }

        /// <inheritdoc />
        bool IGeneric.IsGenericInstance
        {
            get { return false; }
        }


        /// <inheritdoc />
        public MethodDefDeclaration GetMethodDefinition()
        {
            if ( this.cachedMethodDef == null )
            {
                this.cachedMethodDef = this.GetMethodDefinition( BindingOptions.Default );
            }
            return this.cachedMethodDef;
        }

        /// <inheritdoc />
        public MethodDefDeclaration GetMethodDefinition( BindingOptions bindingOptions )
        {
            TypeDefDeclaration typeDef = this.DeclaringType.GetTypeDefinition( bindingOptions );

            if ( typeDef == null )
                return null;

            return (MethodDefDeclaration) typeDef.Methods.GetMethod(
                                              this.Name,
                                              BindingHelper.TranslateMethodSignature( this, typeDef.Module ),
                                              bindingOptions );
        }

        /// <inheritdoc />
        public MethodAttributes Attributes
        {
            get { return this.GetMethodDefinition().Attributes; }
        }

        /// <inheritdoc />
        public Visibility Visibility
        {
            get { return this.GetMethodDefinition().Visibility; }
        }

        /// <inheritdoc />
        public bool IsVirtual
        {
            get { return this.GetMethodDefinition().IsVirtual; }
        }

        /// <inheritdoc />
        public bool IsAbstract
        {
            get { return this.GetMethodDefinition().IsAbstract; }
        }

        /// <inheritdoc />
        public bool IsSealed
        {
            get { return this.GetMethodDefinition().IsSealed; }
        }

        /// <inheritdoc />
        public bool IsNew
        {
            get { return this.GetMethodDefinition().IsNew; }
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        void IMethodInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                               WriteMethodReferenceOptions options )
        {
            writer.WriteCallConvention( this.signature.CallingConvention );
            ( (ITypeSignatureInternal) this.signature.ReturnType ).WriteILReference( writer, GenericMap.Empty,
                                                                                     WriteTypeReferenceOptions.
                                                                                         WriteTypeKind );

            ITypeSignature declaringType = this.DeclaringType;

            if ( declaringType != null )
            {
                ( (ITypeSignatureInternal) declaringType ).WriteILReference( writer, genericMap,
                                                                             WriteTypeReferenceOptions.None );
                writer.WriteSymbol( "::" );
            }

            writer.WriteDottedName( this.Name );

            if ( ( options & WriteMethodReferenceOptions.Override ) != 0 && this.GenericParameterCount > 0 )
            {
                writer.WriteSymbol( "<[" );
                writer.WriteInteger( this.GenericParameterCount, IntegerFormat.Decimal );
                writer.WriteSymbol( "]>" );
            }

            this.signature.WriteSignature( writer, true );
        }

        #endregion

        #region IGenericDefinition Members

        /// <inheritdoc />
        public IGenericParameter GetGenericParameter( int ordinal )
        {
            #region Preconditions

            if ( ordinal > this.GenericParameterCount )
            {
                throw new ArgumentOutOfRangeException( "ordinal" );
            }

            #endregion

            return this.Module.Cache.GetGenericParameter( ordinal, GenericParameterKind.Method );
        }

        /// <inheritdoc />
        public int GenericParameterCount
        {
            get { return this.signature.GenericParameterCount; }
        }

        /// <inheritdoc />
        public GenericMap GetGenericContext( GenericContextOptions options )
        {
            IGeneric genericDeclaringType = this.Parent as IGeneric;
            GenericMap parentContext;

            if ( genericDeclaringType != null )
            {
                parentContext = genericDeclaringType.GetGenericContext( options );
            }
            else
            {
                parentContext = GenericMap.Empty;
            }

            if ( this.IsGenericDefinition )
            {
                if ( ( options & GenericContextOptions.ResolveGenericParameterDefinitions ) != 0 )
                {
                    return this.GetMethodDefinition().GetGenericContext( options );
                }
                else
                {
                    ITypeSignature[] arguments = new ITypeSignature[this.GenericParameterCount];
                    for ( int i = 0; i < arguments.Length; i++ )
                    {
                        arguments[i] = this.Module.Cache.GetGenericParameter( i, GenericParameterKind.Method );
                    }

                    return new GenericMap( parentContext, arguments );
                }
            }
            else
            {
                return parentContext;
            }
        }

        #endregion

        /// <inheritdoc />
        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.methodSpecs.Dispose();
            }
        }

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedMethodDef = null;
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of method references (<see cref="MethodRefDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class MethodRefDeclarationCollection :
            NonUniquelyNamedElementCollection<MethodRefDeclaration>, IMethodCollection

        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MethodRefDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal MethodRefDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }


            /// <inheritdoc />
            public IMethod GetMethod( string name, IMethodSignature signature, BindingOptions bindingOptions )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( name, "name" );
                ExceptionHelper.AssertArgumentNotNull( signature, "signature" );

                #endregion

                IMemberRefResolutionScope owner = (IMemberRefResolutionScope) this.Owner;

                MethodRefDeclaration method = (MethodRefDeclaration) BindingHelper.FindMethod(
                                                                         GenericMap.Empty, signature,
                                                                         this.GetByName( name ) );

                if ( method == null )
                {
                    if ( ( bindingOptions & BindingOptions.ExistenceMask ) == BindingOptions.Default )
                    {
                        method = new MethodRefDeclaration {Name = name, Signature = new MethodSignature( signature )};
                        this.Add( method );
                    }
                    else if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                    {
                        throw ExceptionHelper.Core.CreateBindingException( "BindingCannotFindMethod",
                                                                           name, signature, owner,
                                                                           owner.Module );
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
                return EnumeratorEnlarger.EnlargeEnumerable<MethodRefDeclaration, IMethod>( this.GetByName( name ) );
            }

            #region IEnumerable<IMethod> Members

            IEnumerator<IMethod> IEnumerable<IMethod>.GetEnumerator()
            {
                return EnumeratorEnlarger.EnlargeEnumerator<MethodRefDeclaration, IMethod>( this.GetEnumerator() );
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
                IMemberRefResolutionScope scope = (IMemberRefResolutionScope) this.Owner;
                scope.Module.ModuleReader.ImportMethodRefs( scope );
            }
        }
    }
}