#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Custom attribute meaning that custom attributes of a given type are
    /// bound to the implementation, not to the semantics.
    /// </summary>
    /// <remarks>
    /// <para>This custom attribute influences whether instances of a given other
    /// custom attribute should be moved from the semantic to the implementation,
    /// when the semantic is detached from the implementation. This happens for instance
    /// when a property is generated from a field; the property become the semantic
    /// (and is made public) and the field the implementation (and is made private).
    /// Most custom attributes apply to semantics, so they are moved from the field
    /// to the property. If a custom attribute must not be moved, it should be
    /// marked with the <see cref="ImplementationBoundAttributeAttribute"/>
    /// custom attribute.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ImplementationBoundAttributeAttribute : Attribute
    {
        private readonly Type attributeType;

        /// <summary>
        /// Initializes the new <see cref="ImplementationBoundAttributeAttribute"/>.
        /// </summary>
        /// <param name="attributeType">Type of the custom attribute that
        /// should not be moved from implementation to semantic.</param>
        public ImplementationBoundAttributeAttribute(Type attributeType) 
        {
            this.attributeType = attributeType;
        }

        /// <summary>
        /// Gets the type of the custom attribute that
        /// should not be moved from implementation to semantic
        /// </summary>
        public Type AttributeType { get { return attributeType; } }

    }
}

