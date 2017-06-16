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
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.CodeWeaver
{
    public sealed partial class Weaver
    {
        /// <summary>
        /// State of the <see cref="CodeWeaver"/> for a given method.
        /// </summary>
        private class WeaverState : IDisposable
        {
            public readonly MethodLevelAdvices Advices;

            /// <summary>
            /// Current context.
            /// </summary>
            public readonly WeavingContext Context = new WeavingContext();

            /// <summary>
            /// Shared <see cref="InstructionReader"/>.
            /// </summary>
            public readonly InstructionReader Reader;

            /// <summary>
            /// Kind of position in the code tree.
            /// </summary>
            public Position Position;

            /// <summary>
            /// Current <see cref="InstructionSequence"/>.
            /// </summary>
            public InstructionSequence CurrentSequence;

            /// <summary>
            /// Set of blocks that should not be woven.
            /// </summary>
            public readonly Set<InstructionBlock> SkippedBlocks = new Set<InstructionBlock>();

            /// <summary>
            /// Whether the woven method had been changed.
            /// </summary>
            public bool HasChange;


            /// <summary>
            /// Block initializing the instance, in an instance constructor.
            /// </summary>
            public InstructionBlock InitializationBlock;


            /// <summary>
            /// Initializes a new <see cref="WeaverState"/>.
            /// </summary>
            /// <param name="method">woven method.</param>
            /// <param name="advices">Collection of all advices.</param>
            public WeaverState( MethodDefDeclaration method, MethodLevelAdvices advices )
            {
                this.Advices = advices;
                this.Reader = method.MethodBody.CreateInstructionReader();
            }

            public void Dispose()
            {
                this.Context.Dispose();
            }

            public readonly Stack<InstructionBlock> BlockStack = new Stack<InstructionBlock>();
        }

        /// <summary>
        /// Positions in the code tree.
        /// </summary>
        private enum Position
        {
            BeforeBody = 0,
            BeforeInstruction,
            BeforeSequence,
            AfterSequence,
            BeforeBlock,
            AfterBlock,
            AfterBody,
        }
    }
}