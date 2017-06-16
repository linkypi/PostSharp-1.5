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


#if !SMALL

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// Interface to be implemented by classes that should be serializable using
    /// a <see cref="StateBagSerializer"/>. These classes should also implement
    /// a default (parameterless) constructor.
    /// </summary>
    public interface IStateBagSerializable
    {
        /// <summary>
        /// Serializes the current object into a <see cref="StateBag"/>/
        /// </summary>
        /// <param name="stateBag"><see cref="StateBag"/> into which the current object has to be saved.</param>
        void Serialize( StateBag stateBag );

        /// <summary>
        /// Deserialized the current object from a <see cref="StateBag"/>.
        /// </summary>
        /// <param name="stateBag">The <see cref="StateBag"/> into which the object was previously serialized,
        /// and from which it should be reserialized.</param>
        /// <remarks>
        /// This method is called during deserialization just after the default (parameterless) constructor
        /// has been invoked.
        /// </remarks>
        void Deserialize( StateBag stateBag );
    }
}

#endif