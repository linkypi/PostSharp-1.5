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
using System.Diagnostics.CodeAnalysis;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.TypeSignatures;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of a generic parameter <i>or</i> argument.
    /// </summary>
    public interface IGenericParameter : ITypeSignature
    {
        /// <summary>
        /// Gets the kind of generic parameter (<see cref="GenericParameterKind.Method"/>
        /// or <see cref="GenericParameterKind.Type"/>).
        /// </summary>
        GenericParameterKind Kind { get; }

        /// <summary>
        /// Gets the generic parameter ordinal.
        /// </summary>
        int Ordinal { get; }

        /// <summary>
        /// Gets the <see cref="GenericParameterTypeSignature"/> that references to the current
        /// instance.
        /// </summary>
        /// <returns>A <see cref="GenericParameterTypeSignature"/> with same ordinal and
        /// kind as the current instance.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        GenericParameterTypeSignature GetReference();
    }

    /// <summary>
    /// Exposes the common semantics to generic instances and generic definitions.
    /// </summary>
    public interface IGeneric
    {
        /// <summary>
        /// Gets the generic context inside the scope of the generic instance.
        /// </summary>
        /// <returns>A <see cref="GenericMap"/> mapping ordinals either
        /// to <see cref="GenericParameterDeclaration"/> (in case of
        /// generic definition), either of the type signature associated
        /// to this ordinal (in the case of generic instance).</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        GenericMap GetGenericContext( GenericContextOptions options );

        /// <summary>
        /// Determines whether the current instance is a generic definition, i.e. whether it can
        /// be used to construct generic instances.
        /// </summary>
        /// <remarks>
        /// If the current property returns <b>true</b>, the semantics of <see cref="IGenericDefinition"/>
        /// are meaningfull for the current intance.
        /// </remarks>
        bool IsGenericDefinition { get; }

        /// <summary>
        /// Determines whether the current instance is a generic instance, i.e. whether it has
        /// been constructed from a generic definition.
        /// </summary>
        /// <remarks>
        /// If the current property returns <b>true</b>, the semantics of <see cref="IGenericInstance"/>
        /// are meaningfull for the current intance.
        /// </remarks>
        bool IsGenericInstance { get; }
    }

    /// <summary>
    /// Options of the <see cref="IGeneric.GetGenericContext"/> method.
    /// </summary>
    [Flags]
    public enum GenericContextOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None,

        /// <summary>
        /// Force type and method references (<see cref="TypeRefDeclaration"/> and <see cref="MethodRefDeclaration"/>) to
        /// return the generic context of their definition (<see cref="TypeDefDeclaration"/> and <see cref="MethodDefDeclaration"/>), that
        /// is, to put in the generic context their real generic parameters (<see cref="GenericParameterDeclaration"/>) instead of
        /// a placeholder (<see cref="GenericParameterTypeSignature"/>). Note that it forces the references to be resolved.
        /// </summary>
        ResolveGenericParameterDefinitions = 1
    }

    /// <summary>
    /// Exposes the semantics of a generic definition (i.e. a type 
    /// or method having (unbound) generic parameters). Classes implementing
    /// <see cref=" IGenericDefinition"/> can also represent references to generic definitions.
    /// </summary>
    /// <remarks>
    /// The fact that an object can be casted to <see cref="IGenericDefinition"/> does not
    /// automatically mean that it is actually a generic definition. It is always necessary
    /// to check the <see cref="IGeneric.IsGenericDefinition"/> property.
    /// </remarks>
    /// <see cref="IGenericDefinitionDefinition"/>
    public interface IGenericDefinition : IGeneric, IMetadataDeclaration
    {
        /// <summary>
        /// Gets a generic (formal, unbound) parameter given its ordinal.
        /// </summary>
        /// <param name="ordinal">The generic parameter ordinal (position).</param>
        /// <returns>A <see cref="GenericParameterDeclaration"/>, or <b>null</b>
        /// if the current generic parameter does not exist.</returns>
        /// <remarks>If the current instance is a defined in the current module
        /// (<see cref="MethodDefDeclaration"/>, <see cref="TypeDefDeclaration"/>),
        /// this method returns a <see cref="GenericParameterDeclaration"/>.
        /// Otherwise, it returns a reference to a generic parameter, i.e.
        /// a <see cref="TypeSignatures.GenericParameterTypeSignature"/>.</remarks>
        IGenericParameter GetGenericParameter( int ordinal );

        /// <summary>
        /// Gets the number of (formal, unbound) generic parameters.
        /// </summary>
        int GenericParameterCount { get; }
    }

    /// <summary>
    /// Exposes the semantics of a generic definition but, unlike <see cref="IGenericDefinition"/>,
    /// are a real definition and not a reference.
    /// </summary>
    public interface IGenericDefinitionDefinition : IGenericDefinition
    {
        /// <summary>
        /// Gets the collection of generic parameters.
        /// </summary>
        GenericParameterDeclarationCollection GenericParameters { get; }
    }

    /// <summary>
    /// Exposes the semantics of a generic instance (i.e. an instance of generic definition,
    /// with concretely specified generic, bound arguments).
    /// </summary>
    /// <remarks>
    /// The fact that an object can be casted to <see cref="IGenericInstance"/> does not
    /// automatically mean that it is actually a generic instance. It is always necessary
    /// to check the <see cref="IGeneric.IsGenericInstance"/> property.
    /// </remarks>
    public interface IGenericInstance : IGeneric
    {
        /// <summary>
        /// Gets a generic (concrete, bound) argument given its ordinal.
        /// </summary>
        /// <param name="ordinal">The generic argument ordinal (position).</param>
        /// <returns>An <see cref="IType"/>, or <b>null</b>
        /// if the current generic orginal does not exist.</returns>
        ITypeSignature GetGenericArgument( int ordinal );

        /// <summary>
        /// Gets the number of (concrete, bound) generic arguments.
        /// </summary>
        int GenericArgumentCount { get; }
    }
}