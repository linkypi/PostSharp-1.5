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
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Options for the methods <see cref="ITypeSignature.WriteReflectionTypeName"/>
    /// and <see cref="IMethod.WriteReflectionMethodName"/>.
    /// </summary>
    [Flags]
    [SuppressMessage( "Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue" )]
    public enum ReflectionNameOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use assembly-qualified type names.
        /// </summary>
        UseAssemblyName = 1,

        /// <summary>
        /// Use square brackets (e.g. <c>[T]</c>) instead of angle brackets (e.g. <c>&lt;T&gt;</c>).
        /// By default, angle brackets are used.
        /// </summary>
        UseBracketsForGenerics = 2,

        /// <summary>
        /// Do not write generic parameters in type definitions. By default, the list of generic parameters
        /// is written.
        /// </summary>
        IgnoreGenericTypeDefParameters = 4,

        /// <summary>
        /// Default encoding rules. Masked by <see cref="EncodingMask"/>.
        /// </summary>
        NormalEncoding = 0,

        /// <summary>
        /// Encode using the rule that <see cref="System.Reflection"/> follows when it writes method parameters.
        /// Masked by <see cref="EncodingMask"/>.
        /// </summary>
        MethodParameterEncoding = 8,

        /// <summary>
        /// Encode using the rule that <see cref="System.Reflection"/> follows when it writes generic arguments.
        /// Masked by <see cref="EncodingMask"/>.
        /// </summary>
        GenericArgumentEncoding = 16,

        /// <summary>
        /// Masks the bits determining the encoding rules. Encoding rules are needed when trying to mimmic fidelly
        /// the behavior of <see cref="System.Reflection"/>.
        /// </summary>
        EncodingMask = MethodParameterEncoding | GenericArgumentEncoding,

        /// <summary>
        /// Write ordinals of generic parameters (.e.g. <c>!0</c>, <c>!1</c>, <c>!!0</c>, ...)
        /// instead of the name of generic parameters.
        /// </summary>
        UseOrdinalsForGenerics = 32,

        /// <summary>
        /// Do not write namespaces of types.
        /// </summary>
        SkipNamespace = 64,

        SerializedValue = 128,

        SerializedValueOptions = UseAssemblyName | UseBracketsForGenerics | SerializedValue
    }
}