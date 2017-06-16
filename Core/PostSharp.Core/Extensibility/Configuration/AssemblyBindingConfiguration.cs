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
    /// Section containing the configuration of assembly redirection.
    /// </summary>
    [XmlType( AnonymousType = true )]
    [Serializable]
    public sealed class AssemblyBindingConfiguration
    {
        private readonly DependentAssemblyConfigurationCollection dependentAssemblies =
            new DependentAssemblyConfigurationCollection();

        private readonly ImportAssemblyBindingsConfigurationCollection importAssemblyBindings =
            new ImportAssemblyBindingsConfigurationCollection();

        /// <summary>
        /// Gets the collection of assembly redirections.
        /// </summary>
        [XmlElement( "dependentAssembly", Namespace = ConfigurationHelper.AssemblyBindingNamespace )]
        public DependentAssemblyConfigurationCollection DependentAssemblies { get { return dependentAssemblies; } }

        /// <summary>
        /// Gets the collection of imports of assembly direction configurations.
        /// </summary>
        [XmlElement( "Import" )]
        public ImportAssemblyBindingsConfigurationCollection ImportAssemblyBindings { get { return importAssemblyBindings; } }
    }
}
