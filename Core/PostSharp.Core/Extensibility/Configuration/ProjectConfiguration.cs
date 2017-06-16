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
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Project-level configuration.
    /// </summary>
    [Serializable]
    [XmlRoot( "Project", Namespace = ConfigurationHelper.PostSharpNamespace )]
    public sealed class ProjectConfiguration : BaseConfiguration
    {
        #region Fields

        #endregion

        /// <summary>
        /// Initializes a new <see cref="ProjectConfiguration"/>.
        /// </summary>
        public ProjectConfiguration()
        {
        }


        /// <summary>
        /// Gets or sets the directory according to which relative paths are resolved. By default, 
        /// relative paths are resolved according to the directory that contains 
        ///	the project file. You may use the <c>{$WorkingDirectory}</c> property if
        /// you want to resolve paths according to the working directory.
        /// </summary>
        [XmlAttribute]
        public string ReferenceDirectory { get; set; }

        /// <summary>
        /// Gets or sets the serialized collection of tasks.
        /// </summary>
        [XmlAnyElement( "Tasks", Namespace = ConfigurationHelper.PostSharpNamespace )]
        [SuppressMessage( "Microsoft.Design", "CA1059" /*MembersShouldNotExposeCertainConcreteTypes*/ )]
        [SuppressMessage( "Microsoft.Usage", "CA2235" /*MarkAllNonSerializableFields*/ )]
        public XmlElement TasksElement { get; set; }


        /// <summary>
        /// Loads the project configuration from a file.
        /// </summary>
        /// <param name="fileName">Complete location of the project file.</param>
        /// <returns>A <see cref="ProjectConfiguration"/> resulting from the
        /// deserialization of the content of the file at <paramref name="fileName"/>.</returns>
        /// <remarks>
        /// Errors are logged to the <see cref="Messenger"/> attached to the current thread.
        /// </remarks>
        public static ProjectConfiguration LoadProject( string fileName )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( fileName, "fileName" );

            #endregion

            ProjectConfiguration projectConfiguration =
                (ProjectConfiguration)
                ConfigurationHelper.DeserializeDocument( fileName, ConfigurationDocumentKind.Project );
            projectConfiguration.FileName = fileName;

            return projectConfiguration;
        }
    }


    /// <summary>
    /// Collection of XML elements.
    /// </summary>
    public sealed class XmlElementCollection : Collection<XmlElement>
    {
        /// <summary>
        /// Intializes a new <see cref="XmlElementCollection"/>.
        /// </summary>
        public XmlElementCollection()
        {
        }
    }
}