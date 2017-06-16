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
using System.Reflection;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of a field
    /// (<see cref="FieldDefDeclaration"/>, <see cref="FieldRefDeclaration"/>).
    /// </summary>
    public interface IField : IMember
    {
        /// <summary>
        /// Gets the generic context of the declaring type, or 
        /// an empty context if the member is contained by the module.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        GenericMap GetGenericContext( GenericContextOptions options );


        /// <summary>
        /// Gets the field type.
        /// </summary>
        /// <value>
        /// A <see cref="TypeSignature"/>.
        /// </value>
        ITypeSignature FieldType { get; }

        /// <summary>
        /// Gets the system runtime field corresponding to the current field.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments valid in the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments valid in the current context.</param>
        /// <returns>The system runtime <see cref="System.Reflection.FieldInfo"/>, or <b>null</b> if
        /// the current field could not be bound.</returns>
        FieldInfo GetSystemField( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <summary>
        /// Gets a reflection <see cref="FieldInfo"/> that wraps the current field.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>A <see cref="FieldInfo"/> wrapping current field in the
        /// given generic context.</returns>
        /// <remarks>
        /// This method returns a <see cref="FieldInfo"/> that is different from the system
        /// runtime field that is retrieved by <see cref="GetSystemField"/>. This allows
        /// a have a <b>System.Reflection</b> representation of the current field even
        /// when the declaring type it cannot be loaded in the Virtual Runtime Engine.
        /// </remarks>
         FieldInfo GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <summary>
        /// Finds in the current domain the <see cref="FieldDefDeclaration"/> corresponding
        /// to the current field with default <see cref="BindingOptions"/>.
        /// </summary>
        /// <returns>The <see cref="FieldDefDeclaration"/> corresponding to the current 
        /// instance in the current domain.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        FieldDefDeclaration GetFieldDefinition();

        /// <summary>
        /// Finds in the current domain the <see cref="FieldDefDeclaration"/> corresponding
        /// to the current field and specifies <see cref="BindingOptions"/>.
        /// </summary>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="FieldDefDeclaration"/> corresponding to the current 
        /// instance in the current domain.</returns>
        FieldDefDeclaration GetFieldDefinition( BindingOptions bindingOptions );

        /// <summary>
        /// Translates the current field so that it is meaningful in another
        /// module than the one to which it primarly belong.
        /// </summary>
        /// <param name="targetModule">Module into which the type signature should be
        /// translated.</param>
        /// <returns>A field meaningful in the <paramref name="targetModule"/>
        /// module.</returns>
        IField Translate( ModuleDeclaration targetModule );

        /// <summary>
        /// Gets the field attributes.
        /// </summary>
        FieldAttributes Attributes { get; }

        /// <summary>
        /// Determines whether the field is static.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Determines whether the field is read-only (i.e. init only).
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Determines whether the field is constant (i.e. literal).
        /// </summary>
        bool IsConst { get; }
    }

    internal interface IFieldInternal : IField
    {
        /// <summary>
        /// Writes in IL a reference to the current instance.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The current <see cref="GenericMap"/>.</param>
        void WriteILReference( ILWriter writer, GenericMap genericMap );
    }
}