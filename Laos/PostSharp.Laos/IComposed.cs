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
    /// Interface implemented by composed types (i.e. types implementing an
    /// interface that is implemented, not by the type itself, but by another
    /// type <i>composed</i> in the first one) that want to expose their 
    /// implementation.
    /// </summary>
    /// <typeparam name="T">Composed interface whose implementation
    /// is made accessible.</typeparam>
    /// <remarks>
    /// This interface is <i>optional</i>
    /// and is implemented only when the composed type wants to expose its
    /// implementation. This interface provides a way to <i>protect</i>
    /// the implementation: the implementation is provided only to callers
    /// holding credentials that should have been received by the composed
    /// instance itself. A second defensive approach is to make the implementation
    /// <b>internal</b>.
    /// </remarks>
    public interface IComposed<T>
    {
        /// <summary>
        /// Gets the object implementing the interface <typeparamref name="T"/>
        /// on behalf of the current instance.
        /// </summary>
        /// <param name="credentials">Credentials provided by the current instance.</param>
        /// <returns>The object implementing the interface <typeparamref name="T"/>
        /// on behalf of the current instance.
        /// </returns>
        T GetImplementation( InstanceCredentials credentials );

        /// <summary>
        /// Set the object implementing the interface <typeparamref name="T"/> on behalf
        /// of the current instance.
        /// </summary>
        /// <param name="credentials">Credentials provided by the current instance.</param>
        /// <param name="implementation">Object implementing the <typeparamref name="T"/>
        /// interface on behalf of the current instance.</param>
        void SetImplementation( InstanceCredentials credentials, T implementation );
    }
}