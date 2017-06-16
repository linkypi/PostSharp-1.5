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
using System.IO;
using System.Xml.Serialization;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Determines the source module.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class SourceConfiguration : ConfigurationElement
    {
        /// <summary>
        /// Path of the source module.
        /// </summary>
        private string path;

        /// <summary>
        /// Initializes a new <see cref="SourceConfiguration"/>.
        /// </summary>
        public SourceConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the path of the source module.
        /// </summary>
        /// <value>
        /// An absolute file name, or a location relative to the declaring project
        /// file location.
        /// </value>
        [XmlAttribute( "SourceFile" )]
        public string SourceFile { get { return path; } set { path = value; } }

        /// <summary>
        /// Gets the full path of the current project file.
        /// </summary>
        [XmlIgnore]
        public string FullPath { get { return Path.Combine( Path.GetDirectoryName( this.Root.FileName ), this.path ); } }

        /// <inheritdoc />
        public override bool Validate()
        {
            if ( string.IsNullOrEmpty( this.path ) )
            {
                CoreMessageSource.Instance.Write( SeverityType.Error, "PS0039", null, this.Root.FileName );
                return false;
            }

            return true;
        }
    }
}
