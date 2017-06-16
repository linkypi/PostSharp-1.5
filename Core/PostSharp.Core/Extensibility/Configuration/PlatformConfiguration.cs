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
using System.Collections.Generic;
using System.Xml.Serialization;
using PostSharp.Collections;
using PostSharp.PlatformAbstraction;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Configures a target platform.
    /// </summary>
    /// <remarks>
    /// A target platform is primarly defined by an implementation class,
    /// and optionally by some parameters.
    /// </remarks>
    [Serializable]
    [XmlType( AnonymousType=true )]
    public sealed class PlatformConfiguration : ConfigurationElement
    {
        #region Fields

        #endregion

        /// <summary>
        /// Initializes a new <see cref="PlatformConfiguration"/>.
        /// </summary>
        public PlatformConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the full name of the type implementing <see cref="TargetPlatformAdapter"/>.
        /// </summary>
        [XmlAttribute( "Implementation" )]
        public string Implementation { get; set; }

        /// <summary>
        /// Gets or sets the platform name (primary identifier).
        /// </summary>
        [XmlAttribute( "Name" )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of platform parameters.
        /// </summary>
        /// <remarks>
        /// Parameters are passed to the implementation of <see cref="TargetPlatformAdapter"/>
        /// during instantiation.
        /// </remarks>
        [XmlArray( "Parameters" )]
        [XmlArrayItem( "Parameter" )]
        public NameValuePairCollection Parameters { get; set; }
    }

    /// <summary>
    /// Collection of platform configurations (<see cref="PlatformConfiguration"/>).
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class PlatformConfigurationCollection : MarshalByRefList<PlatformConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="PlatformConfigurationCollection"/>.
        /// </summary>
        public PlatformConfigurationCollection()
        {
        }
    }


    /// <summary>
    /// Dictionary of platform configurations (<see cref="PlatformConfiguration"/>) by name.
    /// </summary>
    [Serializable]
    public sealed class PlatformConfigurationDictionary : MarshalByRefDictionary<string, PlatformConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="PlatformConfigurationDictionary"/>.
        /// </summary>
        public PlatformConfigurationDictionary() :
            base( new Dictionary<string, PlatformConfiguration>( StringComparer.InvariantCultureIgnoreCase ) )
        {
        }
    }
}
