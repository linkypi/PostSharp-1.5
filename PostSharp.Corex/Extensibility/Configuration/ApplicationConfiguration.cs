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

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Application-level configuration.
    /// </summary>
    [XmlRoot( ElementName="Configuration", Namespace=ConfigurationHelper.PostSharpNamespace )]
    [Serializable]
    public sealed class ApplicationConfiguration : BaseConfiguration
    {
        /// <summary>
        /// Gets or sets the collection of phases (<see cref="PhaseConfiguration"/>) in the post-compilation process.
        /// </summary>
        [XmlArray( ElementName = "Phases" )]
        [XmlArrayItem( ElementName = "Phase" )]
        public PhaseConfigurationCollection Phases { get; set; }


        /// <summary>
        /// Initializes a new <see cref="ApplicationConfiguration"/>.
        /// </summary>
        public ApplicationConfiguration()
        {
        }
    }
}