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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Configures a task type.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class TaskTypeConfiguration : ConfigurationElement
    {
        #region Fields

        /// <summary>
        /// Task type name.
        /// </summary>
        private string name;

        /// <summary>
        /// Name of the type implementing the abstract <see cref="Task"/> type.
        /// </summary>
        private string implementation;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="TaskTypeConfiguration"/>.
        /// </summary>
        public TaskTypeConfiguration()
        {
        }


        /// <summary>
        /// Gets the name of the base to which tasks of this type belong.
        /// </summary>
        [XmlAttribute( "Phase" )]
        public string Phase { get; set; }


        /// <summary>
        /// Gets or sets the name of the XML element for this task type.
        /// </summary>
        [XmlAttribute( "Name" )]
        public string Name
        {
            get { return this.name; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotEmptyOrNull( value, "value" );

                #endregion

                this.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the type implementing the <see cref="Task"/>.
        /// </summary>
        [XmlAttribute( "Implementation" )]
        public string Implementation { get { return this.implementation; } set { this.implementation = value; } }

        /// <summary>
        /// Gets or sets the collection of dependencies of this
        /// task type.
        /// </summary>
        [XmlElement( "Dependency" )]
        public DependencyConfigurationCollection Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the collection of task type parameters.
        /// </summary>
        /// <remarks>
        /// Parameters can be used by the implementation of the <see cref="Task"/> type.
        /// </remarks>
        [XmlArray( "Parameters" )]
        [XmlArrayItem( "Parameter" )]
        public NameValuePairCollection Parameters { get; set; }

        /// <summary>
        /// Creates a empty task instance of the current task type.
        /// </summary>
        /// <param name="project">Actual configuration.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task CreateInstance( Project project )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( project, "project" );

            #endregion

            return CreateInstance( null, project );
        }

        /// <summary>
        /// Creates a task instance of the current task type and initializes
        /// it from an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xml">Source XML element, or <b>null</b> to create a non-initialized instance.</param>
        /// <param name="project">Context to which the new task shall belong.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
        internal Task CreateInstance( XmlElement xml, Project project )
        {
            Trace.ProjectLoader.WriteLine( "Instantiating the task {0}.", this.name );

            // Instantiate the task.
            string typeName;
            string assemblyName;
            AssemblyLoadHelper.SplitTypeName(this.implementation, out typeName, out assemblyName);
            AssemblyName assemblyStrongName;
            if ( project.StrongNames.TryGetValue(assemblyName, out assemblyStrongName))
            {
                assemblyName = assemblyStrongName.FullName;
            }

            Type type = AssemblyLoadHelper.LoadType( typeName + ", " + assemblyName );

            /*if ( type == null )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0013",
                                                  new object[] {this.name, this.Implementation}, this.Root.FileName );
                return null;
            }*/

            Task task = null;

            try
            {
                task = Activator.CreateInstance( type ) as Task;
            }
           
            catch ( Exception e )
            {
                TargetInvocationException tie = e as TargetInvocationException;
                if (tie != null) e = tie.InnerException;

                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0014", new object[]
                                                                                    {
                                                                                        this.name, this.Implementation,
                                                                                        e.GetType().Name, e.ToString()
                                                                                    }, this.Root.FileName );
            }

            // Initialize the task and add it to the collection,
            if ( task != null )
            {
                task.Initialize( this, xml, project );
            }
            else
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0049",
                                                  new object[] {this.name, type.FullName}, this.Root.FileName );
            }

            return task;
        }
    }

    /// <summary>
    /// Collection of task type configurations (<see cref="TaskTypeConfiguration"/>).
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class TaskTypeConfigurationCollection : Collection<TaskTypeConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="TaskTypeConfigurationCollection"/>.
        /// </summary>
        public TaskTypeConfigurationCollection()
        {
        }
    }


    /// <summary>
    /// Dictionary mapping the name of a <see cref="TaskTypeConfiguration"/> to its
    /// corresponding instance.
    /// </summary>
    [Serializable]
    public sealed class TaskTypeConfigurationDictionary : MarshalByRefDictionary<string, TaskTypeConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="TaskTypeConfigurationDictionary"/>.
        /// </summary>
        public TaskTypeConfigurationDictionary()
            : base( new Dictionary<string, TaskTypeConfiguration>( StringComparer.InvariantCultureIgnoreCase ) )
        {
        }
    }
}
