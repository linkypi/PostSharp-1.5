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

#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml;
using PostSharp.Collections;
using PostSharp.Extensibility.Configuration;

#endregion

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Execution unit of a <see cref="Project"/>.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Serializable]
    public abstract class Task : MarshalByRefObject, IDisposable, IPositioned, INamed
    {
        #region Fields

        /// <summary>
        /// Name of the type task in the project.
        /// </summary>
        private TaskTypeConfiguration taskType;

        /// <summary>
        /// Project context.
        /// </summary>
        private Project project;


        private int ordinal = Int32.MaxValue;

        private readonly Set<DependencyConfiguration> successors = new Set<DependencyConfiguration>();

        private readonly Set<DependencyConfiguration> predecessors = new Set<DependencyConfiguration>();

        private bool disposed;

        #endregion

        /// <summary>
        /// Initializes the instance (called just after the
        /// default constructor).
        /// </summary>
        /// <param name="projectContext">Context.</param>
        /// <param name="taskType">Task type configuration.</param>
        /// <param name="xmlElement">Serialized task.</param>
        internal void Initialize( TaskTypeConfiguration taskType, XmlElement xmlElement,
                                  Project projectContext )
        {
            this.taskType = taskType;
            this.project = projectContext;

            if ( xmlElement != null )
            {
                this.Deserialize( xmlElement );
            }


            this.Initialize();
        }

        /// <summary>
        /// Initializes the current task.
        /// </summary>
        /// <remarks>
        /// In case that the task is deserialized from XML, this method is called after <see cref="Deserialize"/>.
        /// </remarks>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Gets the task type configuration.
        /// </summary>
        [Browsable( false )]
        public TaskTypeConfiguration TaskType
        {
            get { return this.taskType; }
        }

        /// <summary>
        /// Gets the task name.
        /// </summary>
        [Browsable( false )]
        public string TaskName
        {
            get { return this.taskType.Name; }
        }

        string INamed.Name
        {
            get { return this.TaskName; }
        }

        /// <summary>
        /// Deserializes the current task from an XML element.
        /// </summary>
        /// <param name="xmlElement">The source XML element.</param>
        [SuppressMessage( "Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes" )]
        [SuppressMessage( "Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength" )]
        public bool Deserialize( XmlElement xmlElement )
        {
            bool hasError = false;

            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( xmlElement, "xmlElement" );

            #endregion

            // Build a list of configurable properties.
            Dictionary<string, ConfigurablePropertyInfo> configurableProperties =
                new Dictionary<string, ConfigurablePropertyInfo>( StringComparer.InvariantCultureIgnoreCase );
            foreach ( PropertyInfo propertyInfo in this.GetType().GetProperties() )
            {
                object[] customAttributes =
                    propertyInfo.GetCustomAttributes( typeof(ConfigurablePropertyAttribute), false );
                if ( customAttributes.Length > 0 )
                {
                    ConfigurablePropertyInfo configurablePropertyInfo = new ConfigurablePropertyInfo
                                                                            {
                                                                                Property = propertyInfo,
                                                                                ConfigurablePropertyAttribute =
                                                                                    ( (ConfigurablePropertyAttribute)
                                                                                      customAttributes[0] )
                                                                            };
                    configurableProperties.Add( propertyInfo.Name, configurablePropertyInfo );
                }
            }

            foreach ( XmlAttribute attribute in xmlElement.Attributes )
            {
                if ( attribute.Value.Trim().Length == 0 )
                    continue;

                ConfigurablePropertyInfo configurablePropertyInfo;
                if ( configurableProperties.TryGetValue( attribute.LocalName, out configurablePropertyInfo ) )
                {
                    if ( configurablePropertyInfo.Property.PropertyType.IsEnum )
                    {
                        object value;
                        if ( this.TryParseEnum( attribute, configurablePropertyInfo.Property.PropertyType, out value ) )
                        {
                            configurablePropertyInfo.Property.SetValue( this, value, null );
                        }
                        else
                        {
                            hasError = true;
                        }
                    }
                    else if ( configurablePropertyInfo.Property.PropertyType == typeof(bool) )
                    {
                        bool value;
                        if ( this.TryParseBoolean( attribute, out value ) )
                        {
                            configurablePropertyInfo.Property.SetValue( this, value, null );
                        }
                        else
                        {
                            hasError = true;
                        }
                    }
                    else if ( configurablePropertyInfo.Property.PropertyType == typeof(DateTime) )
                    {
                        DateTime value;
                        if ( this.TryParseDateTime( attribute, out value ) )
                        {
                            configurablePropertyInfo.Property.SetValue( this, value, null );
                        }
                        else
                        {
                            hasError = true;
                        }
                    }
                    else if ( typeof(IConvertible).IsAssignableFrom( configurablePropertyInfo.Property.PropertyType ) )
                    {
                        object value;
                        if ( this.TryParseConvertible( attribute, configurablePropertyInfo.Property.PropertyType,
                                                       out value ) )
                        {
                            configurablePropertyInfo.Property.SetValue( this, value, null );
                        }
                        else
                        {
                            hasError = true;
                        }
                    }
                    else
                    {
                        throw ExceptionHelper.Core.CreateInvalidOperationException( "UnsupportedTaskPropertyType",
                                                                                    configurablePropertyInfo.Property.
                                                                                        PropertyType.FullName
                            );
                    }

                    configurablePropertyInfo.Present = true;
                }
                else
                {
                    CoreMessageSource.Instance.Write( SeverityType.Warning, "PS0021", new object[]
                                                                                          {
                                                                                              attribute.Name,
                                                                                              this.TaskName
                                                                                          } );
                }
            }

            // Check all required properties were provided.
            foreach ( ConfigurablePropertyInfo configurableProperty in configurableProperties.Values )
            {
                if ( configurableProperty.ConfigurablePropertyAttribute.Required &&
                     !configurableProperty.Present )
                {
                    CoreMessageSource.Instance.Write( SeverityType.Error, "PS0082", new object[]
                                                                                        {
                                                                                            this.TaskName,
                                                                                            configurableProperty.Property.
                                                                                                Name
                                                                                        } );
                    hasError = true;
                }
            }

            if ( !hasError )
            {
                return this.Validate();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the task configuration (deserialized from the attributes of the
        /// XML element representing the task instance) is valid.
        /// </summary>
        /// <returns><b>true</b> if the task configuration is valid, otherwise <b>false</b>.</returns>
        /// <remarks>
        /// If the task configuration is invalid, the method implementation should additionally
        /// emit an error message.
        /// </remarks>
        protected virtual bool Validate()
        {
            return true;
        }

        private bool TryParseDateTime( XmlAttribute xmlAttribute, out DateTime value )
        {
            string attributeValue = this.project.Evaluate( xmlAttribute.Value );

            if ( attributeValue == null )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0077",
                                                  new object[]
                                                      {this.TaskName, xmlAttribute.Value, xmlAttribute.LocalName},
                                                  xmlAttribute.OwnerDocument.BaseURI );
                value = DateTime.MinValue;
                return false;
            }


            try
            {
                value = XmlConvert.ToDateTime( attributeValue, XmlDateTimeSerializationMode.RoundtripKind );
            }
            catch ( ArgumentException )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0083",
                                                  new object[]
                                                      {
                                                          this.TaskName, attributeValue, xmlAttribute.LocalName
                                                      },
                                                  xmlAttribute.OwnerDocument.BaseURI );
                value = DateTime.MinValue;
                return false;
            }

            return true;
        }


        private bool TryParseBoolean( XmlAttribute xmlAttribute, out bool value )
        {
            string attributeValue = this.project.Evaluate( xmlAttribute.Value );

            if ( attributeValue == null )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0077",
                                                  new object[]
                                                      {this.TaskName, xmlAttribute.Value, xmlAttribute.LocalName},
                                                  xmlAttribute.OwnerDocument.BaseURI );
                value = false;
                return false;
            }

            switch ( attributeValue.ToLowerInvariant() )
            {
                case "false":
                    value = false;
                    return true;

                case "true":
                    value = true;
                    return true;

                default:
                    CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0017",
                                                      new object[]
                                                          {
                                                              this.TaskName, attributeValue, xmlAttribute.LocalName,
                                                              "True | False"
                                                          },
                                                      xmlAttribute.OwnerDocument.BaseURI );
                    value = false;
                    return false;
            }
        }

        private bool TryParseEnum( XmlAttribute xmlAttribute, Type enumType, out object value )
        {
            string attributeValue = this.project.Evaluate( xmlAttribute.Value );

            if ( attributeValue == null )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0077",
                                                  new object[]
                                                      {this.TaskName, xmlAttribute.Value, xmlAttribute.LocalName},
                                                  xmlAttribute.OwnerDocument.BaseURI );
                value = null;
                return false;
            }

            try
            {
                value = Enum.Parse( enumType, attributeValue, true );
            }
            catch ( ArgumentException )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0017",
                                                  new object[]
                                                      {
                                                          this.TaskName, attributeValue, xmlAttribute.LocalName,
                                                          String.Join( " | ", Enum.GetNames( enumType ) )
                                                      },
                                                  xmlAttribute.OwnerDocument.BaseURI );
                value = false;
                return false;
            }

            return true;
        }

        private bool TryParseConvertible( XmlAttribute xmlAttribute, Type type, out object value )
        {
            string attributeValue = this.project.Evaluate( xmlAttribute.Value );

            if ( attributeValue == null )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0077",
                                                  new object[]
                                                      {this.TaskName, xmlAttribute.Value, xmlAttribute.LocalName},
                                                  xmlAttribute.OwnerDocument.BaseURI );
                value = null;
                return false;
            }

            try
            {
                value = Convert.ChangeType( attributeValue.Trim(), type );
            }
            catch ( ArgumentException )
            {
                // Task "{0}", attribute "{1}": cannot convert "{2}" to a(n) {3}.
                CoreMessageSource.Instance.Write( SeverityType.Error, "PS0081",
                                                  new object[]
                                                      {
                                                          this.TaskName, xmlAttribute.LocalName, attributeValue,
                                                          type.Name
                                                      } );


                value = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this task is disabled.
        /// </summary>
        [Browsable( false )]
        [ConfigurableProperty]
        public bool Disabled { get; set; }

        /// <summary>
        /// Indicates that this task was implicitely required
        /// by another task.
        /// </summary>
        [Browsable( false )]
        public bool ImplicitelyRequired { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Project"/> to which the current task belongs.
        /// </summary>
        [Browsable( false )]
        public Project Project
        {
            get { return this.project; }
        }

        /// <summary>
        /// Execute the current <see cref="Task"/>.
        /// </summary>
        /// <returns><b>true</b> if the exectution was successful, otherwise <b>false</b>.</returns>
        public virtual bool Execute()
        {
            return true;
        }

        /// <summary>
        /// Gets the collection of successors of this task.
        /// </summary>
        internal Set<DependencyConfiguration> Successors
        {
            get { return this.successors; }
        }

        /// <summary>
        /// Gets the collection of predecessors of this task.
        /// </summary>
        internal Set<DependencyConfiguration> Predecessors
        {
            get { return this.predecessors; }
        }

        /// <summary>
        /// Gets the ordinal of the current task in the project.
        /// </summary>
        /// <remarks>
        /// A positive integer, or <see cref="int.MaxValue"/> if the current task has not
        /// yet been ordered.
        /// </remarks>
        internal int Ordinal
        {
            get { return this.ordinal; }
            set
            {
                if ( this.ordinal != value )
                {
                    int oldValue = this.ordinal;
                    this.ordinal = value;
                    if ( this.Parent != null )
                    {
                        this.Parent.OnItemOrdinalChanged( this, oldValue, value );
                    }
                }
            }
        }

        int IPositioned.Ordinal
        {
            get { return this.ordinal; }
        }


        /// <summary>
        /// Gets the state of the current task.
        /// </summary>
        [Browsable( false )]
        public TaskState State { get; internal set; }

        internal TaskCollection Parent { get; set; }

        /// <summary>
        /// Determines whether the current task is required by another task in the project.
        /// </summary>
        /// <returns><b>true</b> if the current task is required by another task
        /// in the project, otherwise <b>false</b>.</returns>
        public bool IsRequired()
        {
            foreach ( DependencyConfiguration predecessor in this.predecessors )
            {
                if ( predecessor.Required )
                {
                    return true;
                }
            }

            foreach ( DependencyConfiguration successor in this.successors )
            {
                if ( successor.Required )
                {
                    return true;
                }
            }

            return false;
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the resources hold by the current instance.
        /// </summary>
        /// <param name="disposing"><b>disposing</b> if the current method is called because an
        /// explicit call of <see cref="Dispose()"/>, or <b>false</b> if the method
        /// is called by the destructor.</param>
        protected virtual void Dispose( bool disposing )
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( !this.disposed )
            {
                this.disposed = true;
                this.Dispose( true );
                GC.SuppressFinalize( this );
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Task()
        {
            if ( !this.disposed )
            {
                this.disposed = true;
                this.Dispose( false );
            }
        }

        #endregion

        private class ConfigurablePropertyInfo
        {
            public bool Present;
            public PropertyInfo Property;
            public ConfigurablePropertyAttribute ConfigurablePropertyAttribute;
        }
    }

    /// <summary>
    /// States of a <see cref="Task"/>.
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// Queued. The task has not been executed.
        /// </summary>
        Queued,

        /// <summary>
        /// Default = <see cref="Queued"/>.
        /// </summary>
        Default = Queued,

        /// <summary>
        /// The task is being executed now.
        /// </summary>
        Executing,

        /// <summary>
        /// The task has already been executed.
        /// </summary>
        Executed
    }
}