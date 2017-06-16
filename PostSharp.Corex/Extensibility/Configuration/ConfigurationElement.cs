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
    /// Base class for all configuration elements.
    /// </summary>
    /// <remarks>
    /// This class provides access to parent elements.
    /// </remarks>
    [Serializable]
    public abstract class ConfigurationElement
    {
        /// <summary>
        /// Parent element.
        /// </summary>
        private ConfigurationElement parent;

        /// <summary>
        /// Ininitializes a new <see cref="ConfigurationElement"/>.
        /// </summary>
        internal ConfigurationElement()
        {
        }

        /// <summary>
        /// Gets the parent configuration element.
        /// </summary>
        [XmlIgnore]
        public ConfigurationElement Parent { get { return this.parent; } internal set { this.parent = value; } }

        /// <summary>
        /// Gets the root configuration element.
        /// </summary>
        [XmlIgnore]
        public BaseConfiguration Root
        {
            get
            {
                BaseConfiguration root = this as BaseConfiguration;
                if ( root != null )
                {
                    return root;
                }
                else if ( this.parent == null )
                {
                    return null;
                }
                else
                {
                    return this.parent.Root;
                }
            }
        }


        /// <summary>
        /// Validates the current configuration element.
        /// </summary>
        /// <returns><b>true</b> if the current configuration element is valid,
        /// otherwise <b>false</b>.</returns>
        /// <remarks>
        /// This method should writer errors (and warnings) to the current <see cref="Messenger"/>.
        /// It should validate child configuration elements recursively.
        /// </remarks>
        public virtual bool Validate()
        {
            return true;
        }
    }
}