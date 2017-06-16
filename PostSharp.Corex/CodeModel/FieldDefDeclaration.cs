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
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a field definition (<see cref="TokenType.FieldDef"/>). 
    /// </summary>
    /// <remarks>
    /// Fieds are owned by a types (<see cref="TypeDefDeclaration"/>) on the
    ///  <see cref="TypeDefDeclaration.Fields"/> property.
    /// </remarks>
    public sealed class FieldDefDeclaration : NamedDeclaration, IFieldInternal,
                                              IWriteILDefinition, IRemoveable
    {
        #region Fields

        /// <summary>
        /// When applied to the <see cref="FieldDefDeclaration.Offset"/> value,
        /// indicates that the offset is determined by the runtime
        /// automatically.
        /// </summary>
        public const int AutoOffset = -1;

        /// <summary>
        /// Field type.
        /// </summary>
        private ITypeSignatureInternal type;


        /// <summary>
        /// Literal value, when the type is <i>literal</i>, i.e. <i>constant</i>.
        /// </summary>
        private SerializedValue literalValue;

        /// <summary>
        /// Marshal type.
        /// </summary>
        /// <value>
        /// A <see cref="MarshalType"/>, or <b>null</b> if the fied
        /// has default marshalling.
        /// </value>
        private MarshalType marshalType;

        /// <summary>
        /// Field offset, when the layout of the declaring type is explicit.
        /// </summary>
        /// <remarks>
        /// A strictly positive integer, or <see cref="AutoOffset"/> is the offset
        /// should be determined by the runtime.
        /// </remarks>
        private int offset = AutoOffset;

        /// <summary>
        /// Field attributes.
        /// </summary>
        private FieldAttributes attributes;

        /// <summary>
        /// Initial field value.
        /// </summary>
        /// <value>
        /// A <see cref="DataSectionDeclaration"/>, or <b>null</b> if the field has
        /// default (zero) initial value.
        /// </value>
        private DataSectionDeclaration data;

        #endregion

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.FieldDef;
        }

        /// <summary>
        /// Gets the generic context of the declaring type.
        /// </summary>
        /// <returns>The generic context of the declaring type, or an empty context
        /// if the current field is declared at module level.</returns>
        public GenericMap GetGenericContext( GenericContextOptions options )
        {
            TypeDefDeclaration declaringType = this.DeclaringType;
            if ( declaringType != null )
            {
                return declaringType.GetGenericContext( options );
            }
            else
            {
                return GenericMap.Empty;
            }
        }

        /// <inheritdoc />
        public FieldInfo GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new FieldWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public FieldInfo GetSystemField( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            Module module = this.Module.GetSystemModule();
            return module != null
                       ? module.ResolveField( (int) this.MetadataToken.Value, genericTypeArguments,
                                              genericMethodArguments )
                       : null;
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemField( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public IField Translate( ModuleDeclaration targetModule )
        {
            return ( (IType) this.DeclaringType.Translate( targetModule ) ).Fields.GetField( this.Name,
                                                                                             this.FieldType.Translate(
                                                                                                 targetModule ),
                                                                                             BindingOptions.Default );
        }

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "CannotRemoveBecauseNoParent" );

            #endregion

            this.DeclaringType.Fields.Remove( this );
        }

        #region Properties

        /// <summary>
        /// Gets the type declaring the current field.
        /// </summary>
        public TypeDefDeclaration DeclaringType
        {
            get { return (TypeDefDeclaration) this.Parent; }
        }

        /// <inheritdoc />
        IType IMember.DeclaringType
        {
            get { return (INamedType) this.Parent; }
        }

        /// <summary>
        /// Gets the field visibility.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                switch ( this.attributes & FieldAttributes.FieldAccessMask )
                {
                    case FieldAttributes.Assembly:
                        return Visibility.Assembly;

                    case FieldAttributes.FamANDAssem:
                        return Visibility.FamilyAndAssembly;

                    case FieldAttributes.FamORAssem:
                        return Visibility.FamilyOrAssembly;

                    case FieldAttributes.Private:
                        return Visibility.Private;

                    case FieldAttributes.Family:
                        return Visibility.Family;

                    case FieldAttributes.Public:
                        return Visibility.Public;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( this.attributes, "this.Attributes" );
                }
            }
        }

        /// <summary>
        /// Gets or set the field type.
        /// </summary>
        /// <value>
        /// A <see cref="TypeSignature"/>.
        /// </value>
        [ReadOnly( true )]
        public ITypeSignature FieldType
        {
            get { return this.type; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                this.type = (ITypeSignatureInternal) value;
            }
        }

        FieldDefDeclaration IField.GetFieldDefinition()
        {
            return this;
        }

        FieldDefDeclaration IField.GetFieldDefinition( BindingOptions bindingOptions )
        {
            return this;
        }


        /// <summary>
        /// Gets the value of the literal field.
        /// </summary>
        /// <value>
        /// A <see cref="SerializedValue"/>, or <b>null</b> if the field is not literal.
        /// </value>
        /// <remarks>
        /// The value of the current property determines the value of a literal field,
        /// i.e. a field whose <see cref="FieldDefDeclaration.Attributes"/> includes
        /// <see cref="FieldAttributes.Literal"/>. Some rules apply to literal fields.
        /// They are not enforced by the Code Object Model.
        /// </remarks>
        [ReadOnly( true )]
        public SerializedValue LiteralValue
        {
            get { return this.literalValue; }
            set { this.literalValue = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataSectionDeclaration"/> used to initialize
        /// the field.
        /// </summary>
        /// <value>
        /// A <see cref="DataSectionDeclaration"/>, or <b>null</b> if the field should be
        /// initialized with its default (zero) value.
        /// </value>
        /// <remarks>
        /// This field should be set if and only if
        /// the field <see cref="FieldDefDeclaration.Attributes"/> includes
        /// <see cref="FieldAttributes.HasFieldRVA"/>. Some rules apply
        /// to initialized fields. They are not enforced by the
        /// Code Object Model.
        /// </remarks>
        [ReadOnly( true )]
        public DataSectionDeclaration InitialValue
        {
            get { return this.data; }
            set { this.data = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="MarshalType"/> of the field.
        /// </summary>
        /// <value>
        /// A <see cref="MarshalType"/>, or <b>null</b> if default
        /// marshalling should be performed.
        /// </value>
        /// <remarks>
        /// This field should be set if and only if
        /// the field <see cref="FieldDefDeclaration.Attributes"/> includes
        /// <see cref="FieldAttributes.HasFieldMarshal"/>. This rule
        /// is not enforced by the Code Object Model.
        /// </remarks>
        [ReadOnly( true )]
        public MarshalType MarshalType
        {
            get { return this.marshalType; }
            set { this.marshalType = value; }
        }

        /// <summary>
        /// Gets or sets the offset of the current field in the type layout.
        /// </summary>
        /// <value>
        /// The offset of the current field in the type layout,
        /// or <see cref="FieldDefDeclaration.AutoOffset"/> if the field
        /// offset is computed by the runtime.
        /// </value>
        [ReadOnly( true )]
        public int Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Gets or sets the field attributes.
        /// </summary>
        /// <value>
        /// A combination of <see cref="FieldAttributes"/>.
        /// </value>
        [ReadOnly( true )]
        public FieldAttributes Attributes
        {
            get { return this.attributes; }
            set { this.attributes = value; }
        }

        /// <inheritdoc />
        public bool IsStatic
        {
            get { return ( this.attributes & FieldAttributes.Static ) != 0; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return ( this.attributes & FieldAttributes.InitOnly ) != 0; }
        }

        /// <inheritdoc />
        public bool IsConst
        {
            get { return ( this.attributes & FieldAttributes.Literal ) != 0; }
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            this.WriteILDefinition( writer, this.DeclaringType.GetGenericContext( GenericContextOptions.None ) );
        }

        /// <summary>
        /// Writes the IL definition of the current field.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of
        /// the containing type.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap )
        {
            writer.WriteKeyword( ".field" );

            if ( this.offset != AutoOffset )
            {
                writer.WriteSymbol( '[' );
                writer.WriteInteger( this.offset, IntegerFormat.Decimal );
                writer.WriteSymbol( ']' );
            }

            #region Field Attributes

            switch ( this.attributes & FieldAttributes.FieldAccessMask )
            {
                case FieldAttributes.Public:
                    writer.WriteKeyword( "public" );
                    break;

                case FieldAttributes.Private:
                    writer.WriteKeyword( "private" );
                    break;
            }

            if ( ( this.attributes & FieldAttributes.Static ) != 0 )
            {
                writer.WriteKeyword( "static" );
            }


            switch ( this.attributes & FieldAttributes.FieldAccessMask )
            {
                case FieldAttributes.PrivateScope:
                    writer.WriteKeyword( "privatescope" );
                    break;


                case FieldAttributes.Family:
                    writer.WriteKeyword( "family" );
                    break;

                case FieldAttributes.Assembly:
                    writer.WriteKeyword( "assembly" );
                    break;

                case FieldAttributes.FamANDAssem:
                    writer.WriteKeyword( "famandassem" );
                    break;

                case FieldAttributes.FamORAssem:
                    writer.WriteKeyword( "famorassem" );
                    break;
            }


            if ( ( this.attributes & FieldAttributes.InitOnly ) != 0 )
            {
                writer.WriteKeyword( "initonly" );
            }

            if ( ( this.attributes & FieldAttributes.Literal ) != 0 )
            {
                writer.WriteKeyword( "literal" );
            }

            if ( ( this.attributes & FieldAttributes.NotSerialized ) != 0 )
            {
                writer.WriteKeyword( "notserialized" );
            }

            if ( ( this.attributes & FieldAttributes.SpecialName ) != 0 )
            {
                writer.WriteKeyword( "specialname" );
            }

            if ( ( this.attributes & FieldAttributes.RTSpecialName ) != 0 )
            {
                writer.WriteKeyword( "rtspecialname" );
            }


            // Todo: marshal.

            #endregion

            if ( this.marshalType != null )
            {
                writer.WriteKeyword( "marshal" );
                writer.WriteSymbol( '(' );
                this.marshalType.WriteILReference( writer );
                writer.WriteSymbol( ')' );
            }

            this.type.WriteILReference( writer, genericMap, WriteTypeReferenceOptions.WriteTypeKind );


            writer.WriteIdentifier( this.Name );

            if ( ( this.attributes & FieldAttributes.HasDefault ) != 0 )
            {
                writer.WriteSymbol( '=' );
                if ( this.literalValue == null )
                {
                    writer.WriteKeyword( "nullref" );
                }
                else
                {
                    this.literalValue.WriteILValue( writer, WriteSerializedValueMode.FieldValue );
                }
            }

            if ( ( this.attributes & FieldAttributes.HasFieldRVA ) != 0 && this.data != null )
            {
                writer.WriteKeyword( "at" );
                this.data.WriteILReference( writer );
            }

            writer.WriteLineBreak();
            this.CustomAttributes.WriteILDefinition( writer );
        }

        /// <inheritdoc />
        void IFieldInternal.WriteILReference( ILWriter writer, GenericMap genericMap )
        {
            TypeDefDeclaration declaringType = this.Parent as TypeDefDeclaration;

            this.type.WriteILReference( writer, GenericMap.Empty, WriteTypeReferenceOptions.WriteTypeKind );

            if ( declaringType != null )
            {
                ( (ITypeSignatureInternal) declaringType ).WriteILReference( writer, genericMap,
                                                                             WriteTypeReferenceOptions.None );
                writer.WriteSymbol( "::" );
            }

            writer.WriteIdentifier( this.Name );
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return this.DeclaringType.ToString() + "/" + this.Name;
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of <see cref="FieldDefDeclaration"/> objects.
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class FieldDefDeclarationCollection :
            OrderedEmitAndByUniqueNameDeclarationCollection<FieldDefDeclaration>,
            IFieldCollection
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FieldDefDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal FieldDefDeclarationCollection( Declaration parent, string role ) : base( parent, role )
            {
            }

            /// <inheritdoc />
            protected override bool RequiresEmitOrdering
            {
                get
                {
                    return true;
                }
            }

            #region IEnumerable<IField> Members

            IField IFieldCollection.GetByName( string name )
            {
                return this.GetByName( name );
            }

            IEnumerator<IField> IEnumerable<IField>.GetEnumerator()
            {
                return EnumeratorEnlarger.EnlargeEnumerator<FieldDefDeclaration, IField>( this.GetEnumerator() );
            }

            #endregion

            /// <inheritdoc />
            public IField GetField( string name, ITypeSignature type, BindingOptions bindingOptions )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( name, "name" );
                ExceptionHelper.AssertArgumentNotNull( type, "type" );

                #endregion

                FieldDefDeclaration field = this.GetByName( name );
                if ( field != null )
                {
                    if ( field.FieldType.MatchesReference( type ) )
                    {
                        return field;
                    }
                    else if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                    {
                        throw ExceptionHelper.Core.CreateBindingException( "FoundFieldWithDifferentType",
                                                                           name, field.DeclaringType.ToString(),
                                                                           field.FieldType.ToString(),
                                                                           type.ToString() );
                    }
                    else
                    {
                        Trace.ReflectionBinding.WriteLine(
                            "TypeDefDeclaration.Fields.GetField( {{{0}}}, {{{1}}} ): a field with the same name " +
                            "but a different type exists.", name, type );

                        return null;
                    }
                }
                else if ( ( bindingOptions & BindingOptions.DontThrowException ) == 0 )
                {
                    throw ExceptionHelper.Core.CreateBindingException( "BindingCannotFindField",
                                                                       name, type.ToString(), this.Owner,
                                                                       ( (IDeclaration) this.Owner ).Module );
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
                TypeDefDeclaration typeDef = (TypeDefDeclaration) this.Owner;
                typeDef.Module.ModuleReader.ImportFieldDefs( typeDef );
            }
        }
    }
}