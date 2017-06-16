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
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents the type of a method pointer.
    /// </summary>
    public sealed class MethodPointerTypeSignature : TypeSignature
    {
        /// <summary>
        /// Signature of the method referenced by a pointer of this type.
        /// </summary>
        private readonly MethodSignature method;

        /// <summary>
        /// Initializes a new <see cref="MethodPointerTypeSignature"/>.
        /// </summary>
        /// <param name="method">Signature of the method referenced by a pointer of this type.</param>
        public MethodPointerTypeSignature( MethodSignature method )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( method, "method" );

            #endregion

            this.method = method;
        }

        /// <summary>
        /// Gets the signature of the method referenced by a pointer of the current type.
        /// </summary>
        public MethodSignature Signature { get { return this.method; } }

        #region TypeSignature implementation

        internal override ITypeSignature GetNakedType(TypeNakingOptions options)
        {
            return this;
        }

        /// <inheritdoc />
        internal override bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap, IsAssignableToOptions options )
        {
            if ( this.Equals( signature ) )
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override void Visit( string role, Visitor<ITypeSignature> visitor )
        {
            ExceptionHelper.AssertArgumentNotNull( visitor, "visitor" );

            this.method.Visit( role, visitor );
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            stringBuilder.Append( "?MethodPointer?" );
        }

        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.Any:
                case TypeClassifications.MethodPointer:
                case TypeClassifications.Signature:
                case TypeClassifications.ValueType:
                    return true;

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override bool ContainsGenericArguments()
        {
            return this.Signature.ReferencesAnyGenericArgument();
        }


        /// <inheritdoc />
        public override ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            if ( this.ContainsGenericArguments() )
            {
                return
                    new MethodPointerTypeSignature( (MethodSignature) this.Signature.MapGenericArguments( genericMap ) );
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc />
        public override ITypeSignature ElementType { get { return null; } }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.method.ToString();
        }

        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            this.method.WriteILDefinition( writer, genericMap, true );
        }

        /// <inheritdoc />
        public override int GetValueSize( PlatformInfo platform )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( platform, "platforn" );

            #endregion

            return platform.NativePointerSize;
        }

        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return typeof(IntPtr);
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return typeof(IntPtr);
        }

        /// <inheritdoc />
        public override ModuleDeclaration Module { get { return this.method.Module; } }

        /// <inheritdoc />
        public override ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            this.AssertSameDomain( targetModule );

            #endregion

            if ( targetModule == this.Module )
            {
                return this;
            }
            else
            {
                return new MethodPointerTypeSignature( (MethodSignature) this.method.Translate( targetModule ) );
            }

            #endregion
        }

        #endregion

        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
