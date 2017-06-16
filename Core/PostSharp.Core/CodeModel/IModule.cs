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

#region Using directives

using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the functionalities that are common to all representations of a module
    /// (<see cref="ModuleDeclaration"/>, <see cref="ModuleRefDeclaration"/>).
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Gets the module name.
        /// </summary>
        /// <value>
        /// The module name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the assembly containing the module.
        /// </summary>
        /// <value>
        /// The assembly (<see cref="IAssembly"/>) containing the module.
        /// </value>
        IAssembly Assembly { get; }
    }

    internal interface IModuleInternal : IModule
    {
        /// <summary>
        /// Writes in IL a reference to the current instance.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        void WriteILReference( ILWriter writer );
    }
}