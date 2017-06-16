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
using System.Diagnostics;
using System.Reflection;

namespace PostSharp.Laos
{
    /// <summary>
    /// Arguments of join points related to a method body, like those of
    /// <see cref="OnMethodBoundaryAspect"/>, <see cref="OnExceptionAspect"/>
    /// or <see cref="ImplementMethodAspect"/>.
    /// </summary>
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
    public sealed class MethodExecutionEventArgs : InstanceBoundLaosEventArgs
    {
        private readonly MethodBase method;
        private object returnValue;
        private readonly object[] arguments;
        private Exception exception;
        private FlowBehavior flowBehavior;
        private object methodExecutionState;


        /// <summary>
        /// Initializes a new <see cref="MethodExecutionEventArgs"/>.
        /// </summary>
        /// <param name="method">Method instance being executed.</param>
        /// <param name="instance">Object instance on which the method is currently
        /// executed, or <b>null</b> if the method is static.</param>
        /// <param name="arguments">Array of arguments of this method.</param>
        public MethodExecutionEventArgs(
            MethodBase method,
            object instance,
            object[] arguments
            ) : base( instance )
        {
            this.method = method;
            this.arguments = arguments;
        }

        /// <summary>
        /// Gets the method instance being executed.
        /// </summary>
        /// <remarks>
        /// If the executed method is generic or if its declaring type is generic,
        /// the current property contains the generic instance being executed.
        /// </remarks>
        public MethodBase Method
        {
            get { return method; }
        }


        /// <summary>
        /// Gets or sets the method return value.
        /// </summary>
        /// <remarks>
        /// You can modify the return value only when the join point is located
        /// after a method execution, typically when the return value has already
        /// been set.
        /// </remarks>
        public object ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }


        /// <summary>
        /// Obsolete.
        /// </summary>
        /// <returns></returns>
        [Obsolete( "Use GetReadOnlyArgumentArray or GetWritableArgumentArray instead." )]
        public object[] GetArguments()
        {
            return arguments;
        }

        /// <summary>
        /// Gets a read-only array of method arguments.
        /// </summary>
        /// <returns>The array of method arguments, or <b>null</b> if the
        /// method has no arguments.</returns>
        /// <remarks>
        /// For performance reason, this method actually returns a writable array. However,
        /// later versions of PostSharp Laos may optimize code generation and not write
        /// back values when you use this method. If you plan to modify values, use
        /// the <see cref="GetWritableArgumentArray"/> instead.
        /// </remarks>
        public object[] GetReadOnlyArgumentArray()
        {
            return arguments;
        }

        /// <summary>
        /// Gets a writable array of method arguments.
        /// </summary>
        /// <returns>The array of method arguments, or <b>null</b> if the
        /// method has no arguments.</returns>
        /// <remarks>
        /// You should modify only arguments passed by reference (<b>out</b> and <b>ref</b> 
        /// in C#).  If you need to change input arguments, consider using
        /// <see cref="OnMethodInvocationAspect"/>.
        /// </remarks>
        public object[] GetWritableArgumentArray()
        {
            return arguments;
        }


        /// <summary>
        /// Gets the exception currently flying.
        /// </summary>
        /// <value>An <see cref="Exception"/>, or <b>null</b> if the method is
        /// exiting normally.</value>
        public Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }

        /// <summary>
        /// Determines the control flow of the target method once the handler is exited.
        /// </summary>
        /// <see cref="PostSharp.Laos.FlowBehavior"/>
        public FlowBehavior FlowBehavior
        {
            get { return flowBehavior; }
            set { flowBehavior = value; }
        }

        /// <summary>
        /// User-defined state information whose lifetime is linked to the
        /// current method execution. Aspects derived from <see cref="OnMethodBoundaryAspect"/>
        /// should use this property to save state information between
        /// different events (<see cref="OnMethodBoundaryAspect.OnEntry"/>,
        /// <see cref="OnMethodBoundaryAspect.OnExit"/> and <see cref="ExceptionHandlerAspect.OnException"/>).
        /// </summary>
        public object MethodExecutionTag
        {
            get { return this.methodExecutionState; }
            set { this.methodExecutionState = value; }
        }
    }
}