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
using System.Xml.Serialization;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Specifies a property assignment.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType=true )]
    public sealed class PropertyConfiguration : ConfigurationElement
    {
        /// <summary>
        /// Property name.
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new <see cref="PropertyConfiguration"/>.
        /// </summary>
        public PropertyConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>
        /// An expression.
        /// </value>
        [XmlAttribute( "Value" )]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the property name (primary key).
        /// </summary>
        [XmlAttribute( "Name" )]
        public string Name
        {
            get { return name; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotEmptyOrNull( value, "value" );

                #endregion

                name = value;
            }
        }
    }

    /// <summary>
    /// Collection of property definitions.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = false )]
    public sealed class PropertyConfigurationCollection : MarshalByRefList<PropertyConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="PropertyConfigurationCollection"/>.
        /// </summary>
        public PropertyConfigurationCollection()
        {
        }
    }
}
