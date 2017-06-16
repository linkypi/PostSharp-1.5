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
    /// Defines the semantics of an aspect that implement method without previous
    /// implementations, like <b>abstract</b> or <b>external</b> methods.
    /// </summary>
    public interface IImplementMethodAspect : ILaosMethodLevelAspect
    {
        /// <summary>
        /// Method called instead of the body of the modified method.
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which method is being
        /// executed, on which object instance and with which parameters.</param>
        /// <remarks>
        /// <para>
        /// The implementation is allowed to modify output arguments passed in the <paramref name="eventArgs"/>
        /// array, but it is required to respect argument types (otherwise an <see cref="InvalidCastException"/>
        /// will be thrown at runtime).
        /// </para>
        /// </remarks>
        void OnExecution( MethodExecutionEventArgs eventArgs );
    }

    /// <summary>
    /// Configuration of the <see cref="IImplementMethodAspect"/> aspect.
    /// </summary>
    /// <seealso cref="ImplementMethodAspectConfigurationAttribute"/>.
    public interface IImplementMethodAspectConfiguration : IInstanceBoundLaosAspectConfiguration
    {
        /// <summary>
        /// Get weaving options.
        /// </summary>
        /// <returns>Weaving options.</returns>
        ImplementMethodAspectOptions GetOptions();
    }

    /// <summary>
    /// Custom attribute that, when applied on a class implementing <see cref="IImplementMethodAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public sealed class ImplementMethodAspectConfigurationAttribute : InstanceBoundAspectConfigurationAttribute,
                                                                      IImplementMethodAspectConfiguration
    {
        private ImplementMethodAspectOptions options;

        /// <summary>
        /// Gets or sets the aspect options.
        /// </summary>
        public ImplementMethodAspectOptions Options
        {
            get { return this.options; }
            set { this.options = value; }
        }

        /// <inheritdoc />
        ImplementMethodAspectOptions IImplementMethodAspectConfiguration.GetOptions()
        {
            return this.options;
        }
    }

    /// <summary>
    /// Weaving options for the <see cref="IImplementMethodAspect"/> aspect.
    /// </summary>
    [Flags]
    public enum ImplementMethodAspectOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None
    }
}