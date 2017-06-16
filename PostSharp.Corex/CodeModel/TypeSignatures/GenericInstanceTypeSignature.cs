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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents a generic type construction.
    /// </summary>
    public sealed class GenericTypeInstanceTypeSignature : TypeSignature, IGenericInstance
    {
        #region Fields

        /// <summary>
        /// The formal generic type.
        /// </summary>
        private readonly INamedType genericDeclaration;

        /// <summary>
        /// List of concrete generic arguments.
        /// </summary>
        private readonly IList<ITypeSignature> genericArguments;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="GenericTypeInstanceTypeSignature"/>.
        /// </summary>
        /// <param name="genericDeclaration">The formal generic type.</param>
        /// <param name="genericArguments">List of concrete generic arguments.</param>
        public GenericTypeInstanceTypeSignature( INamedType genericDeclaration, IList<ITypeSignature> genericArguments )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( genericDeclaration, "genericDeclaration" );
            ExceptionHelper.AssertArgumentNotNull( genericArguments, "genericArguments" );
#if ASSERT
            for ( int i = 0; i < genericArguments.Count; i++ )
            {
                ExceptionHelper.AssertArgumentNotNull( genericArguments[i],
                                                       string.Format( CultureInfo.InvariantCulture,
                                                                      "genericArguments[{0}]", i ) );
            }
#endif

            #endregion

            this.genericDeclaration = genericDeclaration;
            if ( genericArguments.IsReadOnly )
            {
                this.genericArguments = genericArguments;
            }
            else
            {
                this.genericArguments = new ReadOnlyCollection<ITypeSignature>( genericArguments );
            }
        }

        /// <inheritdoc />
        public override void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            bool serializedValue = (options & ReflectionNameOptions.SerializedValue) != 0;
            bool useBrackets = ( options & ReflectionNameOptions.UseBracketsForGenerics ) != 0;

          
            bool writeGenericArgs = true;

            if (serializedValue)
            {
                // Write the type name without assembly name.
                this.genericDeclaration.WriteReflectionTypeName(stringBuilder, (options |
                                                           ReflectionNameOptions.IgnoreGenericTypeDefParameters)
                                                           & ~ReflectionNameOptions.UseAssemblyName);
            }
            else
            {
                this.genericDeclaration.WriteReflectionTypeName(stringBuilder,
                                                           options |
                                                           ReflectionNameOptions.IgnoreGenericTypeDefParameters);

                // Difficult (and stupid): we do not have to write the generic arguments when the definition
                // is a nested type (!).

                if ( (options & ReflectionNameOptions.EncodingMask) == ReflectionNameOptions.MethodParameterEncoding )
                {
                    if ( this.genericDeclaration.DeclaringType != null )
                    {
                        writeGenericArgs = false;
                    }
                }
            }

            if ( writeGenericArgs )
            {
                stringBuilder.Append( useBrackets ? '[' : '<' );
                for ( int i = 0; i < this.genericArguments.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        stringBuilder.Append( ',' );
                    }

                    if ( this.genericArguments[i] == null )
                    {
                        stringBuilder.Append( "?null?" );
                    }
                    else
                    {
                        if ( ( options & ReflectionNameOptions.UseAssemblyName ) != 0 &&
                            !serializedValue)
                        {
                            stringBuilder.Append( '[' );
                            this.genericArguments[i].WriteReflectionTypeName( stringBuilder,
                                                                              ( options &
                                                                                ~ReflectionNameOptions.EncodingMask ) |
                                                                              ReflectionNameOptions.NormalEncoding );
                            stringBuilder.Append( ']' );
                        }
                        else
                        {
                            this.genericArguments[i].WriteReflectionTypeName( stringBuilder,
                                                                              ( options &
                                                                                ~ReflectionNameOptions.EncodingMask ) |
                                                                              ReflectionNameOptions.NormalEncoding );
                        }
                    }
                }
                stringBuilder.Append( useBrackets ? ']' : '>' );
            }

            if ( serializedValue && this.genericDeclaration.DeclaringAssembly != this.DeclaringAssembly )
            {
                // Now write the assembly name.
                stringBuilder.Append( ", " );
                stringBuilder.Append(this.genericDeclaration.DeclaringAssembly.FullName);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder str = new StringBuilder( this.genericDeclaration.ToString() );
            str.Append( '<' );
            for ( int i = 0; i < this.genericArguments.Count; i++ )
            {
                if ( i > 0 )
                {
                    str.Append( ',' );
                }
                if ( this.genericArguments[i] == null )
                {
                    str.Append( "?null?" );
                }
                else
                {
                    str.Append( this.genericArguments[i].ToString() );
                }
            }
            str.Append( '>' );

            return str.ToString();
        }

        /// <summary>
        /// Gets the formal generic type.
        /// </summary>
        public INamedType GenericDefinition
        {
            get { return this.genericDeclaration; }
        }

        /// <summary>
        /// Gets the list of concrete generic arguments.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="TypeSignature"/>.
        /// </value>
        public IList<ITypeSignature> GenericArguments
        {
            get { return this.genericArguments; }
        }

        #region TypeSignature implementation


        internal override ITypeSignature GetNakedType( TypeNakingOptions options )
        {
            return this;
        }

        /// <inheritdoc />
        internal override bool InternalIsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                       IsAssignableToOptions options )
        {
            GenericMap mappedGenericContext = this.GetGenericContext( GenericContextOptions.None ).Apply( genericMap );
            GenericTypeInstanceTypeSignature mappedThis =
                new GenericTypeInstanceTypeSignature( this.genericDeclaration,
                                                      mappedGenericContext.GetGenericTypeParameters() );

            if ( mappedThis.Equals( signature ) )
            {
                return true;
            }

            return
                ( (ITypeSignatureInternal)
                  this.genericDeclaration.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers |
                                                        TypeNakingOptions.IgnorePinned ) ).IsAssignableTo( signature,
                                                                                                           mappedGenericContext,
                                                                                                           options );
        }


        /// <inheritdoc />
        public override NullableBool BelongsToClassification( TypeClassifications typeClassification )
        {
            switch ( typeClassification )
            {
                case TypeClassifications.Any:
                case TypeClassifications.GenericTypeInstance:
                case TypeClassifications.Signature:
                    return true;

                case TypeClassifications.Class:
                case TypeClassifications.Delegate:
                case TypeClassifications.Interface:
                case TypeClassifications.ReferenceType:
                case TypeClassifications.Struct:
                case TypeClassifications.ValueType:
                    return this.genericDeclaration.BelongsToClassification( typeClassification );

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        int IGenericInstance.GenericArgumentCount
        {
            get { return this.genericArguments.Count; }
        }

        /// <inheritdoc />
        ITypeSignature IGenericInstance.GetGenericArgument( int ordinal )
        {
            return this.genericArguments[ordinal];
        }

        /// <inheritdoc />
        public override bool IsGenericInstance
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override GenericMap GetGenericContext( GenericContextOptions options )
        {
            // Generic method parameters are unchanged, but generic type parameters are replaced.
            return new GenericMap( this.genericArguments, null );
        }

        /// <inheritdoc />
        public override bool ContainsGenericArguments()
        {
            for ( int i = 0; i < this.genericArguments.Count; i++ )
            {
                if ( this.genericArguments[i].ContainsGenericArguments() )
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public override ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            if ( this.ContainsGenericArguments() )
            {
                ITypeSignature[] arguments = new ITypeSignature[this.genericArguments.Count];

                for ( int i = 0; i < arguments.Length; i++ )
                {
                    arguments[i] = this.genericArguments[i].MapGenericArguments( genericMap );

                    if ( arguments[i] == null )
                    {
                        return null;
                    }
                }

                return new GenericTypeInstanceTypeSignature( this.genericDeclaration, arguments );
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the formal generic type.
        /// </summary>
        public override ITypeSignature ElementType
        {
            get { return this.genericDeclaration; }
        }

        /// <notSupported />
        public override int GetValueSize( PlatformInfo platform )
        {
            return -1;
        }


        /// <inheritdoc />
        internal override void InternalWriteILReference( ILWriter writer, GenericMap genericMap,
                                                         WriteTypeReferenceOptions options )
        {
            ( (ITypeSignatureInternal) this.genericDeclaration ).WriteILReference( writer, genericMap,
                                                                                   options |
                                                                                   WriteTypeReferenceOptions.
                                                                                       WriteTypeKind );

            if ( this.genericArguments.Count > 0 )
            {
                writer.WriteSymbol( '<' );
                for ( int i = 0; i < this.genericArguments.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        writer.WriteSymbol( ',' );
                    }
                    ( (ITypeSignatureInternal) this.genericArguments[i] ).WriteILReference( writer, genericMap,
                                                                                            options |
                                                                                            WriteTypeReferenceOptions.
                                                                                                WriteTypeKind );
                }
                writer.WriteSymbol( '>' );
            }
        }

        /// <inheritdoc />
        public override Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new GenericTypeInstanceTypeSignatureWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        public override Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            Type genericType = this.genericDeclaration.GetSystemType( genericTypeArguments, genericMethodArguments );
            if ( genericType == null )
            {
                return null;
            }
            Type[] arguments = new Type[this.genericArguments.Count];
            for ( int i = 0; i < arguments.Length; i++ )
            {
                arguments[i] = this.genericArguments[i].GetSystemType( genericTypeArguments, genericMethodArguments );
                if ( arguments[i] == null )
                {
                    return null;
                }
            }

            return genericType.MakeGenericType( arguments );
        }

        /// <inheritdoc />
        public override ModuleDeclaration Module
        {
            get { return this.genericDeclaration.Module; }
        }

        /// <inheritdoc />
        protected override TypeDefDeclaration InternalGetTypeDefinition( BindingOptions bindingOptions )
        {
            return this.genericDeclaration.GetTypeDefinition( bindingOptions );
        }

        /// <inheritdoc />
        public override ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            if ( this.Module == targetModule )
            {
                return this;
            }
            else
            {
                INamedType translatedGenericDef = (INamedType) this.GenericDefinition.Translate( targetModule );
                ITypeSignature[] translatedGenericArgs = new ITypeSignature[this.genericArguments.Count];
                for ( int i = 0; i < translatedGenericArgs.Length; i++ )
                {
                    translatedGenericArgs[i] = this.genericArguments[i].Translate( targetModule );
                }
                return new GenericTypeInstanceTypeSignature( translatedGenericDef, translatedGenericArgs );
            }
        }

        #endregion

        /// <summary>
        /// Return the <see cref="GenericMap"/> valid inside the scope of this generic type specification.
        /// </summary>
        /// <param name="context">The <see cref="GenericMap"/> valid <i>outside</i> the scope
        /// of the generic type definition.</param>
        /// <returns>A <see cref="GenericMap"/>.</returns>
        [SuppressMessage( "Microsoft.Usage", "CA1801" )]
        [SuppressMessage( "Microsoft.Performance", "CA1822" )]
        public GenericMap ResolveGenericContext( GenericMap context )
        {
            throw new NotImplementedException();

            /*
            // We replace the generic type parameters by our (resolved), but let method generic parameters.
            IType[] typeParameters = new IType[this.genericArguments.Count];
            
            for ( int i = 0 ; i < typeParameters.Length ; i++ )
            {
                 GenericParameterDeclaration genericParameterDeclaration = type as GenericParameterDeclaration;

            if ( genericParameterDeclaration != null )
            {
                return (ITypeInternal) genericMap.GetGenericParameter( genericParameterDeclaration.GenericParameterKind, 
                    genericParameterDeclaration.Ordinal );
            }

            GenericParameterTypeSignature genericParameterTypeSignatureTypeSignature = type as GenericParameterTypeSignature;

            if ( genericParameterTypeSignatureTypeSignature != null )
            {
                return (ITypeInternal) genericMap.GetGenericParameter( genericParameterTypeSignatureTypeSignature.GenericParameterKind, 
                    genericParameterTypeSignatureTypeSignature.Ordinal ); ;
            }
            
            }
             *  * */
        }

        /// <inheritdoc />
        public override void Visit( string role, Visitor<ITypeSignature> visitor )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( visitor, "visitor" );

            #endregion

            for ( int i = 0; i < this.genericArguments.Count; i++ )
            {
                visitor( this,
                         string.Format( CultureInfo.InvariantCulture, "GenericArgument:{0}", i ),
                         this.genericArguments[i] );

                this.genericArguments[i].Visit( role, visitor );
            }

            visitor( this, "GenericDeclaration", this.genericDeclaration );
            this.genericDeclaration.Visit( role, visitor );
        }

        internal override bool InternalEquals( ITypeSignature reference, bool isReference )
        {
            GenericTypeInstanceTypeSignature referenceAsGenericInstance = reference as GenericTypeInstanceTypeSignature;
            if ( referenceAsGenericInstance == null )
                return false;

            if (
                !( (ITypeSignatureInternal) this.genericDeclaration ).Equals(
                     referenceAsGenericInstance.genericDeclaration, isReference ) )
                return false;

            if ( this.genericArguments.Count != referenceAsGenericInstance.genericArguments.Count )
                return false;

            for ( int i = 0; i < this.genericArguments.Count; i++ )
            {
                if (
                    !( (ITypeSignatureInternal)
                       this.genericArguments[i].GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers ) ).Equals
                         ( referenceAsGenericInstance.genericArguments[i].GetNakedType(
                               TypeNakingOptions.IgnoreOptionalCustomModifiers ), isReference ) )
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override int GetCanonicalHashCode()
        {
            int hashCode = ( (ITypeSignatureInternal) this.genericDeclaration ).GetCanonicalHashCode();

            for ( int i = 0; i < this.genericArguments.Count; i++ )
            {
                hashCode = HashCodeHelper.CombineHashCodes( hashCode,
                                                            ( this.genericArguments[i] ).
                                                                GetCanonicalHashCode() );
            }

            return hashCode;
        }
    }
}