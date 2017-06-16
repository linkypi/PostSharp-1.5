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

#if !CF
using System;
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Defines the semantics of an aspect that, when applied on a method, intercepts
    /// all calls to this method.
    /// </summary>
    public interface IOnMethodInvocationAspect : ILaosMethodLevelAspect
    {
        /// <summary>
        /// Method called instead of the intercepted method.
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which method
        /// is being executed and which are its arguments. The implementation
        /// should set the return value and ouput arguments.</param>
        void OnInvocation( MethodInvocationEventArgs eventArgs );
    }

    /// <summary>
    /// Configuration of the <see cref="IOnMethodInvocationAspect"/> aspect.
    /// </summary>
    /// <seealso cref="OnMethodInvocationAspectConfigurationAttribute"/>
    public interface IOnMethodInvocationAspectConfiguration : IInstanceBoundLaosAspectConfiguration
    {
        /// <summary>
        /// Gets the weaving options.
        /// </summary>
        /// <returns>Weaving options.</returns>
        [CompileTimeSemantic]
        OnMethodInvocationAspectOptions GetOptions();
    }

    /// <summary>
    /// Weaving options of the <see cref="IOnMethodInvocationAspect"/> aspect.
    /// </summary>
    [Flags]
    public enum OnMethodInvocationAspectOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,


        /// <summary>
        /// Specifies that the aspect should be woven at <i>call</i> site, i.e. <c>call</c>
        /// instructions are to be replaced by calls to our stub. Allows to apply
        /// aspects to methods defined out of the current assembly.
        /// </summary>
        WeaveSiteCall = 2,

        /// <summary>
        /// Specifies that the aspect should be woven at <i>target</i> site, i.e. in
        /// the method to which the aspect is applied.
        /// </summary>
        WeaveSiteTarget = 1,

        /// <summary>
        /// Chooses automatically <see cref="WeaveSiteCall"/> 
        /// when the target method is defined outside the current assembly, or
        /// <see cref="WeaveSiteTarget"/> otherwise.
        /// </summary>
        WeaveSiteAuto = 0,

        /// <summary>
        /// Isolates the bits that specify where the aspect should be woven
        /// (<see cref="WeaveSiteCall"/>, <see cref="WeaveSiteTarget"/>,
        /// <see cref="WeaveSiteAuto"/>).
        /// </summary>
        WeaveSiteMask = 3
    }

    /// <summary>
    /// Custom attribute that, when applied on a class implementing <see cref="IOnMethodInvocationAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public sealed class OnMethodInvocationAspectConfigurationAttribute : InstanceBoundAspectConfigurationAttribute,
                                                                         IOnMethodInvocationAspectConfiguration
    {
        private OnMethodInvocationAspectOptions options;

        /// <summary>
        /// Gets or sets the aspect options.
        /// </summary>
        public OnMethodInvocationAspectOptions Options
        {
            get { return this.options; }
            set { this.options = value; }
        }

        /// <inheritdoc />
        OnMethodInvocationAspectOptions IOnMethodInvocationAspectConfiguration.GetOptions()
        {
            return this.options;
        }
    }
}

#endif