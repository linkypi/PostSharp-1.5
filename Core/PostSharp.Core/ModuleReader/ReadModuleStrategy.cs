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

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Determines how a module should be loaded.
    /// </summary>
    public enum ReadModuleStrategy
    {
        /// <summary>
        /// The module should be loaded from its mapped image in memory,
        /// after it has been loaded into the CLR.
        /// </summary>
        FromMemoryImage,

        /// <summary>
        /// The module should be loaded directly from disk, without
        /// loading it into the CLR.
        /// </summary>
        FromDisk
    }
}