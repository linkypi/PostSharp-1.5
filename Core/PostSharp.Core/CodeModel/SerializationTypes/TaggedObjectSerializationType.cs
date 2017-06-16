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

using PostSharp.CodeModel.TypeSignatures;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel.SerializationTypes
{
    /// <summary>
    /// Represents a tagged object serialization type, i.e. the type of a boxed value.
    /// </summary>
    public sealed class TaggedObjectSerializationType : SerializationType
    {
        private TaggedObjectSerializationType( ModuleDeclaration module ) : base( module )
        {
        }


        /// <summary>
        /// Gets an instance of <see cref="TaggedObjectSerializationType"/> for a specific module.
        /// </summary>
        /// <param name="module">Module for which the instance should be valid.</param>
        /// <returns>A <see cref="TaggedObjectSerializationType"/>.</returns>
        /// <remarks>
        /// User code should use <see cref="DeclarationCache"/>.<see cref="DeclarationCache.TaggedObjectSerializationType"/>
        /// instead of this method.
        /// </remarks>
        public static TaggedObjectSerializationType GetInstance( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            return new TaggedObjectSerializationType( module );
        }

        /// <inheritdoc />
        internal override void WriteILValue( object value,
                                             ILWriter writer, WriteSerializedValueMode mode )
        {
            bool includeType = mode != WriteSerializedValueMode.ArrayElementValue;

            SerializedValue serializedValue = (SerializedValue) value;


            if ( includeType )
            {
                writer.WriteKeyword( "object" );
                writer.WriteSymbol( '(' );
            }

            if ( value == null )
            {
                writer.WriteKeyword( "nullref" );
            }
            else
            {
                serializedValue.WriteILValue( writer, WriteSerializedValueMode.AttributeParameterValue );
            }

            if ( includeType )
            {
                writer.WriteSymbol( ')' );
            }
        }

        /// <inheritdoc />
        internal override void WriteILType( ILWriter writer )
        {
            writer.WriteKeyword( "object" );
        }

        /// <inheritdoc />
        internal override void EmitLoadValue( object value, InstructionEmitter writer )
        {
            // TODO: Verify this method. There is probably a bug.

            if ( value == null )
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                SerializedValue serializedValue = (SerializedValue) value;

                serializedValue.EmitLoadValue( writer );

                ITypeSignature type = serializedValue.Type.GetRuntimeType();

                if ( type.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    IntrinsicTypeSignature intrinsicType = type as IntrinsicTypeSignature;
                    if ( intrinsicType != null )
                    {
                        type = writer.MethodBody.Module.Cache.GetIntrinsicBoxedType( intrinsicType.IntrinsicType );
                    }
                    writer.EmitInstructionType( OpCodeNumber.Box, type );
                }
            }
        }

        /// <inheritdoc />
        public override ITypeSignature GetRuntimeType()
        {
            return this.Module.Cache.GetIntrinsic( IntrinsicType.Object );
        }

        /// <inheritdoc />
        internal override object ToRuntimeValue( object value )
        {
            if ( value == null )
            {
                return null;
            }
            else
            {
                SerializedValue serializedValue = (SerializedValue) value;
                return serializedValue.GetRuntimeValue();
            }
        }

        internal override object FromRuntimeValueImpl( object value )
        {
            if ( value == null ) return new SerializedValue( this, null );

            ITypeSignature valueType = this.Module.FindType( value.GetType(), BindingOptions.Default );
            SerializationType serializationType = GetSerializationType(valueType);
            return new SerializedValue( this, serializationType.FromRuntimeValue( value ) );
        }

        /// <inheritdoc />
        public override void ValidateValue( object value )
        {
            if ( value == null ) return;

            if ( !(value is SerializedValue))
                throw ExceptionHelper.Core.CreateArgumentException(
                   "value", "IntrinsicSerializedValueMismatch",
                   value.GetType(), "SerializedValue");

        }

        public override void ValidateRuntimeValue(object value)
        {
            if ( value == null )
                return;

            ITypeSignature valueType = this.Module.Cache.GetType( value.GetType() );
            GetSerializationType( valueType ).ValidateRuntimeValue( value );
        }

        /// <inheritdoc />
        internal override object ReadValue( BufferReader buffer )
        {
            SerializationType type = ReadType( this.Module, buffer );
            object value = type.ReadValue( buffer );
            return new SerializedValue( type, value );
        }

        internal override void WriteValue(object value, System.IO.BinaryWriter writer)
        {
            SerializedValue serializedValue = (SerializedValue) value;
            serializedValue.Type.WriteType( writer);
            serializedValue.Type.WriteValue( serializedValue.Value, writer );
    }

        internal override void WriteType(System.IO.BinaryWriter writer)
        {
            writer.Write((byte)CorSerializationType.TaggedObject);
        }

        /// <inheritdoc />
        public override SerializationType Translate(ModuleDeclaration module)
        {
            return new TaggedObjectSerializationType(module);
        }
    }
}
