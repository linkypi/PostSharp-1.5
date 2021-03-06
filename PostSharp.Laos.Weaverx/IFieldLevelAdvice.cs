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
    /// the <see cref="LaosTask.FieldLevelAdvices"/> collection of advices.
    /// These are normal advices from <see cref="PostSharp.CodeWeaver"/> with
    /// additional information about the join points to which the advices apply.
    /// </summary>
    public interface IFieldLevelAdvice : IAdvice
    {
        /// <summary>
        /// Gets the type to which the current advice applies.
        /// </summary>
        IField Field { get; }

        /// <summary>
        /// Gets the kinds of join point to which the current advice applies.
        /// </summary>
        JoinPointKinds JoinPointKinds { get; }

        /// <summary>
        /// Determines whether the field should be changed to a property.
        /// </summary>
        bool ChangeToProperty { get; }
    }
}