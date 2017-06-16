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


using System;
using System.Collections.Generic;
using System.Text;

namespace PostSharp.Laos
{
    /// <summary>
    /// Custom attribute enabling a Laos awareness (like awareness of serialization) on the aspect class or the target
    /// assembly on which the custom attribute is applied.
    /// </summary>
    /// <remarks>
    /// When this custom attribute is applied on an <b>aspect class</b>, it means that the awareness should be enabled
    /// on any assembly using that aspect. When this custom attribute is applied on an <b>assembly</b>, it means
    /// that this awareness should be enabled when processing that assembly.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class EnableLaosAwarenessAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="EnableLaosAwarenessAttribute"/>.
        /// </summary>
        /// <param name="pluginName">Short name of the plug-in containing the awareness implementation.</param>
        /// <param name="name">Name of the task implementing the awareness.</param>
        public EnableLaosAwarenessAttribute( string pluginName, string name )
        {
            this.PlugInName = pluginName;
            this.TaskName = name;
        }

        /// <summary>
        /// Gets the short name of the plug-in containing the awareness implementation.
        /// </summary>
        public string PlugInName { get; private set; }

        /// <summary>
        /// Gets the name of the task containing the awareness implementation.
        /// </summary>
        public string TaskName { get; private set; }
    }
}
