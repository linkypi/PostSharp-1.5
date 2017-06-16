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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Bookmark for the <see cref="InstructionReader"/> class.
    /// </summary>
    /// <seealso cref="InstructionReader.CreateBookmark"/>
    /// <seealso cref="InstructionReader.GoToBookmark"/>
    public sealed class InstructionReaderBookmark
    {
        private readonly InstructionReader reader;
        private readonly InstructionSequence sequence;
        private readonly object state;

        internal InstructionReaderBookmark( InstructionReader reader, InstructionSequence sequence, object state )
        {
            this.reader = reader;
            this.sequence = sequence;
            this.state = state;
        }


        internal InstructionReader Reader { get { return reader; } }
        internal InstructionSequence Sequence { get { return sequence; } }
        internal object State { get { return state; } }
    }
}
