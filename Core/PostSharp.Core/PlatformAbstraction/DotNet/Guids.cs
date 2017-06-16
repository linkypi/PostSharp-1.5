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

#if !MONO

#region Using directives

using System;

#endregion

namespace PostSharp.PlatformAbstraction.DotNet
{
    /// <summary>
    /// Defines some useful COM GUIDs.
    /// </summary>
    internal static class Guids
    {
        /// <summary>
        /// CLSID_CorMetadataDispenser = {E5CB7A31-7512-11d2-89CE-0080C792E5D8}.
        /// </summary>
        public static readonly Guid CLSID_CorMetadataDispenser = new Guid( "E5CB7A31-7512-11d2-89CE-0080C792E5D8" );

        /// <summary>
        /// IID_IMetadataImport2 = {FCE5EFA0-8BBA-4f8e-A036-8F2022B08466}.
        /// </summary>
        public static readonly Guid IID_IMetadataImport2 = new Guid( "FCE5EFA0-8BBA-4f8e-A036-8F2022B08466" );
    }
}

#endif