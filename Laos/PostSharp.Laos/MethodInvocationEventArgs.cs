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
using System.Diagnostics;
using System.Reflection;

namespace PostSharp.Laos
{
    /// <summary>
    /// Arguments of a join point related to a method invocation (or method call).
    /// Used by <see cref="OnMethodInvocationAspect"/>.
    /// </summary>
    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public sealed class MethodInvocationEventArgs : LaosEventArgs
    {
        private readonly object[] arguments;
        private readonly Delegate @delegate;
        private object returnValue;


        /// <summary>
        /// Initializes a new <see cref="MethodInvocationEventArgs"/>.
        /// </summary>
        /// <param name="delegate">Delegate representing the method being called.</param>
        /// <param name="arguments">Arguments passed to the method.</param>
        public MethodInvocationEventArgs( Delegate @delegate, object[] arguments )
        {
            this.arguments = arguments;
            this.@delegate = @delegate;
        }

        /// <summary>
        /// Obsolete.
        /// </summary>
        /// <returns></returns>
        [Obsolete( "Use GetArgumentArray instead." )]
        public object[] GetArguments()
        {
            return arguments;
        }

        /// <summary>
        /// Gets the array of arguments being passed to the method.
        /// </summary>
        /// <returns>The array of arguments being passed to the method,
        /// or <b>null</b> if the method takes no argument.</returns>
        public object[] GetArgumentArray()
        {
            return arguments;
        }

        /// <summary>
        /// Gets the delagate representing the method being called.
        /// </summary>
        [Obsolete( "Use the Method and Instance properties instead." )]
        public Delegate Delegate
        {
            get { return @delegate; }
        }

        /// <summary>
        /// Gets the intercepted method.
        /// </summary>
        public MethodInfo Method
        {
            get { return this.@delegate.Method; }
        }

        /// <summary>
        /// Gets the object instance on which the method is being executed.
        /// </summary>
        public object Instance
        {
            get { return this.@delegate.Target; }
        }


        /// <summary>
        /// Gets or sets the method return value.
        /// </summary>
        public object ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }

        /// <summary>
        /// Invokes the intercepted method with original parameters.
        /// </summary>
        public void Proceed()
        {
            this.returnValue = this.@delegate.DynamicInvoke( this.GetArgumentArray() );
        }

        /// <summary>
        /// Invokes the intercepted method and provides new parameters.
        /// </summary>
        /// <param name="parameters"></param>
        public void Proceed( object[] parameters )
        {
            this.returnValue = this.@delegate.DynamicInvoke( parameters );
        }
    }
}

#endif