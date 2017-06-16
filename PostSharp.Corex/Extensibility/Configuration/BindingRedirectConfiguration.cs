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
    /// Specifies the new target of an assembly. Part of a <see cref="DependentAssemblyConfiguration"/>
    /// configuration element.
    /// </summary>
    [XmlType( AnonymousType = true )]
    [Serializable]
    public sealed class BindingRedirectConfiguration
    {
        /// <summary>
        /// Gets or sets the version that the version of the redirected assembly.
        /// </summary>
        /// <remarks>
        /// <para>If this property is <b>null</b>, all versions are matches. Otherwise,
        /// the version can be given in format <c>0.0.0.0</c> or in format <c>0.0.0.0-0.0.0.0</c>.</para>
        /// </remarks>
        [XmlAttribute( "oldVersion" )]
        public string OldVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of the target assembly.
        /// </summary>
        /// <remarks>
        /// If this property is <b>null</b>, the new version will be equal to the old version.
        /// </remarks>
        [XmlAttribute( "newVersion" )]
        public string NewVersion { get; set; }

        /// <summary>
        /// Gets or sets the public key token of the target assembly.
        /// </summary>
        /// <remarks>
        /// If this property is <b>null</b>, the new public key token will be equal to the old one.
        /// </remarks>
        [XmlAttribute( "newPublicKeyToken" )]
        public string NewPublicKeyToken { get; set; }

        /// <summary>
        /// Gets or sets the name of the target assembly.
        /// </summary>
        /// <remarks>
        /// If this property is <b>null</b>, the new name will be equal to the old one.
        /// </remarks>
        [XmlAttribute( "newName" )]
        public string NewName { get; set; }
    }
}
