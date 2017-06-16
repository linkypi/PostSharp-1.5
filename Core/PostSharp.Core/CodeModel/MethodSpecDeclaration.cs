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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a method specification (<see cref="TokenType.MethodSpec"/>), i.e. 
    /// the construction of a concrete generic method.
    /// </summary>
    public sealed class MethodSpecDeclaration : MetadataDeclaration, IMethodInternal, IGenericInstance
    {
        #region Fields

        /// <summary>
        /// List of generic arguments.
        /// </summary>
        private readonly TypeSignatureCollection genericArguments = new TypeSignatureCollection( 2 );

        #endregion

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.MethodSpec;
        }

        /// <inheritdoc />
        public IType DeclaringType
        {
            get { return this.GenericMethod.DeclaringType; }
        }

        /// <summary>
        /// Gets the collection of generic method arguments.
        /// </summary>
        [Browsable( false )]
        public TypeSignatureCollection GenericArguments
        {
            get { return genericArguments; }
        }

        /// <summary>
        /// Gets the generic method (with formal generic method arguments).
        /// </summary>
        [Browsable( false )]
        public IGenericMethodDefinition GenericMethod
        {
            get { return (IGenericMethodDefinition) this.Parent; }
        }


        /// <inheritdoc />
        public void WriteReflectionMethodName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            GenericMap genericMap = this.GetGenericContext( GenericContextOptions.None );

            this.ReturnType.MapGenericArguments( genericMap ).WriteReflectionTypeName( stringBuilder,
                                                                                       options |
                                                                                       ReflectionNameOptions.
                                                                                           MethodParameterEncoding );
            stringBuilder.Append( ' ' );
            stringBuilder.Append( this.Name );

            stringBuilder.Append( '[' );
            for ( int i = 0; i < this.genericArguments.Count; i++ )
            {
                if ( i > 0 )
                {
                    stringBuilder.Append( ',' );
                }

                this.genericArguments[i].MapGenericArguments( genericMap ).WriteReflectionTypeName( stringBuilder,
                                                                                                    options |
                                                                                                    ReflectionNameOptions
                                                                                                        .
                                                                                                        GenericArgumentEncoding );
            }
            stringBuilder.Append( ']' );


            stringBuilder.Append( '(' );
            for ( int i = 0; i < this.ParameterCount; i++ )
            {
                if ( i > 0 )
                {
                    stringBuilder.Append( ", " );
                }
                this.GetParameterType( i ).MapGenericArguments( genericMap ).WriteReflectionTypeName( stringBuilder,
                                                                                                      options |
                                                                                                      ReflectionNameOptions
                                                                                                          .
                                                                                                          MethodParameterEncoding );
            }
            stringBuilder.Append( ')' );
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
            IMethod theMethod = this.GenericMethod;
            StringBuilder strBuilder = new StringBuilder();

            if ( theMethod.DeclaringType != null )
            {
                strBuilder.Append( theMethod.DeclaringType.ToString() );
                strBuilder.Append( "/" );
            }
            strBuilder.Append( theMethod.Name );
            strBuilder.Append( '<' );
            for ( int i = 0; i < this.genericArguments.Count; i++ )
            {
                if ( i > 0 )
                {
                    strBuilder.Append( ',' );
                }
                strBuilder.Append( this.genericArguments[i].ToString() );
            }
            strBuilder.Append( '>' );
            strBuilder.Append( '(' );
            for ( int i = 0; i < theMethod.ParameterCount; i++ )
            {
                if ( i > 0 )
                {
                    strBuilder.Append( ", " );
                }
                strBuilder.Append( theMethod.GetParameterType( i ).ToString() );
            }
            strBuilder.Append( ") : " );
            strBuilder.Append( theMethod.ReturnType.ToString() );

            return strBuilder.ToString();
        }

        #region IMethod Members

        /// <inheritdoc />
        [ReadOnly( true )]
        public CallingConvention CallingConvention { get; set; }


        /// <inheritdoc />
        public int ParameterCount
        {
            get { return this.GenericMethod.ParameterCount; }
        }


        /// <inheritdoc />
        public ITypeSignature GetParameterType( int index )
        {
            return this.GenericMethod.GetParameterType( index );
        }

        /// <inheritdoc />
        public ITypeSignature ReturnType
        {
            get { return this.GenericMethod.ReturnType; }
        }


        /// <inheritdoc />
        public string Name
        {
            get { return this.GenericMethod.Name; }
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
            // First we get the generic method
            // Problem: how to set open types as parameters?
            MethodInfo reflectionGenericMethod =
                (MethodInfo) this.GenericMethod.GetSystemMethod( genericTypeArguments, genericMethodArguments,
                                                                 ( bindingOptions & ~BindingOptions.RequireGenericMask ) |
                                                                 BindingOptions.RequireGenericInstance );
            if ( reflectionGenericMethod == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "MethodSpecDeclaration.GetSystemMethod( {{{0}}} ) : cannot find generic method.",
                    this );
                return null;
            }

            Type[] reflectionGenericArguments = new Type[genericArguments.Count];
            for ( int i = 0; i < reflectionGenericArguments.Length; i++ )
            {
                reflectionGenericArguments[i] =
                    this.genericArguments[i].GetSystemType( genericTypeArguments, genericMethodArguments );
            }

            MethodInfo reflectionMethodSpec = reflectionGenericMethod.MakeGenericMethod( reflectionGenericArguments );
            return reflectionMethodSpec;
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemMethod( genericTypeArguments, genericMethodArguments, BindingOptions.Default );
        }

        /// <inheritdoc />
        public MethodDefDeclaration GetMethodDefinition()
        {
            return this.GenericMethod.GetMethodDefinition();
        }

        /// <inheritdoc />
        public MethodDefDeclaration GetMethodDefinition( BindingOptions bindingOptions )
        {
            return this.GenericMethod.GetMethodDefinition( bindingOptions );
        }

        /// <inheritdoc />
        public MethodAttributes Attributes
        {
            get { return this.GetMethodDefinition().Attributes; }
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
        public Visibility Visibility
        {
            get { return this.GetMethodDefinition().Visibility; }
        }

        /// <inheritdoc />
        public bool IsNew
        {
            get { return this.GetMethodDefinition().IsNew; }
        }

        #endregion

        #region IGeneric Members

        /// <inheritdoc />
        int IGenericInstance.GenericArgumentCount
        {
            get { return this.genericArguments.Count; }
        }

        /// <inheritdoc />
        ITypeSignature IGenericInstance.GetGenericArgument( int ordinal )
        {
            return this.genericArguments[ordinal];
        }

        /// <inheritdoc />
        public bool IsGenericInstance
        {
            get { return this.genericArguments.Count > 0; }
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

            if ( this.genericArguments != null )
            {
                return new GenericMap( parentContext, this.genericArguments );
            }
            else
            {
                return parentContext;
            }
        }

        #endregion

        #region IMethodSignature

        /// <inheritdoc />
        public bool ReferencesAnyGenericArgument()
        {
            foreach ( ITypeSignature genericArgumentType in this.genericArguments )
            {
                if ( genericArgumentType.ContainsGenericArguments() )
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        IMethodSignature IMethodSignature.MapGenericArguments( GenericMap genericMap )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        int IMethodSignature.GenericParameterCount
        {
            get { return this.genericArguments.Count; }
        }


        /// <inheritdoc />
        bool IGeneric.IsGenericDefinition
        {
            get { return false; }
        }

        /// <inheritdoc />
        bool IGeneric.IsGenericInstance
        {
            get { return true; }
        }

        /// <inheritdoc />
        public IMethodSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            if ( this.Domain != targetModule.Domain )
            {
                throw new ArgumentOutOfRangeException(
                    "targetModule" );
            }

            #endregion

            if ( targetModule == this.Module )
            {
                return this;
            }


            // Translate generic arguments.
            TypeSignatureCollection translatedGenericArguments =
                new TypeSignatureCollection( this.genericArguments.Count );
            foreach ( ITypeSignature genericArgument in this.genericArguments )
            {
                translatedGenericArguments.Add( genericArgument.Translate( targetModule ) );
            }

            // Translate the generic method.
            IGenericMethodDefinition translatedGenericMethod =
                (IGenericMethodDefinition) this.GenericMethod.Translate( targetModule );

            // Get the proper method specification.
            return translatedGenericMethod.MethodSpecs.GetGenericInstance( translatedGenericArguments, true );
        }

        #endregion

        #region IWriteILReference Members

        /// <inheritdoc />
        void IMethodInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                               WriteMethodReferenceOptions options )
        {
            /* Samples
		
			  call       int32 [mscorlib]System.Array::BinarySearch<!TKey>(!!0[],
                                                                             int32,
                                                                             int32,
                                                                             !!0,
                                                                             class [mscorlib]System.Collections.Generic.IComparer`1<!!0>)
      
			  
		       call       bool System.Nullable::IsEqual<!T>(valuetype System.Nullable`1<!!0>,
                                                           valuetype System.Nullable`1<!!0>)

   
			   call       int32 System.Array::FindIndex<!!0>(!!0[],
                                                            class System.Predicate`1<!!0>)
  
			   call       int32 System.Array::BinarySearch<!!0>(!!0[],
                                                               int32,
                                                               int32,
                                                               !!0,
                                                               class System.Collections.Generic.IComparer`1<!!0>)

			  callvirt   instance int32 class [PostSharp.Collections]PostSharp.Collections.IndexedCollection`1<!ItemType>::AddIndex<valuetype PostSharp.CodeModel.MetadataToken,!0>
			             (string,
                         class [mscorlib]System.Collections.Generic.IDictionary`2<!!0,!!1>)
			*/


            writer.WriteCallConvention( this.GenericMethod.CallingConvention );

            ( (ITypeSignatureInternal) this.GenericMethod.ReturnType ).WriteILReference( writer, GenericMap.Empty,
                                                                                         WriteTypeReferenceOptions.
                                                                                             WriteTypeKind );

            ITypeSignature declaringType = this.DeclaringType;
            GenericMap typeContext = genericMap.GetTypeContext();

            if ( declaringType != null )
            {
                ( (ITypeSignatureInternal) declaringType ).WriteILReference( writer, typeContext,
                                                                             WriteTypeReferenceOptions.None );
                writer.WriteSymbol( "::" );
            }


            writer.WriteDottedName( this.GenericMethod.Name );
            if ( this.genericArguments.Count > 0 )
            {
                writer.WriteSymbol( '<' );
                for ( int i = 0; i < this.genericArguments.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        writer.WriteSymbol( ',' );
                    }

                    /*this.genericArguments[i].WriteILReference( writer,
					this.GenericMethod.DeclaringAssembly is AssemblyManifestDeclaration ?
					typeContext : GenericMap.Empty );*/

                    ( (ITypeSignatureInternal) this.genericArguments[i] ).WriteILReference( writer, typeContext,
                                                                                            WriteTypeReferenceOptions.
                                                                                                WriteTypeKind );
                }
                writer.WriteSymbol( '>' );
            }
            writer.WriteSymbol( '(' );
            writer.MarkAutoIndentLocation();

            for ( int i = 0; i < this.ParameterCount; i++ )
            {
                if ( i > 0 )
                {
                    writer.WriteSymbol( ',' );
                    writer.WriteLineBreak();
                }

                ( (ITypeSignatureInternal) this.GenericMethod.GetParameterType( i ) ).WriteILReference( writer,
                                                                                                        writer.Options.
                                                                                                            InMethodDefinition
                                                                                                            ? GenericMap
                                                                                                                  .
                                                                                                                  Empty
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
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of method specifications (<see cref="MethodSpecDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class MethodSpecDeclarationCollection :
            SimpleElementCollection<MethodSpecDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MethodSpecDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal MethodSpecDeclarationCollection( Declaration parent, string role )
                :
                    base( parent, role )
            {
            }

            /// <summary>
            /// Gets a generic instance of the generic method that owns this collection but does 
            /// not create the generic instance if it does not exist.
            /// </summary>
            /// <param name="genericArguments">Generic arguments.</param>
            /// <returns>A generic instance of the owner method constructed with <paramref name="genericArguments"/>,
            /// or <b>null</b> if this generic instance does not exist.</returns>
            public MethodSpecDeclaration GetGenericInstance( IList<ITypeSignature> genericArguments )
            {
                return this.GetGenericInstance( genericArguments, false );
            }

            /// <summary>
            /// Gets a generic instance of the generic method that owns this collection and specifies whether to
            /// create the generic instance if it does not exist.
            /// </summary>
            /// <param name="genericArguments">Generic arguments.</param>
            /// <param name="create">Whether the generic instance should be created if it does not exist.</param>
            /// <returns>A generic instance of the owner method constructed with <paramref name="genericArguments"/>,
            /// or <b>null</b> if this generic instance does not exist and the <paramref name="create"/> parameter
            /// is <b>false</b>.</returns>
            public MethodSpecDeclaration GetGenericInstance( IList<ITypeSignature> genericArguments, bool create )
            {
                Domain domain = this.Owner.Domain;

                foreach ( MethodSpecDeclaration method in this )
                {
                    bool match = true;
                    for ( int i = 0; i < genericArguments.Count; i++ )
                    {
                        if ( !method.GenericArguments[i].MatchesReference( genericArguments[i] ) )
                        {
                            match = false;
                            break;
                        }
                    }

                    if ( match )
                    {
                        return method;
                    }
                }

                if ( create )
                {
                    MethodSpecDeclaration method = new MethodSpecDeclaration();
                    this.Add( method );
                    method.GenericArguments.AddRange( genericArguments );
                    method.CallingConvention = CallingConvention.GenericInstance;
                    return method;
                }
                else
                {
                    return null;
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
                IGenericMethodDefinition scope = (IGenericMethodDefinition) this.Owner;
                scope.Module.ModuleReader.ImportMethodSpecs( scope );
            }
        }
    }
}