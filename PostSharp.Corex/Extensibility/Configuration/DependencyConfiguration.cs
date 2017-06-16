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
using System.ComponentModel;
using System.Xml.Serialization;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Represents a dependency between task types.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class DependencyConfiguration : ConfigurationElement
    {
        #region Fields

        /// <summary>
        /// Indicates whether this dependency is required.
        /// </summary>
        private bool required = true;

        /// <summary>
        /// Name of the task type to which the declaring type task
        /// is dependent.
        /// </summary>
        private string taskType;

        /// <summary>
        /// Position of the dependent task type (before or after
        /// the current task type).
        /// </summary>
        private DependencyPosition position = DependencyPosition.Default;

        /// <summary>
        /// Name of the task owning this dependency (filled at runtime).
        /// </summary>
        private string ownerTaskType;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="DependencyConfiguration"/>.
        /// </summary>
        public DependencyConfiguration()
        {
        }

        /// <summary>
        /// Gets the parent task type (<see cref="TaskTypeConfiguration"/>).
        /// </summary>
        [XmlIgnore]
        public new TaskTypeConfiguration Parent { get { return (TaskTypeConfiguration) base.Parent; } internal set { base.Parent = value; } }

        /// <summary>
        /// Indicates whether this dependency is required.
        /// </summary>
        [XmlAttribute( "Required" )]
        [DefaultValue( true )]
        public bool Required { get { return required; } set { required = value; } }

        /// <summary>
        /// Gets or sets the name of the task type to which the declaring type task
        /// is dependent.
        /// </summary>
        [XmlAttribute( "TaskType" )]
        public string TaskType { get { return taskType; } set { taskType = value; } }

        /// <summary>
        /// Gets or sets the position of the dependent task type (before or after
        /// the current task type).
        /// </summary>
        [XmlAttribute( "Position" )]
        [DefaultValue( DependencyPosition.Default )]
        public DependencyPosition Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Gets the name of the task owning the current dependency.
        /// </summary>
        [XmlIgnore]
        public string OwnerTaskType { get { return this.ownerTaskType; } internal set { this.ownerTaskType = value; } }


        [XmlIgnore]
        internal bool Processed { get; set; }

        /// <summary>
        /// Gets the name of the preceding task type in the current dependency.
        /// </summary>
        public string PrecedingTaskType { get { return this.position == DependencyPosition.After ? this.ownerTaskType : this.taskType; } }

        /// <summary>
        /// Gets the name of the succeeding task type in the current dependency.
        /// </summary>
        public string SucceedingTaskType { get { return this.position == DependencyPosition.Before ? this.ownerTaskType : this.taskType; } }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.PrecedingTaskType + " < " + this.SucceedingTaskType + ( this.Required ? " (required)" : "" );
        }
    }

    /// <summary>
    /// Collection of dependencies (<see cref="DependencyConfiguration"/>).
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class DependencyConfigurationCollection : MarshalByRefList<DependencyConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="DependencyConfigurationCollection"/>
        /// </summary>
        public DependencyConfigurationCollection()
        {
        }
    }

    /// <summary>
    /// Enumerates the relative position of dependencies (<see cref="DependencyPosition.Before"/> 
    /// or <see cref="DependencyPosition.After"/>).
    /// </summary>
    public enum DependencyPosition
    {
        /// <summary>
        /// Default = <see cref="Before"/>.
        /// </summary>
        [XmlEnum( "Default" )]
        Default = 0,

        /// <summary>
        /// Before.
        /// </summary>
        [XmlEnum( "Before" )]
        Before = 0,

        /// <summary>
        /// After.
        /// </summary>
        [XmlEnum( "After" )]
        After,
    }
}
