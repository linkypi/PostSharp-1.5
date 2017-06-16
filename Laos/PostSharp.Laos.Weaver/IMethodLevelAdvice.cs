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

using PostSharp.CodeModel;
using PostSharp.CodeWeaver;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Semantics to be implemented by advices that are added to
    /// the <see cref="LaosTask.MethodLevelAdvices"/> collection of advices.
    /// These are normal advices from <see cref="PostSharp.CodeWeaver"/> with
    /// additional information about the join points to which the advices apply.
    /// </summary>
    public interface IMethodLevelAdvice : IAdvice
    {
        /// <summary>
        /// Gets the method to which the current advice applies, or <b>null</b>
        /// if the current advice applies to all methods.
        /// </summary>
        MethodDefDeclaration Method { get; }

        /// <summary>
        /// Gets the operand to which the current advice applies, or <b>null</b>
        /// if the current advices apply to all operands or if the operand is
        /// not relevant.
        /// </summary>
        MetadataDeclaration Operand { get; }

        /// <summary>
        /// Gets the kinds of join point to which the current advice applies.
        /// </summary>
        JoinPointKinds JoinPointKinds { get; }
    }
}