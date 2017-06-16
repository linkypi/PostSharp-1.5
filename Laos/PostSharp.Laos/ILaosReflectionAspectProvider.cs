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

using PostSharp.Extensibility;

namespace PostSharp.Laos
{
#if !SMALL
    /// <summary>
    /// Interface that should be implemented by aspects that want
    /// to provide other aspects to the weaver (i.e. combined
    /// aspects, see <see cref="ICompoundAspect"/>).
    /// </summary>
    public interface ILaosReflectionAspectProvider
    {
        /// <summary>
        /// Provides new aspects.
        /// </summary>
        /// <param name="collection">Collection into which
        /// new aspects have to be added.</param>
        [CompileTimeSemantic]
        void ProvideAspects( LaosReflectionAspectCollection collection );
    }
#endif
}