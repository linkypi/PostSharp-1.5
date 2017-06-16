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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.SerializationTypes
{
    /// <summary>
    /// Represents instrinsic serialization types (boolean, int32, int64, ...).
    /// </summary>
    public sealed class IntrinsicSerializationType : SerializationType
    {

        private readonly IntrinsicType type;


        /// <inheritdoc />
        private IntrinsicSerializationType( IntrinsicType type, ModuleDeclaration module ) : base( module )
        {
            this.type = type;
        }

        /// <summary>
        /// Gets an instance <see cref="IntrinsicSerializationType"/> for the current module.
        /// </summary>
        /// <param name="module">Module in which the instance should be valid.</param>
        /// <param name="type">Intrinsic type that should be represented by the instance.</param>
        /// <returns>An <see cref="IntrinsicSerializationType"/> represening <paramref name="type"/>,
        /// valid in <paramref name="module"/>.</returns>
        /// <remarks>
        /// User code should use the <see cref="DeclarationCache"/>.
        /// <see cref="DeclarationCache.GetIntrinsicSerializationType(IntrinsicType)"/>
        /// method instead.
        /// </remarks>
        public static IntrinsicSerializationType GetInstance( ModuleDeclaration module, IntrinsicType type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            return new IntrinsicSerializationType( type, module );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing an intrinsic type.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="type">Type of the intrinsic value.</param>
        /// <param name="value">A boxed value of type <paramref name="type"/>.</param>
        /// <returns>A <see cref="SerializedValue"/> of type <paramref name="type"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, IntrinsicType type, object value )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            return new SerializedValue( module.Cache.GetIntrinsicSerializationType( type ), value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, bool value )
        {
            return CreateValue( module, IntrinsicType.Boolean, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, byte value )
        {
            return CreateValue( module, IntrinsicType.Byte, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, char value )
        {
            return CreateValue( module, IntrinsicType.Char, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Double"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, double value )
        {
            return CreateValue( module, IntrinsicType.Double, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Int16"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, Int16 value )
        {
            return CreateValue( module, IntrinsicType.Int16, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, Int32 value )
        {
            return CreateValue( module, IntrinsicType.Int32, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, Int64 value )
        {
            return CreateValue( module, IntrinsicType.Int64, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.SByte"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, SByte value )
        {
            return CreateValue( module, IntrinsicType.SByte, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.Single"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, Single value )
        {
            return CreateValue( module, IntrinsicType.Single, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.String"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, String value )
        {
            return CreateValue( module, IntrinsicType.String, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.UInt16"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, UInt16 value )
        {
            return CreateValue( module, IntrinsicType.UInt16, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.UInt32"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, UInt32 value )
        {
            return CreateValue( module, IntrinsicType.UInt32, value );
        }

        /// <summary>
        /// Creates a <see cref="SerializedValue"/> representing a <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="module">Module in which the <see cref="SerializedValue"/> should be valid.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="value"/> in <paramref name="module"/>.</returns>
        public static SerializedValue CreateValue( ModuleDeclaration module, UInt64 value )
        {
            return CreateValue( module, IntrinsicType.UInt64, value );
        }


        /// <summary>
        /// Gets the <see cref="IntrinsicType"/> enumeration value.
        /// </summary>
        public IntrinsicType Type { get { return type; } }


        /// <inheritdoc />
        internal override object ToRuntimeValue( object value )
        {
            return value;
        }

        internal override object FromRuntimeValueImpl( object value )
        {
            return value;
        }

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Performance", "CA1800:DoNotCastUnnecessarily" )]
        internal override void WriteILValue( object value, ILWriter writer, WriteSerializedValueMode mode )
        {
            // Parse options.
            IntegerFormat integerFormat;
            bool includeType;

            switch ( mode )
            {
                case WriteSerializedValueMode.ArrayElementValue:
                    integerFormat = IntegerFormat.Decimal;
                    includeType = false;
                    break;

                case WriteSerializedValueMode.AttributeParameterValue:
                    integerFormat = IntegerFormat.Decimal;
                    includeType = true;
                    break;

                case WriteSerializedValueMode.FieldValue:
                    integerFormat = IntegerFormat.HexUpper;
                    includeType = true;
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( mode, "mode" );
            }


            switch ( this.type )
            {
                case IntrinsicType.Boolean:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "bool" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteKeyword( (bool) value ? "true" : "false" );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.SByte:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "int8" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (sbyte) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.Byte:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "uint8" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (byte) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.Int16:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "int16" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (short) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.UInt16:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "uint16" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (ushort) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.Int32:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "int32" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (int) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.UInt32:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "uint32" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (int) (uint) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.Int64:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "int64" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (long) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.UInt64:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "uint64" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (long) (ulong) value, integerFormat );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.Single:
                    {
                        float floatValue = (float) value;

                        if ( includeType )
                        {
                            writer.WriteKeyword( "float32" );
                            writer.WriteSymbol( '(' );
                        }

                        if ( float.IsNegativeInfinity( floatValue ) )
                        {
                            writer.WriteRaw( "0xFF800000" );
                        }
                        else if ( float.IsPositiveInfinity( floatValue ) )
                        {
                            writer.WriteRaw( "0x7F800000" );
                        }
                        else if ( float.IsNaN( floatValue ) )
                        {
                            writer.WriteRaw( "0xFFC00000" );
                        }
                        else
                        {
                            writer.WriteSingle( floatValue );
                        }

                        if ( includeType )
                        {
                            writer.WriteSymbol( ')' );
                        }
                    }
                    break;

                case IntrinsicType.Double:
                    double doubleValue = (double) value;

                    if ( includeType )
                    {
                        writer.WriteKeyword( "float64" );
                        writer.WriteSymbol( '(' );
                    }

                    if ( double.IsNegativeInfinity( doubleValue ) )
                    {
                        writer.WriteRaw( "0xFFF0000000000000" );
                    }
                    else if ( double.IsPositiveInfinity( doubleValue ) )
                    {
                        writer.WriteRaw( "0x7FF0000000000000" );
                    }
                    else if ( double.IsNaN( doubleValue ) )
                    {
                        writer.WriteRaw( "0xFFF8000000000000" );
                    }
                    else if ( doubleValue == 0 )
                    {
                        writer.WriteRaw( "0." );
                    }
                    else
                    {
                        writer.WriteDouble( doubleValue );
                    }

                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.Char:
                    if ( includeType )
                    {
                        writer.WriteKeyword( "char" );
                        writer.WriteSymbol( '(' );
                    }
                    writer.WriteInteger( (short) (char) value, IntegerFormat.HexUpper );
                    if ( includeType )
                    {
                        writer.WriteSymbol( ')' );
                    }
                    break;

                case IntrinsicType.String:
                    if ( mode != WriteSerializedValueMode.FieldValue )
                    {
                        if ( includeType )
                        {
                            writer.WriteKeyword( "string" );
                            writer.WriteSymbol( '(' );
                        }
                        if ( value == null )
                        {
                            writer.WriteKeyword( "nullref" );
                        }
                        else
                        {
                            writer.WriteQuotedString( (string) value, WriteStringOptions.None );
                        }
                        if ( includeType )
                        {
                            writer.WriteSymbol( ')' );
                        }
                    }
                    else
                    {
                        if ( value == null )
                        {
                            writer.WriteKeyword( "nullref" );
                        }
                        else
                        {
                            writer.WriteQuotedString((string)value, WriteStringOptions.DoubleQuoted );
                        }
                    }
                    break;
            }
        }

        /// <inheritdoc />
        internal override void EmitLoadValue( object value, InstructionEmitter writer )
        {
            switch ( this.type )
            {
                case IntrinsicType.Boolean:
                case IntrinsicType.Byte:
                case IntrinsicType.Char:
                case IntrinsicType.Int16:
                case IntrinsicType.Int32:
                case IntrinsicType.SByte:
                case IntrinsicType.UInt16:
                case IntrinsicType.UInt32:
                    writer.EmitInstructionInt32( OpCodeNumber.Ldc_I4, Convert.ToInt32( value,
                                                                                       CultureInfo.InvariantCulture ) );
                    break;

                case IntrinsicType.String:
                    writer.EmitInstructionString( OpCodeNumber.Ldstr, (string) value );
                    break;


                case IntrinsicType.Single:
                    writer.EmitInstructionDouble( OpCodeNumber.Ldc_R4, (float) value );
                    break;

                case IntrinsicType.Double:
                    writer.EmitInstructionDouble( OpCodeNumber.Ldc_R8, (double) value );
                    break;

                case IntrinsicType.Int64:
                    writer.EmitInstructionInt64( OpCodeNumber.Ldc_I8, (long) value );
                    break;

                case IntrinsicType.UInt64:
                    writer.EmitInstructionInt64( OpCodeNumber.Ldc_I8, (long) (ulong) value );
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.type, "this.Type" );
            }
        }

        /// <inheritdoc />
        public override ITypeSignature GetRuntimeType()
        {
            return this.Module.Cache.GetIntrinsic( this.type );
        }

        /// <inheritdoc />
        internal override void WriteILType( ILWriter writer )
        {
            switch ( this.type )
            {
                case IntrinsicType.Boolean:
                    writer.WriteKeyword( "bool" );
                    break;

                case IntrinsicType.SByte:
                    writer.WriteKeyword( "int8" );
                    break;

                case IntrinsicType.Byte:
                    writer.WriteKeyword( "uint8" );
                    break;

                case IntrinsicType.Int16:
                    writer.WriteKeyword( "int16" );
                    break;

                case IntrinsicType.UInt16:
                    writer.WriteKeyword( "uint16" );
                    break;

                case IntrinsicType.Int32:
                    writer.WriteKeyword( "int32" );
                    break;

                case IntrinsicType.UInt32:
                    writer.WriteKeyword( "uint32" );
                    break;

                case IntrinsicType.Int64:
                    writer.WriteKeyword( "int64" );
                    break;

                case IntrinsicType.UInt64:
                    writer.WriteKeyword( "uint64" );
                    break;

                case IntrinsicType.Single:
                    writer.WriteKeyword( "float32" );
                    break;

                case IntrinsicType.Double:
                    writer.WriteKeyword( "float64" );
                    break;

                case IntrinsicType.Char:
                    writer.WriteKeyword( "char" );
                    break;

                case IntrinsicType.String:
                    writer.WriteKeyword( "string" );
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.type, "this.Type" );
            }
        }

        /// <inheritdoc />
        public override void ValidateValue( object value )
        {
            if (value == null)
            {
                if ( this.type != IntrinsicType.String )
                {
                    throw new ArgumentNullException( "value" );
                }
            }
            else
            {
                Type reflectionType = this.Module.Cache.GetIntrinsic(this.type).GetSystemType(null, null);

                if ( !reflectionType.IsAssignableFrom(value.GetType()) )
                {
                    throw ExceptionHelper.Core.CreateArgumentException(    "value", "IntrinsicSerializedValueMismatch",
                                                                            value.GetType(), reflectionType );
                }
            }
        }

        public override void ValidateRuntimeValue(object value)
        {
            this.ValidateValue( value );
        }

        /// <inheritdoc />
        internal override object ReadValue( BufferReader buffer )
        {
            switch ( this.type )
            {
                case IntrinsicType.Boolean:
                    return buffer.ReadByte() != 0;

                case IntrinsicType.String:
                    return buffer.ReadSerString();

                default:
                    return buffer.Read( this.type );
            }
        }

        internal override void WriteType(System.IO.BinaryWriter writer)
        {
            writer.Write( (byte) IntrinsicTypeSignature.MapIntrinsicTypeToCorElementType( this.type ));
    }

        internal override void WriteValue(object value, System.IO.BinaryWriter writer)
        {
            switch ( this.type )
            {
                case IntrinsicType.Boolean:
                    writer.Write((bool)value ? (byte)1 : (byte)0);
                    break;

                case IntrinsicType.Byte:
                    writer.Write( (byte) value);
                    break;

                case IntrinsicType.Double:
                    writer.Write((double)value);
                    break;

                case IntrinsicType.Char:
                    writer.Write((char)value);
                    break;

                case IntrinsicType.Int16:
                    writer.Write((short)value);
                    break;

                case IntrinsicType.Int32:
                    writer.Write((int)value);
                    break;

                case IntrinsicType.Int64:
                    writer.Write((long)value);
                    break;

                case IntrinsicType.SByte:
                    writer.Write((sbyte)value);
                    break;

                case IntrinsicType.Single:
                    writer.Write((float)value);
                    break;

                case IntrinsicType.UInt16:
                    writer.Write((ushort)value);
                    break;

                case IntrinsicType.UInt32:
                    writer.Write((uint)value);
                    break;

                case IntrinsicType.UInt64:
                    writer.Write((ulong)value);
                    break;

                case IntrinsicType.String:
                    SerializationUtil.WriteSerString( (string) value, writer);
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.type, "this.type" );
                
            }
        }

        /// <inheritdoc />
        public override SerializationType Translate(ModuleDeclaration module)
        {
            return GetInstance(module, this.type);
        }
    }
}
