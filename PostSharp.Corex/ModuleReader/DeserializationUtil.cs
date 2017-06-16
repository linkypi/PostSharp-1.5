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

using System.Collections.Generic;
using System.Globalization;
using PostSharp.CodeModel;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Deserializes custom attributes and permission sets.
    /// </summary>
    internal class DeserializationUtil
    {
        #region Fields

        /// <summary>
        /// Module to which this instance is attached.
        /// </summary>
        private readonly ModuleDeclaration module;

        /// <summary>
        /// Cache of types by name.
        /// </summary>
        private readonly Dictionary<string, ITypeSignature> mapTypesByName = new Dictionary<string, ITypeSignature>();

        #endregion

        /// <summary>
        /// Initializes a new <see cref="DeserializationUtil"/>.
        /// </summary>
        /// <param name="module">The module to which this instance is assigned.</param>
        public DeserializationUtil( ModuleDeclaration module )
        {
            this.module = module;
        }


        /// <summary>
        /// Deserializes a custom attribute.
        /// </summary>
        /// <param name="customAttribute">The <see cref="CustomAttributeDeclaration"/>
        /// to be deserialized.</param>
        /// <returns><b>true</b> if the buffer was completely read, otherwise <b>false</b>.</returns>
        public bool DeserializeCustomAttribute( CustomAttributeDeclaration customAttribute )
        {
            IMethod constructor = customAttribute.Constructor;
            BufferReader buffer = new BufferReader( customAttribute.OriginalSerialization );

            // Read the prolog
            short prolog = buffer.ReadInt16();

            if ( prolog != 1 )
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidToken", prolog,
                                                                           "custom attribute prolog" );
            }

            // Read the constructor arguments
            int constructorArgumentsCount = constructor.ParameterCount;
            customAttribute.ConstructorArguments.Capacity = constructorArgumentsCount;

            for ( int j = 0 ; j < constructorArgumentsCount ; j++ )
            {
                ITypeSignature parameterTypeSignature = constructor.GetParameterType( j );
                SerializationType parameterSerializationType =
                    SerializationType.GetSerializationType( parameterTypeSignature );

                object value = parameterSerializationType.ReadValue( buffer );

                MemberValuePair memberValue = new MemberValuePair(
                    MemberKind.Parameter, j, j.ToString( CultureInfo.InvariantCulture ),
                    new SerializedValue( parameterSerializationType, value ) );

                customAttribute.ConstructorArguments.Add( memberValue );
            }

            // Read the optional arguments
            short numNamed;

            numNamed = buffer.ReadInt16();

            customAttribute.NamedArguments.Capacity = numNamed;
            for ( int j = 0 ; j < numNamed ; j++ )
            {
                MemberValuePair memberValue = this.ReadSerializedNamedArgument( j, buffer );
                customAttribute.NamedArguments.Add( memberValue );
            } // loop named arguments

            return buffer.Offset == buffer.Size;
        }

        /// <summary>
        /// Given a full type name, gets the corresponding <see cref="IType"/>.
        /// </summary>
        /// <param name="className">The full type name.</param>
        /// <returns>The <see cref="IType"/> corresponding to <paramref name="className"/>.</returns>
        /// <remarks>
        /// This method accepts stand-alone, i.e. non-linked declarations.
        /// </remarks>
        public ITypeSignature GetTypeByName( string className )
        {
            ITypeSignature typeReference;

            if ( !mapTypesByName.TryGetValue( className, out typeReference ) )
            {
                typeReference = this.module.FindType( className, BindingOptions.WeakReference );
                mapTypesByName.Add( className, typeReference );
            }

            return typeReference;
        }

        /// <summary>
        /// Reads a named argument (indistinctively of a custom attribute or
        /// permission attribute).
        /// </summary>
        /// <param name="buffer">A <see cref="BufferReader"/> properly positioned.</param>
        /// <param name="ordinal">Ordinal of the value.</param>
        /// <returns>A <see cref="MemberValuePair"/>.</returns>
        public MemberValuePair ReadSerializedNamedArgument( int ordinal, BufferReader buffer )
        {
            // Gets the member kind.
            CorSerializationType memberKind = (CorSerializationType) buffer.ReadByte();

            if ( memberKind != CorSerializationType.TypeProperty &&
                 memberKind != CorSerializationType.TypeField )
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidToken", memberKind,
                                                                           "kind of CA serialized named argument" );
            }

            SerializationType type = SerializationType.ReadType( this.module, buffer );
            string memberName = buffer.ReadSerString();
            object value = type.ReadValue( buffer );

            // Create the return value.
            MemberValuePair memberValue = new MemberValuePair(
                memberKind == CorSerializationType.TypeProperty ? MemberKind.Property : MemberKind.Field,
                ordinal,
                memberName,
                new SerializedValue( type, value ) );

            return memberValue;
        }
    }
}