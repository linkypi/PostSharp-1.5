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
using System.Diagnostics;
using System.Reflection.Emit;
using PostSharp.ModuleReader;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Reads a stream of binary IL instructions and debugging sequence points.
    /// </summary>
    /// <remarks>
    /// <para>
    ///		Each <see cref="InstructionReader"/> instance is assigned to
    ///		a single method. Developers must use the 
    ///		<see cref="MethodBodyDeclaration.CreateInstructionReader()"/> method
    ///		to get an <see cref="InstructionReader"/> instance.
    /// </para>
    /// <para>
    ///		When you want to read an instruction block, you first need to <i>enter</i>
    ///		it using the <see cref="InstructionReader.EnterInstructionBlock"/> method. The
    ///		<see cref="InstructionReader.EnterInstructionBlock"/> method expect that the active
    ///		block of the <see cref="InstructionReader"/> is the parent of the block
    ///		you want to enter, so you have to enter all ascendant blocks, from
    ///		the root to the leave, like in a stack model. Then you attach the reader
    ///		to a sequence using <see cref="InstructionReader.EnterInstructionSequence"/>.
    ///		To read another sequence, you need first to use 
    ///		<see cref="InstructionReader.LeaveInstructionSequence"/>, then
    ///		again <see cref="InstructionReader.EnterInstructionSequence"/>. Use
    ///		<see cref="InstructionReader.LeaveInstructionBlock"/> to leave a block.
    /// </para>
    /// <para>
    ///		If you want to enter a specific block without entering all its ancestors,
    ///		use the <see cref="InstructionReader.JumpToInstructionBlock"/> method, which will
    ///		make the proper calls to <see cref="InstructionReader.LeaveInstructionBlock"/>
    ///		and <see cref="InstructionReader.EnterInstructionBlock"/> automatically.
    /// </para>
    /// <para>
    ///		When the <see cref="InstructionReader"/> has entered an
    ///		<see cref="InstructionSequence"/>, you can use it as a classic reader.
    ///		The <see cref="ReadInstruction"/> reads the next
    ///		instruction and you can read it on the instance properties of
    ///		the <see cref="InstructionReader"/>.
    /// </para>
    /// </remarks>
    public sealed class InstructionReader : IDisposable
    {
        #region Fields

        /// <summary>
        /// A <see cref="BufferReader"/> reused in this instance.
        /// </summary>
        private readonly BufferReader bufferReader = new BufferReader();

        /// <summary>
        /// The parent module.
        /// </summary>
        private readonly ModuleDeclaration module;

        /// <summary>
        /// A stack mapping local variable ordinal to local variable symbols,
        /// or to <b>null</b> if there is no symbol for the given ordinal.
        /// </summary>
        private readonly Stack<Dictionary<int, LocalVariableSymbol>> localVariableSymbolsStack;


        /// <summary>
        /// Current method body.
        /// </summary>
        private readonly MethodBodyDeclaration methodBody;

        /// <summary>
        /// Current block.
        /// </summary>
        private InstructionBlock currentBlock;

        /// <summary>
        /// Current sequence.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionSequence"/>, or <b>null</b> if the reader
        /// is not positioned in an <see cref="InstructionSequence"/>.
        /// </value>
        private InstructionSequence currentSequence;

        private State currentState;

        private readonly bool resolveSymbols;

        private readonly Instruction currentInstruction;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="InstructionReader"/> 
        /// and assigns it to a <see cref="MethodBodyDeclaration"/>.
        /// </summary>
        /// <param name="methodBody">The <see cref="MethodBodyDeclaration"/> to which
        /// the reader will be assigned.</param>
        /// <param name="resolveSymbols">Whether the instruction reader should resolve
        /// local variable symbols.</param>
        internal InstructionReader( MethodBodyDeclaration methodBody, bool resolveSymbols )
        {
            Trace.InstructionReader.WriteLine( "Opening InstructionReader for {0}.", methodBody.Method );

            this.currentState.NextSequencePointOffset = -1;
            this.currentState.NextSequencePointOrdinal = -1;
            this.resolveSymbols = resolveSymbols;
            this.methodBody = methodBody;
            this.module = methodBody.Module;
            this.currentInstruction = new Instruction( this );

            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.module != null, "DeclarationShouldBeInModule" );

            #endregion

            if ( resolveSymbols )
            {
                this.localVariableSymbolsStack = new Stack<Dictionary<int, LocalVariableSymbol>>( 8 );
                this.localVariableSymbolsStack.Push( new Dictionary<int, LocalVariableSymbol>() );
            }
        }

        #region Position

        /// <summary>
        /// Gets the current instruction.
        /// </summary>
        /// <remarks>
        /// You always get the same instance of the <see cref="Instruction"/> class, so it is useless
        /// to "store" it somewhere for later use. Once the cursor has moved, the returned instance
        /// will be modified.
        /// </remarks>
        public Instruction CurrentInstruction { get { return this.currentInstruction; } }

        /// <summary>
        /// Gets the <see cref="InstructionSequence"/> on which the current <see cref="InstructionReader"/>
        /// is positioned.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionSequence"/>, or <b>null</b> if the current <see cref="InstructionReader"/>
        /// is not positioned at an <see cref="InstructionSequence"/>.
        /// </value>
        public InstructionSequence CurrentInstructionSequence { get { return this.currentSequence; } }

        /// <summary>
        /// Gets the <see cref="InstructionBlock"/> on which the current <see cref="InstructionReader"/>
        /// is positioned.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if the current <see cref="InstructionReader"/>
        /// is not positioned at an <see cref="InstructionBlock"/>.
        /// </value>
        public InstructionBlock CurrentInstructionBlock { get { return this.currentBlock; } }

        /// <summary>
        /// Gets the <see cref="MethodBodyDeclaration"/> to which the current 
        /// <see cref="InstructionReader"/> is assigned.
        /// </summary>
        public MethodBodyDeclaration MethodBody { get { return this.methodBody; } }

        /// <summary>
        /// Gets the module to which this <see cref="InstructionReader"/> is associates.
        /// </summary>
        public ModuleDeclaration Module { get { return this.module; } }

        /// <summary>
        /// Gets the IL offset before the current instruction (relatively to the start of the current
        /// <see cref="InstructionSequence"/>).
        /// </summary>
        internal int OffsetBefore { get { return this.currentState.OffsetBefore; } }

        /// <summary>
        /// Gets the IL offset after the current instruction (relatively to the start of the current
        /// <see cref="InstructionSequence"/>).
        /// </summary>
        internal int OffsetAfter { get { return this.currentState.OffsetAfter; } }

        /// <summary>
        /// Determines whether the current instruction is the last of the sequence.
        /// </summary>
        public bool IsAtEnfOfSequence { get { return this.currentState.OffsetAfter >= this.currentSequence.ByteCount; } }

        /// <summary>
        /// Determines wether resolution of local variable symbols is enabled for the current reader.
        /// </summary>
        public bool IsSymbolResolutionEnabled
        {
            get { return this.resolveSymbols; }
        }

        /// <summary>
        /// Gets the <see cref="LocalVariableSymbol"/> of a local variable given its ordinal
        /// and optionally creates default symbol is no symbol is associated to the given
        /// ordinal in the current context.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="createDefault">Determines whether a default 
        /// <see cref="LocalVariableSymbol"/> should be created is none is associated to
        /// the given ordinal in the current context.</param>
        /// <returns>The <see cref="LocalVariableSymbol"/> of the local variable at
        /// position <paramref name="ordinal"/>, or <b>null</b> if no symbol is associated to
        /// the given ordinal in the current context and the <paramref name="createDefault"/>
        /// parameter is false.</returns>
        public LocalVariableSymbol GetLocalVariableSymbol( int ordinal, bool createDefault )
        {
            #region Preconditions

            this.EnsureTracksSymbol();

            #endregion

            LocalVariableSymbol value;
            Dictionary<int, LocalVariableSymbol> symbols = this.localVariableSymbolsStack.Peek();
            if ( !symbols.TryGetValue( ordinal, out value ) )
            {
                if ( createDefault )
                {
                    value = new LocalVariableSymbol( this.methodBody.GetLocalVariable( ordinal ), null );
                    symbols[ordinal] = value;
                }
                else
                {
                    return null;
                }
            }
            return value;
        }

        /// <summary>
        /// Gets the <see cref="LocalVariableSymbol"/> associated to a given ordinal
        /// in the current context.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns></returns>
        public LocalVariableSymbol GetLocalVariableSymbol( int ordinal )
        {
            return this.GetLocalVariableSymbol( ordinal, false );
        }

        #endregion

        #region Navigation

        [Conditional( "ASSERT" )]
        internal void EnsureTracksSymbol()
        {
            ExceptionHelper.Core.AssertValidOperation( this.resolveSymbols, "InstructionReaderDoesNotTrackSymbols" );
        }

        /// <summary>
        /// Prepares the current <see cref="InstructionReader"/> so that it
        /// can read a specific block, without assumption on
        /// the current state of the <see cref="InstructionReader"/>.
        /// </summary>
        /// <param name="block">The target <see cref="InstructionBlock"/>.</param>
        /// <exception cref="ArgumentException">The target instruction block
        /// do not belong to the method body to which the current <see cref="InstructionReader"/>
        /// is attached.</exception>
        /// <remarks>
        /// For better performances, use <see cref="EnterInstructionBlock"/> and 
        /// <see cref="LeaveInstructionBlock"/>.
        /// </remarks>
        public void JumpToInstructionBlock( InstructionBlock block )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( block, "block" );
            ExceptionHelper.Core.AssertValidArgument( block.MethodBody == this.methodBody, "block",
                                                      "InstructionBlockOutsideMethodBody" );
            this.EnsureTracksSymbol();

            #endregion

            if ( this.currentSequence != null )
            {
                this.LeaveInstructionSequence();
            }

            if ( block != this.currentBlock )
            {
                // We have to determine leave all the blocks until we find
                // the common ancestor, then enter all parent blocks of the new block.

                InstructionBlock commonAncestor;

                if ( this.currentBlock == null )
                {
                    commonAncestor = null;
                }
                else
                {
                    commonAncestor = InstructionBlock.FindCommonAncestor( this.currentBlock, block );
                    while ( this.currentBlock != commonAncestor )
                    {
                        this.LeaveInstructionBlock();
                    }
                }

                Stack<InstructionBlock> ancestors = new Stack<InstructionBlock>( 10 );
                InstructionBlock cursor = block;
                while ( cursor != commonAncestor )
                {
                    ancestors.Push( cursor );
                    cursor = cursor.ParentBlock;
                }
                while ( ancestors.Count > 0 )
                {
                    this.EnterInstructionBlock( ancestors.Pop() );
                }
            }
        }

        /// <summary>
        /// Enters an <see cref="InstructionBlock"/>.
        /// </summary>
        /// <remarks>
        /// Prepares the current <see cref="InstructionReader"/> so that it
        /// can read a specific <see cref="InstructionBlock"/>, with the assumption that
        /// the <see cref="InstructionReader"/> is in the parent of the block
        /// to open.
        /// </remarks>
        /// <param name="block">The <see cref="InstructionBlock"/> to open.</param>
        /// <exception cref="InvalidOperationException">
        ///		The current <see cref="InstructionReader"/> is currently at
        ///		an invalid position. It should be open on the parent 
        ///		of <paramref name="block"/>.
        ///	</exception>
        public void EnterInstructionBlock( InstructionBlock block )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( block, "block" );
            ExceptionHelper.Core.AssertValidOperation(
                block.ParentBlock == this.currentBlock && this.currentSequence == null,
                "InstructionReaderInvalidPosition" );
            this.EnsureTracksSymbol();

            #endregion

            Trace.InstructionReader.WriteLine( "Entering block {0}.", block );


            this.currentBlock = block;

            if ( block.HasLocalVariableSymbols )
            {
                Dictionary<int, LocalVariableSymbol> localVariableSymbols =
                    new Dictionary<int, LocalVariableSymbol>( this.localVariableSymbolsStack.Peek() );

                for ( int i = 0 ; i < block.LocalVariableSymbolCount ; i++ )
                {
                    LocalVariableSymbol symbol = block.GetLocalVariableSymbol( i );
                    localVariableSymbols[symbol.LocalVariable.Ordinal] = symbol;
                }

                this.localVariableSymbolsStack.Push( localVariableSymbols );
            }
        }

        /// <summary>
        /// Leaves the active <see cref="InstructionBlock"/>, and sets the position
        /// of the current <see cref="InstructionReader"/> to
        /// the parent block of the active block.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current <see cref="InstructionReader"/> is not positioned in
        /// an <see cref="InstructionBlock"/>.
        /// </exception>
        public void LeaveInstructionBlock()
        {
            #region Preconditions

            this.EnsureTracksSymbol();
            ExceptionHelper.Core.AssertValidOperation( this.currentBlock != null && this.currentSequence == null,
                                                       "InstructionReaderInvalidPosition" );

            #endregion

            if ( this.currentBlock.HasLocalVariableSymbols )
            {
                this.localVariableSymbolsStack.Pop();
            }

            this.currentBlock = this.currentBlock.ParentBlock;
        }

        /// <summary>
        /// Enters an <see cref="InstructionSequence"/>.
        /// </summary>
        /// <remarks>
        /// Prepares the current <see cref="InstructionReader"/> so that it
        /// can read a specific <see cref="InstructionSequence"/>, with the assumption that
        /// the <see cref="InstructionReader"/> is in the parent of the sequence
        /// to open.
        /// </remarks>
        /// <param name="sequence">The <see cref="InstructionSequence"/> to open.</param>
        /// <exception cref="InvalidOperationException">
        ///		The current <see cref="InstructionReader"/> is currently at
        ///		an invalid position. It should be open on the parent block
        ///		of <paramref name="sequence"/>.
        ///	</exception>
        public void EnterInstructionSequence( InstructionSequence sequence )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sequence, "sequence" );
            ExceptionHelper.Core.AssertValidOperation(
                !this.resolveSymbols ||
                ( this.currentSequence == null && this.currentBlock == sequence.ParentInstructionBlock ),
                "InstructionReaderInvalidPosition" );

            #endregion

            Trace.InstructionReader.WriteLine( "Entering sequence {0}.", sequence );

            this.currentSequence = sequence;

            if ( this.currentSequence.IsOriginal )
            {
                this.bufferReader.Initialize(
                    (IntPtr) ( this.methodBody.OriginalInstructions.Origin.ToInt64() + sequence.StartOffset ),
                    sequence.EndOffset - sequence.StartOffset );

                // Detect the next sequence point.
                this.currentState.NextSequencePointOrdinal = this.currentSequence.FirstOriginalSymbolSequencePoint;
                if ( this.currentState.NextSequencePointOrdinal >= 0 )
                {
                    this.currentState.NextSequencePointOffset =
                        this.methodBody.OriginalSymbolSequencePoints[this.currentState.NextSequencePointOrdinal].Offset;
                }
                else
                {
                    this.currentState.NextSequencePointOffset = -1;
                }
            }
            else
            {
                this.bufferReader.Initialize( this.currentSequence.ModifiedInstructionBytes );
                this.currentState.NextSequencePointOffset = -1;
                this.currentState.NextSequencePointOrdinal = -1;
            }

            this.currentState.OffsetBefore = 0;
        }

        /// <summary>
        /// Leaves the active <see cref="InstructionSequence"/>, and sets the position
        /// of the current <see cref="InstructionReader"/> to
        /// the parent block of the active sequence.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current <see cref="InstructionReader"/> is not positioned in
        /// an <see cref="InstructionSequence"/>.
        /// </exception>
        public void LeaveInstructionSequence()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.currentSequence != null, "InstructionReaderInvalidPosition" );

            #endregion

            this.currentSequence = null;
        }

        /// <summary>
        /// Reads the next instruction of the active <see cref="InstructionSequence"/>.
        /// </summary>
        /// <returns><b>true</b> if some instruction could be read,
        /// or <b>false</b> if the end of the instruction sequence was reached.</returns>
        /// <exception cref="InvalidOperationException">
        /// The current <see cref="InstructionReader"/> is not positioned in
        /// an <see cref="InstructionSequence"/>.
        /// </exception>
        public bool ReadInstruction()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.currentSequence != null, "InstructionReaderInvalidPosition" );

            #endregion

            if ( this.bufferReader.Offset >= this.bufferReader.Size )
            {
                return false;
            }

            try
            {
                this.currentState.OffsetBefore = this.bufferReader.Offset;

                #region Read the prefix and the instruction

                bool readNext;
                InstructionPrefixes instructionPrefix = InstructionPrefixes.None;
                OpCodeNumber opCode;
                this.currentInstruction.SymbolSequencePoint = null;

                do
                {
                    byte currentByte = this.bufferReader.ReadByte();

                    if ( currentByte == 0xFE )
                    {
                        // Two-byte opCode.
                        currentByte = this.bufferReader.ReadByte();
                        opCode = (OpCodeNumber) ( 0xFE00 | currentByte );

                        Trace.InstructionReader.WriteLine( "Read instruction {0}.", opCode );

                        if ( opCode == OpCodeNumber._SequencePoint )
                        {
                            int symbolToken = this.bufferReader.ReadInt16();
                            if ( symbolToken == -1 )
                            {
                                this.currentInstruction.SymbolSequencePoint =
                                    SymbolSequencePoint.Hidden;
                                this.currentInstruction.LastSymbolSequencePoint =
                                    SymbolSequencePoint.Hidden;
                            }
                            else
                            {
                                this.currentInstruction.SymbolSequencePoint =
                                    this.methodBody.GetAdditionalSequencePoint(
                                        symbolToken );
                                this.currentInstruction.LastSymbolSequencePoint =
                                    this.currentInstruction.SymbolSequencePoint;
                            }
                            readNext = true;
                        }
                        else
                        {
                            switch ( opCode )
                            {
                                case OpCodeNumber.Tail:
                                    instructionPrefix |= InstructionPrefixes.Tail;
                                    readNext = true;
                                    break;

                                case OpCodeNumber.Volatile:
                                    instructionPrefix |= InstructionPrefixes.Volatile;
                                    readNext = true;
                                    break;

                                case OpCodeNumber.Readonly:
                                    instructionPrefix |= InstructionPrefixes.ReadOnly;
                                    readNext = true;
                                    break;

                                case OpCodeNumber.Unaligned:
                                    {
                                        byte operand = this.bufferReader.ReadByte();
                                        Trace.InstructionReader.WriteLine( "Read Unaligned operand." );

                                        switch ( operand )
                                        {
                                            case 1:
                                                instructionPrefix |= InstructionPrefixes.Unaligned1;
                                                break;

                                            case 2:
                                                instructionPrefix |= InstructionPrefixes.Unaligned2;
                                                break;

                                            case 4:
                                                instructionPrefix |= InstructionPrefixes.Unaligned4;
                                                break;

                                            default:
                                                throw ExceptionHelper.Core.CreateAssertionFailedException(
                                                    "UnexpectedOperand", operand );
                                        }
                                    }

                                    readNext = true;
                                    break;

                                case OpCodeNumber.Constrained:
                                    instructionPrefix |= InstructionPrefixes.Constrained;
                                    Trace.InstructionReader.WriteLine( "Read Constrained operand." );
                                    this.currentInstruction.ConstrainedToken =
                                        new MetadataToken( this.bufferReader.ReadUInt32() );
                                    readNext = true;
                                    break;

                                default:
                                    readNext = false;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        opCode = (OpCodeNumber) currentByte;
                        if ( Trace.InstructionReader.Enabled )
                        {
                            Trace.InstructionReader.WriteLine( "Read instruction {0}.", opCode );
                        }
                        readNext = false;
                    }
                } while ( readNext );

#if ASSERT
                try
                {
                    OpCodeMap.AssertValidInstruction( opCode );
                }
                catch ( ArgumentException )
                {
                    // Invalid instruction code {0} in method {{{1}}}, sequence {{{2}}}, offset {3}.
                    ExceptionHelper.Core.Assert( false,
                                                 "InvalidOpCodeNumberWithLocation", (int) opCode,
                                                 this.methodBody.Method.ToString(), this.currentSequence.ToString(),
                                                 this.currentState.OffsetBefore );
                }
#endif

                this.currentInstruction.InstructionPrefixes = instructionPrefix;
                this.currentInstruction.OpCodeNumber = opCode;
                this.currentInstruction.UnresolvedSwitchTargetsOperand = null;

                #endregion

                #region Read the operand

                if ( Trace.InstructionReader.Enabled && this.currentInstruction.OperandType != OperandType.InlineNone )
                {
                    Trace.InstructionReader.WriteLine( "Read operand of type {0}.", this.currentInstruction.OperandType );
                }

                switch ( this.currentInstruction.OperandType )
                {
                    case OperandType.InlineBrTarget:
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineSig:
                    case OperandType.InlineTok:
                    case OperandType.InlineType:
                    case OperandType.InlineString:
                    case OperandType.InlineI:
                        this.currentInstruction.Int32Operand = bufferReader.ReadInt32();
                        break;

                    case OperandType.ShortInlineR:
                        this.currentInstruction.SingleOperand = bufferReader.ReadSingle();
                        break;

                    case OperandType.InlineI8:
                        this.currentInstruction.Int64Operand = bufferReader.ReadInt64();
                        break;

                    case OperandType.InlineR:
                        this.currentInstruction.DoubleOperand = bufferReader.ReadDouble();
                        break;

                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineVar:
                    case OperandType.ShortInlineI:
                        this.currentInstruction.ByteOperand = bufferReader.ReadByte();
                        break;

                    case OperandType.InlineVar:
                        this.currentInstruction.Int16Operand = bufferReader.ReadInt16();
                        break;


                    case OperandType.InlineNone:
                        break;

                    case OperandType.InlineSwitch:
                        {
                            uint nbr = bufferReader.ReadUInt32();
                            int[] switchTargetsOperand = new int[nbr];
                            for ( uint i = 0 ; i < nbr ; i++ )
                            {
                                switchTargetsOperand[i] = bufferReader.ReadInt32();
                            }
                            this.currentInstruction.UnresolvedSwitchTargetsOperand = switchTargetsOperand;
                        }
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException(
                            this.currentInstruction.OperandType,
                            "operandType" );
                }

                #endregion

                this.currentState.OffsetAfter = this.bufferReader.Offset;

                #region Read the sequence points

                if ( this.currentState.NextSequencePointOffset >= 0 )
                {
                    if ( this.currentState.OffsetBefore + this.currentSequence.StartOffset >= this.currentState.NextSequencePointOffset )
                    {
                        this.currentInstruction.SymbolSequencePoint =
                            this.methodBody.OriginalSymbolSequencePoints[this.currentState.NextSequencePointOrdinal];
                        this.currentInstruction.LastSymbolSequencePoint = this.currentInstruction.SymbolSequencePoint;
                        this.currentState.LastSymbolSequencePoint = this.currentInstruction.SymbolSequencePoint;
                       
                        this.currentState.NextSequencePointOrdinal++;
                        if ( this.currentState.NextSequencePointOrdinal < this.methodBody.OriginalSymbolSequencePoints.Length )
                        {
                            this.currentState.NextSequencePointOffset =
                                this.methodBody.OriginalSymbolSequencePoints[this.currentState.NextSequencePointOrdinal].Offset;
                        }
                        else
                        {
                            this.currentState.NextSequencePointOffset = -1;
                        }
                    }
                }

                #endregion

                
            }
            catch ( BufferOverflowException )
            {
                throw new BadImageFormatException( 
                    string.Format( "Buffer overflow while decoding the method '{0}', sequence '{1}'.",
                    this.currentSequence.MethodBody.Method.ToString(),
                    this.currentSequence.ToString()));
            }


            return true;
        }

        #endregion

        private struct State
        {
            public int OffsetBefore;
            public int OffsetAfter;
            public int NextSequencePointOrdinal;
            public int NextSequencePointOffset;
            public SymbolSequencePoint LastSymbolSequencePoint;
        }

        #region Bookmarks
        /// <summary>
        /// Creates a bookmark, thanks to which the current <see cref="InstructionReader"/>
        /// can move forward and then return to the current location.
        /// </summary>
        /// <returns>A bookmark for the current location.</returns>
        public InstructionReaderBookmark CreateBookmark()
        {
            return new InstructionReaderBookmark(this, this.currentSequence, this.currentState);
        }

        /// <summary>
        /// Moves the current <see cref="InstructionReader"/> to a given bookmark.
        /// </summary>
        /// <param name="bookmark">A bookmark that was created for the current <see cref="InstructionReader"/>
        /// and for the <see cref="InstructionSequence"/> into which the bookmark was created.</param>
        public void GoToBookmark( InstructionReaderBookmark bookmark )
        {
            #region Preconditions
            ExceptionHelper.AssertArgumentNotNull(bookmark, "bookmark");
            ExceptionHelper.Core.AssertValidOperation(bookmark.Reader == this && bookmark.Sequence == this.currentSequence, "InstructionReaderBookmarkNotValidHere");
            #endregion

            this.currentState = (State) bookmark.State;
            this.bufferReader.Offset = this.currentState.OffsetBefore;
            this.currentInstruction.LastSymbolSequencePoint = this.currentState.LastSymbolSequencePoint;
            this.ReadInstruction();
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            this.bufferReader.Dispose();
        }

        #endregion
    }
}