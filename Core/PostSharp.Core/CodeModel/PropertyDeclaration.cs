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
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a property (<see cref="TokenType.Property"/>).
    /// </summary>
    /// <remarks>
    /// Properties are owned by types (<see cref="TypeDefDeclaration"/>).
    /// </remarks>
    public sealed class PropertyDeclaration : MethodGroupDeclaration,
                                              IWriteILDefinition, IRemoveable
    {
        #region Fields

        /// <summary>
        /// Property attributes.
        /// </summary>
        private PropertyAttributes attributes;

        /// <summary>
        /// Default value.
        /// </summary>
        /// <value>
        /// A <see cref="SerializedValue"/>, or <b>null</b> if the property has no
        /// default value.
        /// </value>
        private SerializedValue defaultValue;

        /// <summary>
        /// Calling convention of accessors.
        /// </summary>
        private CallingConvention callingConvention;

        /// <summary>
        /// Type of the property value.
        /// </summary>
        private ITypeSignatureInternal propertyType;

        /// <summary>
        /// Parameters of the property accessors.
        /// </summary>
        private readonly TypeSignatureCollection parameters = new TypeSignatureCollection( 1 );

        private PropertyInfo cachedReflectionProperty;

        #endregion

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.Property;
        }

        #region Properties

        /// <summary>
        /// Gets the parent <see cref="TypeDefDeclaration"/>.
        /// </summary>
        public new TypeDefDeclaration Parent
        {
            get { return (TypeDefDeclaration) base.Parent; }
        }

        /// <summary>
        /// Gets the collection of parameter types.
        /// </summary>
        [Browsable( false )]
        public TypeSignatureCollection Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// Gets or sets the calling convention.
        /// </summary>
        [ReadOnly( true )]
        public CallingConvention CallingConvention
        {
            get { return this.callingConvention; }
            set { this.callingConvention = value; }
        }

        /// <summary>
        /// Gets or sets the type of the property value.
        /// </summary>
        [ReadOnly( true )]
        public ITypeSignature PropertyType
        {
            get { return propertyType; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                propertyType = (ITypeSignatureInternal) value;
            }
        }

        /// <summary>
        /// Gets or sets the property attributes.
        /// </summary>
        [ReadOnly( true )]
        public PropertyAttributes Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// A <see cref="SerializedValue"/>, or <b>null</b> if the property
        /// has no default value.
        /// </value>
        [ReadOnly( true )]
        public SerializedValue DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        /// <summary>
        /// Determines whether the current property can be read (i.e. has a <b>get</b> accessor).
        /// </summary>
        public bool CanRead
        {
            get { return this.Members.Contains( MethodSemantics.Getter ); }
        }

        /// <summary>
        /// Gets the getter accessor, or <b>null</b> if the current property has no getter.
        /// </summary>
        public MethodDefDeclaration Getter
        {
            get { return this.GetAccessor( MethodSemantics.Getter ); }
        }

        /// <summary>
        /// Determines whether the current property can be written (i.e. has a <b>set</b> accessor).
        /// </summary>
        public bool CanWrite
        {
            get { return this.Members.Contains( MethodSemantics.Setter ); }
        }

        /// <summary>
        /// Gets the getter accessor, or <b>null</b> if the current property has no getter.
        /// </summary>
        public MethodDefDeclaration Setter
        {
            get { return this.GetAccessor( MethodSemantics.Setter ); }
        }

        #endregion

        /// <summary>
        /// Gets the system runtime property corresponding to the current property.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments valid in the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments valid in the current context.</param>
        /// <returns>The system runtime <see cref="System.Reflection.PropertyInfo"/>, or <b>null</b> if
        /// the current property could not be bound.</returns>
        public PropertyInfo GetSystemProperty( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            if ( this.cachedReflectionProperty == null )
            {
                Type declaringReflectionType =
                    this.DeclaringType.GetSystemType( genericTypeArguments, genericMethodArguments );
                if ( declaringReflectionType == null )
                {
                    return null;
                }

                if ( genericTypeArguments == null && declaringReflectionType.IsGenericType )
                {
                    genericTypeArguments = declaringReflectionType.GetGenericArguments();
                }

                Type[] paramTypes = new Type[this.Parameters.Count];
                for ( int i = 0; i < paramTypes.Length; i++ )
                {
                    paramTypes[i] = this.Parameters[i].GetSystemType( genericTypeArguments, genericMethodArguments );
                    if ( paramTypes[i] == null )
                    {
                        return null;
                    }
                }

                Type reflectionPropertyType = this.PropertyType.GetSystemType( genericTypeArguments,
                                                                               genericMethodArguments );

                if ( reflectionPropertyType == null )
                {
                    return null;
                }

                this.cachedReflectionProperty =
                    declaringReflectionType.GetProperty( this.Name,
                                                         BindingFlags.Instance | BindingFlags.Public |
                                                         BindingFlags.NonPublic | BindingFlags.Static |
                                                         BindingFlags.DeclaredOnly, null,
                                                         reflectionPropertyType,
                                                         paramTypes, null );
            }

            return this.cachedReflectionProperty;
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemProperty( genericTypeArguments, genericMethodArguments );
        }

        /// <summary>
        /// Gets a reflection <see cref="PropertyInfo"/> that wraps the current property.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>A <see cref="PropertyInfo"/> wrapping current property in the
        /// given generic context.</returns>
        /// <remarks>
        /// This method returns a <see cref="PropertyInfo"/> that is different from the system
        /// runtime property that is retrieved by <see cref="GetSystemProperty"/>. This allows
        /// a have a <b>System.Reflection</b> representation of the current property even
        /// when the declaring type it cannot be loaded in the Virtual Runtime Engine.
        /// </remarks>
        public PropertyInfo GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new PropertyWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        #region writer IL

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            this.WriteILDefinition( writer, this.Parent.GetGenericContext( GenericContextOptions.None ) );
        }

        /// <summary>
        /// Writes the IL definition of the current property.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of the
        /// declaring type.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap )
        {
            writer.WriteKeyword( ".property" );
            writer.MarkAutoIndentLocation();
            if ( ( this.attributes & PropertyAttributes.SpecialName ) != 0 )
            {
                writer.WriteKeyword( "specialname" );
            }
            if ( ( this.attributes & PropertyAttributes.SpecialName ) != 0 )
            {
                writer.WriteKeyword( "rtspecialname" );
            }

            writer.WriteCallConvention( this.callingConvention );

            this.propertyType.WriteILReference( writer, genericMap, WriteTypeReferenceOptions.WriteTypeKind );
            writer.WriteConditionalLineBreak();

            writer.WriteIdentifier( this.Name );
            writer.WriteSymbol( '(' );
            for ( int i = 0; i < this.parameters.Count; i++ )
            {
                if ( i > 0 )
                {
                    writer.WriteSymbol( ',' );
                    writer.WriteLineBreak();
                }

                ( (ITypeSignatureInternal) this.parameters[i] ).WriteILReference( writer, genericMap,
                                                                                  WriteTypeReferenceOptions.
                                                                                      WriteTypeKind );
            }
            writer.WriteSymbol( ')' );
            if ( ( this.attributes & PropertyAttributes.HasDefault ) != 0 )
            {
                writer.WriteSymbol( '=' );
                this.defaultValue.WriteILValue( writer, WriteSerializedValueMode.FieldValue );
            }
            writer.ResetIndentLocation();
            writer.WriteLineBreak();
            writer.BeginBlock();

            this.CustomAttributes.WriteILDefinition( writer );

            this.WriteMethodsILDefinition( writer, genericMap );

            writer.EndBlock();
        }

        #endregion

        #region IRemoveable Members

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "CannotRemoveBecauseNoParent" );

            #endregion

            this.DeclaringType.Properties.Remove( this );
        }

        #endregion

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedReflectionProperty = null;
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of properties (<see cref="PropertyDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class PropertyDeclarationCollection :
            OrderedEmitAndByNonUniqueNameDeclarationCollection<PropertyDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal PropertyDeclarationCollection( Declaration parent, string role ) :
                base( parent, role )
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

            /// <summary>
            /// Gets a property given its name and the signature of its get accessor.
            /// </summary>
            /// <param name="name">Property name.</param>
            /// <param name="getterSignature">Signature of the get accessor.</param>
            /// <returns>The property named <paramref name="name"/> whose get accessor has the signature
            /// <paramref name="getterSignature"/>, or <b>null</b> if there no such a property.</returns>
            public PropertyDeclaration GetProperty( string name, IMethodSignature getterSignature )
            {
                foreach ( PropertyDeclaration property in this.GetByName( name ) )
                {
                    MethodDefDeclaration getter = property.Getter;
                    if ( getter == null )
                        continue;

                    if ( getter.MatchesReference( getterSignature ) )
                        return property;
                }

                return null;
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
                TypeDefDeclaration typeDef = (TypeDefDeclaration) this.Owner;
                typeDef.Module.ModuleReader.ImportProperties( typeDef );
            }
        }
    }
}