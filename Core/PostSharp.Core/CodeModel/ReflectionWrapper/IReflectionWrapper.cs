#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

namespace PostSharp.CodeModel.ReflectionWrapper
{

    /// <summary>
    /// Defines common public semantics of all reflection wrappers.
    /// </summary>
    /// <typeparam name="T">Type of underlying object.</typeparam>
    public interface IReflectionWrapper<T> : IReflectionWrapper
    {
        /// <summary>
        /// Gets the underlying <b>PostSharp.CodeModel</b> object.
        /// </summary>
        T WrappedObject { get; }
    }
}
