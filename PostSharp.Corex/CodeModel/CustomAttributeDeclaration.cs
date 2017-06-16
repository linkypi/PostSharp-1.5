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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;
using PostSharp.Reflection;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a custom attribute (<see cref="TokenType.CustomAttribute"/>).
    /// </summary>
    /// <remarks>
    /// <para>Any <see cref="MetadataDeclaration"/> can
    /// have custom attributes. They are exposed on the 
    /// <see cref="MetadataDeclaration.CustomAttributes"/> property.
    /// </para>
    /// <para>
    /// If you change any member of an instance of <see cref="CustomAttributeDeclaration"/>,
    /// you have to call the <see cref="InvalidateSerialization"/> to force the
    /// custom attribute to be serialized again.
    /// </para>
    /// </remarks>
    public sealed class CustomAttributeDeclaration : MetadataDeclaration,
                                                     IWriteILDefinition, IAnnotationInstance, IAnnotationValue,
                                                     IRemoveable
    {
        #region Fields

        /// <summary>
        /// Reference to the custom attribute constructor.
        /// </summary>
        private readonly IMethod constructor;

        /// <summary>
        /// Collection of constructor arguments.
        /// </summary>
        /// <value>
        /// A <see cref="MemberValuePairCollection"/>, or <b>null</b> if the custom
        /// attributes has not been deserialized.
        /// </value>
        private MemberValuePairCollection constructorArguments;

        /// <summary>
        /// Collection of constructor arguments.
        /// </summary>
        /// <value>
        /// A <see cref="MemberValuePairCollection"/>, or <b>null</b> if the custom
        /// attributes has not been deserialized.
        /// </value>
        private MemberValuePairCollection namedArguments;

        /// <summary>
        /// Location of the original custom attribute serialization.
        /// </summary>
        /// <value>
        /// A <see cref="UnmanagedBuffer"/>, or <b>null</b> if the custom attribute
        /// serialization has been invalidated and should be recomputed.
        /// </value>
        private UnmanagedBuffer originalSerialization;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAttributeDeclaration"/> type.
        /// </summary>
        /// <param name="constructor">Custom attribute constructor.</param>
        /// <remarks>
        /// The <paramref name="constructor"/> parameter should be set to a valid
        /// instance constructor. This rule is not enforced programmatically.
        /// </remarks>
        public CustomAttributeDeclaration( IMethod constructor )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( constructor, "constructor" );

            #endregion

            this.constructor = constructor;
            this.constructorArguments = new MemberValuePairCollection();
            this.namedArguments = new MemberValuePairCollection();
        }

        internal CustomAttributeDeclaration( IMethod constructor, UnmanagedBuffer buffer )
        {
            this.constructor = constructor;
            this.originalSerialization = buffer;
        }

        /// <summary>
        /// Initializes a new <see cref="CustomAttributeDeclaration"/> cloned from
        /// a <see cref="IAnnotationValue"/>.
        /// </summary>
        /// <param name="source">Source <see cref="IAnnotationValue"/> from which
        /// the new instance has to be copied.</param>
        public CustomAttributeDeclaration( IAnnotationValue source )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( source, "source" );

            #endregion

            this.constructor = source.Constructor;
            this.constructorArguments = source.ConstructorArguments.Clone();
            this.namedArguments = source.NamedArguments.Clone();
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.CustomAttribute;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Concat( "{CustomAttributeDeclaration ", this.constructor.DeclaringType.ToString(),
                                  "::", this.constructor.ToString(), "}" );
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }

        #region Serialization/Deserialization

        /// <summary>
        /// Gets or sets the location of the original serialization of the
        /// custom attribute.
        /// </summary>
        internal UnmanagedBuffer OriginalSerialization
        {
            get { return this.originalSerialization; }
        }

        /// <summary>
        /// Deserializes the custom attributes if not yet done.
        /// </summary>
        private void EnsureDeserialized()
        {
            if ( this.constructorArguments == null )
            {
                this.constructorArguments = new MemberValuePairCollection();
                this.namedArguments = new MemberValuePairCollection();

                this.Module.DeserializationUtil.DeserializeCustomAttribute( this );
            }
        }

        /// <summary>
        /// Invalidates the original serialization. Indicates that
        /// the serialization should be computed again, if requested.
        /// </summary>
        /// <remarks>
        /// Use the current method if you change a value in the custom attribute.
        /// </remarks>
        public void InvalidateSerialization()
        {
            this.EnsureDeserialized();
            this.originalSerialization = UnmanagedBuffer.Void;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        MetadataDeclaration IAnnotationInstance.TargetElement
        {
            get { return this.Parent; }
        }

        /// <inheritdoc />
        IAnnotationValue IAnnotationInstance.Value
        {
            get { return this; }
        }


        /// <summary>
        /// Gets the parent <see cref="MetadataDeclaration"/>.
        /// </summary>
        [Browsable( false )]
        public new MetadataDeclaration Parent
        {
            get { return (MetadataDeclaration) base.Parent; }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public MemberValuePairCollection ConstructorArguments
        {
            get
            {
                this.EnsureDeserialized();
                return this.constructorArguments;
            }
        }

        /// <inheritdoc />
        [Browsable( false )]
        public MemberValuePairCollection NamedArguments
        {
            get
            {
                this.EnsureDeserialized();
                return this.namedArguments;
            }
        }

        /// <inheritdoc />
        public IMethod Constructor
        {
            get { return this.constructor; }
        }

        #endregion

        /// <summary>
        /// Gets the runtime object 
        /// constructed from the current instance.
        /// </summary>
        /// <returns>An object constructed from <see cref="Constructor"/> with all <see cref="NamedArguments"/>
        /// properly set.</returns>
        /// <exception cref="CustomAttributeConstructorException">The constructor or a property setter
        /// threw an exception.</exception>
        public object ConstructRuntimeObject()
        {
            return CustomAttributeHelper.ConstructRuntimeObject( this, this.Module );
        }

        #region writer IL

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            writer.WriteKeyword( ".custom" );

            ( (IMethodInternal) this.constructor ).WriteILReference( writer, GenericMap.Empty,
                                                                     WriteMethodReferenceOptions.None );

            if ( ( this.originalSerialization.IsVoid || writer.Options.VerboseCustomAttributes ) &&
                 ( writer.Options.Compatibility & ILWriterCompatibility.ForbidVerboseCustomAttribute ) == 0 )
            {
                this.EnsureDeserialized();

                writer.WriteConditionalLineBreak( 80 );
                writer.Indent++;
                writer.WriteSymbol( '=' );


                writer.WriteSymbol( '{' );
                for ( int i = 0; i < this.constructorArguments.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        writer.WriteLineBreak();
                    }
                    this.constructorArguments[i].WriteILDefinition( this.Module, writer );
                }
                for ( int i = 0; i < this.namedArguments.Count; i++ )
                {
                    if ( i > 0 || this.constructorArguments.Count > 0 )
                    {
                        writer.WriteLineBreak();
                    }

                    this.namedArguments[i].WriteILDefinition( this.Module, writer );
                }
                writer.WriteSymbol( '}' );
                writer.Indent--;
            }
            else
            {
                byte[] serialization;

                if ( this.originalSerialization.IsVoid )
                {
                    using ( MemoryStream stream = new MemoryStream() )
                    using ( BinaryWriter binaryWriter = new BinaryWriter( stream ) )
                    {
                        SerializationUtil.SerializeCustomAttribute( this, binaryWriter );
                        binaryWriter.Flush();
                        serialization = stream.ToArray();
                    }
                }
                else
                {
                    serialization = this.originalSerialization.ToByteArray();
                }

                writer.WriteSymbol( '=' );

                writer.WriteBytes( serialization );
            }

            writer.WriteLineBreak();
        }

        #endregion

        #region IRemoveable Members

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "CannotRemoveBecauseNoParent" );

            #endregion

            this.Parent.CustomAttributes.Remove( this );
        }

        #endregion

        #region Implementation of IObjectConstruction

        string IObjectConstruction.TypeName
        {
            get
            {
                StringBuilder s = new StringBuilder();
                this.Constructor.DeclaringType.WriteReflectionTypeName( s, ReflectionNameOptions.UseAssemblyName );
                return s.ToString();
            }
        }

        int IObjectConstruction.ConstructorArgumentCount
        {
            get { return this.constructorArguments.Count; }
        }

        object IObjectConstruction.GetConstructorArgument( int index )
        {
            return this.constructorArguments[index].Value.GetRuntimeValue();
        }

        string[] IObjectConstruction.GetPropertyNames()
        {
            string[] names = new string[this.namedArguments.Count];
            for ( int i = 0; i < names.Length; i++ )
            {
                names[i] = this.namedArguments[i].MemberName;
            }

            return names;
        }

        object IObjectConstruction.GetPropertyValue( string name )
        {
            return this.namedArguments[name].Value.GetRuntimeValue();
        }

        #endregion

        /// <inheritdoc />
        IAnnotationValue IAnnotationValue.Translate( ModuleDeclaration module )
        {
            return this.Translate( module );
        }

        /// <summary>
        /// Translates the custom attribute so that it can be used in another module.
        /// </summary>
        /// <param name="module">A module.</param>
        /// <returns>A <see cref="CustomAttributeDeclaration"/> equivalent to the current instance, but valid
        /// inside the other <paramref name="module"/>.</returns>
        public CustomAttributeDeclaration Translate( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            if ( module == this.Module )
                return this;

            this.EnsureDeserialized();

            CustomAttributeDeclaration translated = new CustomAttributeDeclaration( (IMethod)
                                                                                    this.constructor.Translate( module ) );
            if (this.constructorArguments != null)
            {
                translated.constructorArguments = new MemberValuePairCollection(this.constructorArguments.Count);
                foreach ( MemberValuePair constructorArgument in this.constructorArguments )
                {
                    translated.constructorArguments.Add( constructorArgument.Translate( module ) );
                }
            }

            if (this.namedArguments != null)
            {
                translated.namedArguments = new MemberValuePairCollection(this.namedArguments.Count);
                foreach ( MemberValuePair namedArgument in this.namedArguments )
                {
                    translated.namedArguments.Add( namedArgument.Translate( module ) );
                }
            }

            return translated;
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of custom attributes (<see cref="CustomAttributeDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class CustomAttributeDeclarationCollection :
            OrderedEmitDeclarationCollection<CustomAttributeDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomAttributeDeclarationCollection"/> class.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal CustomAttributeDeclarationCollection( Declaration parent, string role )
                :
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
            /// Writes the definition of all custom attributes in the collection.
            /// </summary>
            /// <param name="writer">An <see cref="ILWriter"/>.</param>
            internal void WriteILDefinition( ILWriter writer )
            {
#if ORDERED_EMIT
                IEnumerable<CustomAttributeDeclaration> enumerable = this.GetByEmitOrder();
#else
                IEnumerable<CustomAttributeDeclaration> enumerable = this.Implementation;
#endif

                if ( enumerable != null )
                {
                    foreach ( CustomAttributeDeclaration attribute in enumerable )
                    {
                        if ( writer.Options.Compatibility != ILWriterCompatibility.MsRoundtrip ||
                             ( (INamedType) attribute.Constructor.DeclaringType ).Name !=
                             "System.Diagnostics.DebuggableAttribute" )
                        {
                            attribute.WriteILDefinition( writer );
                        }
                        else
                        {
                            writer.WriteCommentLine( "Intentionally skipped: System.Diagnostics.DebuggableAttribute" );
                        }
                    }
                }
            }

            /// <summary>
            /// Gets an enumerator of all custom attribute instances of a given type.
            /// </summary>
            /// <param name="type">Type of requested custom attributes.</param>
            /// <returns>An enumerator of all custom attribute instances of a given type.
            /// This method does not return custom attributes <i>derived</i> from
            /// <paramref name="type"/>.</returns>
            public IEnumerator<CustomAttributeDeclaration> GetByTypeEnumerator( IType type )
            {
                foreach ( CustomAttributeDeclaration attribute in this )
                {
                    if ( attribute.Constructor.DeclaringType.MatchesReference( type ) )
                    {
                        yield return attribute;
                    }
                }
            }

            /// <summary>
            /// Gets one or zero custom attribute instance of a given type.
            /// </summary>
            /// <param name="type">Type of the requested custom attribute.</param>
            /// <returns>A <see cref="CustomAttributeDeclaration"/> of type <paramref name="type"/>,
            /// or <b>null</b> if no such custom attribute was found.
            /// This method does not return custom attributes <i>derived</i> from
            /// <paramref name="type"/>.</returns>
            public CustomAttributeDeclaration GetOneByType( IType type )
            {
                foreach ( CustomAttributeDeclaration attribute in this )
                {
                    if ( attribute.Constructor.DeclaringType.MatchesReference( type ) )
                    {
                        return attribute;
                    }
                }

                return null;
            }


            /// <summary>
            /// Determines whether the current collection contains a custom
            /// attribute of a given type.
            /// </summary>
            /// <param name="type">Type of the seeked custom attribute.</param>
            /// <returns><b>true</b> if the current collection contains at least
            /// one custom attribute of type <paramref name="type"/>, otherwise
            /// <b>false</b>.</returns>
            [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
            public bool Contains( IType type )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( type, "type" );

                #endregion

                foreach ( CustomAttributeDeclaration attribute in this )
                {
                    if ( attribute.Constructor.DeclaringType.MatchesReference( type ) )
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Constructs runtime objects for some of all declarations contained in the current collection
            /// and store them in a collection.
            /// </summary>
            /// <param name="type">The type that built custom attributes should be of, or <b>null</b> if
            /// all custom attributes should be built.</param>
            /// <param name="objects">Collection into which custom attribute instances are added.</param>
            /// <exception cref="CustomAttributeConstructorException">The constructor or a property setter
            /// threw an exception.</exception>
            [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
            public void ConstructRuntimeObjects( IType type, IList objects )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( objects, "objects" );

                #endregion

                foreach ( CustomAttributeDeclaration attribute in this )
                {
                    if ( type != null )
                    {
                        if ( !attribute.Constructor.DeclaringType.MatchesReference( type ) )
                        {
                            continue;
                        }
                    }

                    objects.Add( attribute.ConstructRuntimeObject() );
                }
            }

            /// <summary>
            /// Constructs runtime objects for some of all declarations contained in the current collection
            /// and return them as an array.
            /// </summary>
            /// <param name="type">The type that built custom attributes should be of, or <b>null</b> if
            /// all custom attributes should be built.</param>
            /// <exception cref="CustomAttributeConstructorException">The constructor or a property setter
            /// threw an exception.</exception>
            public object[] ConstructRuntimeObjects( IType type )
            {
                List<object> objects = new List<object>( this.Count );
                this.ConstructRuntimeObjects( type, objects );
                return objects.ToArray();
            }

            /// <summary>
            /// Moved all the custom attributes from this collection to another.
            /// </summary>
            /// <param name="target">Collection where the custom attributes
            /// should be moved.</param>
            public void MoveContentTo( CustomAttributeDeclarationCollection target )
            {
                List<CustomAttributeDeclaration> attributes = new List<CustomAttributeDeclaration>( this.Count );
                attributes.AddRange( this );
                this.Clear();
                target.AddRange( attributes );
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                MetadataDeclaration owner = (MetadataDeclaration) this.Owner;
                owner.Module.ModuleReader.ImportCustomAttributes( owner );
            }
        }
    }
}