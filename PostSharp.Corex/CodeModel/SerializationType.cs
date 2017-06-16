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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using PostSharp.CodeModel.SerializationTypes;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents the type of a serialized value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations of this abstract type are present in
    /// the <see cref="PostSharp.CodeModel.SerializationTypes"/> namespace.
    /// </para>
    /// <para>
    /// Note that this type and its implementation only represent <i>intrinsic</i>
    /// serialization types and arrays. Compound types are not covered.
    /// </para>
    /// </remarks>
    public abstract class SerializationType
    {
        private readonly ModuleDeclaration module;

        internal SerializationType( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            this.module = module;
        }

        /// <summary>
        /// Gets the module to which the current <see cref="SerializationType"/> belongs.
        /// </summary>
        public ModuleDeclaration Module
        {
            get { return this.module; }
        }

        /// <summary>
        /// Gets the value as required by the runtime.
        /// </summary>
        /// <returns>The value typed for use outside PostSharp.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This method has a non-trivial cost." )]
        internal abstract object ToRuntimeValue( object value );

        internal abstract object FromRuntimeValueImpl( object value );

        public SerializedValue FromRuntimeValue(object value)
        {
            return new SerializedValue( this, this.FromRuntimeValueImpl( value ) );
        }


        internal abstract void WriteILValue( object value, ILWriter writer, WriteSerializedValueMode mode );

        /// <summary>
        /// Writes the value type to IL.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal abstract void WriteILType( ILWriter writer );

        /// <summary>
        /// Emits instructions that load the current value on the stack.
        /// </summary>
        /// <param name="value">Value to be loaded on the stack.</param>
        /// <param name="writer">Writer.</param>
        internal abstract void EmitLoadValue( object value, InstructionEmitter writer );

        /// <summary>
        /// Gets the runtime, deserialized type correspondint to the current
        /// <see cref="SerializationType"/>.
        /// </summary>
        /// <returns>Then runtime type that corresponds to the serialized type of the 
        /// current <see cref="SerializationType"/>.</returns>
        public abstract ITypeSignature GetRuntimeType();

        /// <summary>
        /// Ensures that a value is compatible with the current type.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <remarks>
        /// <para>Implementations should raise an <see cref="ArgumentException"/> if the value
        /// is invalid. 
        /// </para>
        /// <para>
        /// When PostSharp.Core.dll is compiled without the ASSERT constant, implementations
        /// do not throw exceptions and this method is always successful.
        /// </para>
        /// </remarks>
        public abstract void ValidateValue( object value );

        public abstract void ValidateRuntimeValue( object value );

        /// <summary>
        /// Reads a value of the current type from a binary buffer.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <returns>A value compatible with the current <see cref="SerializationType"/>.</returns>
        internal abstract object ReadValue( BufferReader buffer );

        internal abstract void WriteType( BinaryWriter writer );

        internal abstract void WriteValue( object value, BinaryWriter writer );


        /// <summary>
        /// Gets the <see cref="SerializationType"/> corresponding to a given <see cref="ITypeSignature"/>.
        /// </summary>
        /// <param name="typeSignature">A <see cref="ITypeSignature"/>.</param>
        /// <returns>The <see cref="SerializationType"/> corresponding to <paramref name="typeSignature"/>.</returns>
        /// <exception cref="ArgumentException">The given type cannot be serialized in intrinsic types.</exception>
        public static SerializationType GetSerializationType( ITypeSignature typeSignature )
        {
            #region Process intrinsic types

            IntrinsicTypeSignature intrinsic = typeSignature as IntrinsicTypeSignature;
            if ( intrinsic != null )
            {
                switch ( intrinsic.IntrinsicType )
                {
                    case IntrinsicType.Object:
                        return typeSignature.Module.Cache.TaggedObjectSerializationType;

                    default:
                        return typeSignature.Module.Cache.GetIntrinsicSerializationType( intrinsic.IntrinsicType );
                }
            }

            #endregion

            #region Process arrays

            ArrayTypeSignature array = typeSignature as ArrayTypeSignature;

            if ( array != null )
            {
                SerializationType elementType = GetSerializationType( array.ElementType );
                return new ArraySerializationType( elementType );
            }

            #endregion

            #region Process other types

            TypeSpecDeclaration typeSpec = typeSignature as TypeSpecDeclaration;

            if ( typeSpec != null )
            {
                return GetSerializationType( typeSpec.Signature );
            }

            INamedType type = typeSignature as INamedType;
            if ( type != null )
            {
                ITypeSignature typeType = typeSignature.Module.Cache.GetType( "System.Type, mscorlib" );
                if ( typeSignature.IsAssignableTo(typeType ) )
                {
                    return typeSignature.Module.Cache.TypeSerializationType;
                }

                if ( typeSignature.BelongsToClassification( TypeClassifications.Enum ) )
                {
                    return new EnumerationSerializationType( type );
                }
            }

            #endregion

            throw ExceptionHelper.Core.CreateArgumentException( "typeSignature", "CannotSerializeTypeSignature",
                                                                typeSignature );
        }

        /// <summary>
        /// Decode a serialization type signature from a binary buffer.
        /// </summary>
        /// <param name="module">Module in which type references have to be resolved.</param>
        /// <param name="buffer">Buffer.</param>
        /// <returns>The decoded <see cref="SerializationType"/>.</returns>
        internal static SerializationType ReadType( ModuleDeclaration module, BufferReader buffer )
        {
            CorSerializationType serializationType = (CorSerializationType) buffer.ReadByte();

            switch ( serializationType )
            {
                case CorSerializationType.Type:
                    return module.Cache.TypeSerializationType;

                case CorSerializationType.TaggedObject:
                    return module.Cache.TaggedObjectSerializationType;

                case CorSerializationType.Enum:
                    {
                        string enumTypeName = buffer.ReadSerString();

                        // First we look for the type from the current module.
                        bool hasAssemblyQualifier = enumTypeName.Contains( "[" );
                        ITypeSignature type =
                            module.FindType( enumTypeName,
                                             BindingOptions.WeakReference |
                                             ( hasAssemblyQualifier
                                                   ? BindingOptions.DisallowIntrinsicSubstitution
                                                   : BindingOptions.DontThrowException ) );

                        // If we did not find it AND it has no assembly qualifier.
                        if ( type == null && !hasAssemblyQualifier )
                        {
                            type = module.FindMscorlib().GetAssemblyEnvelope().ManifestModule.FindType( enumTypeName,
                                                                                                        BindingOptions.
                                                                                                            WeakReference |
                                                                                                        BindingOptions.
                                                                                                            DontThrowException );
                        }

                        if ( type == null )
                        {
                            throw ExceptionHelper.Core.CreateBindingException( "CannotFindTypeInCurrentModule",
                                                                               enumTypeName, module.Name );
                        }

                        return
                            new EnumerationSerializationType(
                                (INamedType) type );
                    }

                case CorSerializationType.Array:
                    {
                        SerializationType elementType = ReadType( module, buffer );
                        return new ArraySerializationType( elementType );
                    }

                default:
                    return module.Cache.GetIntrinsicSerializationType( (CorElementType) serializationType );
            }
        }

        /// <summary>
        /// Translates the current <see cref="SerializationType"/> so that it is valid
        /// in another module.
        /// </summary>
        /// <param name="module">The module for which the <see cref="SerializationType"/> should be expressed.</param>
        /// <returns>An instance of <see cref="SerializationType"/> expressed for the other <paramref name="module"/>.</returns>
        public abstract SerializationType Translate( ModuleDeclaration module );

        /// <summary>
        /// Translates a value of the current <see cref="SerializationType"/> so that it is valid
        /// in another module.
        /// </summary>
        /// <param name="module">The module for which the value should be expressed.</param>
        /// <param name="value">The value to be translated.</param>
        /// <returns>An equivalent to <paramref name="value"/> expressed for the other <paramref name="module"/>.</returns>
        internal virtual object TranslateValue( ModuleDeclaration module, object value )
        {
            return value;
        }
    }

    [Flags]
    internal enum WriteSerializedValueMode
    {
        FieldValue,
        AttributeParameterValue,
        ArrayElementValue
    }
}