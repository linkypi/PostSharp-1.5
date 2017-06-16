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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a serialized value and stores enough information to
    /// serialize it back using the exact representation.
    /// </summary>
    /// <remarks>
    /// <para>An instance of this class should be understood as a 'location' that
    /// can be assigned a value. The location has a type (<see cref="Type"/>
    /// property) and optionally a value (<see cref="Value"/> property).</para>
    /// </remarks>
    /// <seealso cref="SerializationType"/>.
    public sealed class SerializedValue
    {
        private SerializationType type;
        private object value;

        /// <summary>
        /// Initializes a new <see cref="SerializedValue"/>.
        /// </summary>
        /// <param name="type">Value type.</param>
        /// <param name="value">Value.</param>
        public SerializedValue( SerializationType type, object value )
        {
            this.Set( type, value );
        }


        /// <summary>
        /// Gets the type of the serialized value.
        /// </summary>
        /// <remarks>This property is read-only. Use the <see cref="Set"/> method
        /// to change the type and/or method of this instance.</remarks>
        public SerializationType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the value of the serialized value.
        /// </summary>
        /// <remarks>This property is read-only. Use the <see cref="Set"/> method
        /// to change the type and/or method of this instance.</remarks>
        public object Value
        {
            get { return value; }
        }


        /// <summary>
        /// Set a new value and its type.
        /// </summary>
        /// <param name="type">Value type.</param>
        /// <param name="value">Value.</param>
        public void Set( SerializationType type, object value )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );
#if ASSERT
            type.ValidateValue( value );
#endif

            #endregion

            this.type = type;
            this.value = value;
        }


        /// <summary>
        /// Gets the value as required by the runtime.
        /// </summary>
        /// <returns>The value typed for use outside PostSharp.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This method has a non-trivial cost." )]
        public object GetRuntimeValue()
        {
            return this.type.ToRuntimeValue( this.value );
        }


        internal void WriteILValue( ILWriter writer, WriteSerializedValueMode mode )
        {
            this.type.WriteILValue( this.value, writer, mode );
        }

        /// <summary>
        /// Writes the value type to IL.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal void WriteILType( ILWriter writer )
        {
            this.type.WriteILType( writer );
        }

        /// <summary>
        /// Emits instructions that load the current value on the stack.
        /// </summary>
        /// <param name="writer">Writer.</param>
        public void EmitLoadValue( InstructionEmitter writer )
        {
            this.type.EmitLoadValue( this.value, writer );
        }

        internal void WriteValue( BinaryWriter writer )
        {
            this.type.WriteValue( this.value, writer );
        }

        /// <summary>
        /// Gets the runtime, deserialized type of the current <see cref="SerializedValue"/>.
        /// </summary>
        /// <returns>Then runtime type that corresponds to the serialized type of the 
        /// current <see cref="SerializedValue"/>.</returns>
        public ITypeSignature GetRuntimeType()
        {
            return this.type.GetRuntimeType();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.value == null ? "nullref" : this.value.ToString();
        }

        /// <summary>
        /// Translates the current value so that it is valid in another module.
        /// </summary>
        /// <param name="module">The module for which the <see cref="SerializedValue"/> should be expressed.</param>
        /// <returns>A <see cref="SerializedValue"/> equivalent to the current instance, but
        /// expressed for the other <paramref name="module"/>.</returns>
        public SerializedValue Translate( ModuleDeclaration module )
        {
            return new SerializedValue( type.Translate( module ), type.TranslateValue( module, this.value ) );
        }

        /// <summary>
        /// Gets an instance of <see cref="SerializedValue"/>.
        /// </summary>
        /// <param name="typeSignature">Value type.</param>
        /// <param name="value">Value.</param>
        /// <returns>A <see cref="SerializedValue"/> encapsulating <paramref name="typeSignature"/>
        /// and <paramref name="value"/>.</returns>
        public static SerializedValue GetValue( ITypeSignature typeSignature, object value )
        {
            SerializationType serializationType = SerializationType.GetSerializationType( typeSignature );
            return new SerializedValue( serializationType, value );
        }
    }
}