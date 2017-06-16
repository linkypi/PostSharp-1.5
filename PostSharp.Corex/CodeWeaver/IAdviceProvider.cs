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

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Pluggable tasks (<see cref="PostSharp.Extensibility.Task"/>) should implement this interface
    /// when they want to provide advices (<see cref="IAdvice"/>) to the <see cref="WeaverTask"/>.
    /// </summary>
    public interface IAdviceProvider
    {
        /// <summary>
        /// When implemented, adds advices to a <see cref="Weaver"/>, typically
        /// using the <see cref="Weaver.AddMethodLevelAdvice"/> and 
        /// <see cref="Weaver.AddTypeLevelAdvice"/> methods.
        /// </summary>
        /// <param name="codeWeaver">The weaver to which advices should be added.</param>
        void ProvideAdvices( Weaver codeWeaver );
    }
}