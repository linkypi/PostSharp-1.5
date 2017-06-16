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
    /// Base interface for th run-time semantics of all Laos aspects.
    /// </summary>
    /// <seealso cref="ILaosAspectBuildSemantics"/>
    /// <seealso cref="ILaosAspectConfiguration"/>
    public interface ILaosAspect
    {
      
    }

    #if !SMALL
    /// <summary>
    /// Compile-time semantics of <see cref="ILaosAspect"/>.
    /// </summary>
    public interface ILaosAspectBuildSemantics : ILaosAspectConfiguration
    {
        /// <summary>
        /// Method invoked at compile time to ensure that the aspect has been applied to
        /// the right target.
        /// </summary>
        /// <param name="target">Target element.</param>
        /// <returns><b>true</b> if the aspect was applied to an acceptable target, otherwise
        /// <b>false</b>.</returns>
        /// <remarks>The implementation of this method is expected to emit an error message
        /// or an exception in case of error. Only returning <b>false</b> causes the aspect
        /// to be silently ignored.</remarks>
        bool CompileTimeValidate(object target);
    }
#endif


    /// <summary>
    /// Configuration of an <see cref="ILaosAspect"/>.
    /// </summary>
    public interface ILaosAspectConfiguration
    {
        /// <summary>
        /// Determines whether the aspect requires reflection wrappers.
        /// If not, compile-time semantics of the aspect will receive
        /// normal reflection objects, requiring the enhanced assembly to
        /// be loaded in the weaver.
        /// </summary>
        bool? RequiresReflectionWrapper { get; }
    }
}