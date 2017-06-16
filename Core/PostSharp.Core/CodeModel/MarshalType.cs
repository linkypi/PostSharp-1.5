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

using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Marshal types. Determines how fields and parameters are marshalled
    /// during PInvoke. 
    /// </summary>
    /// <remarks>
    /// Concrete implementations are in the 
    /// <see cref="PostSharp.CodeModel.MarshalTypes"/> namespace.
    /// </remarks>
    public abstract class MarshalType
    {
        /// <summary>
        /// Initializes a new <see cref="MarshalType"/>.
        /// </summary>
        internal MarshalType()
        {
        }

        /// <summary>
        /// Writes the IL definition of the current <see cref="MarshalType"/>.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal abstract void WriteILReference( ILWriter writer );
    }
}