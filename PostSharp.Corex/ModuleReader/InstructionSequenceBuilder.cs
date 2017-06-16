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

using System.Collections;
using System.Reflection.Emit;
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Analyzes IL instructions of a method body and build linear instruction sequences.
    /// </summary>
    internal sealed class InstructionSequenceBuilder
    {
        #region Fields

        /// <summary>
        /// Array of bit where a byte is set at <i>x</i> position if and only if
        /// a sequence starts at offset <i>x</i>.
        /// </summary>
        private readonly BitArray branchPoints;

        /// <summary>
        /// Method body.
        /// </summary>
        private readonly MethodBodyDeclaration methodBody;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MethodBodyDeclaration"/>.
        /// </summary>
        /// <param name="methodBody">Method body.</param>
        private InstructionSequenceBuilder( MethodBodyDeclaration methodBody )
        {
            this.methodBody = methodBody;
            this.branchPoints = new BitArray( methodBody.OriginalInstructions.Size );
        }

        /// <summary>
        /// Builds the instruction sequences (<see cref="InstructionSequence"/>) of a
        /// <see cref="MethodBodyDeclaration"/>.
        /// </summary>
        /// <param name="methodBody">A <see cref="MethodBodyDeclaration"/>.</param>
        public static void BuildInstructionSequences( MethodBodyDeclaration methodBody )
        {
            InstructionSequenceBuilder builder = new InstructionSequenceBuilder( methodBody );
            builder.InternalBuildSequences();
        }

        /// <summary>
        /// Does the work.
        /// </summary>
        private void InternalBuildSequences()
        {
            // Initialize the reader
            InstructionReader reader = methodBody.CreateInstructionReader( false );
            InstructionSequence sequence = new InstructionSequence( this.methodBody )
                                               {
                                                   StartOffset = 0,
                                                   EndOffset = this.methodBody.OriginalInstructions.Size
                                               };
            reader.EnterInstructionSequence( sequence );

            // Read the instructions and set the disjoin and join points
            while ( reader.ReadInstruction() )
            {
                // Look for branching instructions
                switch ( OpCodeMap.GetFlowControl( reader.CurrentInstruction.OpCodeNumber ) )
                {
                    case FlowControl.Branch:
                    case FlowControl.Break:
                    case FlowControl.Cond_Branch:
                    case FlowControl.Return:
                    case FlowControl.Throw:
                        switch ( reader.CurrentInstruction.OperandType )
                        {
                            case OperandType.InlineBrTarget:
                                this.branchPoints.Set( reader.OffsetAfter + reader.CurrentInstruction.Int32Operand, true );
                                break;

                            case OperandType.ShortInlineBrTarget:
                                this.branchPoints.Set(
                                    reader.OffsetAfter + (sbyte) reader.CurrentInstruction.ByteOperand, true );
                                break;

                            case OperandType.InlineSwitch:
                                for ( int i = 0 ;
                                      i < reader.CurrentInstruction.UnresolvedSwitchTargetsOperand.Length ;
                                      i++ )
                                {
                                    this.branchPoints.Set(
                                        reader.OffsetAfter + reader.CurrentInstruction.UnresolvedSwitchTargetsOperand[i],
                                        true );
                                }
                                break;
                        }

                        if ( reader.OffsetAfter < this.methodBody.OriginalInstructions.Size )
                        {
                            this.branchPoints.Set( reader.OffsetAfter, true );
                        }
                        break;
                }
            }

            // Close the readers.
            reader.LeaveInstructionSequence();

            this.SetBlockStartPoint( this.methodBody.RootInstructionBlock );

            // Count the sequences
            int count = 0;
            for ( int i = 0 ; i < this.branchPoints.Length ; i++ )
            {
                if ( this.branchPoints[i] )
                {
                    count++;
                }
            }

            // Initialize the collection of sequences.
            this.methodBody.SetInstructionSequenceCapacity( count );

            // Create the sequences in their blocks.
            this.CreateSequencesForBlock( this.methodBody.RootInstructionBlock );
        }

        /// <summary>
        /// Recursively set the beginning of instruction blocks as beginning
        /// of instruction sequences.
        /// </summary>
        /// <param name="block">An <see cref="InstructionBlock"/>.</param>
        private void SetBlockStartPoint( InstructionBlock block )
        {
            this.branchPoints.Set( block.StartOffset, true );

            if ( block.HasChildrenBlocks )
            {
                InstructionBlock child = block.FirstChildBlock;
                while ( child != null )
                {
                    this.SetBlockStartPoint( child );
                    child = child.NextSiblingBlock;
                }
            }
        }


        /// <summary>
        /// Recursively create the instruction sequences for individual instruction blocks.
        /// </summary>
        /// <param name="block">An <see cref="InstructionBlock"/>.</param>
        private void CreateSequencesForBlock( InstructionBlock block )
        {
            if ( block.HasChildrenBlocks )
            {
                InstructionBlock child = block.FirstChildBlock;
                while ( child != null )
                {
                    CreateSequencesForBlock( child );
                    child = child.NextSiblingBlock;
                }
            }
            else
            {
                InstructionSequence previousSequence = null;

                // Iterate all bytes of this block and look for branching points.
                for ( int i = block.StartOffset ; i < block.EndOffset ; i++ )
                {
                    // Close the previous sequence
                    if ( previousSequence != null )
                    {
                        previousSequence.EndOffset = i;
                    }

                    // If the current point is a branch point, close the previous
                    // sequence and start a new one.
                    if ( this.branchPoints[i] )
                    {
                        // Create a sequence and register it in the method body.
                        InstructionSequence sequence = new InstructionSequence( this.methodBody ) {StartOffset = i};
                        this.methodBody.RegisterInstructionSequence( sequence );

                        // Add the instruction sequence in the current block.
                        block.AddInstructionSequence( sequence, NodePosition.After, null );

                        previousSequence = sequence;
                    }
                }

                if ( previousSequence != null )
                {
                    previousSequence.EndOffset = block.EndOffset;
                }
            }
        }
    }
}