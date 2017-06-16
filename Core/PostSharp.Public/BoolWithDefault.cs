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

namespace PostSharp
{
    /// <summary>
    /// Boolean with a "default" value. For use in design-time configurable boolean properties.
    /// </summary>
    public enum BoolWithDefault
    {
        /// <summary>
        /// Indicates that the value has not been set.
        /// </summary>
        Default,

        /// <summary>
        /// Indicates that the value has been set to <b>true</b>.
        /// </summary>
        True,

        /// <summary>
        /// Indicates that the value has been set to <b>false</b>.
        /// </summary>
        False
    }
}