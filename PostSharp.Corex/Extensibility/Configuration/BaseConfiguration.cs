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
using System.Xml;
using System.Xml.Serialization;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Elements of configuration that are common to the application-level,
    /// plugin-level and project-level configuration.
    /// </summary>
    [Serializable]
    [XmlType( "BaseConfiguration", Namespace=ConfigurationHelper.PostSharpNamespace )]
    public abstract class BaseConfiguration : ConfigurationElement
    {
        #region Fields

        #endregion

        /// <summary>
        /// Initializes a new <see cref="BaseConfiguration"/>.
        /// </summary>
        protected BaseConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the name of the file declaring the current configuration.
        /// </summary>
        [XmlIgnore]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the collection of directories to be added to the search path.
        /// </summary>
        /// <value>
        /// A collection of directories given either absolutely either relatively
        /// to the declaring file.
        /// </value>
        [XmlElement( "SearchPath" )]
        public SearchPathConfigurationCollection SearchPath { get; set; }

        /// <summary>
        /// Gets or sets the collection of plug-ins to be included in the project.
        /// </summary>
        /// <value>
        /// A collection of plug-in configuration file names given either absolutely
        /// either relatively to the declaring file.
        /// </value>
        [XmlElement( "Using" )]
        public UsingConfigurationCollection UsedPlugIns { get; set; }

        /// <summary>
        /// Gets or sets the collection of task types defined in the current file.
        /// </summary>
        /// <value>
        /// A collection of task types (<see cref="TaskTypeConfiguration"/>).
        /// </value>
        [XmlElement( "TaskType" )]
        public TaskTypeConfigurationCollection TaskTypes { get; set; }

        /// <summary>
        /// Gets or sets the collection of platforms defined in the current file.
        /// </summary>
        /// <value>
        /// A collection of platforms (<see cref="PlatformConfiguration"/>).
        /// </value>
        [XmlElement( "Platform" )]
        public PlatformConfigurationCollection Platforms { get; set; }


        /// <summary>
        /// Gets or sets the collection of properties defined in the current file.
        /// </summary>
        /// <value>
        /// A collection of properties.
        /// </value>
        [XmlElement( "Property" )]
        public PropertyConfigurationCollection Properties { get; set; }

        /// <summary>
        /// Gets or sets the section configuring assembly redirectio policies.
        /// </summary>
        [XmlElement( "AssemblyBinding" )]
        public AssemblyBindingConfiguration AssemblyBinding { get; set; }

        /// <summary>
        /// Gets or sets the collection of strong names.
        /// </summary>
        /// <remarks>
        /// Strong names allow to automatically map short assembly names on strong assembly names,
        /// when short assembly names are provided in a configuration file.
        /// </remarks>
        [XmlElement( "StrongName" )]
        public string[] StrongNames { get; set; }
    }

}
