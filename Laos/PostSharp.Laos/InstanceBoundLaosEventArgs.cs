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

using System.Diagnostics;

namespace PostSharp.Laos
{
    /// <summary>
    /// Arguments of events bound to an instance (because they
    /// relate to a field or to a method, for instance).
    /// </summary>
    public class InstanceBoundLaosEventArgs : LaosEventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="InstanceBoundLaosEventArgs"/>
        /// </summary>
        /// <param name="instance">Object instance, or <b>null</b>
        /// if the event relates to a static field or method.</param>
        [DebuggerNonUserCode]
        public InstanceBoundLaosEventArgs( object instance )
        {
            this.Instance = instance;
        }

        /// <summary>
        /// Gets or sets the object instance on which the method is being executed.
        /// </summary>
        /// <remarks>
        /// This set may be set by user code only when the instance is a value type.
        /// As usually, user code is responsible for setting an object of the
        /// right type.
        /// </remarks>
        [DebuggerNonUserCode]
        public object Instance { get; set; }

        /// <summary>
        /// Gets or sets the instance tag.
        /// </summary>
        /// <remarks>
        /// <para>Not applicable (and null) if the event does relates to a static
        /// field or method.
        /// </para>
        /// <para>
        /// The instance tag is a location where aspects can store custom
        /// informations (tags). This location is instance-dependent. It is
        /// implemented as an instance field.
        /// </para>
        /// </remarks>
        [DebuggerNonUserCode]
        public object InstanceTag { get; set; }

        /// <summary>
        /// Gets the credentials of the current instance.
        /// </summary>
        /// <remarks>
        /// Credentials allow to use protected interfaces like <see cref="IComposed{T}"/>.
        /// </remarks>
        [DebuggerNonUserCode]
        public InstanceCredentials InstanceCredentials { get; set; }
    }
}