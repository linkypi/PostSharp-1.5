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
using System.Collections.Specialized;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.Extensibility;
using PostSharp.ModuleWriter;

namespace PostSharp.PlatformAbstraction
{
    /// <summary>
    /// Provides platform-specific functionalities to compile for
    /// a specific target platform.
    /// </summary>
    /// <remarks>
    /// Additionally to implementing all abstract methods, derived class
    /// should also implement a constructor taking a <see cref="NameValueCollection"/>
    /// as its only parameter.
    /// </remarks>
    [Serializable]
    public abstract class TargetPlatformAdapter : MarshalByRefObject
    {
        /// <summary>
        /// Gets the default <see cref="Encoding"/> for MSIL text files.
        /// </summary>
        /// <returns>An <see cref="Encoding"/>.</returns>
        public abstract Encoding GetDefaultMsilEncoding();

        /// <summary>
        /// Gets compatibility flags for the IL generation process.
        /// </summary>
        /// <returns>A combination of <see cref="ILWriterCompatibility"/> flags.</returns>
        public abstract ILWriterCompatibility GetILWriterCompatibility();


        /// <summary>
        /// Assembles text IL code to a binary module.
        /// </summary>
        /// <param name="module">The module to assemble (module properties, like target
        /// architecture, have to be read here).</param>
        /// <param name="options">Options</param>
        public abstract void Assemble( ModuleDeclaration module, AssembleOptions options );

        /// <summary>
        /// Executes the PEVERIFY utility against a file.
        /// </summary>
        /// <param name="file">The path of the file to be verified.</param>
        /// <remarks>
        /// The implementation should write a warning (not an error) if the PEVERIFY utility is not installed.
        /// </remarks>
        public abstract void Verify( string file );
    }
}