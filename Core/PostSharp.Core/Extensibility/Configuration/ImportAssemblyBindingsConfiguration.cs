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
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Specifies that assembly redirection configuration should be
    /// imported from another XML file. Part of the <see cref="AssemblyBindingConfiguration"/>
    /// element.
    /// </summary>
    [XmlType( AnonymousType = true )]
    [Serializable]
    public sealed class ImportAssemblyBindingsConfiguration
    {
        /// <summary>
        /// Gets or sets the path of the imported XML file.
        /// </summary>
        [XmlAttribute( "File" )]
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the XPath expression evaluating to a node of type <see cref="AssemblyBindingExternalConfiguration"/>.
        /// </summary>
        [XmlAttribute( "Select" )]
        public string Select { get; set; }
    }

    /// <summary>
    /// Collection of <see cref="ImportAssemblyBindingsConfiguration"/>.
    /// </summary>
    [XmlType( AnonymousType = true )]
    [Serializable]
    public sealed class ImportAssemblyBindingsConfigurationCollection : Collection<ImportAssemblyBindingsConfiguration>
    {
    }
}
