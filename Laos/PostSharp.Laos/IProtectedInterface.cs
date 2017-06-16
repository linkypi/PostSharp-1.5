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
    /// Gives access to a 'protected' interface. 
    /// </summary>
    /// <typeparam name="T">The exposed interface.</typeparam>
    /// <remarks>
    /// A 'protected' interface has necessarily public visibility, but objects
    /// don't expose them directly. Instead, they expose them through
    /// the <see cref="IProtectedInterface{T}"/> interface. An object that
    /// wants access to a protected interface needs to get <see cref="InstanceCredentials"/>.
    /// Since the method providing <see cref="InstanceCredentials"/> is protected, it
    /// allows an instance to control which code can have access to protected semantics.
    /// </remarks>
    public interface IProtectedInterface<T>
    {
        /// <summary>
        /// Gets a protected interface.
        /// </summary>
        /// <param name="credentials">Credentials of the current instance.</param>
        /// <returns>The implementation of the interface <typeparamref name="T"/>.</returns>
        T GetInterface( InstanceCredentials credentials );
    }
}