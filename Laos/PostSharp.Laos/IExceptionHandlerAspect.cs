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
using System.Reflection;
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Base interface for aspects defining an exception handler
    /// (<see cref="IOnExceptionAspect"/>, <see cref="IOnMethodBoundaryAspect"/>).
    /// </summary>
    public interface IExceptionHandlerAspect : ILaosMethodLevelAspect
    {
        /// <summary>
        /// Method executed when an exception occurs in the methods to which the current
        /// custom attribute has been applied.
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which method
        /// is being called and with which parameters.</param>
        void OnException( MethodExecutionEventArgs eventArgs );
    }

    /// <summary>
    /// Configuration of the <see cref="IExceptionHandlerAspect"/> aspect.
    /// </summary>
    /// <seealso cref="ExceptionHandlerOptionsAttribute"/>
    public interface IExceptionHandlerAspectConfiguration : IInstanceBoundLaosAspectConfiguration
    {
        /// <summary>
        /// Gets the type name of exceptions that are caught by this aspect.
        /// </summary>
        /// <param name="method">Method to which the current custom attribute instance
        /// is applied.</param>
        /// <returns>The assembly-qualified name of the type (derived from <see cref="Exception"/>),
        /// or <b>null</b> if any <see cref="Exception"/> is to be caught.</returns>
        [CompileTimeSemantic]
        TypeIdentity GetExceptionType( MethodBase method );
    }

    /// <summary>
    /// Base class of a custom attribute that, when applied on a class implementing <see cref="IExceptionHandlerAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public abstract class ExceptionHandlerOptionsAttribute : InstanceBoundAspectConfigurationAttribute,
                                                             IExceptionHandlerAspectConfiguration
    {
        private string exceptionType = "System.Exception, mscorlib";

        /// <summary>
        /// Gets or sets the type of the exceptions caught by this handler.
        /// </summary>
        /// <value>
        /// Default value: <see cref="Exception"/>.
        /// </value>
        public string ExceptionType
        {
            get { return this.exceptionType; }
            set { this.exceptionType = value; }
        }

        /// <inheritdoc />
        TypeIdentity IExceptionHandlerAspectConfiguration.GetExceptionType( MethodBase method )
        {
            return TypeIdentity.Wrap( this.exceptionType );
        }
    }
}