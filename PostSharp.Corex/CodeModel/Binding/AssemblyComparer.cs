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
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.CodeModel.Binding
{
    /// <summary>
    /// Compares two assemblies (<see cref="IAssembly"/>).
    /// </summary>
    /// <remarks>
    /// The implementation of this class is preliminary. In the future, it should
    /// support cross-module comparison.
    /// </remarks>
    public class AssemblyComparer : IEqualityComparer<IAssemblyName>
    {

        private AssemblyComparer()
        {
        }

        private static readonly AssemblyComparer instance = new AssemblyComparer();

        /// <summary>
        /// Gets a singleton instance of the <see cref="AssemblyComparer"/> class.
        /// </summary>
        /// <returns></returns>
        public static AssemblyComparer GetInstance() { return instance; }

        #region IEqualityComparer<IAssembly> Members

        /// <inheritdoc />
        public bool Equals( IAssemblyName x, IAssemblyName y )
        {
            if ( x == null || y == null )
                return false;

            ;
            return x.Equals( y );
        }

        /// <inheritdoc />
        public int GetHashCode( IAssemblyName obj )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( obj, "obj" );

            #endregion

            return obj.Name.GetHashCode();
        }

        #endregion
    }
}
