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
using System.Reflection;
using System.Text;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the functionalities that are common to all representations
    /// of a method (<see cref="MethodDefDeclaration"/>, <see cref="MethodRefDeclaration"/>,
    /// <see cref="MethodSpecDeclaration"/>).
    /// </summary>
    public interface IMethod : IMethodSignature, IMember, IGeneric, IEquatable<IMethod>
    {
        /// <summary>
        /// Gets the system runtime method corresponding to the current method.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments valid in the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments valid in the current context.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The system runtime <see cref="MethodBase"/>, or <b>null</b> if
        /// the current method could not be bound.</returns>
        MethodBase GetSystemMethod( Type[] genericTypeArguments, Type[] genericMethodArguments,
                                    BindingOptions bindingOptions );

        /// <summary>
        /// Gets a reflection <see cref="MethodInfo"/> that wraps the current method.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>A <see cref="MethodInfo"/> wrapping current method in the
        /// given generic context.</returns>
        /// <remarks>
        /// This method returns a <see cref="MethodInfo"/> that is different from the system
        /// runtime method that is retrieved by <see cref="GetSystemMethod"/>. This allows
        /// a have a <b>System.Reflection</b> representation of the current method even
        /// when the declaring type it cannot be loaded in the Virtual Runtime Engine.
        /// </remarks>
        MethodBase GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <summary>
        /// Writes to a <see cref="StringBuilder"/> the full method signature as it would
        /// be output by <see cref="System.Reflection"/>. It tries more specifically to produce
        /// the same result as <b>MethodBase.ToString()</b>.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> to which the signature
        /// should be written.</param>
        /// <param name="options">Options.</param>
        [SuppressMessage( "Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters" )]
        void WriteReflectionMethodName( StringBuilder stringBuilder, ReflectionNameOptions options );

        /// <summary>
        /// Finds in the current domain the <see cref="MethodDefDeclaration"/> corresponding
        /// to the current method with default <see cref="BindingOptions"/>.
        /// </summary>
        /// <returns>The <see cref="MethodDefDeclaration"/> corresponding to the current 
        /// instance in the current domain.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        MethodDefDeclaration GetMethodDefinition();

        /// <summary>
        /// Finds in the current domain the <see cref="MethodDefDeclaration"/> corresponding
        /// to the current method and specifies <see cref="BindingOptions"/>.
        /// </summary>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="MethodDefDeclaration"/> corresponding to the current 
        /// instance in the current domain.</returns>
        MethodDefDeclaration GetMethodDefinition( BindingOptions bindingOptions );

        /// <summary>
        /// Gets the attributes of the method.
        /// </summary>
        MethodAttributes Attributes { get; }

         /// <summary>
        /// Gets the visibility of the current method.
        /// </summary>
        Visibility Visibility { get; }

        /// <summary>
        /// Determines whether the method is virtual.
        /// </summary>
        bool IsVirtual { get; }

        /// <summary>
        /// Determines whether the method is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Determines whether the method is sealed.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Determines whether the method takes a new slot.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Determines whether the current method matches a given method reference.
        /// </summary>
        /// <param name="reference">The method reference.</param>
        /// <returns><b>true</b> if the current method matches <paramref name="reference"/>,
        /// otherwise <b>false</b>.</returns>
        /// <remarks>A method reference can use an incomplete assembly reference. Therefore, the matching
        /// is performed using <see cref="IAssembly.MatchesReference"/>.
        /// Matching an assembly reference is a looser requirement than matching an assembly name exactly;
        /// an assembly reference may set no requirement on the public key token or the version, for instance.</remarks>
        bool MatchesReference(IMethod reference);
    }

    internal interface IMethodInternal : IMethod
    {
        /// <summary>
        /// Writes in IL a reference to the current instance.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The current <see cref="GenericMap"/>.</param>
        /// <param name="options">Options.</param>
        void WriteILReference( ILWriter writer, GenericMap genericMap, WriteMethodReferenceOptions options );
    }

    [Flags]
    internal enum WriteMethodReferenceOptions
    {
        None,
        Override = 1
    }
}