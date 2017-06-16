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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Exposes the semantics of a method signature.
    /// </summary>
    public interface IMethodSignature : IModuleScoped, IEquatable<IMethodSignature>
    {
        /// <summary>
        /// Gets the method calling convention.
        /// </summary>
        /// <value>
        /// The calling convention.
        /// </value>
        CallingConvention CallingConvention { get; }

        /// <summary>
        /// Gets the number of parameters.
        /// </summary>
        /// <value>
        /// The number of parameters.
        /// </value>
        int ParameterCount { get; }

        /// <summary>
        /// Gets the type of a parameter given its position.
        /// </summary>
        /// <param name="index">The parameter position.</param>
        /// <returns>The type (<see cref="TypeSignature"/>) of the parameter.</returns>
        ITypeSignature GetParameterType( int index );

        /// <summary>
        /// Gets the return type.
        /// </summary>
        /// <value>
        /// The return type.
        /// </value>
        ITypeSignature ReturnType { get; }

        /// <summary>
        /// Determines whether generic arguments are used in the current signature.
        /// </summary>
        bool ReferencesAnyGenericArgument();

        /// <summary>
        /// Resolves all generic arguments in the current method signature.
        /// </summary>
        /// <param name="genericMap">Generic context in which generic arguments have to be resolved.</param>
        /// <returns>A method signature resolved against <paramref name="genericMap"/>.</returns>
        IMethodSignature MapGenericArguments( GenericMap genericMap );

        /// <summary>
        /// Translates the current method signature so that it is meaningful in another
        /// module than the one to which it primarly belong.
        /// </summary>
        /// <param name="targetModule">Module into which the type signature should be
        /// translated.</param>
        /// <returns>A method signature meaningful in the <paramref name="targetModule"/>
        /// module.</returns>
        IMethodSignature Translate( ModuleDeclaration targetModule );

        /// <summary>
        /// Gets the number of generic parameters or arguments (i.e. the <i>arity</i>).
        /// </summary>
        int GenericParameterCount { get; }

        /// <summary>
        /// Determines whether the current method signature matches a given method signature reference.
        /// </summary>
        /// <param name="reference">The method signature reference.</param>
        /// <returns><b>true</b> if the current method signature matches <paramref name="reference"/>,
        /// otherwise <b>false</b>.</returns>
        /// <remarks>A method signature reference can use an incomplete assembly reference. Therefore, the matching
        /// is performed using <see cref="IAssembly.MatchesReference"/>.
        /// Matching an assembly reference is a looser requirement than matching an assembly name exactly;
        /// an assembly reference may set no requirement on the public key token or the version, for instance.</remarks>
        bool MatchesReference(IMethodSignature reference);
    }
}