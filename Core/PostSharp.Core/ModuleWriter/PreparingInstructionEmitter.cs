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

using System.Reflection;
using System.Reflection.Emit;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Extensibility;

namespace PostSharp.ModuleWriter
{
    /// <summary>
    /// Performs some preparing computations on a method body like
    /// determining address ranges of instruction sequences, missing symbols
    /// of local variables or unreferenced local variables.
    /// </summary>
    /// <remarks>
    /// <see cref="PreparingInstructionEmitter"/> is currently used during
    /// the process of writing IL code. It is used in a first pass to determined
    /// missing symbols on local variables, unreferenced local variables
    /// and principally ranges of intruction sequences addresses. Then it is used
    /// in the second pass to determine the final address of instruction sequences.
    /// </remarks>
    internal class PreparingInstructionEmitter : InstructionEmitter
    {
        #region Fields

        /// <summary>
        /// Range of the address of the current instruction.
        /// </summary>
        private AddressRange currentAddress = new AddressRange( 0, 0 );

        /// <summary>
        /// Ranges of all instruction sequences.
        /// </summary>
        private readonly AddressRange[] instructionSequenceRanges;

        public int[] instructionSequenceStackHeights;

        private int currentStackHeight;

        private int currentMaxStack;

        /// <summary>
        /// Determines whether undeterminate distances should be considered as far.
        /// </summary>
        private readonly bool undeterminateIsFar;

        /// <summary>
        /// Determines whether NOP instructons should be skipped.
        /// </summary>
        private bool ignoreNop;

        /// <summary>
        /// An <see cref="InstructionReader"/>.
        /// </summary>
        private InstructionReader reader;

        /// <summary>
        /// Determines, for each local variable, whether it is at least one referenced
        /// in a lexical scope where there is no symbol for this local variable.
        /// </summary>
        protected bool[] localVariableHasMissingSymbol;

        /// <summary>
        /// Determines, for each local variable, whether it is referenced at least once.
        /// </summary>
        protected bool[] localVariableHasReference;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="PreparingInstructionEmitter"/>.
        /// </summary>
        /// <param name="body">The <see cref="MethodBodyDeclaration"/> whose instructions
        /// have to be emitted.</param>
        /// <remarks>
        /// This constructor is used during the first pass of an IL compilation.
        /// </remarks>
        internal PreparingInstructionEmitter( MethodBodyDeclaration body )
            : this( body,
                    new AddressRange[body.InstructionSequenceCount],
                    new bool[body.LocalVariableCount],
                    new bool[body.LocalVariableCount],
                    false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="PreparingInstructionEmitter"/> with the values
        /// of a previous run of <see cref="Prepare"/>.
        /// </summary>
        /// <param name="body">The <see cref="MethodBodyDeclaration"/> whose instructions
        /// have to be emitted.</param>
        /// <param name="instructionSequenceRanges">Range of instruction sequence addresses.</param>
        /// <param name="localVariableHasMissingSymbol">Determines, for each local variable, whether it is at least one referenced
        /// in a lexical scope where there is no symbol for this local variable.</param>
        /// <param name="localVariableHasReference">Determines, for each local variable, whether it is referenced at least once.</param>
        /// <param name="undeterminateIsFar">Determines whether undeterminate distances should be considered as far.</param>
        protected PreparingInstructionEmitter( MethodBodyDeclaration body,
                                               AddressRange[] instructionSequenceRanges,
                                               bool[] localVariableHasMissingSymbol,
                                               bool[] localVariableHasReference,
                                               bool undeterminateIsFar )
            : base( body, false )
        {
            this.undeterminateIsFar = undeterminateIsFar;
            this.instructionSequenceRanges = instructionSequenceRanges;
            this.localVariableHasMissingSymbol = localVariableHasMissingSymbol;
            this.localVariableHasReference = localVariableHasReference;

            if ( body.MaxStack == MethodBodyDeclaration.RecomputeMaxStack )
            {
                this.instructionSequenceStackHeights = new int[body.InstructionSequenceCount];
                for ( int i = 0; i < this.instructionSequenceStackHeights.Length; i++ )
                {
                    this.instructionSequenceStackHeights[i] = -1;
                }
            }
        }

        /// <summary>
        /// Determines whether the NOP instructions should be ignored.
        /// </summary>
        public bool IgnoreNop
        {
            get { return this.ignoreNop; }
            set { this.ignoreNop = value; }
        }

        /// <summary>
        /// Performs some computations on the method body instructions: predicted ranges of 
        /// instruction sequences addresses, missing local variable symbols and
        /// unreferenced local variables.
        /// </summary>
        /// <param name="instructionSequenceRanges">Range of instruction sequence addresses.</param>
        /// <param name="localVariableHasMissingSymbol">Determines, for each local variable, whether it is at least one referenced
        /// in a lexical scope where there is no symbol for this local variable.</param>
        /// <param name="localVariableHasReference">Determines, for each local variable, whether it is referenced at least once.</param>
        public void Prepare( out AddressRange[] instructionSequenceRanges,
                             out bool[] localVariableHasMissingSymbol,
                             out bool[] localVariableHasReference )
        {
            this.reader = this.MethodBody.CreateInstructionReader();
            ProcessBlock( this.MethodBody.RootInstructionBlock );
            if ( this.MethodBody.MaxStack == MethodBodyDeclaration.RecomputeMaxStack )
            {
                this.MethodBody.MaxStack = this.currentMaxStack;
            }
            this.reader = null;
            instructionSequenceRanges = this.instructionSequenceRanges;
            localVariableHasReference = this.localVariableHasReference;
            localVariableHasMissingSymbol = this.localVariableHasMissingSymbol;
        }

        /// <summary>
        /// Processes recursively an <see cref="InstructionBlock"/>.
        /// </summary>
        /// <param name="block">An <see cref="InstructionBlock"/>.</param>
        private void ProcessBlock( InstructionBlock block )
        {
            reader.EnterInstructionBlock( block );

            if ( block.HasLocalVariableSymbols )
            {
                foreach ( LocalVariableSymbol symbol in block.LocalVariableSymbols )
                {
                    this.localVariableHasReference[symbol.LocalVariable.Ordinal] = true;
                }
            }

            if ( block.HasChildrenBlocks )
            {
                // Process children.

                InstructionBlock child = block.FirstChildBlock;
                while ( child != null )
                {
                    if ( !child.IsExceptionHandler )
                    {
                        ProcessBlock( child );
                    }

                    child = child.NextSiblingBlock;
                }
            }
            else if ( block.HasInstructionSequences )
            {
                // Process all instruction sequences.

                InstructionSequence sequence = block.FirstInstructionSequence;
                while ( sequence != null )
                {
                    this.reader.EnterInstructionSequence( sequence );

                    // Set stack height
                    if ( this.instructionSequenceStackHeights != null )
                    {
                        if ( this.instructionSequenceStackHeights[sequence.Token] != -1 )
                        {
                            this.currentStackHeight = this.instructionSequenceStackHeights[sequence.Token];
                        }

                        Trace.InstructionEmitter.WriteLine( "Entering sequence with StackHeight={0}.",
                                                            this.currentStackHeight );
                    }


                    // Set the address of the current instruction sequence.
                    this.instructionSequenceRanges[sequence.Token] = this.currentAddress;

                    // Process all instructions of this instruction sequence.
                    while ( reader.ReadInstruction() )
                    {
                        #region Inspect local variables

                        // Uncompress the instruction and determines
                        // whether the operand is a local variable.
                        OpCodeNumber uncompressedOpCodeNumber;

                        short localVariableOrdinal = -1;
                        UncompressedOpCode uncompressedOpCode =
                            OpCodeMap.GetUncompressedOpCode( reader.CurrentInstruction.OpCodeNumber );

                        if ( !uncompressedOpCode.IsNull )
                        {
                            if ( uncompressedOpCode.OperandType == OperandType.InlineVar &&
                                 !OpCodeMap.IsParameterOperand( uncompressedOpCode.OpCodeNumber ) )
                            {
                                localVariableOrdinal = (short) uncompressedOpCode.Operand;
                            }

                            // TODO: Check meaning of this variable.
                            uncompressedOpCodeNumber = uncompressedOpCode.OpCodeNumber;
                        }
                        else
                        {
                            uncompressedOpCodeNumber = reader.CurrentInstruction.OpCodeNumber;

                            switch ( reader.CurrentInstruction.OperandType )
                            {
                                case OperandType.ShortInlineVar:
                                    if ( !OpCodeMap.IsParameterOperand( uncompressedOpCodeNumber ) )
                                    {
                                        localVariableOrdinal = reader.CurrentInstruction.ByteOperand;
                                    }
                                    break;

                                case OperandType.InlineVar:
                                    if ( !OpCodeMap.IsParameterOperand( uncompressedOpCodeNumber ) )
                                    {
                                        localVariableOrdinal = reader.CurrentInstruction.Int16Operand;
                                    }
                                    break;
                            }
                        }

                        if ( localVariableOrdinal != -1 )
                        {
                            this.localVariableHasReference[localVariableOrdinal] = true;

                            if ( reader.GetLocalVariableSymbol( localVariableOrdinal ) == null )
                            {
                                Trace.ModuleWriter.WriteLine(
                                    "Reference to local variable with missing symbol in block {{{0}}}, instruction {{{1}}}, local {{{2}}}.",
                                    block, reader.CurrentInstruction, localVariableOrdinal );
                                this.localVariableHasMissingSymbol[localVariableOrdinal] = true;
                            }
                        }

                        #endregion

                        #region Determines MaxStack

                        if ( this.MethodBody.MaxStack == MethodBodyDeclaration.RecomputeMaxStack )
                        {
                            StackBehaviour stackBehaviour =
                                OpCodeMap.GetStackBehaviourPop( reader.CurrentInstruction.OpCodeNumber );
                            switch ( stackBehaviour )
                            {
                                case StackBehaviour.Pop0:
                                    break;

                                case StackBehaviour.Pop1:
                                case StackBehaviour.Popi:
                                case StackBehaviour.Popref:
                                    this.currentStackHeight--;
                                    break;

                                case StackBehaviour.Pop1_pop1:
                                case StackBehaviour.Popi_pop1:
                                case StackBehaviour.Popi_popi:
                                case StackBehaviour.Popi_popi8:
                                case StackBehaviour.Popi_popr4:
                                case StackBehaviour.Popi_popr8:
                                case StackBehaviour.Popref_pop1:
                                case StackBehaviour.Popref_popi:
                                    this.currentStackHeight -= 2;
                                    break;


                                case StackBehaviour.Popi_popi_popi:
                                case StackBehaviour.Popref_popi_pop1:
                                case StackBehaviour.Popref_popi_popi:
                                case StackBehaviour.Popref_popi_popi8:
                                case StackBehaviour.Popref_popi_popr4:
                                case StackBehaviour.Popref_popi_popr8:
                                case StackBehaviour.Popref_popi_popref:
                                    this.currentStackHeight -= 3;
                                    break;

                                case StackBehaviour.Varpop:
                                    switch ( reader.CurrentInstruction.OpCodeNumber )
                                    {
                                        case OpCodeNumber.Newobj:
                                            this.currentStackHeight -=
                                                reader.CurrentInstruction.MethodOperand.ParameterCount;
                                            break;

                                        case OpCodeNumber.Calli:
                                            this.currentStackHeight -=
                                                    reader.CurrentInstruction.SignatureOperand.MethodSignature.ParameterTypes.Count;

                                            if ((reader.CurrentInstruction.SignatureOperand.MethodSignature.CallingConvention &
                                                   CallingConvention.HasThis) != 0)
                                            {
                                                this.currentStackHeight--;
                                            }
                                            break;

                                        case OpCodeNumber.Call:
                                        case OpCodeNumber.Callvirt:
                                            this.currentStackHeight -=
                                                reader.CurrentInstruction.MethodOperand.ParameterCount;

                                            if ( ( reader.CurrentInstruction.MethodOperand.CallingConvention &
                                                   CallingConvention.HasThis ) != 0 )
                                            {
                                                this.currentStackHeight--;
                                            }
                                            break;

                                        case OpCodeNumber.Ret:
                                            this.currentStackHeight = 0;
                                            break;

                                        default:
                                            throw ExceptionHelper.Core.CreateAssertionFailedException(
                                                "UnexpectedInstructionWithVarPop",
                                                reader.CurrentInstruction.OpCodeNumber );
                                    }
                                    break;

                                default:
                                    throw ExceptionHelper.CreateInvalidEnumerationValueException( stackBehaviour,
                                                                                                  "OpCodeMap.GetStackBehaviourPop()" );
                            }

                            switch ( OpCodeMap.GetStackBehaviourPush( reader.CurrentInstruction.OpCodeNumber ) )
                            {
                                case StackBehaviour.Push0:
                                    break;

                                case StackBehaviour.Push1:
                                case StackBehaviour.Pushi:
                                case StackBehaviour.Pushi8:
                                case StackBehaviour.Pushr4:
                                case StackBehaviour.Pushr8:
                                case StackBehaviour.Pushref:
                                    this.currentStackHeight++;
                                    break;

                                case StackBehaviour.Push1_push1:
                                    this.currentStackHeight += 2;
                                    break;

                                case StackBehaviour.Varpush:
                                    switch ( reader.CurrentInstruction.OpCodeNumber )
                                    {
                                        case OpCodeNumber.Calli:
                                            if (
                                              !IntrinsicTypeSignature.Is(
                                                   reader.CurrentInstruction.SignatureOperand.MethodSignature.ReturnType,
                                                   IntrinsicType.Void))
                                            {
                                                this.currentStackHeight++;
                                            }
                                            break;
                                       
                                        case OpCodeNumber.Call:
                                        case OpCodeNumber.Callvirt:

                                            if (
                                                !IntrinsicTypeSignature.Is(
                                                     reader.CurrentInstruction.MethodOperand.ReturnType,
                                                     IntrinsicType.Void ) )
                                            {
                                                this.currentStackHeight++;
                                            }
                                            break;

                                        default:
                                            throw ExceptionHelper.Core.CreateAssertionFailedException(
                                                "UnexpectedInstructionWithVarPush",
                                                reader.CurrentInstruction.OpCodeNumber );
                                    }
                                    break;

                                default:
                                    throw ExceptionHelper.CreateInvalidEnumerationValueException( stackBehaviour,
                                                                                                  "OpCodeMap.GetStackBehaviourPush()" );
                            }

                            Trace.InstructionEmitter.WriteLine( "StackHeight = {0}.", this.currentStackHeight );


                            if ( this.currentStackHeight < 0 )
                            {
                                CoreMessageSource.Instance.Write( SeverityType.Warning,
                                                                  "PS0050", new object[]
                                                                                {
                                                                                    this.MethodBody.Method.ToString(),
                                                                                    reader.CurrentInstruction.
                                                                                        OpCodeNumber,
                                                                                    this.currentStackHeight
                                                                                } );
                                this.currentStackHeight = 0;
                            }

                            if ( this.currentStackHeight > this.currentMaxStack )
                            {
                                this.currentMaxStack = this.currentStackHeight;
                            }

                            switch ( OpCodeMap.GetFlowControl( reader.CurrentInstruction.OpCodeNumber ) )
                            {
                                case FlowControl.Branch:
                                    this.instructionSequenceStackHeights[
                                        reader.CurrentInstruction.BranchTargetOperand.Token] = this.currentStackHeight;
                                    this.currentStackHeight = 0;
                                    break;

                                case FlowControl.Cond_Branch:
                                    if ( reader.CurrentInstruction.OpCodeNumber != OpCodeNumber.Switch )
                                    {
                                        this.instructionSequenceStackHeights[
                                            reader.CurrentInstruction.BranchTargetOperand.Token] =
                                            this.currentStackHeight;
                                    }
                                    else
                                    {
                                        for ( int i = 0; i < reader.CurrentInstruction.SwitchOperandTargetCount; i++ )
                                        {
                                            this.instructionSequenceStackHeights[
                                                reader.CurrentInstruction.GetSwitchOperandTarget( i ).Token] =
                                                this.currentStackHeight;
                                        }
                                    }
                                    break;

                                case FlowControl.Throw:
                                    this.currentStackHeight = 0;
                                    break;
                            }
                        }

                        #endregion

                        reader.CurrentInstruction.Write( this );
                    }

                    this.reader.LeaveInstructionSequence();
                    sequence = sequence.NextSiblingSequence;
                }
            }


            reader.LeaveInstructionBlock();

            // Process exception handlers.
            if ( block.HasExceptionHandlers )
            {
                ExceptionHandler handler = block.FirstExceptionHandler;
                while ( handler != null )
                {
                    if ( handler.FilterBlock != null )
                    {
                        this.currentStackHeight = 1;
                        ProcessBlock( handler.FilterBlock );
                        this.currentStackHeight = 1;
                    }

                    if ( handler.Options == ExceptionHandlingClauseOptions.Clause )
                    {
                        this.currentStackHeight = 1;
                    }

                    ProcessBlock( handler.HandlerBlock );

                    handler = handler.NextSiblingExceptionHandler;
                }
            }
        }


        /// <summary>
        /// Gets the size of an opcode (without the operand).
        /// </summary>
        /// <param name="code">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The size of <paramref name="code"/> in bytes.</returns>
        private static int GetInstructionSize( OpCodeNumber code )
        {
            if ( ( (int) code & 0xFF00 ) == 0 )
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        /// <inheritdoc />
        protected override void InternalEmitPrefix( InstructionPrefixes prefix )
        {
            if ( prefix == InstructionPrefixes.None )
            {
                return;
            }

            int size = 0;

            if ( ( prefix & InstructionPrefixes.Tail ) != 0 )
            {
                size += 2;
            }

            if ( ( prefix & InstructionPrefixes.Volatile ) != 0 )
            {
                size += 2;
            }

            if ( ( prefix & InstructionPrefixes.UnalignedMask ) != 0 )
            {
                size += 3;
            }

            this.currentAddress += size;
        }


        /// <inheritdoc />
        protected override void InternalEmitInstructionDouble( OpCodeNumber code, double operand )
        {
            this.currentAddress += GetInstructionSize( code ) + sizeof(double);
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionSingle( OpCodeNumber code, float operand )
        {
            this.currentAddress += GetInstructionSize( code ) + sizeof(float);
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionInt32( OpCodeNumber code, int operand )
        {
            this.currentAddress += GetInstructionSize( code ) + sizeof(int);
        }


        /// <inheritdoc />
        protected override void InternalEmitInstructionInt64( OpCodeNumber code, long operand )
        {
            this.currentAddress += GetInstructionSize( code ) + sizeof(long);
        }


        /// <inheritdoc />
        protected override void InternalEmitInstructionField( OpCodeNumber code, IField field )
        {
            this.currentAddress += GetInstructionSize( code ) + 4;
        }


        /// <inheritdoc />
        protected override void InternalEmitInstructionMethod( OpCodeNumber code, IMethod method )
        {
            this.currentAddress += GetInstructionSize( code ) + 4;
        }


        /// <inheritdoc />
        protected override void InternalEmitInstructionType( OpCodeNumber code, ITypeSignature type )
        {
            this.currentAddress += GetInstructionSize( code ) + 4;
        }


        /// <inheritdoc />
        protected override void InternalEmitInstructionInt16( OpCodeNumber code, short operand )
        {
            this.currentAddress += GetInstructionSize( code ) + sizeof(short);
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionByte( OpCodeNumber code, byte operand )
        {
            this.currentAddress += GetInstructionSize( code ) + sizeof(byte);
        }


        /// <inheritdoc />
        protected override AddressDistance InternalEmitBranchingInstruction( OpCodeNumber nearCode, OpCodeNumber farCode,
                                                                             InstructionSequence sequence )
        {
            int nearSize = GetInstructionSize( nearCode ) + 1;

            AddressDistance distance =
                ( this.currentAddress + nearSize - instructionSequenceRanges[sequence.Token] ).Distance;
            switch ( distance )
            {
                case AddressDistance.Near:
                    this.currentAddress += nearSize;
                    return AddressDistance.Near;

                case AddressDistance.Far:
                    this.currentAddress += GetInstructionSize( farCode ) + 4;
                    return AddressDistance.Far;

                case AddressDistance.Undeterminate:
                    if ( this.undeterminateIsFar )
                    {
                        this.currentAddress += GetInstructionSize( farCode ) + 4;
                        return AddressDistance.Far;
                    }
                    else
                    {
                        this.currentAddress += new AddressRange( nearSize, GetInstructionSize( farCode ) + 4 );
                        return AddressDistance.Undeterminate;
                    }

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( distance, "distance" );
            }
        }


        /// <inheritdoc />
        protected override void InternalEmitInstruction( OpCodeNumber code )
        {
            if ( code != OpCodeNumber.Nop || !this.ignoreNop )
            {
                this.currentAddress += GetInstructionSize( code );
            }
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionSignature( OpCodeNumber code,
                                                                  StandaloneSignatureDeclaration signature )
        {
            this.currentAddress += GetInstructionSize( code ) + 4;
        }

        /// <inheritdoc />
        protected override void InternalEmitSwitchInstruction( InstructionSequence[] switchTargets )
        {
            this.currentAddress += GetInstructionSize( OpCodeNumber.Switch ) + 4*( 1 + switchTargets.Length );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionString( OpCodeNumber code, LiteralString operand )
        {
            this.currentAddress += GetInstructionSize( code ) + 4;
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionParameter( OpCodeNumber code, ParameterDeclaration parameter,
                                                                  OperandType operandType )
        {
            this.currentAddress += GetInstructionSize( code ) +
                                   ( operandType == OperandType.ShortInlineVar ? sizeof(byte) : sizeof(short) );
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionLocalVariable( OpCodeNumber code,
                                                                      LocalVariableSymbol localVariableSymbol,
                                                                      OperandType operandType )
        {
            this.currentAddress += GetInstructionSize( code ) +
                                   ( operandType == OperandType.ShortInlineVar ? sizeof(byte) : sizeof(short) );
        }

        /// <inheritdoc />
        protected override void InternalEmitPrefixType( OpCodeNumber prefix, ITypeSignature type )
        {
            this.currentAddress += GetInstructionSize( prefix ) + 4;
        }
    }
}