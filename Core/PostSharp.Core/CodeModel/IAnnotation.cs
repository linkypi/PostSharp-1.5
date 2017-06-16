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

using PostSharp.CodeModel.Collections;
using PostSharp.Reflection;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of a custom attribute value,
    /// i.e. basically a constructor, its arguments and other named arguments.
    /// </summary>
    public interface IAnnotationValue : IObjectConstruction
    {
        /// <summary>
        /// Gets the custom attribute constructor.
        /// </summary>
        IMethod Constructor { get; }

        /// <summary>
        /// Gets the set of named arguments.
        /// </summary>
        MemberValuePairCollection NamedArguments { get; }

        /// <summary>
        /// Gets the set of constructor arguments.
        /// </summary>
        MemberValuePairCollection ConstructorArguments { get; }

        /// <summary>
        /// Translates the current annotation so that it is valid in another module.
        /// </summary>
        /// <param name="module">The module into which the current <see cref="IAnnotationValue"/>
        /// should be translated.</param>
        /// <returns>An <see cref="IAnnotationValue"/> valid in <paramref name="module"/>.</returns>
        IAnnotationValue Translate( ModuleDeclaration module );
    }

    /// <summary>
    /// Exposes the fact that a custom attribute (<see cref="IAnnotationValue"/>)
    /// is applied to an element (<see cref="MetadataDeclaration"/>).
    /// </summary>
    public interface IAnnotationInstance
    {
        /// <summary>
        /// Custom attribute value.
        /// </summary>
        IAnnotationValue Value { get; }

        /// <summary>
        /// Element on which the custom attribute value is applied.
        /// </summary>
        MetadataDeclaration TargetElement { get; }
    }
}