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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines a <b>Module</b> property.
    /// </summary>
    public interface IModuleScoped
    {
        /// <summary>
        /// Gets the declaring module.
        /// </summary>
        ModuleDeclaration Module { get; }

        /// <summary>
        /// Gets the declaring assembly.
        /// </summary>
        IAssembly DeclaringAssembly { get; }
    }

    /// <summary>
    /// Exposes the semantics of a declaration, which is basically a module-scoped domain element.
    /// </summary>
    public interface IDeclaration : IElement, IModuleScoped
    {
    }
}