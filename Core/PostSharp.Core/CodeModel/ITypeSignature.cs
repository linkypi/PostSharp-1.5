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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PostSharp.CodeModel.Binding;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the functionalities that are common to all representations
    /// of a type (<see cref="TypeDefDeclaration"/>, <see cref="TypeRefDeclaration"/>,
    /// <see cref="TypeSpecDeclaration"/>, <see cref="TypeSignature"/>).
    /// </summary>
    public interface ITypeSignature : IModuleScoped, IVisitable<ITypeSignature>, IGeneric,
                                      IEquatable<ITypeSignature>
    {
        /// <summary>
        /// Determines whether the type signature belongs to a given classification,
        /// i.e. whether it fulfills a given predicate.
        /// </summary>
        /// <param name="typeClassification">The classification (or predicate) 
        /// (combination of bits are not allowed).</param>
        /// <returns><see cref="NullableBool.True"/> if the predicate is true, 
        /// <see cref="NullableBool.False"/> if the predicate is false or
        /// <see cref="NullableBool.Null"/> if it cannot be determined. </returns>
        NullableBool BelongsToClassification( TypeClassifications typeClassification );

        /// <summary>
        /// Gets the size of the value type.
        /// </summary>
        /// <param name="platform">Information about the target platform.</param>
        /// <returns>The size of the value type in bytes, or -1 if the
        /// type is not a value type or has no fixed size.</returns>
        int GetValueSize( PlatformInfo platform );


        /// <summary>
        /// Gets the system, runtime <see cref="Type"/> corresponding to the current type.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>The system <see cref="Type"/> associated the current type in the
        /// given generic context.</returns>
        Type GetSystemType( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <summary>
        /// Gets a reflection <see cref="Type"/> that wraps the current type.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>A <see cref="Type"/> wrapping current type in the
        /// given generic context.</returns>
        /// <remarks>
        /// This method returns a <see cref="Type"/> that is different from the system
        /// runtime type that is retrieved by <see cref="GetSystemType"/>. This allows
        /// a have a <b>System.Reflection</b> representation of the current type even
        /// when it cannot be loaded in the Virtual Runtime Engine.
        /// </remarks>
        Type GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <summary>
        /// Gets the type name as used in <b>System.Reflection</b>.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        [SuppressMessage( "Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters" )]
        void WriteReflectionTypeName( StringBuilder stringBuilder, ReflectionNameOptions options );

        /// <summary>
        /// Determines whether the type signature contains a generic argument.
        /// </summary>
        /// <returns><b>true</b> if the type signature contains a generic argument, otherwise <b>false</b>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        bool ContainsGenericArguments();

        /// <summary>
        /// Resolves all generic arguments in the current type signature.
        /// </summary>
        /// <param name="genericMap">Generic context in which generic arguments have to be resolved.</param>
        /// <returns>A <see cref="IType"/> resolved against <paramref name="genericMap"/>.</returns>
        ITypeSignature MapGenericArguments( GenericMap genericMap );


        /// <summary>
        /// Returns the meaningful type. Removes specified modifiers
        /// and resolves type references.
        /// </summary>
        /// <param name="options">Specifies what has to be ignored.</param>
        /// <returns>A 'naked' <see cref="IType"/>.</returns>
        /// <remarks>This method only removes the modifiers on the current object,
        /// all of the nodes of a type signature. However, the object returned by 
        /// this method (i.e. the head of the chain) is guaranteed to be
        /// free from modifiers specified in <paramref name="options"/>.</remarks>
        ITypeSignature GetNakedType( TypeNakingOptions options );

        /// <summary>
        /// Translates the current type signature so that it is meaningful in another
        /// module than the one to which it primarly belong.
        /// </summary>
        /// <param name="targetModule">Module into which the type signature should be
        /// translated.</param>
        /// <returns>A type signature meaningful in the <paramref name="targetModule"/>
        /// module.</returns>
        ITypeSignature Translate( ModuleDeclaration targetModule );

        /// <summary>
        /// Finds in the current domain the <see cref="TypeDefDeclaration"/> corresponding
        /// to the current type and uses default <see cref="BindingOptions"/>.
        /// </summary>
        /// <returns>The <see cref="TypeDefDeclaration"/> corresponding to the current 
        /// instance in the current domain.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        TypeDefDeclaration GetTypeDefinition();

        /// <summary>
        /// Finds in the current domain the <see cref="TypeDefDeclaration"/> corresponding
        /// to the current type and specifies <see cref="BindingOptions"/>.
        /// </summary>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="TypeDefDeclaration"/> corresponding to the current 
        /// instance in the current domain.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        TypeDefDeclaration GetTypeDefinition( BindingOptions bindingOptions );

        /// <summary>
        /// Determines whether the current type signature matches a given type signature.
        /// </summary>
        /// <param name="reference">The type reference.</param>
        /// <returns><b>true</b> if the current type signature matches <paramref name="reference"/>,
        /// otherwise <b>false</b>.</returns>
        /// <remarks>A type reference can use an incomplete assembly reference. Therefore, the matching
        /// is performed using <see cref="IAssembly.MatchesReference"/>.
        /// Matching an assembly reference is a looser requirement than matching an assembly name exactly;
        /// an assembly reference may set no requirement on the public key token or the version, for instance.</remarks>
        bool MatchesReference(ITypeSignature reference);

        /// <summary>
        /// Determines whether instances of the current type is assignable to
        /// locations of a specified type (i.e. whether the current type derives
        /// or implements this type) and specifies a <see cref="GenericMap"/>.
        /// </summary>
        /// <param name="type">The type that the current type may derive or inherit.</param>
        /// <param name="genericMap">Map that has to be applied on generic arguments of <paramref name="type"/>
        /// before the comparison is performed. It is generally wrong to pass an empty generic map (like <see cref="GenericMap.Empty"/>).
        /// The identical generic map should be passed instrad.</param>
        /// <returns><b>true</b> if the current type can be assigned to (i.e., derives or implements) <paramref name="type"/>,
        /// otherwise <b>false</b>.</returns>
        /// <remarks>There are some exceptions to the rule that a type can be assigned to another if it derives
        /// or implements it. For instance, an <see cref="int"/> is assignable to an <see cref="uint"/>.</remarks>
        bool IsAssignableTo( ITypeSignature type, GenericMap genericMap );

        /// <summary>
        /// Determines whether instances of the current type is assignable to
        /// locations of a specified type (i.e. whether the current type derives
        /// or implements this type).
        /// </summary>
        /// <param name="type">The type that the current type may derive or inherit.</param>
        /// <returns><b>true</b> if the current type can be assigned to (i.e., derives or implements) <paramref name="type"/>,
        /// otherwise <b>false</b>.</returns>
        /// <remarks>There are some exceptions to the rule that a type can be assigned to another if it derives
        /// or implements it. For instance, an <see cref="int"/> is assignable to an <see cref="uint"/>.</remarks>
        bool IsAssignableTo(ITypeSignature type);

        /// <summary>
        /// Gets a hash code that is invariant under type signature equality
        /// (i.e. if two types are equal under <see cref="IEquatable{ITypeSignature}"/>, they have the
        /// same canonical has code).
        /// </summary>
        /// <returns>A hash code that is invariant under type signature equality.</returns>
        /// <remarks>This method is of course useful to build dictionaries. The <see cref="TypeComparer"/> class
        /// uses this method.</remarks>
        int GetCanonicalHashCode();
    }

    /// <summary>
    /// Defines internal methods that have to be implemented by all types
    /// implementing <see cref="IType"/>.
    /// </summary>
    internal interface ITypeSignatureInternal : ITypeSignature
    {
        /// <summary>
        /// Writes in IL a reference to the current instance.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The current <see cref="GenericMap"/>.</param>
        /// <param name="options">Options.</param>
        void WriteILReference( ILWriter writer, GenericMap genericMap, WriteTypeReferenceOptions options );

        bool IsAssignableTo( ITypeSignature signature, GenericMap genericMap, IsAssignableToOptions options );

        bool Equals( ITypeSignature reference, bool strict );
    }

    [Flags]
    internal enum IsAssignableToOptions
    {
        None = 0,
        DisallowUnconditionalObjectAssignability = 1
    }

    [Flags]
    internal enum WriteTypeReferenceOptions
    {
        None = 0,
        WriteTypeKind = 1,
        SerializedTypeReference = 2
    }

    /// <summary>
    /// Collection of types (<see cref="IType"/>).
    /// </summary>
    public sealed class TypeSignatureCollection : NonNullableList<ITypeSignature>
    {
        internal TypeSignatureCollection() : this( 4 )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TypeSignatureCollection"/>.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        internal TypeSignatureCollection( int capacity ) : base( capacity )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TypeSignatureCollection"/> and fills it
        /// with an existing collection.
        /// </summary>
        /// <param name="items">An existing collection of items.</param>
        internal TypeSignatureCollection( ICollection<ITypeSignature> items ) : base( items )
        {
        }
    }
}