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

using System.Globalization;
using PostSharp.CodeModel;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// A join point is a location in the code tree.
    /// </summary>
    public sealed class JoinPoint
    {
        private Instruction instruction;
        private JoinPointKinds kind;

        /// <summary>
        /// Current position w.r.t. the location (<see cref="JoinPointPosition.Before"/>,
        /// <see cref="JoinPointPosition.InsteadOf"/>, <see cref="JoinPointPosition.After"/>).
        /// </summary>
        public JoinPointPosition Position { get; internal set; }

        /// <summary>
        /// Gets the current instruction.
        /// </summary>
        public Instruction Instruction
        {
            get { return this.instruction; }
            internal set { this.instruction = value; }
        }

        /// <summary>
        /// Kind of join point.
        /// </summary>
        public JoinPointKinds JoinPointKind
        {
            get { return kind; }
            internal set { kind = value; }
        }

        /// <summary>
        /// Current exception handler.
        /// </summary>
        public ExceptionHandler ExceptionHandler { get; internal set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "JoinPoint {{{0}}} at {{{1}}}", this.kind,
                this.instruction );
        }
    }

    /// <summary>
    /// Position of point points w.r.t. locations.
    /// </summary>
    public enum JoinPointPosition
    {
        /// <summary>
        /// Before the location.
        /// </summary>
        Before,

        /// <summary>
        /// After the location.
        /// </summary>
        After,

        /// <summary>
        /// Instead of the current instruction.
        /// </summary>
        InsteadOf
    }
}