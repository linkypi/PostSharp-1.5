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
    /// Includes a plug-in in the current project.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class UsingConfiguration : BaseConfiguration
    {
        /// <summary>
        /// Initializes a new <see cref="UsingConfiguration"/>.
        /// </summary>
        public UsingConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the path of the plug-in configuration file.
        /// </summary>
        [XmlAttribute( "PlugInFile" )]
        public string PlugInFile { get; set; }
    }

    /// <summary>
    /// Collection of <see cref="UsingConfiguration"/>.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class UsingConfigurationCollection : MarshalByRefList<UsingConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="UsingConfigurationCollection"/>.
        /// </summary>
        public UsingConfigurationCollection()
        {
        }
    }
}
