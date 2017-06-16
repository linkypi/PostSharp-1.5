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

using System.ComponentModel;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Base type for all declarations of a module. 
    /// </summary>
    /// <remarks>
    /// A <see cref="Declaration"/> is basically an <see cref="Element"/>
    /// that has a <see cref="ModuleDeclaration"/> in its ancestor axis.
    /// It is exposed in the <see cref="Module"/> property.
    /// </remarks>
    public abstract class Declaration : Element, IDeclaration
    {
        /// <inheritdoc />
        [Browsable( false )]
        public ModuleDeclaration Module
        {
            get; protected set;
        }

        /// <inheritdoc />
        [Browsable( false )]
        public virtual IAssembly DeclaringAssembly
        {
            get
            {
                ModuleDeclaration module = this.Module;
                return module == null ? null : module.Assembly;
            }
        }

        internal override void OnAddingToParent(Element parent, string role)
        {
            Declaration declaration = parent as Declaration;
            if (declaration != null)
            {
                ExceptionHelper.Core.AssertValidOperation(declaration.Module != null, "ParentElementNotAttached");
                ExceptionHelper.Core.AssertValidOperation(this.Module == null || this.Module == declaration.Module, "ElementAttachedToDifferentModule");
                this.Module = declaration.Module;
            }

            base.OnAddingToParent(parent, role);
        }
    }
}