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

using System.Runtime.InteropServices;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Interface of PostSharp with a hypothetical unmanaged bootstrapper,
    /// typically implemented by the application domain manager.
    /// </summary>
    [ComVisible(true)]
    [Guid("022170A0-668A-475F-A0A3-9AA8FEF56070")]
    public interface IPostSharpDomainManager
    {
        /// <summary>
        /// Initializes PostSharp.
        /// </summary>
        /// <param name="assemblyResolve"><b>true</b> if PostSharp should handle the
        /// <b>AppDomain.AssemblyResolve</b> event itself, <b>false</b> if it is done by
        /// the unmanaged host.</param>
        void Initialize(bool assemblyResolve);

        /// <summary>
        /// Starts the PostSharp command line.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Exit code.</returns>
        int Start([In] string[] args);

        /// <summary>
        /// Finds an assembly given its name.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <returns>Assembly location, or <b>null</b> if the
        /// assembly could not be found.</returns>
        string FindAssembly([MarshalAs(UnmanagedType.LPWStr)] string assemblyName);
    }
}