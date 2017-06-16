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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a generic parameter (<see cref="TokenType.GenericParam"/>). 
    /// </summary>
    /// <remarks>
    /// Generic parameters
    /// are owned by types (<see cref="TypeDefDeclaration"/>) or 
    /// methods (<see cref="MethodDefDeclaration"/>).
    /// </remarks>
    public sealed class GenericParameterDeclaration : NamedDeclaration, ITypeSignatureInternal,
                                                      IPositioned, IGenericParameter
    {
        #region Fields

        /// <summary>
        /// Generic parameter attributes.
        /// </summary>
        private GenericParameterAttributes attributes;

        /// <summary>
        /// Collection of constraints.
        /// </summary>
        private readonly GenericParameterConstraintDeclarationCollection constraints;


        /// <summary>
        /// Ordinal of the current generic parameter in list of generic
        /// parameters of the declaring type or method.
        /// </summary>
        private int ordinal = -1;

        /// <summary>
        /// The kind of generic parameter.
        /// </summary>
        private GenericParameterKind kind = GenericParameterKind.Type;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="GenericParameterDeclaration"/>.
        /// </summary>
        public GenericParameterDeclaration()
        {
            this.constraints = new GenericParameterConstraintDeclarationCollection( this, "constraints" );
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.GenericParam;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }

        /// <inheritdoc />
        public GenericParameterTypeSignature GetReference()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Module != null, "" );

            #endregion

            return this.Module.Cache.GetGenericParameter( this.ordinal, this.kind );
        }

        #region Properties

        /// <summary>
        /// Gets the kind of generic parameters (<see cref="GenericParameterKind.Type"/> 
        /// or <see cref="GenericParameterKind.Method"/>).
        /// </summary>
        /// <value>
        /// A <see cref="GenericParameterKind"/>.
        /// </value>
        [ReadOnly( true )]
        public GenericParameterKind Kind
        {
            get { return kind; }
            set { this.kind = value; }
        }

        /// <summary>
        /// Gets or sets the generic parameter ordinal.
        /// </summary>
        /// <value>
        /// The ordinal of the current generic parameter,
        /// i.e. its position in its parent collection.
        /// </value>
        [ReadOnly( true )]
        public int Ordinal
        {
            get { return ordinal; }
            set
            {
                int oldValue = this.ordinal;

                if ( value != this.ordinal )
                {
                    this.ordinal = value;
                    this.OnPropertyChanged( "Ordinal", oldValue, value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the attributes of the generic parameter. 
        /// </summary>
        /// <value>
        /// A combination of <see cref="GenericParameterAttributes"/>.
        /// </value>
        [ReadOnly( true )]
        public GenericParameterAttributes Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        /// <summary>
        /// Gets the list of type constraints applying on the type parameter. 
        /// </summary>
        [Browsable( false )]
        public GenericParameterConstraintDeclarationCollection Constraints
        {
            get { return constraints; }
        }

        /// <summary>
        /// Gets the <see cref="IGenericDefinition"/> that declares the current
        /// generic parameter.
        /// </summary>
        public IGenericDefinition DeclaringGenericDefinition
        {
            get { return (IGenericDefinition) this.Parent; }
        }

        #endregion

        #region IType Members

        /// <inheritdoc />
        public NullableBool BelongsToClassification( TypeClassifications typeClassification )
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
        bool ITypeSignature.ContainsGenericArguments()
        {
            return true;
        }

        /// <inheritdoc />
        TypeDefDeclaration ITypeSignature.GetTypeDefinition()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public ITypeSignature MapGenericArguments( GenericMap genericMap )
        {
            return genericMap.GetGenericParameter( this.kind, this.ordinal );
        }

        /// <inheritdoc />
        bool ITypeSignatureInternal.IsAssignableTo( ITypeSignature signature, GenericMap genericMap,
                                                    IsAssignableToOptions options )
        {
            if ( this.Equals( signature ) )
                return true;

            if ( ( this.attributes & GenericParameterAttributes.ReferenceTypeConstraint ) != 0 &&
                 ( options & IsAssignableToOptions.DisallowUnconditionalObjectAssignability ) == 0 &&
                 IntrinsicTypeSignature.Is( signature, IntrinsicType.Object ) )
            {
                return true;
            }

            foreach ( GenericParameterConstraintDeclaration constraint in this.constraints )
            {
                if ( ( (ITypeSignatureInternal) constraint.ConstraintType ).IsAssignableTo( signature, genericMap,
                                                                                                 options |
                                                                                                 IsAssignableToOptions.
                                                                                                     DisallowUnconditionalObjectAssignability ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        bool ITypeSignatureInternal.Equals( ITypeSignature reference, bool strict )
        {
            IGenericParameter referenceAsGenericParameter = reference as IGenericParameter;

            if ( referenceAsGenericParameter == null )
                return false;

            return this.kind == referenceAsGenericParameter.Kind &&
                   this.ordinal == referenceAsGenericParameter.Ordinal;
        }

        /// <summary>
        /// Compares the current <see cref="GenericParameterDeclaration"/> with an other.
        /// </summary>
        /// <param name="other">Another <see cref="GenericParameterDeclaration"/>.</param>
        /// <param name="compareConstraints"><b>true</b> if constraints of generic parameters
        /// have to be compared also, otherwise <b>false</b>.</param>
        /// <returns><b>true</b> if both generic parameters are equal, otherwise <b>false</b>.</returns>
        public bool Equals( GenericParameterDeclaration other, bool compareConstraints )
        {
            if ( other == null ) return false;

            if ( this.kind != other.Kind || this.ordinal == other.Ordinal )
                return false;

            if ( compareConstraints )
            {
                // Compare attributes.
                if ( this.Attributes != other.Attributes )
                {
                    return false;
                }

                // Compare number of constraints.
                if ( this.Constraints.Count != other.Constraints.Count )
                {
                    return false;
                }

                // Compare individual constraints.
                foreach ( GenericParameterConstraintDeclaration myConstraint in constraints )
                {
                    bool found = false;
                    foreach ( GenericParameterConstraintDeclaration otherConstraint in other.constraints )
                    {
                        if ( myConstraint.ConstraintType.Equals( otherConstraint.ConstraintType ) )
                        {
                            found = true;
                            break;
                        }
                    }

                    if ( !found ) return false;
                }
            }

            // Both are equal.
            return true;
        }

        /// <inheritdoc />
        public bool Equals( ITypeSignature other )
        {
            return ( (ITypeSignatureInternal) this ).Equals( other, true );
        }

        /// <inheritdoc />
        public bool MatchesReference( ITypeSignature reference )
        {
            return ( (ITypeSignatureInternal) this ).Equals( reference, false );
        }

        /// <inheritdoc />
        public bool IsAssignableTo( ITypeSignature type, GenericMap genericMap )
        {
            return
                ( (ITypeSignatureInternal) this ).IsAssignableTo(
                    type.GetNakedType( TypeNakingOptions.IgnoreOptionalCustomModifiers |
                                            TypeNakingOptions.IgnorePinned ), genericMap, IsAssignableToOptions.None );
        }

        /// <inheritdoc />
        public bool IsAssignableTo(ITypeSignature type)
        {
            return this.IsAssignableTo(type, this.Module.Cache.IdentityGenericMap);
        }


        /// <inheritdoc />
        public int GetCanonicalHashCode()
        {
            return HashCodeHelper.CombineHashCodes( (int) this.kind, this.ordinal );
        }

        TypeDefDeclaration ITypeSignature.GetTypeDefinition( BindingOptions bindingOptions )
        {
            throw new NotSupportedException();
        }


        /// <inheritdoc />
        /// <remarks>
        /// Returns always -1 (unknown size).
        /// </remarks>
        public int GetValueSize( PlatformInfo platform )
        {
            return -1;
        }

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Design", "CA106:ValidateArgumentsOfPublicMethod" )]
        public Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            switch ( this.kind )
            {
                case GenericParameterKind.Method:
                    if ( genericMethodArguments != null && this.ordinal <= genericMethodArguments.Length )
                        return genericMethodArguments[this.ordinal];
                    break;

                case GenericParameterKind.Type:
                    if ( genericTypeArguments != null && this.ordinal <= genericTypeArguments.Length )
                        return genericTypeArguments[this.ordinal];
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.kind, "this.Kind" );
            }

            // The generic parameter was not mapped, so we have to look at the generic definition.

            TypeDefDeclaration declaringTypeDef = this.DeclaringGenericDefinition as TypeDefDeclaration;
            MethodDefDeclaration declatingMethodDef;
            if ( declaringTypeDef != null )
            {
                genericTypeArguments = declaringTypeDef.GetSystemType( null, null ).GetGenericArguments();
                genericMethodArguments = null;
            }
            else
            {
                declatingMethodDef = (MethodDefDeclaration) this.DeclaringGenericDefinition;
                genericTypeArguments =
                    declatingMethodDef.DeclaringType.GetSystemType( null, null ).GetGenericArguments();
                genericMethodArguments =
                    declatingMethodDef.GetSystemMethod( null, null, BindingOptions.Default ).GetGenericArguments();
            }

            switch ( this.kind )
            {
                case GenericParameterKind.Method:
                    if ( genericMethodArguments != null && this.ordinal <= genericMethodArguments.Length )
                        return genericMethodArguments[this.ordinal];
                    break;

                case GenericParameterKind.Type:
                    if ( genericTypeArguments != null && this.ordinal <= genericTypeArguments.Length )
                        return genericTypeArguments[this.ordinal];
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.kind, "this.Kind" );
            }

            return null;
        }

        /// <inheritdoc />
        public Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            switch ( this.kind )
            {
                case GenericParameterKind.Method:
                    if ( genericMethodArguments != null && this.ordinal < genericMethodArguments.Length )
                    {
                        return genericMethodArguments[this.ordinal];
                    }
                    break;


                case GenericParameterKind.Type:
                    if ( genericTypeArguments != null && this.ordinal < genericTypeArguments.Length )
                    {
                        return genericTypeArguments[this.ordinal];
                    }
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.kind, "this.kind" );
            }

            return new GenericParameterWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetSystemType( genericTypeArguments, genericMethodArguments );
        }

        /// <inheritdoc />
        ITypeSignature ITypeSignature.GetNakedType( TypeNakingOptions options )
        {
            return this;
        }

        /// <inheritdoc />
        public ITypeSignature Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            ExceptionHelper.Core.AssertValidArgument( targetModule.Domain == this.Domain, "module", "ModuleInSameDomain" );

            #endregion

            if ( targetModule == this.Module )
            {
                return this;
            }
            else
            {
                return this.Module.Cache.GetGenericParameter( this.ordinal, this.kind );
            }
        }

        /// <inheritdoc />
        public void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options )
        {
            if ( ( options & ReflectionNameOptions.UseOrdinalsForGenerics ) == 0 )
            {
                stringBuilder.Append( this.Name );
            }
            else
            {
                if ( this.kind == GenericParameterKind.Method )
                {
                    stringBuilder.Append( "!!" );
                }
                else
                {
                    stringBuilder.Append( '!' );
                }
                stringBuilder.Append( this.ordinal );
            }
        }

        /// <inheritdoc />
        void IVisitable<ITypeSignature>.Visit( string role, Visitor<ITypeSignature> visitor )
        {
        }

        #endregion

        #region writer IL

        /// <inheritdoc />
        void ITypeSignatureInternal.WriteILReference( ILWriter writer, GenericMap genericMap,
                                                      WriteTypeReferenceOptions options )
        {
            switch ( this.kind )
            {
                case GenericParameterKind.Method:
                    writer.WriteSymbol( "!!" );
                    break;

                case GenericParameterKind.Type:
                    writer.WriteSymbol( '!' );
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.kind, "this.Kind" );
            }

            if ( !string.IsNullOrEmpty( this.Name ) )
            {
                writer.WriteIdentifier( this.Name );
            }
            else
            {
                writer.WriteInteger( this.ordinal );
            }
        }


        /// <summary>
        /// Writes the IL definition of the current generic parameter.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of
        /// the containing type.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap )
        {
            if ( ( this.attributes & GenericParameterAttributes.ReferenceTypeConstraint ) != 0 )
            {
                writer.WriteKeyword( "class" );
            }


            if ( ( this.attributes & GenericParameterAttributes.NotNullableValueTypeConstraint ) != 0 )
            {
                writer.WriteKeyword( "valuetype" );
            }

            if ( ( this.attributes & GenericParameterAttributes.DefaultConstructorConstraint ) != 0 )
            {
                writer.WriteKeyword( ".ctor" );
            }

            if ( this.Constraints.Count > 0 )
            {
                writer.WriteSymbol( '(' );
                int j = 0;
                foreach ( GenericParameterConstraintDeclaration constraint in constraints )
                {
                    if ( j > 0 )
                    {
                        writer.WriteSymbol( ',' );
                    }

                    ( (ITypeSignatureInternal) constraint.ConstraintType ).WriteILReference( writer, genericMap,
                                                                                                  WriteTypeReferenceOptions
                                                                                                      .None );

                    j++;
                }
                writer.WriteSymbol( ')' );
            }
            writer.WriteIdentifier( this.Name );
        }

        #endregion

        /// <summary>
        /// Clones the current instance and makes the clone compatible within a given module.
        /// </summary>
        /// <param name="targetModule">Module in which the clone should be meaningfull.</param>
        /// <param name="genericMap">Generic map with which constraints are transformed.</param>
        /// <returns>A <see cref="GenericParameterDeclaration"/> meaningfull in <paramref name="targetModule"/>.</returns>
        /// <remarks>This method does not clone the constraints.</remarks>
        public GenericParameterDeclaration Clone( ModuleDeclaration targetModule, GenericMap genericMap )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );

            #endregion

            GenericParameterDeclaration clone = new GenericParameterDeclaration
                                                    {
                                                        Name = this.Name,
                                                        Kind = this.kind,
                                                        Ordinal = this.ordinal,
                                                        Attributes = this.Attributes
                                                    };


            return clone;
        }

        #region IGeneric Members

        GenericMap IGeneric.GetGenericContext( GenericContextOptions options )
        {
            return GenericMap.Empty;
        }

        bool IGeneric.IsGenericDefinition
        {
            get { return false; }
        }

        bool IGeneric.IsGenericInstance
        {
            get { return false; }
        }

        #endregion
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of generic parameters (<see cref="GenericParameterDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public class GenericParameterDeclarationCollection :
            OrdinalDeclarationCollection<GenericParameterDeclaration>, IList<ITypeSignature>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GenericParameterDeclarationCollection"/> class.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal GenericParameterDeclarationCollection( Declaration parent, string role ) : base( parent, role )
            {
            }

            #region IList<IType> Members

            int IList<ITypeSignature>.IndexOf( ITypeSignature item )
            {
                throw new NotSupportedException();
            }

            void IList<ITypeSignature>.Insert( int index, ITypeSignature item )
            {
                throw new NotSupportedException();
            }

            void IList<ITypeSignature>.RemoveAt( int index )
            {
                throw new NotSupportedException();
            }

            ITypeSignature IList<ITypeSignature>.this[ int index ]
            {
                get { return this[index]; }
                set { throw new NotSupportedException(); }
            }

            #endregion

            #region ICollection<IType> Members

            void ICollection<ITypeSignature>.Add( ITypeSignature item )
            {
                throw new NotSupportedException();
            }


            bool ICollection<ITypeSignature>.Contains( ITypeSignature item )
            {
                GenericParameterDeclaration typedItem = item as GenericParameterDeclaration;
                if ( typedItem != null )
                {
                    return this.Contains( typedItem );
                }
                else
                {
                    return false;
                }
            }

            void ICollection<ITypeSignature>.CopyTo( ITypeSignature[] array, int arrayIndex )
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( array, "array" );

                #endregion

                for ( int i = 0; i < this.Count; i++ )
                {
                    array[arrayIndex + i] = this[i];
                }
            }


            bool ICollection<ITypeSignature>.Remove( ITypeSignature item )
            {
                throw new NotSupportedException();
            }

            #endregion

            #region IEnumerable<IType> Members

            IEnumerator<ITypeSignature> IEnumerable<ITypeSignature>.GetEnumerator()
            {
                foreach ( GenericParameterDeclaration item in this )
                {
                    yield return item;
                }
            }

            #endregion

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                IGenericDefinitionDefinition owner = (IGenericDefinitionDefinition) this.Owner;
                owner.Module.ModuleReader.ImportGenericParameters( owner );
            }
        }
    }
}