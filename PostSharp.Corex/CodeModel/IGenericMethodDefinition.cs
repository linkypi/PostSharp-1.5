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
using PostSharp.CodeModel.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Declares the semantics of a generic method definition, i.e. a method
    /// that <i>may</i> have unbound generic parameter and method constructions
    /// (<see cref="MethodSpecDeclaration"/>).
    /// </summary>
    public interface IGenericMethodDefinition : IMethod, IGenericDefinition
    {
        /// <summary>
        /// Gets the collection of method specifications (<see cref="MethodSpecDeclaration"/>).
        /// </summary>
        MethodSpecDeclarationCollection MethodSpecs { get; }

        /// <summary>
        /// Finds or construct a specific generic instance of the current generic method definition.
        /// </summary>
        /// <param name="genericArguments">Generic arguments.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The generic instance, or <b>null</b> if it was not found and was not
        /// requested to be created.</returns>
        IMethod FindGenericInstance( IList<ITypeSignature> genericArguments, BindingOptions bindingOptions );
    }
}