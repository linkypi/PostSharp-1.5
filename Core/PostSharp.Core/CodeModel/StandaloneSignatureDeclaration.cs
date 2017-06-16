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
using PostSharp.CodeModel.Collections;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a stand-alone signature (<see cref="TokenType.Signature"/>). 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Stand-alone signatures
    /// are owned by the module (<see cref="ModuleDeclaration"/>).
    /// </para>
    /// Stand-alone signatures are used solely inside method body. This information is
    /// read internally by PostSharp, so user code should not need to manipulate
    /// stand-alone signatures.
    /// </remarks>
    public sealed class StandaloneSignatureDeclaration : MetadataDeclaration
    {
        #region Fields

        /// <summary>
        /// Signature (<see cref="MethodSignature"/>, <see cref="IType"/>
        /// or <see cref="LocalVariableDeclarationCollection"/>).
        /// </summary>
        private object signature;

        /// <summary>
        /// Kind of signature.
        /// </summary>
        private StandaloneSignatureKind kind;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="StandaloneSignatureDeclaration"/>.
        /// </summary>
        public StandaloneSignatureDeclaration()
        {
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.Signature;
        }

        #region Setters

        /// <summary>
        /// Notify that the current <see cref="StandaloneSignatureDeclaration"/> shall
        /// contain local variables, and return the collection of local variables.
        /// </summary>
        public LocalVariableDeclarationCollection SetLocalVariables()
        {
            LocalVariableDeclarationCollection localVariables =
                new LocalVariableDeclarationCollection( this, "localVariables" );
            this.signature = localVariables;
            this.kind = StandaloneSignatureKind.LocalVariables;
            return localVariables;
        }

        /// <summary>
        /// Sets the current stand-alone signature to a method signature.
        /// </summary>
        /// <param name="value">A method signature.</param>
        public void SetMethodSignature( MethodSignature value )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( value, "value" );

            #endregion

            this.signature = value;
            this.kind = StandaloneSignatureKind.MethodSignature;
        }

        /// <summary>
        /// Sets the current stand-alone signature to a type signature.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="kind"></param>
        public void SetTypeSignature( ITypeSignature value, StandaloneSignatureKind kind )
        {
            if ( kind != StandaloneSignatureKind.FieldSignature &&
                 kind != StandaloneSignatureKind.PropertySignature )
            {
                throw new ArgumentOutOfRangeException( "kind" );
            }

            this.signature = value;
            this.kind = kind;
        }

        #endregion

        #region Getters (properties)

        /// <summary>
        /// Gets the kind of signature contained in the current instance.
        /// </summary>
        public StandaloneSignatureKind SignatureKind { get { return this.kind; } }

        /// <summary>
        /// Gets the collection of local variables 
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance contains a different kind of signature.
        /// </exception>
        public LocalVariableDeclarationCollection LocalVariables
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.kind == StandaloneSignatureKind.LocalVariables,
                                                           "StandaloneSignatureInvalidKind" );

                #endregion

                return (LocalVariableDeclarationCollection) signature;
            }
        }

        /// <summary>
        /// Gets the method signature.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance contains a different kind of signature.
        /// </exception>
        public MethodSignature MethodSignature
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.kind == StandaloneSignatureKind.MethodSignature,
                                                           "StandaloneSignatureInvalidKind" );

                #endregion

                return (MethodSignature) signature;
            }
        }

        /// <summary>
        /// Gets the type signature.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance contains a different kind of signature.
        /// </exception>
        public TypeSignature TypeSignature
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.kind == StandaloneSignatureKind.FieldSignature ||
                                                           this.kind == StandaloneSignatureKind.PropertySignature,
                                                           "StandaloneSignatureInvalidKind" );

                #endregion

                return (TypeSignature) signature;
            }
        }

        #endregion

        #region IWriteILReference Members

        /// <summary>
        /// Writes the signature to IL.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The current <see cref="GenericMap"/>.</param>
        /// <param name="options">Options.</param>
        internal void WriteILReference( ILWriter writer, GenericMap genericMap, WriteTypeReferenceOptions options )
        {
            switch ( this.kind )
            {
                case StandaloneSignatureKind.FieldSignature:
                case StandaloneSignatureKind.PropertySignature:
                    this.TypeSignature.InternalWriteILReference( writer, genericMap, options );
                    break;

                case StandaloneSignatureKind.LocalVariables:
                    throw ExceptionHelper.Core.CreateInvalidOperationException( "CannotWriteReferenceOfLocalVariables" );

                case StandaloneSignatureKind.MethodSignature:
                    this.MethodSignature.WriteILReference( writer, genericMap );
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.kind, "this.Kind" );
            }
        }

        #endregion

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return new NotSupportedException();
        }

        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Enumerates the kind of stand-alone signatures (<see cref="StandaloneSignatureDeclaration"/>).
    /// </summary>
    public enum StandaloneSignatureKind
    {
        /// <summary>
        /// Collection of local variables.
        /// </summary>
        LocalVariables,
        /// <summary>
        /// Method signature.
        /// </summary>
        MethodSignature,
        /// <summary>
        /// Field type signature.
        /// </summary>
        FieldSignature,
        /// <summary>
        /// Property type signature.
        /// </summary>
        PropertySignature
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of <see cref="StandaloneSignatureDeclaration"/>.
        /// </summary>
        public sealed class StandaloneSignatureDeclarationCollection : ElementCollection<StandaloneSignatureDeclaration>
        {
            private static readonly ICollectionFactory<StandaloneSignatureDeclaration> collectionFactory =
                ListFactory<StandaloneSignatureDeclaration>.Default;

            internal StandaloneSignatureDeclarationCollection(Element parent, string role) : base(parent, role)
            {
            }

            internal override ICollectionFactory<StandaloneSignatureDeclaration> GetCollectionFactory()
            {
                return collectionFactory;
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get
                {
                    return true;
                }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
               ((ModuleDeclaration) this.Owner).ModuleReader.ImportStandaloneSignatures();
            }
        }
    }
}