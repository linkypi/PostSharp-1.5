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
    /// Specifies that a directory should be included in the search path.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class SearchPathConfiguration
    {
        /// <summary>
        /// Initializes a new <see cref="SearchPathConfiguration"/>.
        /// </summary>
        public SearchPathConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the directory to be included in the search path.
        /// </summary>
        [XmlAttribute( "Directory" )]
        public string Directory { get; set; }

        /// <summary>
        /// Gets or sets the file to be included in the search path.
        /// </summary>
        [XmlAttribute( "File" )]
        public string File { get; set; }
    }

    /// <summary>
    /// Collection of <see cref="SearchPathConfiguration"/>.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class SearchPathConfigurationCollection : MarshalByRefList<SearchPathConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="SearchPathConfigurationCollection"/>.
        /// </summary>
        public SearchPathConfigurationCollection()
        {
        }
    }
}
