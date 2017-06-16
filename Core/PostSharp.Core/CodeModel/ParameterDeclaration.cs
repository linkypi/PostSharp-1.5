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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a method parameter (<see cref="TokenType.ParamDef"/>). 
    /// </summary>
    /// <remarks>
    /// Parameters are
    /// owned by methods (<see cref="MethodDefDeclaration"/>).
    /// </remarks>
    public sealed class ParameterDeclaration : MetadataDeclaration, IWriteILDefinition, ICloneable, IPositioned

    {
        #region Fields

        /// <summary>
        /// Position of the parameter in the list of method parameters.
        /// </summary>
        private int ordinal;

        /// <summary>
        /// Parameter type.
        /// </summary>
        private ITypeSignatureInternal type;

        /// <summary>
        /// Parameter attributes.
        /// </summary>
        private ParameterAttributes attributes;

        /// <summary>
        /// Default value.
        /// </summary>
        /// <value>
        /// A <see cref="SerializedValue"/>, or <b>null</b> if the parameter
        /// has no default value.
        /// </value>
        private SerializedValue defaultValue;

        /// <summary>
        /// Marshal type.
        /// </summary>
        /// <value>
        /// A <see cref="MarshalType"/>, or <b>null</b> if the parameter
        /// has default marshalling.
        /// </value>
        private MarshalType marshalType;

        private string name;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ParameterDeclaration"/>.
        /// </summary>
        public ParameterDeclaration()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ParameterDeclaration"/> and sets some of its properties.
        /// </summary>
        /// <param name="ordinal">Position of the parameter in the signature.</param>
        /// <param name="name">Parameter name.</param>
        /// <param name="parameterType">Parameter type.</param>
        public ParameterDeclaration( int ordinal, string name, ITypeSignature parameterType )
            : this()
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( parameterType, "parameterType" );

            #endregion

            this.ordinal = ordinal;
            this.Name = name;
            this.ParameterType = parameterType;
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.ParamDef;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if ( ( this.attributes & ParameterAttributes.Retval ) == ParameterAttributes.Retval )
            {
                return "return-value : " + this.type;
            }
            else if ( this.Name == null )
            {
                return this.type.ToString();
            }
            else
            {
                return this.Name + " : " + this.type;
            }
        }

        /// <summary>
        /// Gets the system runtime parameter corresponding to the current parameter.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments valid in the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments valid in the current context.</param>
        /// <returns>The system runtime <see cref="System.Reflection.ParameterInfo"/>, or <b>null</b> if
        /// the current parameter could not be bound.</returns>
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters" )]
        [SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" )]
        public ParameterInfo GetSystemParameter( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            MethodBase method = this.DeclaringMethod.GetSystemMethod( genericTypeArguments, genericMethodArguments,
                                                                      BindingOptions.Default );

            if ( ( this.attributes & ParameterAttributes.Retval ) != 0 )
            {
                MethodInfo methodInfo = method as MethodInfo;
                return methodInfo != null ? methodInfo.ReturnParameter : null;
            }
            else
            {
                return method.GetParameters()[this.ordinal];
            }
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemParameter( genericTypeArguments, genericMethodArguments );
        }

        /// <summary>
        /// Gets a reflection <see cref="ParameterInfo"/> that wraps the current parameter.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>A <see cref="ParameterInfo"/> wrapping current parameter in the
        /// given generic context.</returns>
        /// <remarks>
        /// This method returns a <see cref="ParameterInfo"/> that is different from the system
        /// runtime parameter that is retrieved by <see cref="GetSystemParameter"/>. This allows
        /// a have a <b>System.Reflection</b> representation of the current parameter even
        /// when the declaring type it cannot be loaded in the Virtual Runtime Engine.
        /// </remarks>
        public ParameterInfo GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new ParameterWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        #region Properties

        /// <summary>
        /// Gets or sets the parameter name.
        /// </summary>
        [ReadOnly( true )]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }


        /// <summary>
        /// Gets or sets the parameter type.
        /// </summary>
        [ReadOnly( true )]
        public ITypeSignature ParameterType
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

        /// <summary>
        /// Gets the parent <see cref="MethodDefDeclaration"/>.
        /// </summary>
        /// <remarks>
        /// This is a synonym to <see cref="DeclaringMethod"/>.
        /// </remarks>
        [Browsable( false )]
        public new MethodDefDeclaration Parent
        {
            get { return this.DeclaringMethod; }
        }

        /// <summary>
        /// Gets the parent <see cref="MethodDefDeclaration"/>.
        /// </summary>
        [Browsable( false )]
        public MethodDefDeclaration DeclaringMethod
        {
            get { return (MethodDefDeclaration) base.Parent; }
        }

        /// <summary>
        /// Gets or sets the parameters attributes.
        /// </summary>
        [ReadOnly( true )]
        public ParameterAttributes Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <remarks>
        /// A <see cref="SerializedValue"/>, or <b>null</b> if the parameter
        /// has no default value.
        /// </remarks>
        [ReadOnly( true )]
        public SerializedValue DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        /// <summary>
        /// Gets or sets the parameter marshal type.
        /// </summary>
        /// <value>
        /// A <see cref="MarshalType"/>, or <b>null</b> if the parameter has
        /// default marshalling.
        /// </value>
        [ReadOnly( true )]
        public MarshalType MarshalType
        {
            get { return marshalType; }
            set { marshalType = value; }
        }

        /// <summary>
        /// Gets or sets the parameter ordinal, i.e. its position
        /// in the list of parameters of the parent method.
        /// </summary>
        [ReadOnly( true )]
        public int Ordinal
        {
            get { return this.ordinal; }
            set
            {
                int oldValue = this.ordinal;

                if ( value != oldValue )
                {
                    this.ordinal = value;
                    this.OnPropertyChanged( "Ordinal", oldValue, value );
                }
            }
        }

        int IPositioned.Ordinal
        {
            get { return this.ordinal; }
        }

        #endregion

        #region writer IL

        /// <summary>
        /// Writes the name of the current parameter.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal void WriteILReference( ILWriter writer )
        {
            if ( string.IsNullOrEmpty( this.name ) )
            {
                writer.WriteIdentifier( "A_" +
                                        ( this.ordinal + ( this.Parent.IsStatic ? 0 : 1 ) ).ToString(
                                            CultureInfo.InvariantCulture ) );
            }
            else
            {
                writer.WriteIdentifier( this.Name );
            }
        }

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            this.WriteILDefinition( writer, this.Parent.GetGenericContext( GenericContextOptions.None ), false );
        }

        /// <summary>
        /// Writes the IL definition of the current parameter given the generic context
        /// of the declaring method.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of the
        /// parent method.</param>
        /// <param name="lineBreakAroundMarshal">Whether line breaks should be issued
        /// around marshal type.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap, bool lineBreakAroundMarshal )
        {
            if ( ( this.Attributes & ParameterAttributes.In ) != 0 )
            {
                writer.WriteKeyword( "[in]" );
            }

            if ( ( this.Attributes & ParameterAttributes.Out ) != 0 )
            {
                writer.WriteKeyword( "[out]" );
            }

            if ( ( this.Attributes & ParameterAttributes.Optional ) != 0 )
            {
                writer.WriteKeyword( "[opt]" );
            }

            ( (ITypeSignatureInternal) this.ParameterType ).WriteILReference( writer, genericMap,
                                                                              WriteTypeReferenceOptions.WriteTypeKind );

            if ( this.marshalType != null )
            {
                if ( lineBreakAroundMarshal )
                {
                    writer.WriteLineBreak();
                }


                writer.WriteKeyword( "marshal" );
                writer.WriteSymbol( '(' );
                this.marshalType.WriteILReference( writer );
                writer.WriteSymbol( ')' );


                if ( lineBreakAroundMarshal )
                {
                    writer.WriteLineBreak();
                }
            }

            if ( ( this.Attributes & ParameterAttributes.Retval ) == 0 )
            {
                if ( !string.IsNullOrEmpty( this.Name ) )
                {
                    writer.WriteIdentifier( this.Name );
                }
                else
                {
                    writer.WriteIdentifier( "A_" + ( this.ordinal + ( this.Parent.IsStatic ? 0 : 1 ) ).
                                                       ToString( CultureInfo.InvariantCulture ) );
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns a copy of the current parameter and allows to translate it
        /// to a different module as the module of the current parameter.
        /// </summary>
        /// <returns>A copy of the current parameter, detached from its parent method,
        /// translated to <paramref name="targetModule"/>.</returns>
        public ParameterDeclaration Clone( ModuleDeclaration targetModule )
        {
            ParameterDeclaration clone = new ParameterDeclaration
                                             {
                                                 attributes = this.attributes,
                                                 defaultValue = this.defaultValue,
                                                 marshalType = this.marshalType,
                                                 ordinal = this.ordinal,
                                                 type = ( (ITypeSignatureInternal) this.type.Translate( targetModule ) ),
                                                 name = this.name
                                             };
            return clone;
        }

        /// <summary>
        /// Returns a copy of the current parameter, targeted to the same module
        /// as the current parameter.
        /// </summary>
        /// <returns>A copy of the current parameter, detached from its parent method,
        /// but related to the same module as the current parameter.</returns>
        public ParameterDeclaration Clone()
        {
            return this.Clone( this.Module );
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// Creates a new instance of <see cref="ParameterDeclaration"/> representing a return parameter.
        /// </summary>
        /// <param name="returnType">Return type.</param>
        /// <returns>A new instance of <see cref="ParameterDeclaration"/> whose parameter type is <paramref name="returnType"/>,
        /// representing a return parameter.</returns>
        public static ParameterDeclaration CreateReturnParameter(ITypeSignature returnType)
        {
            return new ParameterDeclaration(-1, "retval", returnType) { Attributes = ParameterAttributes.Retval };

        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of parameters (<see cref="ParameterDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public class ParameterDeclarationCollection :
            OrdinalDeclarationCollection<ParameterDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal ParameterDeclarationCollection( Declaration parent, string role )
                :
                    base( parent, role )
            {
            }

            #region Overrides of ElementCollection<ParameterDeclaration>

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return false; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                throw new NotSupportedException();
            }

            #endregion
        }
    }
}