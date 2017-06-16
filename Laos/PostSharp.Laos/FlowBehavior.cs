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

namespace PostSharp.Laos
{
    /// <summary>
    /// Specifies how the control flow will behave once it will leave the aspect method.
    /// </summary>
    /// <remarks>
    /// This enumeration is used by the <see cref="MethodExecutionEventArgs"/> class.
    /// </remarks>
    public enum FlowBehavior
    {
        /// <summary>
        /// Default flow behavior for the current method. For <b>OnEntry</b> or <b>OnExit</b>, the fault flow is
        /// <see cref="Continue"/>, for <b>OnException</b> it is <see cref="RethrowException"/>.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Continue normally.
        /// </summary>
        Continue = 1,

        /// <summary>
        /// The current exception will be rethrown. Available only for <b>OnException</b>.
        /// </summary>
        RethrowException,

        /// <summary>
        /// Return immediately from the current method. Available only for <b>OnEntry</b> and
        /// <b>OnException</b>. Note that you may want to set the <see cref="MethodExecutionEventArgs.ReturnValue"/>
        /// property.
        /// </summary>
        Return
    }
}