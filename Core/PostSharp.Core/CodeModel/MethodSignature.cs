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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a method signature, which specifies primarly a calling convention and 
    /// the type of parameters.
    /// </summary>
    public sealed class MethodSignature : IMethodSignature, IVisitable<ITypeSignature>
    {
        #region Fields

        private readonly ModuleDeclaration module;

        /// <summary>
        /// Calling convention.
        /// </summary>
        private CallingConvention callingConvention;

        /// <summary>
        /// Return type.
        /// </summary>
        private ITypeSignature returnType;

        /// <summary>
        /// Explicit number of generic method parameters.
        /// </summary>
        private int arity;

        /// <summary>
        /// Type of variable parameters in a <see cref="PostSharp.CodeModel.CallingConvention.VarArg"/>
        /// method.
        /// </summary>
        /// <remarks>
        /// A collection of type signatures, or <b>null</b> if the signature
        /// has no variable parameters.
        /// </remarks>
        private TypeSignatureCollection variableParameterTypes;


        /// <summary>
        /// Type of parameters.
        /// </summary>
        private readonly TypeSignatureCollection parameterTypes;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MethodSignature"/>.
        /// </summary>
        public MethodSignature( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            this.returnType = module.Cache.GetIntrinsic( IntrinsicType.Void );
            this.parameterTypes = new TypeSignatureCollection( 4 );
            this.module = module;
        }

        /// <summary>
        /// Initializes a new <see cref="MethodSignature"/> from an existing <see cref="IMethodSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to copy.</param>
        public MethodSignature( IMethodSignature signature )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( signature, "source" );

            #endregion

            this.module = signature.Module;
            this.callingConvention = signature.CallingConvention;
            this.returnType = signature.ReturnType;
            this.parameterTypes = new TypeSignatureCollection( signature.ParameterCount );
            this.arity = signature.GenericParameterCount;

            for ( int i = 0; i < signature.ParameterCount; i++ )
            {
                this.parameterTypes.Add( signature.GetParameterType( i ) );
            }

            if ( ( this.CallingConvention & CallingConvention.VarArg ) != 0 )
            {
                this.variableParameterTypes = new TypeSignatureCollection( 4 );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="MethodSignature"/>.
        /// </summary>
        /// <param name="module">Module to which the new signature belongs.</param>
        /// <param name="callingConvention">Calling convention.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="parameterTypes">Parameter types (the content of the current collection
        /// is copied, i.e. the collection is not copied by reference).</param>
        /// <param name="genericParameterCount">Number of generic parameters (<i>arity</i>).</param>
        public MethodSignature( ModuleDeclaration module,
                                CallingConvention callingConvention,
                                ITypeSignature returnType,
                                ICollection<ITypeSignature> parameterTypes,
                                int genericParameterCount )
        {
            this.callingConvention = callingConvention;
            this.returnType = returnType;
            this.arity = genericParameterCount;
            this.module = module;

            if ( parameterTypes != null )
            {
                this.parameterTypes = new TypeSignatureCollection( parameterTypes );
            }
            else
            {
                this.parameterTypes = new TypeSignatureCollection( 0 );
            }

            if ( callingConvention == CallingConvention.VarArg )
            {
                this.variableParameterTypes = new TypeSignatureCollection( 4 );
            }
        }

        #region Properties

        /// <inheritdoc />
        public ModuleDeclaration Module
        {
            get { return this.module; }
        }

        /// <inheritdoc />
        public IAssembly DeclaringAssembly
        {
            get { return module == null ? null : module.Assembly; }
        }

        /// <summary>
        /// Gets or sets the arity, i.e. the number of generic method parameters.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming",
            "CA1704", Justification = "Spelling is correct." )]
        public int GenericParameterCount
        {
            get { return this.arity; }
            set { this.arity = value; }
        }

        /// <inheritdoc />
        public bool MatchesReference( IMethodSignature reference )
        {
            return CompareHelper.Equals( this, reference, false );
        }


        /// <summary>
        /// Gets or sets the calling convention.
        /// </summary>
        /// <value>
        /// A <see cref="CallingConvention"/>.
        /// </value>
        public CallingConvention CallingConvention
        {
            get { return callingConvention; }
            set
            {
                bool changing = value != callingConvention;
                callingConvention = value;

                if ( changing )
                {
                    this.OnCallingConventionChanged();
                }
            }
        }

        /// <inheritdoc />
        private void OnCallingConventionChanged()
        {
            if ( this.CallingConvention == CallingConvention.VarArg )
            {
                this.variableParameterTypes = new TypeSignatureCollection( 4 );
            }
            else
            {
                this.variableParameterTypes = null;
            }
        }


        /// <summary>
        /// Gets or sets the return type.
        /// </summary>
        public ITypeSignature ReturnType
        {
            get { return returnType; }
            set
            {
                #region Preconditions

                if ( value == null )
                {
                    throw new ArgumentNullException( "value" );
                }

                #endregion

                returnType = value;
            }
        }

        /// <inheritdoc />
        int IMethodSignature.ParameterCount
        {
            get { return this.parameterTypes.Count; }
        }

        /// <inheritdoc />
        ITypeSignature IMethodSignature.GetParameterType( int index )
        {
            return this.parameterTypes[index];
        }


        /// <summary>
        /// Gets the collection of parameter types.
        /// </summary>
        public TypeSignatureCollection ParameterTypes
        {
            get { return parameterTypes; }
        }

        /// <summary>
        /// Gets the collection of variable parameters types
        /// (i.e. the parameters sent as the list of unknown
        /// parameters of a <b>vararg</b> method.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The calling convention is not <see cref="PostSharp.CodeModel.CallingConvention.VarArg"/>.
        /// </exception>
        public TypeSignatureCollection VariableParameterTypes
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.variableParameterTypes != null,
                                                           "CallingConventionIsNotVarArg" );

                #endregion

                return variableParameterTypes;
            }
        }

        #endregion

        /// <inheritdoc />
        public void Visit( string role, Visitor<ITypeSignature> visitor )
        {
            ExceptionHelper.AssertArgumentNotNull( visitor, "visitor" );

            if ( this.returnType != null )
            {
                visitor( this, "ReturnType", this.returnType );
                this.returnType.Visit( role, visitor );
            }

            for ( int i = 0; i < this.parameterTypes.Count; i++ )
            {
                ITypeSignature typeSignature = this.parameterTypes[i];
                visitor( this,
                         string.Format( CultureInfo.InvariantCulture, "ParameterType:{0}", i ),
                         typeSignature );
                typeSignature.Visit( role, visitor );
            }

            if ( this.variableParameterTypes != null )
            {
                for ( int i = 0; i < this.variableParameterTypes.Count; i++ )
                {
                    ITypeSignature typeSignature = this.variableParameterTypes[i];
                    visitor( this,
                             string.Format( CultureInfo.InvariantCulture, "VariableParameterType:{0}", i ),
                             typeSignature );
                    typeSignature.Visit( role, visitor );
                }
            }
        }

        /// <inheritdoc />
        public bool Equals( IMethodSignature other )
        {
            return CompareHelper.Equals( this, other, true );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append( "(" );

            for ( int i = 0; i < this.ParameterTypes.Count; i++ )
            {
                if ( i > 0 )
                {
                    stringBuilder.Append( ", " );
                }

                stringBuilder.Append( this.ParameterTypes[i].ToString() );
            }

            if ( this.variableParameterTypes != null && this.variableParameterTypes.Count > 0 )
            {
                stringBuilder.Append( " ; " );
                for ( int i = 0; i < this.variableParameterTypes.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        stringBuilder.Append( ", " );
                    }

                    stringBuilder.Append( this.variableParameterTypes[i].ToString() );
                }
            }

            stringBuilder.Append( ")" );
            if ( this.returnType != null )
            {
                stringBuilder.Append( " : " );
                stringBuilder.Append( this.returnType.ToString() );
            }


            return stringBuilder.ToString();
        }

        /// <inheritdoc />
        public bool ReferencesAnyGenericArgument()
        {
            if ( this.returnType.ContainsGenericArguments() )
            {
                return true;
            }

            foreach ( ITypeSignature parameterType in this.parameterTypes )
            {
                if ( parameterType.ContainsGenericArguments() )
                {
                    return true;
                }
            }

            if ( this.variableParameterTypes != null )
            {
                foreach ( ITypeSignature parameterType in this.variableParameterTypes )
                {
                    if ( parameterType.ContainsGenericArguments() )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc />
        public IMethodSignature MapGenericArguments( GenericMap genericMap )
        {
            if ( this.ReferencesAnyGenericArgument() )
            {
                TypeSignatureCollection parameters = new TypeSignatureCollection( this.ParameterTypes.Count );

                for ( int i = 0; i < this.ParameterTypes.Count; i++ )
                {
                    parameters.Add( this.ParameterTypes[i].MapGenericArguments( genericMap ) );
                }

                MethodSignature newSignature = new MethodSignature( this.module,
                                                                    this.CallingConvention,
                                                                    this.ReturnType.MapGenericArguments( genericMap ),
                                                                    parameters, this.arity );

                if ( this.variableParameterTypes != null )
                {
                    foreach ( ITypeSignature type in this.variableParameterTypes )
                    {
                        newSignature.variableParameterTypes.Add( type.MapGenericArguments( genericMap ) );
                    }
                }

                return newSignature;
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc />
        public IMethodSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );

            #endregion

            if ( targetModule == module )
            {
                return this;
            }

            TypeSignatureCollection translatedParameterTypes = new TypeSignatureCollection( this.ParameterTypes.Count );
            foreach ( ITypeSignature parameterType in this.ParameterTypes )
            {
                translatedParameterTypes.Add( parameterType.Translate( targetModule ) );
            }

            ITypeSignature translatedReturnType = this.returnType != null
                                                      ? this.returnType.Translate( targetModule )
                                                      : null;

            MethodSignature methodSignature = new MethodSignature(
                targetModule,
                this.CallingConvention,
                translatedReturnType, translatedParameterTypes, this.arity );

            if ( this.variableParameterTypes != null )
            {
                methodSignature.variableParameterTypes =
                    new TypeSignatureCollection( this.variableParameterTypes.Count );
                foreach ( ITypeSignature parameterType in this.variableParameterTypes )
                {
                    methodSignature.variableParameterTypes.Add( parameterType.Translate( targetModule ) );
                }
            }

            return methodSignature;
        }

        #region writer IL

        /// <inheritdoc cref="IMethodInternal.WriteILReference"/>
        internal void WriteILReference( ILWriter writer, GenericMap genericMap )
        {
            this.WriteILDefinition( writer, genericMap, false );
        }

        /// <summary>
        /// Writes the IL signature (i.e. the list of parameters).
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="lineBreak">Whether line breaks should be issued
        /// between parameters.</param>
        internal void WriteSignature( ILWriter writer, bool lineBreak )
        {
            writer.WriteSymbol( '(' );

            writer.MarkAutoIndentLocation();
            for ( int i = 0; i < this.ParameterTypes.Count; i++ )
            {
                if ( i > 0 )
                {
                    writer.WriteSymbol( ',' );
                    if ( lineBreak )
                    {
                        writer.WriteLineBreak();
                    }
                }

                ( (ITypeSignatureInternal) this.ParameterTypes[i] ).WriteILReference( writer, GenericMap.Empty,
                                                                                      WriteTypeReferenceOptions.
                                                                                          WriteTypeKind );

                // todo: marshal
            }
            if ( this.variableParameterTypes != null && this.variableParameterTypes.Count > 0 )
            {
                if ( this.ParameterTypes.Count > 0 )
                {
                    writer.WriteSymbol( ',' );
                    if ( lineBreak )
                    {
                        writer.WriteLineBreak();
                    }
                }
                writer.WriteSymbol( "..." );

                for ( int i = 0; i < this.variableParameterTypes.Count; i++ )
                {
                    writer.WriteSymbol( ',' );
                    if ( lineBreak )
                    {
                        writer.WriteLineBreak();
                    }

                    ( (ITypeSignatureInternal) this.variableParameterTypes[i] ).WriteILReference( writer,
                                                                                                  GenericMap.Empty,
                                                                                                  WriteTypeReferenceOptions
                                                                                                      .WriteTypeKind );
                }
            }
            writer.WriteSymbol( ')' );
            writer.ResetIndentLocation();
        }

        /// <summary>
        /// Writes the IL definition of the current signature.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The current <see cref="GenericMap"/>.</param>
        /// <param name="asMethodPointer"><b>true</b> if the definition should be
        /// written as for a method pointer, otherwise <b>false</b>.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap,
                                         bool asMethodPointer )
        {
            if ( asMethodPointer )
            {
                writer.WriteKeyword( "method" );
            }
            writer.WriteCallConvention( this.CallingConvention );
            ( (ITypeSignatureInternal) this.ReturnType ).WriteILReference( writer, genericMap,
                                                                           WriteTypeReferenceOptions.WriteTypeKind );
            if ( asMethodPointer )
            {
                writer.WriteSymbol( '*' );
            }

            this.WriteSignature( writer, !asMethodPointer );
        }

        #endregion
    }
}