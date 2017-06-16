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
using System.Globalization;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents a reference to a generic parameter.
    /// </summary>
    public sealed class GenericParameterTypeSignature : TypeSignature, IGenericParameter
    {
        #region Fields

        private readonly ModuleDeclaration module;

        /// <summary>
        /// Index in the list of generic parameters.
        /// </summary>
        private readonly int ordinal;

        /// <summary>
        /// Kind of generic parameter (method, type).
        /// </summary>
        private readonly GenericParameterKind kind;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new <see cref="GenericParameterTypeSignature"/>.
        /// </summary>
        /// <param name="ordinal">Ordinal.</param>
        /// <param name="module">Module in which the instance is valid.</param>
        /// <param name="genericParameterKind">Kind of generic parameter (method or type).</param>
        private GenericParameterTypeSignature( ModuleDeclaration module, int ordinal,
                                               GenericParameterKind genericParameterKind )
        {
            this.module = module;
            this.ordinal = ordinal;
            this.kind = genericParameterKind;
        }


        /// <summary>
        /// Gets a <see cref="GenericParameterTypeSignature"/>.
        /// </summary>
        /// <param name="ordinal">Ordinal.</param>
        /// <param name="genericParameterKind">Kind of generic parameter (method or type).</param>
        /// <param name="module">Module for which the instance is valid.</param>
        /// <returns>The requested <see cref="GenericParameterTypeSignature"/>.</returns>
        /// <remarks>
        /// User code should call <see cref="DeclarationCache"/>.<see cref="DeclarationCache.GetGenericParameter"/>
        /// instead of this method.
        /// </remarks>
        public static GenericParameterTypeSignature GetInstance( ModuleDeclaration module, int ordinal,
                                                                 GenericParameterKind genericParameterKind )
        {
            return new GenericParameterTypeSignature( module, ordinal, genericParameterKind );
        }

        #endregion

        /// <summary>
        /// Gets the generic parameter orginal.
        /// </summary>
        public int Ordinal
        {
            get { return this.ordinal; }
        }

        /// <summary>
        /// Gets the kind of generic parameter (type or method).
        /// </summary>
        public GenericParameterKind Kind
        {
            get { return this.kind; }
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            stringBuilder.Append( this.kind == GenericParameterKind.Method ? "!!" : "!" );
            stringBuilder.Append( this.ordinal );
        }

        /// <inheritdoc />
        GenericParameterTypeSignature IGenericParameter.GetReference()
        {
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Concat( this.kind == GenericParameterKind.Type ? "!" : "!!",
                                  ordinal.ToString( CultureInfo.InvariantCulture ) );
        }

        #region TypeSignature implementation

        internal override bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                       IsAssignableToOptions options )
        {
            if ( this.Equals( signature ) )
                return true;

            // Resolve the current generic parameter.
            ITypeSignatureInternal resolved = (ITypeSignatureInternal) genericMap.GetGenericParameter( this.kind, this.ordinal ).GetNakedType( TypeNakingOptions.None );

            // If the resolved generic parameter is still a generic parameter (i.e. we dont have any more info on this generic parameter),
            // we test for equality. 
            GenericParameterTypeSignature resolvedGenericParameter = resolved as GenericParameterTypeSignature;
            if ( resolvedGenericParameter != null )
            {
                return resolved.Equals( signature );
            }
            

            // We check the assignability of the resolved type.
            // We still pass the generic map so that, if the generic argument has
            // been resolved to a generic parameter, constraints can be resolved.
            return resolved.IsAssignableTo( signature, genericMap, options );
        }

        internal override ITypeSignature GetNakedType( TypeNakingOptions options )
        {
            return this;
        }

        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.Any:
                case TypeClassifications.GenericParameter:
                case TypeClassifications.Signature:
                    return true;

                case TypeClassifications.Module:
                    return false;

                default:
                    return NullableBool.Null;
            }
        }

        /// <inheritdoc />
        public override bool ContainsGenericArguments()
        {
            return true;
        }

        /// <inheritdoc />
        public override ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            return genericMap.GetGenericParameter( this.kind, this.ordinal );
        }

        /// <inheritdoc />
        public override ITypeSignature ElementType
        {
            get { return null; }
        }

        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            IGenericParameter decl;

            if ( genericMap.ContainsGenericParameter( this.kind, this.ordinal ) )
            {
                decl = (IGenericParameter) genericMap.GetGenericParameter( this.kind, this.ordinal );
            }
            else
            {
                decl = null;
            }

            if ( decl != null && decl != this )
            {
                ( (ITypeSignatureInternal) decl ).WriteILReference( writer, genericMap, options );
            }
            else
            {
                switch ( this.kind )
                {
                    case GenericParameterKind.Type:
                        writer.WriteSymbol( "!" );
                        break;

                    case GenericParameterKind.Method:
                        writer.WriteSymbol( "!!" );
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( this.kind, "this.Kind" );
                }

                writer.WriteInteger( this.ordinal, IntegerFormat.Decimal );
            }
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            if ( this.kind == GenericParameterKind.Type )
            {
                if ( genericTypeArguments != null && genericTypeArguments.Length > this.ordinal )
                {
                    return genericTypeArguments[this.ordinal];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if ( genericMethodArguments != null && genericMethodArguments.Length > this.ordinal )
                {
                    return genericMethodArguments[this.ordinal];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            Type reflectionType;

            switch ( this.kind )
            {
                case GenericParameterKind.Method:
                    if ( genericMethodArguments == null )
                    {
                        reflectionType = null;
                    }
                    else
                    {
                        reflectionType = genericMethodArguments[this.ordinal];
                    }
                    break;

                case GenericParameterKind.Type:
                    if ( genericTypeArguments == null )
                    {
                        reflectionType = null;
                    }
                    else
                    {
                        reflectionType = genericTypeArguments[this.ordinal];
                    }
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.kind, "this.Kind" );
            }

            if ( reflectionType == null )
            {
                Trace.ReflectionBinding.WriteLine(
                    "Cannot resolve the type argument {0} because the argument is not linked in the given generic context.",
                    this );
            }

            return reflectionType;
        }

        /// <inheritdoc />
        public override ModuleDeclaration Module
        {
            get { return this.module; }
        }

        /// <inheritdoc />
        public override ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            return this;
        }

        #endregion

        /// <inheritdoc />
        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            IGenericParameter referenceAsGenericParameter = reference as IGenericParameter;
            if ( referenceAsGenericParameter == null )
                return false;

            return this.kind == referenceAsGenericParameter.Kind &&
                   this.ordinal == referenceAsGenericParameter.Ordinal;
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            return HashCodeHelper.CombineHashCodes( (int) this.kind, this.ordinal );
        }
    }
}