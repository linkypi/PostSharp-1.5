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
    /// Simple name-value pair of strings.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType=true )]
    public sealed class NameValuePair : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new <see cref="NameValuePair"/>.
        /// </summary>
        public NameValuePair()
        {
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute( "Value" )]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute( "Name" )]
        public string Name { get; set; }
    }


    /// <summary>
    /// Collection of name-value pairs (<see cref="NameValuePair"/>).
    /// </summary>
    [Serializable]
    [XmlType( "nameValuePairCollection", Namespace = ConfigurationHelper.PostSharpNamespace )]
    public sealed class NameValuePairCollection : MarshalByRefList<NameValuePair>
    {
        /// <summary>
        /// Initializes a new <see cref="NameValuePairCollection"/>.
        /// </summary>
        public NameValuePairCollection()
        {
        }
    }
}
