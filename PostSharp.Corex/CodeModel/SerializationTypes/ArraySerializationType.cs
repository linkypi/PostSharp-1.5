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
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel.SerializationTypes
{
    /// <summary>
    /// Represents the serialization type of an array.
    /// </summary>
    public sealed class ArraySerializationType : SerializationType
    {
        private SerializationType elementType;

        /// <summary>
        /// Initializes a new <see cref="ArraySerializationType"/>.
        /// </summary>
        /// <param name="elementType">Type of array elements.</param>
        public ArraySerializationType( SerializationType elementType ) : base( elementType.Module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( elementType, "elementType" );

            #endregion

            this.elementType = elementType;
        }

        /// <summary>
        /// Gets or sets the type of array elements.
        /// </summary>
        public SerializationType ElementType
        {
            get { return this.elementType; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                this.elementType = value;
            }
        }

        /// <inheritdoc />
        internal override object ToRuntimeValue( object value )
        {
            if (value == null) return null;

            IList sourceArray = (IList) value;
            Type elementReflectionType = elementType.GetRuntimeType().GetSystemType( null, null );
            Array array = Array.CreateInstance( elementReflectionType, sourceArray.Count );
            for ( int i = 0 ; i < sourceArray.Count ; i++ )
            {
                array.SetValue( this.elementType.ToRuntimeValue( sourceArray[i] ), i );
            }
            return array;
        }

        internal override object FromRuntimeValueImpl( object value )
        {
            if ( value == null ) return null;

            IList sourceArray = (IList) value;
            object[] targetArray = new object[sourceArray.Count];
            for ( int i = 0; i < targetArray.Length; i++)
            {
                targetArray[i] = this.elementType.FromRuntimeValueImpl( sourceArray[i] );
            }

            return targetArray;
        }

        /// <inheritdoc />
        internal override void WriteILValue( object value, ILWriter writer, WriteSerializedValueMode mode )
        {
            IList array = (IList) value;
            this.elementType.WriteILType( writer );
            writer.WriteSymbol( '[' );
            writer.WriteInteger( array.Count, IntegerFormat.Decimal );
            writer.WriteSymbol( ']' );
            writer.WriteSymbol( '(' );
            for ( int i = 0 ; i < array.Count ; i++ )
            {
                this.elementType.WriteILValue( array[i], writer, WriteSerializedValueMode.ArrayElementValue );
            }
            writer.WriteSymbol( ')' );
        }

        /// <inheritdoc />
        internal override void WriteILType( ILWriter writer )
        {
            this.elementType.WriteILType( writer );
            writer.WriteSymbol( "[]" );
        }

        /// <inheritdoc />
        internal override void EmitLoadValue( object value, InstructionEmitter writer )
        {
            if ( value == null )
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                IList list = (IList) value;
                writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, list.Count );
                writer.EmitInstructionType( OpCodeNumber.Newarr, this.elementType.GetRuntimeType() );

                for ( int i = 0; i < list.Count; i++ )
                {
                    writer.EmitInstruction(OpCodeNumber.Dup );
                    writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, i );
                    this.elementType.EmitLoadValue( list[i], writer );
                    writer.EmitInstructionStoreIndirect( this.elementType.GetRuntimeType() );
                }
            }
        }

        /// <inheritdoc />
        public override ITypeSignature GetRuntimeType()
        {
            ITypeSignature elementTypeSignature = this.elementType.GetRuntimeType();
            return new ArrayTypeSignature( elementTypeSignature );
        }

        /// <inheritdoc />
        public override void ValidateValue( object value )
        {
            if (value == null) return;
            
            IList list = value as IList;
            if (list == null) throw ExceptionHelper.Core.CreateArgumentException( "value",
                "IntrinsicSerializedValueMismatch",
                "IList", value.GetType() );

            foreach ( object item in list )
            {
                this.elementType.ValidateValue( item );
            }
        }

        public override void ValidateRuntimeValue(object value)
        {
            if (value == null) return;

            IList list = value as IList;
            if (list == null) throw ExceptionHelper.Core.CreateArgumentException("value",
                "IntrinsicSerializedValueMismatch",
                "IList", value.GetType());

            foreach (object item in list)
            {
                this.elementType.ValidateRuntimeValue(item);
            }
        }

        /// <inheritdoc />
        internal override object ReadValue( BufferReader buffer )
        {
            int n = buffer.ReadInt32();
            if ( n == -1 )
            {
                return null;
            }
            object[] array = new object[n];
            for ( int i = 0 ; i < n ; i++ )
            {
                array[i] = this.elementType.ReadValue( buffer );
            }

            return array;
        }

        internal override void WriteValue(object value, System.IO.BinaryWriter writer)
        {
            if (value == null)
            {
                writer.Write(0);
                return;
            }

            IList array = (IList)value;

            writer.Write( array.Count );

            for ( int i = 0; i < array.Count; i++)
            {
                this.elementType.WriteValue( array[i], writer);
            }
        }

        internal override void WriteType(System.IO.BinaryWriter writer)
        {
            writer.Write( (byte) CorSerializationType.Array);
            this.elementType.WriteType( writer );
        }

        /// <inheritdoc />
        public override SerializationType Translate(ModuleDeclaration module)
        {
            return new ArraySerializationType(this.elementType.Translate(module));
        }
    }
}
