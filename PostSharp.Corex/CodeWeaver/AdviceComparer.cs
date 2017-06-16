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

using System.Collections.Generic;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Compares advices (<see cref="IAdvice"/>) so they can be sorted by priority.
    /// </summary>
    internal sealed class AdviceComparer : IComparer<AdviceJoinPointKindsPair>
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly AdviceComparer Instance = new AdviceComparer();

        /// <summary>
        /// Initializes a new <see cref="AdviceComparer"/>.
        /// </summary>
        private AdviceComparer()
        {
        }

        /// <inheritdoc />
        public int Compare( AdviceJoinPointKindsPair x, AdviceJoinPointKindsPair y )
        {
            if ( y.Advice.Priority < x.Advice.Priority )
            {
                return -1;
            }
            else if ( y.Advice.Priority > x.Advice.Priority )
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}