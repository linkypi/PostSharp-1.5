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
    /// Configures an assembly redirection policy.
    /// </summary>
    [XmlType( AnonymousType = true)]
    [Serializable]
    public sealed class DependentAssemblyConfiguration
    {
        /// <summary>
        /// Gets or sets the element identifying the source of the redirection.
        /// </summary>
        [XmlElement( "assemblyIdentity", Namespace = ConfigurationHelper.AssemblyBindingNamespace )]
        public AssemblyIdentityConfiguration AssemblyIdentity { get; set; }

        /// <summary>
        /// Gets or sets the element identifying the target of the redirection.
        /// </summary>
        [XmlElement( "bindingRedirect", Namespace = ConfigurationHelper.AssemblyBindingNamespace )]
        public BindingRedirectConfiguration BindingRedirect { get; set; }
    }

    /// <summary>
    /// Collection of <see cref="DependentAssemblyConfiguration"/>.
    /// </summary>
    [XmlType( AnonymousType = true )]
    [Serializable]
    public sealed class DependentAssemblyConfigurationCollection : Collection<DependentAssemblyConfiguration>
    {
    }
}
