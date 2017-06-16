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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection.Emit;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Gives read-only access to the current instruction of an <see cref="InstructionReader"/>.
    /// </summary>
    public sealed class Instruction
    {
        private readonly InstructionReader reader;
        private readonly MethodBodyDeclaration methodBody;
        private readonly ModuleDeclaration module;

        /// <summary>
        /// Opcode of the current instruction.
        /// </summary>
        private OpCodeNumber opCodeNumber = OpCodeNumber.Nop;

        /// <summary>
        /// Prefix of the current instruction.
        /// </summary>
        private InstructionPrefixes instructionPrefix = InstructionPrefixes.None;

        /// <summary>
        /// If the current instruction has a <b>constrained</b> prefix,
        /// operand of this prefix.
        /// </summary>
        private MetadataToken constrainedToken = default( MetadataToken );

        /// <summary>
        /// Operand type of the current instruction.
        /// </summary>
        private OperandType operandType = OperandType.InlineNone;

        /// <summary>
        /// Operand of the current instruction, if <see cref="byte"/>.
        /// </summary>
        private byte byteOperand;


        /// <summary>
        /// Operand of the current instruction, if <see cref="int"/>.
        /// </summary>
        private Int32 int32Operand;

        /// <summary>
        /// Operand of the current instruction, if <see cref="Int16"/>.
        /// </summary>
        private Int16 int16Operand;

        /// <summary>
        /// Operand of the current instruction, if <see cref="Int64"/>.
        /// </summary>
        private Int64 int64Operand;

        /// <summary>
        /// Operand of the current instruction, if <see cref="Single"/>.
        /// </summary>
        private Single singleOperand;

        /// <summary>
        /// Operand of the current instruction, if <see cref="Double"/>.
        /// </summary>
        private Double doubleOperand;

        /// <summary>
        /// Operand of the current instruction, if a switch target.
        /// </summary>
        private int[] switchTargetsOperand;

        /// <summary>
        /// Current sequence point.
        /// </summary>
        private SymbolSequencePoint sequencePoint;


        internal Instruction( InstructionReader reader )
        {
            this.reader = reader;
            this.methodBody = reader.MethodBody;
            this.module = this.methodBody.Module;
        }

        /// <summary>
        /// Gets the <see cref="MethodBodyDeclaration"/> to which the current instruction
        /// belong.
        /// </summary>
        public MethodBodyDeclaration MethodBody
        {
            get { return this.methodBody; }
        }

        /// <summary>
        /// Gets the <see cref="ModuleDeclaration"/> to which the current instruction
        /// belong.
        /// </summary>
        public ModuleDeclaration Module
        {
            get { return this.module; }
        }

        #region Current instruction

        /// <summary>
        /// Gets the combination of prefixes modifying the current instruction.
        /// </summary>
        public InstructionPrefixes InstructionPrefixes
        {
            get { return this.instructionPrefix; }
            internal set { this.instructionPrefix = value; }
        }

        /// <summary>
        /// Gets the value of the <b>unaligned</b> prefix.
        /// </summary>
        /// <value>
        /// The value <b>1</b>, <b>2</b> or <b>4</b> if there is an <b>unaligned</b>
        /// prefix for the current instruction, otherwise <b>0</b>.
        /// </value>
        public int UnalignedPrefix
        {
            get
            {
                switch ( this.instructionPrefix & InstructionPrefixes.UnalignedMask )
                {
                    case InstructionPrefixes.Unaligned1:
                        return 1;

                    case InstructionPrefixes.Unaligned2:
                        return 2;

                    case InstructionPrefixes.Unaligned4:
                        return 4;

                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="OperandType"/> of the current instruction.
        /// </summary>
        public OperandType OperandType
        {
            get { return this.operandType; }
        }

        /// <summary>
        /// Gets the <see cref="OpCodeNumber"/> of the current instruction.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1706" /* Microsoft.Naming */ )]
        public OpCodeNumber OpCodeNumber
        {
            get { return this.opCodeNumber; }
            internal set
            {
                this.opCodeNumber = value;
                this.operandType = OpCodeMap.GetOperandType( opCodeNumber );
            }
        }

        /// <summary>
        /// Gets the unresolved targets of a <b>switch</b> instruction.
        /// </summary>
        /// <value>
        /// An array containing the relative address of the targets, with respect
        /// to the end of the current <b>switch</b> instruction.
        /// </value>
        internal int[] UnresolvedSwitchTargetsOperand
        {
            get { return this.switchTargetsOperand; }
            set { this.switchTargetsOperand = value; }
        }

        /// <summary>
        /// Gets the <see cref="Byte"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public byte ByteOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.ShortInlineBrTarget ||
                    this.operandType == OperandType.ShortInlineI ||
                    this.operandType == OperandType.ShortInlineR ||
                    this.operandType == OperandType.ShortInlineVar,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return byteOperand;
            }

            internal set { this.byteOperand = value; }
        }

        /// <summary>
        /// Gets the <see cref="Int16"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public Int16 Int16Operand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineVar,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return int16Operand;
            }

            internal set { this.int16Operand = value; }
        }

        /// <summary>
        /// Gets the <see cref="Int32"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public Int32 Int32Operand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineI ||
                    this.operandType == OperandType.InlineBrTarget,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return int32Operand;
            }

            internal set { this.int32Operand = value; }
        }

        /// <summary>
        /// Gets the <see cref="MetadataToken"/> operand of the current instruction.
        /// </summary>
        internal MetadataToken TokenOperand
        {
            get { return new MetadataToken( (uint) this.int32Operand ); }
        }

        /// <summary>
        /// Gets the <see cref="MetadataDeclaration"/> operand for the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public MetadataDeclaration DeclarationOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineTok,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.module.Tables.GetDeclaration( this.TokenOperand );
            }
        }

        /// <summary>
        /// Gets the <see cref="Int64"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public Int64 Int64Operand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineI8,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return int64Operand;
            }

            internal set { this.int64Operand = value; }
        }

        /// <summary>
        /// Gets the <see cref="Single"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public Single SingleOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.ShortInlineR,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return singleOperand;
            }

            internal set { this.singleOperand = value; }
        }

        /// <summary>
        /// Gets the <see cref="Double"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public Double DoubleOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineR,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.doubleOperand;
            }

            internal set { this.doubleOperand = value; }
        }

        /// <summary>
        /// Gets the <see cref="InstructionSequence"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public InstructionSequence BranchTargetOperand
        {
            get
            {
                int target;
                switch ( this.operandType )
                {
                    case OperandType.InlineBrTarget:
                        target = this.int32Operand;
                        break;

                    case OperandType.ShortInlineBrTarget:
                        target = (sbyte) this.byteOperand;
                        break;

                    default:
                        throw ExceptionHelper.Core.CreateInvalidOperationException(
                            "InstructionReaderInvalidOperandType" );
                }

                if ( this.reader.CurrentInstructionSequence.IsOriginal )
                {
                    // The value is the offset.
                    return
                        this.methodBody.GetInstructionSequenceByOffset(
                            this.reader.CurrentInstructionSequence.StartOffset + this.reader.OffsetAfter + target );
                }
                else
                {
                    // The value is the token.
                    return this.methodBody.GetInstructionSequenceByToken( (short) target );
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ParameterDeclaration"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public ParameterDeclaration ParameterOperand
        {
            get
            {
                int operand;

                switch ( this.operandType )
                {
                    case OperandType.InlineVar:
                        operand = this.int16Operand;
                        break;

                    case OperandType.ShortInlineVar:
                        operand = this.byteOperand;
                        break;

                    default:
                        throw ExceptionHelper.Core.CreateInvalidOperationException(
                            "InstructionReaderInvalidOperandType" );
                }

                return this.GetParameter( operand );
            }
        }


        /// <summary>
        /// Gets the <see cref="LocalVariableSymbol"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public LocalVariableSymbol LocalVariableOperand
        {
            get
            {
                short ordinal;

                switch ( this.operandType )
                {
                    case OperandType.InlineVar:
                        ordinal = this.int16Operand;
                        break;

                    case OperandType.ShortInlineVar:
                        ordinal = this.byteOperand;
                        break;

                    default:
                        throw ExceptionHelper.Core.CreateInvalidOperationException(
                            "InstructionReaderInvalidOperandType" );
                }

                return this.reader.GetLocalVariableSymbol( ordinal, true );
            }
        }

        /// <summary>
        /// Gets the <see cref="IType"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public ITypeSignature TypeOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineType ||
                    this.operandType == OperandType.InlineTok,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.module.Tables.GetType( this.TokenOperand );
            }
        }

        /// <summary>
        /// Gets the <see cref="IMethod"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public IMethod MethodOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineTok ||
                    this.operandType == OperandType.InlineMethod,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.module.Tables.GetMethod( this.TokenOperand );
            }
        }

        /// <summary>
        /// Gets the <see cref="IField"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public IField FieldOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineTok ||
                    this.operandType == OperandType.InlineField,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.module.Tables.GetField( this.TokenOperand );
            }
        }

        /// <summary>
        /// Gets the <see cref="StringOperand"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public LiteralString StringOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineString,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                if ( this.reader.CurrentInstructionSequence.ModifiedInstructionBytes == null )
                {
                    return this.module.Tables.GetUserString( this.TokenOperand );
                }
                else
                {
                    return this.module.Tables.GetCustomString( this.TokenOperand );
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="MemberRefDeclaration"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public MemberRefDeclaration MemberRefOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineTok,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.module.Tables.GetMemberRef( this.TokenOperand );
            }
        }


        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public InstructionSequence[] GetSwitchTargetsOperand()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation(
                this.operandType == OperandType.InlineSwitch,
                "InstructionReaderInvalidOperandType" );

            #endregion

            InstructionSequence[] targets = new InstructionSequence[this.switchTargetsOperand.Length];

            for ( int i = 0; i < targets.Length; i++ )
            {
                targets[i] = this.GetSwitchOperandTarget( i );
            }

            return targets;
        }

        /// <summary>
        /// Gets the number of targets in the <b>switch</b> operand.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public int SwitchOperandTargetCount
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineSwitch,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.switchTargetsOperand.Length;
            }
        }

        /// <summary>
        /// Gets a target of a <b>switch</b> operand given its index.
        /// </summary>
        /// <param name="index">Index of the required target.</param>
        /// <returns>
        /// The target <see cref="InvalidOperationException"/> whose position
        /// in the <b>switch</b> statement is <paramref name="index"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public InstructionSequence GetSwitchOperandTarget( int index )
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation(
                this.operandType == OperandType.InlineSwitch,
                "InstructionReaderInvalidOperandType" );

            #endregion

            int target = this.switchTargetsOperand[index];

            if ( this.reader.CurrentInstructionSequence.IsOriginal )
            {
                // The value is the offset.
                return
                    this.methodBody.GetInstructionSequenceByOffset( this.reader.CurrentInstructionSequence.StartOffset +
                                                                    this.reader.OffsetAfter + target );
            }
            else
            {
                // The value is the token.
                return this.methodBody.GetInstructionSequenceByToken( (short) target );
            }
        }

        /// <summary>
        /// Gets the <see cref="StringOperand"/> operand of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has a different operand type.
        /// </exception>
        public StandaloneSignatureDeclaration SignatureOperand
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    this.operandType == OperandType.InlineSig,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.module.Tables.GetStandaloneSignature( new MetadataToken( (uint) this.int32Operand ) );
            }
        }

        internal MetadataToken ConstrainedToken
        {
            //get { return this.constrainedToken; }
            set { this.constrainedToken = value; }
        }

        /// <summary>
        /// Gets the operand of the <b>constrained</b> prefix of the current instruction.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///		The current instruction has no 
        ///		<see cref="PostSharp.CodeModel.InstructionPrefixes.Constrained"/>
        ///		prefix.
        /// </exception>
        public ITypeSignature ConstrainedType
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation(
                    ( this.instructionPrefix & InstructionPrefixes.Constrained ) != 0,
                    "InstructionReaderInvalidOperandType" );

                #endregion

                return this.module.Tables.GetType( this.constrainedToken );
            }
        }

        /// <summary>
        /// Determines whether there is a <see cref="SymbolSequencePoint"/> before the current instruction.
        /// </summary>
        /// <value>
        /// <b>true</b> if there is a <see cref="SymbolSequencePoint"/> before the current instruction,
        /// otherwise <b>false</b>.
        /// </value>
        public bool HasSymbolSequencePoint
        {
            get { return this.sequencePoint != null; }
        }

        /// <summary>
        /// Gets the <see cref="SymbolSequencePoint"/> before the current instruction.
        /// </summary>
        /// <value>
        /// A <see cref="SymbolSequencePoint"/>, or a null <see cref="SymbolSequencePoint"/>
        /// if there is no symbol sequence point at the current instruction.
        /// </value>
        public SymbolSequencePoint SymbolSequencePoint
        {
            get { return this.sequencePoint; }
            internal set { this.sequencePoint = value; }
        }


        /// <summary>
        /// Gets the last symbol sequence point, which covers the current
        /// instruction.
        /// </summary>
        public SymbolSequencePoint LastSymbolSequencePoint { get; internal set; }

        #endregion

        /// <summary>
        /// Gets the typed (and eventually boxed) operand.
        /// </summary>
        /// <returns>The typed operand of the current instruction, 
        /// or <b>null</b> if the current instruction has no operand.
        /// </returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        public object GetOperand()
        {
            switch ( this.operandType )
            {
                case OperandType.InlineVar:
                case OperandType.ShortInlineVar:
                    if ( OpCodeMap.IsParameterOperand( this.opCodeNumber ) )
                    {
                        return this.ParameterOperand;
                    }
                    else
                    {
                        return this.LocalVariableOperand;
                    }

                case OperandType.InlineI:
                    return int32Operand;

                case OperandType.InlineI8:
                    return int64Operand;

                case OperandType.InlineBrTarget:
                case OperandType.ShortInlineBrTarget:
                    return this.BranchTargetOperand;

                case OperandType.InlineField:
                    return this.FieldOperand;

                case OperandType.InlineMethod:
                    return this.MethodOperand;

                case OperandType.InlineNone:
                    return null;

                case OperandType.InlineR:
                    return this.singleOperand;

                case OperandType.InlineSig:
                    return this.SignatureOperand;

                case OperandType.InlineString:
                    return this.StringOperand;

                case OperandType.InlineSwitch:
                    return this.GetSwitchTargetsOperand();

                case OperandType.InlineTok:
                    return this.module.Tables.GetDeclaration( this.TokenOperand );

                case OperandType.InlineType:
                    return this.TypeOperand;

                case OperandType.ShortInlineI:
                    return this.byteOperand;

                case OperandType.ShortInlineR:
                    return this.singleOperand;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.operandType,
                                                                                  "Instruction.OperandType" );
            }
        }

        /// <summary>
        /// Gets a <see cref="ParameterDeclaration"/> given the index of the <b>ldarg</b>
        /// intruction.
        /// </summary>
        /// <param name="index">Parameter index. 0-based in static methods, 1-based
        /// in instant methods</param>
        /// <returns>The <see cref="ParameterDeclaration"/> corresponding to <paramref name="index"/>.</returns>
        public ParameterDeclaration GetParameter( int index )
        {
            int shift = this.methodBody.Method.IsStatic ? 0 : 1;
            ParameterDeclaration parameter = this.methodBody.Method.Parameters[index - shift];
            ExceptionHelper.Core.Assert( parameter != null, "UndefinedParameter", index );
            return parameter;
        }

        /// <summary>
        /// Gets the <see cref="LocalVariableSymbol"/> corresponding to a given ordinal
        /// in the current context.
        /// </summary>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>The <see cref="LocalVariableSymbol"/> corresponding to <paramref name="ordinal"/>
        /// in the current context.</returns>
        public LocalVariableSymbol GetLocalVariableSymbol( int ordinal )
        {
            return this.reader.GetLocalVariableSymbol( ordinal, true );
        }

        /// <summary>
        /// Writes the current <see cref="Instruction"/> an an <see cref="InstructionEmitter"/>.
        /// </summary>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        public void Write( InstructionEmitter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            // Emit the symbol sequence point.
            if ( this.HasSymbolSequencePoint )
            {
                writer.EmitSymbolSequencePoint( this.SymbolSequencePoint );
            }

            // Emit prefixes.
            switch ( this.InstructionPrefixes )
            {
                case InstructionPrefixes.None:
                    break;

                case InstructionPrefixes.Constrained:
                    writer.EmitPrefixType( OpCodeNumber.Constrained, this.ConstrainedType );
                    break;

                default:
                    writer.EmitPrefix( this.InstructionPrefixes );
                    break;
            }

            // Emit the instruction according to the operand type.
            switch ( operandType )
            {
                case OperandType.InlineBrTarget:
                case OperandType.ShortInlineBrTarget:
                    writer.EmitBranchingInstruction( this.OpCodeNumber,
                                                     this.BranchTargetOperand );
                    break;

                case OperandType.InlineField:
                    writer.EmitInstructionField( this.OpCodeNumber,
                                                 this.FieldOperand );
                    break;

                case OperandType.InlineMethod:
                    writer.EmitInstructionMethod( this.OpCodeNumber,
                                                  this.MethodOperand );
                    break;

                case OperandType.InlineI:
                    writer.EmitInstructionInt32( this.OpCodeNumber,
                                                 this.Int32Operand );
                    break;

                case OperandType.InlineI8:
                    writer.EmitInstructionInt64( this.OpCodeNumber,
                                                 this.Int64Operand );
                    break;


                case OperandType.InlineNone:
                    writer.EmitInstruction( this.OpCodeNumber );
                    break;

                case OperandType.InlineR:
                    writer.EmitInstructionDouble( this.OpCodeNumber,
                                                  this.DoubleOperand );
                    break;

                case OperandType.InlineString:
                    writer.EmitInstructionString( this.OpCodeNumber,
                                                  this.StringOperand );
                    break;

                case OperandType.InlineSwitch:
                    writer.EmitSwitchInstruction( this.GetSwitchTargetsOperand() );
                    break;

                case OperandType.InlineTok:
                    switch ( this.TokenOperand.TokenType )
                    {
                        case TokenType.FieldDef:
                            writer.EmitInstructionField( this.OpCodeNumber,
                                                         this.FieldOperand );
                            break;

                        case TokenType.MemberRef:
                            {
                                MemberRefDeclaration memberRef = this.MemberRefOperand;
                                FieldRefDeclaration externalField = memberRef as FieldRefDeclaration;
                                if ( externalField != null )
                                {
                                    writer.EmitInstructionField( this.OpCodeNumber, externalField );
                                }
                                else
                                {
                                    MethodRefDeclaration externalMethod = memberRef as MethodRefDeclaration;
                                    if ( externalMethod != null )
                                    {
                                        writer.EmitInstructionMethod( this.OpCodeNumber, externalMethod );
                                    }
                                    else
                                    {
                                        throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidType",
                                                                                                   memberRef.GetType().
                                                                                                       FullName,
                                                                                                   "FieldRefDeclaration, MethodRefDeclaration" );
                                    }
                                }
                                break;
                            }

                        case TokenType.MethodDef:
                        case TokenType.MethodSpec:
                            writer.EmitInstructionMethod( this.OpCodeNumber, this.MethodOperand );
                            break;

                        case TokenType.TypeDef:
                        case TokenType.TypeRef:
                        case TokenType.TypeSpec:
                        case TokenType.GenericParam:
                            writer.EmitInstructionType( this.OpCodeNumber, this.TypeOperand );
                            break;

                        default:
                            throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidTokenOperand",
                                                                                       this.TokenOperand.TokenType );
                    }
                    break;

                case OperandType.InlineType:
                    writer.EmitInstructionType( this.OpCodeNumber, this.TypeOperand );
                    break;


#if DEBUG
                case OperandType.InlineVar:
                    if ( this.reader.IsSymbolResolutionEnabled )
                    {
                        if ( OpCodeMap.IsParameterOperand( this.opCodeNumber ) )
                        {
                            writer.EmitInstructionParameter( this.opCodeNumber, this.ParameterOperand );
                        }
                        else
                        {
                            writer.EmitInstructionLocalVariable( this.opCodeNumber, this.LocalVariableOperand );
                        }
                    }
                    else
                    {
                        writer.EmitInstructionInt16( this.opCodeNumber, this.int16Operand );
                    }
                    break;

                case OperandType.ShortInlineVar:
                    if ( this.reader.IsSymbolResolutionEnabled )
                    {
                        if ( OpCodeMap.IsParameterOperand( this.opCodeNumber ) )
                        {
                            writer.EmitInstructionParameter( this.opCodeNumber, this.ParameterOperand );
                        }
                        else
                        {
                            writer.EmitInstructionLocalVariable( this.opCodeNumber, this.LocalVariableOperand );
                        }
                    }
                    else
                    {
                        writer.EmitInstructionByte( this.opCodeNumber, this.byteOperand );
                    }
                    break;
#else
                case OperandType.InlineVar:
                    writer.EmitInstructionInt16( this.opCodeNumber, this.int16Operand );
                    break;

                case OperandType.ShortInlineVar:
                    writer.EmitInstructionByte(this.opCodeNumber, this.byteOperand);
                    break;
#endif

                case OperandType.ShortInlineI:
                    writer.EmitInstructionByte( this.OpCodeNumber, this.ByteOperand );
                    break;

                case OperandType.ShortInlineR:
                    writer.EmitInstructionSingle( this.OpCodeNumber, this.SingleOperand );
                    break;

                case OperandType.InlineSig:
                    writer.EmitInstructionSignature( this.OpCodeNumber, this.SignatureOperand );
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidOperandType", operandType );
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format( CultureInfo.InvariantCulture, "{0} {1}", this.opCodeNumber, this.GetOperand() );
        }
    }
}