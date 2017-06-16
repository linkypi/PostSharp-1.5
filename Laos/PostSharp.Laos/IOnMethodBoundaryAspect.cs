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
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Defines the semantics of an aspect that, when applied on a method,
    /// defines an exception handler around the whole method body
    /// and let the implementation of this interface be invoked before the
    /// method execution (<see cref="OnEntry"/>), in the exception handling
    /// block (<see cref="IExceptionHandlerAspect.OnException"/>) and
    /// after the method execution, in the <i>finally</i> block (<see cref="OnExit"/>).
    /// </summary>
    public interface IOnMethodBoundaryAspect : IExceptionHandlerAspect
    {
        /// <summary>
        /// Method executed <b>before</b> the body of methods to which this aspect is applied.
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which method
        /// is being executed, which are its arguments, and how should the execution continue
        /// after the execution of <see cref="OnEntry"/>.</param>
        /// <remarks>
        /// If the aspect is applied to a constructor, the current method is invoked
        /// after the <b>this</b> pointer has been initialized, that is, after
        /// the base constructor has been called.
        /// </remarks>
        void OnEntry( MethodExecutionEventArgs eventArgs );

        /// <summary>
        /// Method executed <b>after</b> the body of methods to which this aspect is applied,
        /// even when the method exists with an exception (this method is invoked from
        /// the <b>finally</b> block).
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which method
        /// is being executed and which are its arguments.</param>
        void OnExit( MethodExecutionEventArgs eventArgs );

        /// <summary>
        /// Method executed <b>after</b> the body of methods to which this aspect is applied,
        /// but only when the method succesfully returns (i.e. when no exception flies out
        /// the method.).
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which method
        /// is being executed and which are its arguments.</param>
        void OnSuccess( MethodExecutionEventArgs eventArgs );
    }

    /// <summary>
    /// Configuration of the <see cref="IOnMethodBoundaryAspect"/> aspect.
    /// </summary>
    /// <seealso cref="OnMethodBoundaryAspectConfigurationAttribute"/>
    public interface IOnMethodBoundaryAspectConfiguration : IExceptionHandlerAspectConfiguration
    {
        /// <summary>
        /// Gets the weaving options.
        /// </summary>
        /// <returns>The weaving options.</returns>
        [CompileTimeSemantic]
        OnMethodBoundaryAspectOptions? GetOptions();
    }

    /// <summary>
    /// Weaving options of the <see cref="IOnMethodBoundaryAspect"/> aspect.
    /// </summary>
    [Flags]
    public enum OnMethodBoundaryAspectOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0
    }

    /// <summary>
    /// Custom attribute that, when applied on a class implementing <see cref="IOnMethodBoundaryAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public sealed class OnMethodBoundaryAspectConfigurationAttribute : ExceptionHandlerOptionsAttribute,
                                                                       IOnMethodBoundaryAspectConfiguration
    {
        private OnMethodBoundaryAspectOptions? options;

        /// <summary>
        /// Gets or sets aspect options.
        /// </summary>
        public OnMethodBoundaryAspectOptions Options
        {
            get { return this.options ?? OnMethodBoundaryAspectOptions.None; }
            set { this.options = value; }
        }

        /// <inheritdoc />
        OnMethodBoundaryAspectOptions? IOnMethodBoundaryAspectConfiguration.GetOptions()
        {
            return this.options;
        }
    }
}