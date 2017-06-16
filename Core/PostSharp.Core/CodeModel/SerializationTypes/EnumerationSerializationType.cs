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
using System.Text;
using PostSharp.CodeModel.Helpers;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel.SerializationTypes
{
    /// <summary>
    /// Represents the serialization type of an enumeration.
    /// </summary>
    public sealed class EnumerationSerializationType : SerializationType
    {
        private INamedType enumerationType;

        /// <summary>
        /// Initializes a new <see cref="EnumerationSerializationType"/>.
        /// </summary>
        /// <param name="enumerationType">Runtime type of the enumeration.</param>
        public EnumerationSerializationType( INamedType enumerationType ) : base( enumerationType.Module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( enumerationType, "enumerationType" );

            #endregion

            this.enumerationType = enumerationType;
        }

        /// <summary>
        /// Gets or sets the runtime type of the enumeration.
        /// </summary>
        public INamedType EnumerationType
        {
            get { return enumerationType; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                enumerationType = value;
            }
        }


        /// <inheritdoc />
        internal override object ToRuntimeValue( object value )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( value, "value" );

            #endregion

            Type reflectionEnumType = this.enumerationType.GetSystemType( null, null );
            return Enum.ToObject( reflectionEnumType, value );
        }

        internal override object FromRuntimeValueImpl( object value )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(value, "value");

            #endregion


            return Convert.ChangeType( value, this.Module.Cache.GetIntrinsic(this.GetIntrinsicType()).GetSystemType( null, null ) );
        }

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        public IntrinsicType GetIntrinsicType()
        {
            return EnumHelper.GetUnderlyingType( this.enumerationType ).IntrinsicType;
        }

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        public IntrinsicSerializationType GetIntrinsicSerializationType()
        {
            IntrinsicType intrinsicType = this.GetIntrinsicType();
            return this.Module.Cache.GetIntrinsicSerializationType( intrinsicType );
        }

        /// <inheritdoc />
        internal override void WriteILValue( object value, ILWriter writer, WriteSerializedValueMode mode )
        {
            this.GetIntrinsicSerializationType().WriteILValue( value, writer, mode );
        }

        /// <inheritdoc />
        internal override void EmitLoadValue( object value, InstructionEmitter writer )
        {
            this.GetIntrinsicSerializationType().EmitLoadValue( value, writer );
        }

        /// <inheritdoc />
        public override ITypeSignature GetRuntimeType()
        {
            return this.enumerationType;
        }

        /// <inheritdoc />
        internal override void WriteILType( ILWriter writer )
        {
            writer.WriteKeyword( "enum" );

            ( (ITypeSignatureInternal) this.enumerationType ).WriteILReference( writer, GenericMap.Empty,
                                                                                WriteTypeReferenceOptions.None );
        }


        /// <inheritdoc />
        public override void ValidateValue( object value )
        {
            if ( value == null ) throw new ArgumentNullException("value");
            
            Type expectedType =
                this.enumerationType.Module.Cache.GetIntrinsic(this.GetIntrinsicType()).GetSystemType(null, null);

            Type valueType = value.GetType();
            if (valueType.IsEnum)
                valueType = Enum.GetUnderlyingType(valueType);

            if (!expectedType.IsAssignableFrom(valueType))
            {
                throw ExceptionHelper.Core.CreateArgumentException(
                    "value", "IntrinsicSerializedValueMismatch",
                    value.GetType(), expectedType);
            }
        }

        public override void ValidateRuntimeValue(object value)
        {
            this.ValidateValue( value );
        } 


        /// <inheritdoc />
        internal override object ReadValue( BufferReader buffer )
        {
            return this.GetIntrinsicSerializationType().ReadValue( buffer );
        }

        internal override void WriteValue(object value, System.IO.BinaryWriter writer)
        {
            this.GetIntrinsicSerializationType().WriteValue( value, writer );
    }

        internal override void WriteType(System.IO.BinaryWriter writer)
        {
            writer.Write( (byte) CorSerializationType.Enum );
            StringBuilder stringBuilder = new StringBuilder( );
            this.enumerationType.WriteReflectionTypeName( stringBuilder, ReflectionNameOptions.UseAssemblyName);
            SerializationUtil.WriteSerString( stringBuilder.ToString(), writer);
        }

        /// <inheritdoc />
        public override SerializationType Translate(ModuleDeclaration module)
        {
            return new EnumerationSerializationType( (INamedType)this.enumerationType.Translate(module));
        }
    }
}
