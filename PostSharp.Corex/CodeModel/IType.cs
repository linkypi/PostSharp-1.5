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
using System.Reflection;
using PostSharp.CodeModel.Collections;
using PostSharp.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of a type.
    /// </summary>
    /// <remarks>
    /// A type has all the semantics of a type signature (<see cref="ITypeSignature"/>) and exposes additionally
    /// methods and fields.
    /// </remarks>
// ReSharper disable PossibleInterfaceMemberAmbiguity
    public interface IType : ITypeSignature, IMetadataDeclaration
// ReSharper restore PossibleInterfaceMemberAmbiguity
    {
        /// <summary>
        /// Gets the declaring type (i.e. the type in which the current type
        /// is nested), or <b>null</b> if the current type is not nested.
        /// </summary>
        IType DeclaringType { get; }

        /// <summary>
        /// Gets the collection of methods.
        /// </summary>
        IMethodCollection Methods { get; }

        /// <summary>
        /// Gets the collection of fields.
        /// </summary>
        IFieldCollection Fields { get; }

        /// <summary>
        /// Gets or sets the type attributes.
        /// </summary>
        TypeAttributes Attributes { get; }

        /// <summary>
        /// Determines whether the type is sealed.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Determines whether the type is an interface.
        /// </summary>
        bool IsInterface { get; }

        /// <summary>
        /// Determines whether the class is abstract.
        /// </summary>
        bool IsAbstract { get; }
    }

    /// <summary>
    /// Defines the semantics of a type (<see cref="IType"/>) that can be named.
    /// </summary>
    /// <remarks>The only named types are <see cref="TypeDefDeclaration"/>
    /// and <see cref="TypeRefDeclaration"/>.
    /// </remarks>
    public interface INamedType : IType, INamed
    {
        /// <summary>
        /// Gets the collection of types nested in the current type.
        /// </summary>
        INamedTypeCollection NestedTypes { get; }

    }


    namespace Collections
    {
        /// <summary>
        /// Semantics of collections of types as required by <see cref="IType"/>.
        /// </summary>
        public interface INamedTypeCollection : IEnumerable<INamedType>
        {
            /// <inheritdoc />
            INamedType GetByName( string name );
        }

        /// <summary>
        /// Semantics of collections of methods as required by <see cref="IType"/>.
        /// </summary>
        public interface IMethodCollection : IEnumerable<IMethod>
        {
            /// <summary>
            /// Finds a method in the type given its name and signature.
            /// </summary>
            /// <param name="name">Method name.</param>
            /// <param name="signature">Method signature.</param>
            /// <param name="bindingOptions">Determines the behavior in case the method is not
            /// found.</param>
            /// <returns>The method, or <b>null</b> if the method could not be found.</returns>
            IMethod GetMethod( string name, IMethodSignature signature, BindingOptions bindingOptions );

            /// <summary>
            /// Gets a set of methods given their name.
            /// </summary>
            /// <param name="name">Method name.</param>
            /// <returns>The set of methods named <paramref name="name"/>.</returns>
            IEnumerable<IMethod> GetByName( string name );
        }

        /// <summary>
        /// Semantics of collections of fields as required by <see cref="IType"/>.
        /// </summary>
        public interface IFieldCollection : IEnumerable<IField>
        {
            /// <summary>
            /// Finds a field in the type given its name and type.
            /// </summary>
            /// <param name="name">Field name.</param>
            /// <param name="type">Field type.</param>
            /// <param name="bindingOptions">Determines the behavior in case the field is not
            /// found.</param>
            /// <returns>The field, or <b>null</b> if the method could not be found.</returns>
            IField GetField( string name, ITypeSignature type, BindingOptions bindingOptions );

            /// <summary>
            /// Gets a field given its name.
            /// </summary>
            /// <param name="name">Field name.</param>
            /// <returns>The field named <paramref name="name"/>, or <b>null</b> if no such
            /// field exist in the collection.</returns>
            IField GetByName( string name );
        }
    }
}