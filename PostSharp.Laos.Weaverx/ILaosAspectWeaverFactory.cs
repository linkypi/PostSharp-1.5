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

using PostSharp.Extensibility;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Interface to be implemented by PostSharp tasks (<see cref="Task"/>)
    /// that provide weavers for Laos aspects. This interface has a method
    /// <see cref="CreateAspectWeaver"/> that is invoked when a weaver
    /// should be created for an aspect.
    /// </summary>
    public interface ILaosAspectWeaverFactory
    {
        /// <summary>
        /// Creates a weaver for a given aspect.
        /// </summary>
        /// <param name="aspectSemantic">The aspect for which the weaver should be created.</param>
        /// <returns>A weaver for <paramref name="aspectSemantic"/>, or <b>null</b> if
        /// the current factory does not recognize this aspect.</returns>
        LaosAspectWeaver CreateAspectWeaver( AspectTargetPair aspectSemantic );
    }
}