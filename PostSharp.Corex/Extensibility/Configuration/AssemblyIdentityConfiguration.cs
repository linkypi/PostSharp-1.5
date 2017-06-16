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
    /// Part of an <see cref="DependentAssemblyConfigurationCollection"/>.
    /// Identifies a set if assembly.
    /// </summary>
    [XmlType( AnonymousType = true )]
    [Serializable]
    public sealed class AssemblyIdentityConfiguration
    {
        /// <summary>
        /// Gets of sets short name of matching assemblies, or '*' to match all assemblies.
        /// </summary>
        [XmlAttribute( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public key token of matching assemblies.
        /// </summary>
        /// <remarks>
        /// If this property is <b>null</b>, any signed or unsigned assembly will match.
        /// </remarks>
        [XmlAttribute( "publicKeyToken" )]
        public string PublicKeyToken { get; set; }


        /// <summary>
        /// Gets or sets the culture name of the matching assembly.
        /// </summary>
        /// <remarks>
        /// If this property is <b>null</b>, assemblies of any culture will match.
        /// </remarks>
        [XmlAttribute( "culture" )]
        public string Culture { get; set; }
    }
}
