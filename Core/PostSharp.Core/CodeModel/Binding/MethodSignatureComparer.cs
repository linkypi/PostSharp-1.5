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
    /// Compares two method signatures (<see cref="IMethodSignature"/>).
    /// </summary>

    public sealed class MethodSignatureComparer : IEqualityComparer<IMethodSignature>
    {
        private MethodSignatureComparer()
        {
        }

        private static readonly MethodSignatureComparer instance = new MethodSignatureComparer();

        /// <summary>
        /// Gets a singleton instance of <see cref="MethodSignatureComparer"/>.
        /// </summary>
        /// <returns></returns>
        public static MethodSignatureComparer GetInstance() { return instance; }


        /// <inheritdoc />
        bool IEqualityComparer<IMethodSignature>.Equals(IMethodSignature x, IMethodSignature y)
        {
            return CompareHelper.Equals(x, y, true);
        }


        /// <inheritdoc />
        int IEqualityComparer<IMethodSignature>.GetHashCode(IMethodSignature obj)
        {
            throw new NotImplementedException();
        }
    }
}
