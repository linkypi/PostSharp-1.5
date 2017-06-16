#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

using System;

namespace PostSharp.Laos
{
    /// <summary>
    /// Configuration of aspects that are bound to an object instance,
    /// for instance to a field or to a method. Note that if the aspect is 
    /// applied to a static field or method, methods of this interface are
    /// never invoked.
    /// </summary>
    /// <seealso cref="InstanceBoundAspectConfigurationAttribute"/>
    public interface IInstanceBoundLaosAspectConfiguration : ILaosWeavableAspectConfiguration
    {
        /// <summary>
        /// Gets the <i>Instance Tag</i> required by this aspect.
        /// </summary>
        /// <returns>An <see cref="InstanceTagRequest"/>, or <b>null</b> if
        /// no instance tag is requested by this aspect.</returns>
        InstanceTagRequest GetInstanceTagRequest();
    }

    /// <summary>
    /// Request of an instance tag. An instance tag is an instance field
    /// where aspects can store and share information.
    /// </summary>
    /// <remarks>
    /// By requesting the same instance tag, many aspects can share the
    /// same instance field, so they can share information.
    /// </remarks>
    /// <seealso cref="IInstanceBoundLaosAspectConfiguration"/>
    /// <seealso cref="InstanceBoundAspectConfigurationAttribute"/>
    public sealed class InstanceTagRequest
    {
        private readonly string preferredName;
        private readonly Guid guid;
        private readonly bool forceStatic;

        /// <summary>
        /// Initializes a new <see cref="InstanceTagRequest"/>.
        /// </summary>
        /// <param name="preferredName">Preferred short name of the instance field.</param>
        /// <param name="guid">Unique field identifier.</param>
        public InstanceTagRequest( string preferredName, Guid guid ) : this( preferredName, guid, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="InstanceTagRequest"/>, and specifies whether the
        /// instance tag should be forced to be static.
        /// </summary>
        /// <param name="preferredName">Preferred short name of the instance field.</param>
        /// <param name="guid">Unique field identifier.</param>
        /// <param name="forceStatic"><b>true</b> if the instance tag is forced to be
        /// static (i.e. will be shared between all instances of the target type), 
        /// or <b>false</b> otherwise.</param>
        public InstanceTagRequest( string preferredName, Guid guid, bool forceStatic )
        {
            #region Preconditions

            if ( preferredName == null )
            {
                throw new ArgumentNullException( "preferredName" );
            }

            #endregion

            this.preferredName = preferredName;
            this.guid = guid;
            this.forceStatic = forceStatic;
        }

        /// <summary>
        /// Gets the preferred short name of the instance field.
        /// </summary>
        public string PreferredName
        {
            get { return preferredName; }
        }

        /// <summary>
        /// Gets the unique identifier of the requested field.
        /// </summary>
        public Guid Guid
        {
            get { return guid; }
        }

        /// <summary>
        /// Determines whether the instance tag should be static, i.e. shared
        /// between all instances of the same target type.
        /// </summary>
        public bool ForceStatic
        {
            get { return this.forceStatic; }
        }
    }

    /// <summary>
    /// Base class of a custom attribute that, when applied on a class implementing an instance-bound aspect
    /// (see<see cref="IInstanceBoundLaosAspectConfiguration"/>), defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public abstract class InstanceBoundAspectConfigurationAttribute : LaosWeavableAspectConfigurationAttribute,
                                                                      IInstanceBoundLaosAspectConfiguration
    {
        private string instanceTagPreferredName;
        private string instanceTagGuid;
        private bool instanceTagForceStatic;

        /// <summary>
        /// Gets or sets the preferred short name of the instance field.
        /// </summary>
        public string InstanceTagPreferredName
        {
            get { return this.instanceTagPreferredName; }
            set { this.instanceTagPreferredName = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier of the requested field.
        /// </summary>
        public string InstanceTagGuid
        {
            get { return this.instanceTagGuid; }
            set { this.instanceTagGuid = value; }
        }

        /// <summary>
        /// Determines whether the instance tag should be static, i.e. shared
        /// between all instances of the same target type.
        /// </summary>
        public bool InstanceTagForceStatic
        {
            get { return this.instanceTagForceStatic; }
            set { this.instanceTagForceStatic = value; }
        }

        /// <inheritdoc />
        public InstanceTagRequest GetInstanceTagRequest()
        {
            return !string.IsNullOrEmpty( this.instanceTagGuid )
                       ? new InstanceTagRequest( this.instanceTagPreferredName, new Guid( this.instanceTagGuid ),
                                                 instanceTagForceStatic )
                       : null;
        }
    }
}