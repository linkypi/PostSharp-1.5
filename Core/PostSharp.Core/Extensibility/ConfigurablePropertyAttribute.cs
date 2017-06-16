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

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Custom attribute that, then applied on a property of a class derived
    /// from <see cref="Task"/>, specifies that this property is configurable
    /// as an XML attribute in the XML project file.
    /// </summary>
    /// <remarks>
    /// Configurable properties may be of one of the following types: enumeration,
    /// <see cref="DateTime"/>, or any type implementing <see cref="IConvertible"/>.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Property )]
    public sealed class ConfigurablePropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="ConfigurablePropertyAttribute"/>, specifying
        /// that the property is not required.
        /// </summary>
        public ConfigurablePropertyAttribute()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ConfigurablePropertyAttribute"/>, and specifies
        /// whether the property is required or not.
        /// </summary>
        /// <param name="required"></param>
        public ConfigurablePropertyAttribute( bool required )
        {
            this.Required = required;
        }

        /// <summary>
        /// Determines whether the property is required.
        /// </summary>
        public bool Required { get; set; }
    }
}