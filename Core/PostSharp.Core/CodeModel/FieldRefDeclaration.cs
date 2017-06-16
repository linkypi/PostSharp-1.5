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
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a reference to a field of a
    /// type defined in an external assembly (<see cref="TokenType.MemberRef"/>). 
    /// </summary>
    /// <remarks>
    /// Field references are
    /// owned by type references (<see cref="TypeRefDeclaration"/>) on the
    /// <see cref="TypeRefDeclaration.FieldRefs"/> property, or by type
    /// specifications (<see cref="TypeSpecDeclaration"/>) on the
    /// <see cref="TypeSpecDeclaration.FieldRefs"/> property.
    /// </remarks>
    public sealed class FieldRefDeclaration : MemberRefDeclaration, IFieldInternal
    {
        /// <summary>
        /// Type of the referenced field.
        /// </summary>
        private ITypeSignatureInternal type;

        private FieldDefDeclaration cachedFieldDef;

        /// <summary>
        /// Initializes a new <see cref="FieldRefDeclaration"/>.
        /// </summary>
        public FieldRefDeclaration()
        {
        }

        /// <summary>
        /// Gets or sets the type of the referenced field.
        /// </summary>
        /// <value>
        /// A <see cref="TypeSignature"/>.
        /// </value>
        [ReadOnly( true )]
        public ITypeSignature FieldType
        {
            get { return type; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                type = (ITypeSignatureInternal) value;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ResolutionScope.ToString() + "/" + this.Name + " : " + this.type.ToString();
        }

        /// <inheritdoc />
        public FieldInfo GetSystemField( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            Module module = this.Module.GetSystemModule();
            return module != null ?
                module.ResolveField( (int) this.MetadataToken.Value, genericTypeArguments,
                genericMethodArguments ) : null;
        }

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.GetSystemField(genericTypeArguments, genericMethodArguments);
        }

        /// <inheritdoc />
        public FieldInfo GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new FieldWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.GetReflectionWrapper(genericTypeArguments, genericMethodArguments);
        }

        /// <inheritdoc />
        void IFieldInternal.WriteILReference( ILWriter writer, GenericMap genericMap )
        {
            ITypeSignature declaringType = this.DeclaringType;

            this.type.WriteILReference( writer, GenericMap.Empty, WriteTypeReferenceOptions.WriteTypeKind );

            if ( declaringType != null )
            {
                ( (ITypeSignatureInternal) declaringType ).WriteILReference( writer, genericMap,
                                                                             WriteTypeReferenceOptions.None );
                writer.WriteSymbol( "::" );
            }

            writer.WriteIdentifier( this.Name );
        }

        /// <inheritdoc />
        public GenericMap GetGenericContext( GenericContextOptions options )
        {
            IType declaringType = this.DeclaringType;
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
        public FieldDefDeclaration GetFieldDefinition()
        {
            if (this.cachedFieldDef == null)
            {
                this.cachedFieldDef =  this.GetFieldDefinition(BindingOptions.Default);
            }
            return this.cachedFieldDef;
        }
        /// <inheritdoc />
        public FieldDefDeclaration GetFieldDefinition(BindingOptions bindingOptions)
        {
            TypeDefDeclaration typeDef = this.DeclaringType.GetTypeDefinition(bindingOptions);

            if (typeDef == null) return null;

            return this.cachedFieldDef =
                (FieldDefDeclaration)
                typeDef.Fields.GetField(this.Name, this.type.Translate(typeDef.Module),
                                        BindingOptions.OnlyExisting | bindingOptions);

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
        public FieldAttributes Attributes
        {
            get { return this.GetFieldDefinition().Attributes; }
        }

        /// <inheritdoc />
        public bool IsStatic
        {
            get { return this.GetFieldDefinition().IsStatic; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return this.GetFieldDefinition().IsReadOnly; }
        }

        /// <inheritdoc />
        public bool IsConst
        {
            get { return this.GetFieldDefinition().IsConst; }
        }

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedFieldDef.ClearCache();
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of <see cref="FieldRefDeclaration"/> objects.
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class FieldRefDeclarationCollection :
            UniquelyNamedElementCollection<FieldRefDeclaration>, IFieldCollection
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FieldRefDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal FieldRefDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }

            IField IFieldCollection.GetByName(string name)
            {
                return this.GetByName(name);
            }

            /// <inheritdoc />
            public IField GetField( string name, ITypeSignature type, BindingOptions bindingOptions )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( name, "name" );
                ExceptionHelper.AssertArgumentNotNull( type, "type" );

                #endregion

                FieldRefDeclaration field = this.GetByName( name );

                if ( field != null )
                {
                    if ( field.FieldType.MatchesReference( type ))
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
                            "TypeRefDeclaration.Fields.GetField( {{{0}}}, {{{1}}} ): a field with the same name " +
                            "but a different type exists.", name, type );

                        return null;
                    }
                }
                else
                {
                    if ( ( bindingOptions & BindingOptions.ExistenceMask ) != BindingOptions.OnlyExisting )
                    {
                        field = new FieldRefDeclaration {Name = name, FieldType = type};
                        this.Add( field );
                        return field;
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
            }

            #region IEnumerable<IField> Members

            IEnumerator<IField> IEnumerable<IField>.GetEnumerator()
            {
                return EnumeratorEnlarger.EnlargeEnumerator<FieldRefDeclaration, IField>( this.GetEnumerator() );
            }

            #endregion

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
                IMemberRefResolutionScope scope = (IMemberRefResolutionScope) this.Owner;
                scope.Module.ModuleReader.ImportFieldRefs( scope);
            }
        }
    }
}
