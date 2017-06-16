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

using System;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Custom attribute that, when applied on another custom attribute (a class derived 
    /// from <see cref="Attribute"/>), means that assemblies with elements
    /// annotated with that custom attribute should be processed by PostSharp.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public sealed class RequirePostSharpAttribute : Attribute
    {
        private readonly string plugIn;
        private readonly string task;

        /// <summary>
        /// Initializes a new <see cref="RequirePostSharpAttribute"/>.
        /// </summary>
        /// <param name="plugIn">Name of the required plug-in (file name without extension).</param>
        /// <param name="task">Name of the required task (should be defined in <paramref name="plugIn"/>).</param>
        public RequirePostSharpAttribute( string plugIn, string task )
        {
            this.plugIn = plugIn;
            this.task = task;
        }

        /// <summary>
        /// Gets the name of the required plug-in (file name without the extension).
        /// </summary>
        public string PlugIn
        {
            get { return this.plugIn; }
        }

        /// <summary>
        /// Gets the name of the required task (should be defined in <see cref="PlugIn"/>).
        /// </summary>
        public string Task
        {
            get { return this.task; }
        }
    }
}