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
    /// Defines the semantics of an aspect that, when applied on a method,
    /// defines an exception handler around the whole method body
    /// and let the implementation of this interface handle the exception.
    /// </summary>
    public interface IOnExceptionAspect : IExceptionHandlerAspect
    {
    }

    /// <summary>
    /// Configuration of the <see cref="IOnExceptionAspect"/> aspect.
    /// </summary>
    /// <seealso cref="OnExceptionAspectConfigurationAttribute"/>
    public interface IOnExceptionAspectConfiguration : IExceptionHandlerAspectConfiguration
    {
        /// <summary>
        /// Get the weaving options.
        /// </summary>
        /// <returns>The weaving options, or <b>null</b> if options are not specified.</returns>
        OnExceptionAspectOptions? GetOptions();
    }

    /// <summary>
    /// Weaving options for <see cref="IOnExceptionAspect"/>.
    /// </summary>
    [Flags]
    public enum OnExceptionAspectOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None
    }

    /// <summary>
    /// Custom attribute that, when applied on a class implementing <see cref="IOnExceptionAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public sealed class OnExceptionAspectConfigurationAttribute : ExceptionHandlerOptionsAttribute,
                                                                  IOnExceptionAspectConfiguration
    {
        private OnExceptionAspectOptions? options;

        /// <summary>
        /// Gets or sets the aspect options.
        /// </summary>
        public OnExceptionAspectOptions Options
        {
            get { return this.options ?? OnExceptionAspectOptions.None; }
            set { this.options = value; }
        }

        /// <inheritdoc />
        OnExceptionAspectOptions? IOnExceptionAspectConfiguration.GetOptions()
        {
            return this.options;
        }
    }
}