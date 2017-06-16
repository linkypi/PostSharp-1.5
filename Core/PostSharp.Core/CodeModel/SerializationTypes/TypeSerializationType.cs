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
using System.Text;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel.SerializationTypes
{
    /// <summary>
    /// Represents the serialization type of a type reference.
    /// </summary>
    public sealed class TypeSerializationType : SerializationType
    {
        private TypeSerializationType( ModuleDeclaration module ) : base( module )
        {
        }

        /// <summary>
        /// Gets a <see cref="TypeSerializationType"/> for a given module.
        /// </summary>
        /// <param name="module">Module in which the instance should be valid.</param>
        /// <returns>A <see cref="TypeSerializationType"/> value for this module.</returns>
        public static TypeSerializationType GetInstance( ModuleDeclaration module )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );

            #endregion

            return new TypeSerializationType( module );
        }

        /// <inheritdoc />
        internal override object ToRuntimeValue( object value )
        {
            if ( value == null )
                return null;

            ITypeSignature typeSig = (ITypeSignature) value;
            return typeSig.GetSystemType( null, null );
        }

        internal override object FromRuntimeValueImpl( object value )
        {
            if (value == null)
                return null;

            return this.Module.Cache.GetType( (Type) value );
        }

        /// <inheritdoc />
        internal override void WriteILValue( object value,
                                             ILWriter writer, WriteSerializedValueMode mode )
        {
            bool includeType = mode != WriteSerializedValueMode.ArrayElementValue;

            if ( includeType )
            {
                writer.WriteKeyword( "type" );
                writer.WriteSymbol( '(' );
            }

            if ( value == null )
            {
                writer.WriteKeyword( "nullref" );
            }
            else
            {
                ITypeSignatureInternal typeSig = (ITypeSignatureInternal) value;
                if ( typeSig.DeclaringAssembly != this.Module.Assembly ||
                     typeSig.BelongsToClassification( TypeClassifications.Signature ) )
                {
                    typeSig.WriteILReference( writer, GenericMap.Empty,
                                              WriteTypeReferenceOptions.SerializedTypeReference );
                }
                else
                {
                    typeSig.WriteILReference( writer, GenericMap.Empty,
                                              WriteTypeReferenceOptions.None );
                }
            }

            if ( includeType )
            {
                writer.WriteSymbol( ')' );
            }
        }

        /// <inheritdoc />
        internal override void WriteILType( ILWriter writer )
        {
            writer.WriteKeyword( "type" );
        }

        /// <inheritdoc />
        internal override void EmitLoadValue( object value, InstructionEmitter writer )
        {
            if (value != null)
            {
                DeclarationCache cache = writer.MethodBody.Module.Cache;
                writer.EmitInstructionType( OpCodeNumber.Ldtoken, (ITypeSignature) value );
                writer.EmitInstructionMethod( OpCodeNumber.Call,
                                              cache.GetItem(cache.TypeGetTypeFromHandle));
            }
            else
            {
                writer.EmitInstruction(OpCodeNumber.Ldnull );
            }
        }

        /// <inheritdoc />
        public override ITypeSignature GetRuntimeType()
        {
            return this.Module.Cache.GetType( "System.Type, mscorlib" );
        }

        /// <inheritdoc />
        public override void ValidateValue( object value )
        {
            if (value != null && !(value is ITypeSignature))
            {
                throw ExceptionHelper.Core.CreateArgumentException(
                    "value", "InvalidArgumentType",
                    typeof(ITypeSignature), value.GetType() );
            }
        }

        public override void ValidateRuntimeValue(object value)
        {
            if ( value != null && !(value is Type))
            {
                throw ExceptionHelper.Core.CreateArgumentException(
                  "value", "InvalidArgumentType",
                  typeof(Type), value.GetType());
            }
        }

        /// <inheritdoc />
        internal override object ReadValue( BufferReader buffer )
        {
            string typeName = buffer.ReadSerString();

            if ( typeName == null )
                return null;

            return
                this.Module.FindType( typeName, BindingOptions.WeakReference | BindingOptions.RequireGenericInstance );
        }

        internal override void WriteValue(object value, System.IO.BinaryWriter writer)
        {
            StringBuilder stringBuilder = new StringBuilder();
            ((ITypeSignature) value).WriteReflectionTypeName(stringBuilder, ReflectionNameOptions.SerializedValueOptions);
            SerializationUtil.WriteSerString( stringBuilder.ToString(), writer);
         
    }

        internal override void WriteType(System.IO.BinaryWriter writer)
        {
            writer.Write((byte)CorSerializationType.Type);
        }

        /// <inheritdoc />
        public override SerializationType Translate(ModuleDeclaration module)
        {
            return new TypeSerializationType(module);
        }

        internal override object TranslateValue(ModuleDeclaration module, object value)
        {
            if (module == this.Module)
                return value;
            else
            {
                // We use this way to find the method so we have a chance to get a weak reference.
                StringBuilder stringBuilder = new StringBuilder();
                ((ITypeSignature) value).WriteReflectionTypeName(stringBuilder, ReflectionNameOptions.UseAssemblyName | ReflectionNameOptions.UseBracketsForGenerics);
                return this.Module.FindType(stringBuilder.ToString(), BindingOptions.WeakReference | BindingOptions.RequireGenericInstance);
            }

        }
    }
}
