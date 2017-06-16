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
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Collection of properties.
    /// </summary>
    [Serializable]
    [SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
    public sealed class PropertyCollection
    {
        private readonly Dictionary<string, string> dictionary =
            new Dictionary<string, string>( 20, StringComparer.InvariantCultureIgnoreCase );


        /// <summary>
        /// Gets or sets a property.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <returns>The property value, or <b>null</b> if the property is not defined.</returns>
        public string this[ string key ]
        {
            get
            {
                string value;
                this.dictionary.TryGetValue( key, out value );
                return value;
            }

            set { this.dictionary[key] = value; }
        }

        /// <summary>
        /// Merge another collection of properties into the current collection.
        /// </summary>
        /// <param name="properties">Another collection of properties.</param>
        public void Merge( PropertyCollection properties )
        {
            if ( properties != null )
            {
                foreach ( KeyValuePair<string, string> pair in properties.dictionary )
                {
                    this.dictionary[pair.Key] = pair.Value;
                }
            }
        }
    }
}