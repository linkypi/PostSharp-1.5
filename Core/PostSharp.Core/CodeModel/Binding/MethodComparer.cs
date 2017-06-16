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
using PostSharp.CodeModel.Helpers;

namespace PostSharp.CodeModel.Binding
{
    /// <summary>
    /// Compares two methods (<see cref="IMethod"/>).
    /// </summary>

    public sealed class MethodComparer : IEqualityComparer<IMethod>
    {
        private MethodComparer()
        {
        }

        private static readonly MethodComparer instance = new MethodComparer();

        /// <summary>
        /// Gets a singleton instance of <see cref="MethodComparer"/>.
        /// </summary>
        /// <returns></returns>
        public static MethodComparer GetInstance() { return instance; }

        #region IEqualityComparer<IMethod> Members

        /// <inheritdoc />
        bool IEqualityComparer<IMethod>.Equals( IMethod x, IMethod y )
        {
            return CompareHelper.Equals( x, y, true );
        }

        /// <inheritdoc />
        public int GetHashCode( IMethod obj )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
