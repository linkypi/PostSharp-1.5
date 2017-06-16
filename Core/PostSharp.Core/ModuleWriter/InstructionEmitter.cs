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
using System.Reflection;
using System.Reflection.Emit;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace PostSharp.ModuleWriter
{
    /// <summary>
    /// Provides the base operations for emitting IL instructions. This abstract class
    /// does not specify to which kind of target instructions are written. This is specific
    /// to classes deriving <see cref="InstructionEmitter"/>.
    /// </summary>
    public abstract class InstructionEmitter
    {
        #region Fields

        /// <summary>
        /// Method body assigned to the current instance.
        /// </summary>
        private MethodBodyDeclaration body;

        /// <summary>
        /// Whether far branching operands should be unconditionnally used.
        /// </summary>
        private readonly bool forceFarBranchingOperands;

        /// <summary>
        /// Array of near (short) branching instructions. Used in pair with <see cref="farBranchingOpCodes"/>.
        /// </summary>
        private static readonly OpCodeNumber[] nearBranchingOpCodes = new[]
                                                                          {
                                                                              OpCodeNumber.Br_S, OpCodeNumber.Brtrue_S,
                                                                              OpCodeNumber.Brfalse_S,
                                                                              OpCodeNumber.Leave_S,
                                                                              OpCodeNumber.Beq_S,
                                                                              OpCodeNumber.Ble_S, OpCodeNumber.Ble_Un_S,
                                                                              OpCodeNumber.Blt_S, OpCodeNumber.Blt_Un_S,
                                                                              OpCodeNumber.Bge_S, OpCodeNumber.Bge_Un_S,
                                                                              OpCodeNumber.Bgt_S, OpCodeNumber.Bgt_Un_S,
                                                                              OpCodeNumber.Bne_Un_S
                                                                          };

        /// <summary>
        /// Array of far (long) branching instructions. Used in pair with <see name="nearBranchingOpCodes"/>.
        /// </summary>
        private static readonly OpCodeNumber[] farBranchingOpCodes = new[]
                                                                         {
                                                                             OpCodeNumber.Br, OpCodeNumber.Brtrue,
                                                                             OpCodeNumber.Brfalse, OpCodeNumber.Leave,
                                                                             OpCodeNumber.Beq,
                                                                             OpCodeNumber.Ble, OpCodeNumber.Ble_Un,
                                                                             OpCodeNumber.Blt, OpCodeNumber.Blt_Un,
                                                                             OpCodeNumber.Bge, OpCodeNumber.Bge_Un,
                                                                             OpCodeNumber.Bgt, OpCodeNumber.Bgt_Un,
                                                                             OpCodeNumber.Bne_Un
                                                                         };

#if DEBUG
        private const bool checkEnabled = true;
#else
        bool checkEnabled = true;
#endif

        #endregion

        /// <summary>
        /// Initializes a new <see cref="InstructionEmitter"/>.
        /// </summary>
        internal InstructionEmitter()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="InstructionEmitter"/> and assigns it to
        /// a <see cref="MethodBodyDeclaration"/>.
        /// </summary>
        /// <param name="body">A <see cref="MethodBodyDeclaration"/>,
        /// or <b>null</b> if the body will be set later using the <see cref="MethodBody"/>
        /// property.</param>
        /// <param name="forceFarBranchingOperands">Determines whether far branching operands should be unconditionnally used.</param>
        internal InstructionEmitter( MethodBodyDeclaration body, bool forceFarBranchingOperands )
        {
            this.body = body;
            this.forceFarBranchingOperands = forceFarBranchingOperands;
        }

        /// <summary>
        /// Gets or sets the method body assigned to the current <see cref="InstructionEmitter"/>.
        /// </summary>
        public MethodBodyDeclaration MethodBody
        {
            get { return this.body; }
            protected set { this.body = value; }
        }

        /// <summary>
        /// Determines whether far branching instructions should be unconditionnally used.
        /// </summary>
        /// <remarks>
        /// When this property is <b>true</b>, this class will not try to emit different
        /// instructions according to the address distance. This may be only used when
        /// (1) one does not emit final IL code, (2) near and far instructions are
        /// considered equivalently and (3) the reader is aware that near instructions
        /// expect a large operand. This is typically the case when we emit temporary IL
        /// code and the operand is an <see cref="InstructionSequence"/> token.
        /// </remarks>
        public bool ForceFarBranchingOperands
        {
            get { return this.forceFarBranchingOperands; }
        }

        /// <summary>
        /// Given an arbitrary branching opcode, looks up the corresponding near and
        /// far opcodes.
        /// </summary>
        /// <param name="opCode">An arbitrary branching opcode (short or long).</param>
        /// <param name="nearOpCode">The near variant of <paramref name="opCode"/>.</param>
        /// <param name="farOpCode">The far variant of <paramref name="opCode"/>.</param>
        [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
        protected static void FindBranchingOpCode( OpCodeNumber opCode, out OpCodeNumber nearOpCode,
                                                   out OpCodeNumber farOpCode )
        {
            for ( int i = 0; i < nearBranchingOpCodes.Length; i++ )
            {
                if ( opCode == nearBranchingOpCodes[i] ||
                     opCode == farBranchingOpCodes[i] )
                {
                    nearOpCode = nearBranchingOpCodes[i];
                    farOpCode = farBranchingOpCodes[i];
                    return;
                }
            }

            throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidBranchingInstruction", (int) opCode );
        }


        /// <summary>
        /// Emits a symbol sequence point.
        /// </summary>
        /// <param name="sequencePoint">A <see cref="SymbolSequencePoint"/>.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected virtual void InternalEmitSymbolSequencePoint( SymbolSequencePoint sequencePoint )
        {
        }

        /// <summary>
        /// Emits a prefix without operand.
        /// </summary>
        /// <param name="prefix">A combination of prefixes.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitPrefix( InstructionPrefixes prefix );

        /// <summary>
        /// Emits a prefix with an <see cref="IType"/> operand.
        /// </summary>
        /// <param name="prefix">A single prefix (without operand).</param>
        /// <param name="type">An <see cref="IType"/>.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitPrefixType( OpCodeNumber prefix, ITypeSignature type );

        /// <summary>
        /// Emits an instruction without operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstruction( OpCodeNumber code );

        /// <summary>
        /// Emits an instruction with an <see cref="IField"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="field">The <see cref="IField"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionField( OpCodeNumber code, IField field );

        /// <summary>
        /// Emits an instruction with a <see cref="byte"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="operand">The <see cref="byte"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionByte( OpCodeNumber code, byte operand );

        /// <summary>
        /// Emits an instruction with an <see cref="long"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="operand">The <see cref="long"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionInt64( OpCodeNumber code, long operand );


        /// <summary>
        /// Emits an instruction with an <see cref="long"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="operand">The <see cref="long"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionInt32( OpCodeNumber code, int operand );

        /// <summary>
        /// Emits an instruction with an <see cref="short"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="operand">The <see cref="short"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionInt16( OpCodeNumber code, short operand );

        /// <summary>
        /// Emits an instruction with an <see cref="float"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="operand">The <see cref="float"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionSingle( OpCodeNumber code, float operand );

        /// <summary>
        /// Emits an instruction with an <see cref="double"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="operand">The <see cref="double"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionDouble( OpCodeNumber code, double operand );

        /// <summary>
        /// Emits an instruction with an string operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="operand">The string operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionString( OpCodeNumber code, LiteralString operand );

        /// <summary>
        /// Emits an instruction with an <see cref="IMethod"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="method">The <see cref="IMethod"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionMethod( OpCodeNumber code, IMethod method );

        /// <summary>
        /// Emits a <see cref="OpCodeNumber.Switch"/> instruction.
        /// </summary>
        /// <param name="switchTargets">An array of instruction sequences (<see cref="InstructionSequence"/>)
        /// which are the targets of the switch instruction.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitSwitchInstruction( InstructionSequence[] switchTargets );

        /// <summary>
        /// Emits an instruction with an <see cref="IType"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="type">The <see cref="IType"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionType( OpCodeNumber code, ITypeSignature type );

        /// <summary>
        /// Emits an instruction with an <see cref="ParameterDeclaration"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="parameter">The <see cref="ParameterDeclaration"/> operand.</param>
        /// <param name="operandType"><see cref="OperandType.InlineVar"/> or <see cref="OperandType.ShortInlineVar"/>.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="operandType"/> parameter is determined by 
        /// <see cref="EmitInstructionParameter"/> in order to avoid multiples evaluations
        /// of the operand size.
        /// </para>
        /// <para>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </para>
        /// </remarks>
        protected abstract void InternalEmitInstructionParameter( OpCodeNumber code, ParameterDeclaration parameter,
                                                                  OperandType operandType );

        /// <summary>
        /// Emits an instruction with an <see cref="LocalVariableSymbol"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="localVariableSymbol">The <see cref="LocalVariableSymbol"/> operand.</param>
        /// <param name="operandType"><see cref="OperandType.InlineVar"/> or <see cref="OperandType.ShortInlineVar"/>.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="operandType"/> parameter is determined by 
        /// <see cref="EmitInstructionLocalVariable"/> in order to avoid multiples evaluations
        /// of the operand size.
        /// </para>
        /// <para>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </para>
        /// </remarks>
        protected abstract void InternalEmitInstructionLocalVariable( OpCodeNumber code,
                                                                      LocalVariableSymbol localVariableSymbol,
                                                                      OperandType operandType );

        /// <summary>
        /// Emits an instruction with an <see cref="StandaloneSignatureDeclaration"/> operand.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="signature">The <see cref="StandaloneSignatureDeclaration"/> operand.</param>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract void InternalEmitInstructionSignature( OpCodeNumber code,
                                                                  StandaloneSignatureDeclaration signature );

        /// <summary>
        /// Emits a branching instruction.
        /// </summary>
        /// <param name="nearCode">Near opcode of the instruction (to be ignored in <see cref="ForceFarBranchingOperands"/>
        /// is <b>true</b>).</param>
        /// <param name="farCode">Far opcode of the instruction.</param>
        /// <param name="sequence">Target <see cref="InstructionSequence"/>.</param>
        /// <returns>The address distance (or <see cref="AddressDistance.Far"/> if 
        /// <see cref="ForceFarBranchingOperands"/> is true.</returns>
        /// <remarks>
        /// This method is called internally after preconditions have been checked. Derived
        /// classes should not check preconditions.
        /// </remarks>
        protected abstract AddressDistance InternalEmitBranchingInstruction( OpCodeNumber nearCode, OpCodeNumber farCode,
                                                                             InstructionSequence sequence );


        /// <summary>
        /// Emits a prefix without operand.
        /// </summary>
        /// <param name="prefix">A combination of prefixes.</param>
        public void EmitPrefix( InstructionPrefixes prefix )
        {
            this.InternalEmitPrefix( prefix );
        }

        /// <summary>
        /// Emits a prefix with an <see cref="IType"/> operand.
        /// </summary>
        /// <param name="prefix">A single prefix (not a combination).</param>
        /// <param name="type">The <see cref="IType"/> operand.</param>
        public void EmitPrefixType( OpCodeNumber prefix, ITypeSignature type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            this.InternalEmitPrefixType( prefix, type );
        }

        /// <summary>
        /// Emits an instruction without operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        public void EmitInstruction( OpCodeNumber code )
        {
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineNone,
                    "code", "IncompatibleOpCode", code );
            }
#endif

            this.InternalEmitInstruction( code );
        }

        /// <summary>
        /// Emits an instruction with an <see cref="IField"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="field">The <see cref="IField"/> operand.</param>
        public void EmitInstructionField( OpCodeNumber code, IField field )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( field, "field" );

            #endregion

#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineTok ||
                    operandType == OperandType.InlineField,
                    "code", "IncompatibleOpCode", code );
            }
#endif


            this.InternalEmitInstructionField( code, field );
        }


        /// <summary>
        /// Emits an instruction with a <see cref="byte"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="operand">The <see cref="byte"/> operand.</param>
        public void EmitInstructionByte( OpCodeNumber code, byte operand )
        {
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );

                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.ShortInlineVar ||
                    operandType == OperandType.ShortInlineI,
                    "code", "IncompatibleOpCode", code );

                if ( OpCodeMap.IsParameterOperand( code ) )
                {
                    MethodDefDeclaration method = this.body.Method;
                    ExceptionHelper.Core.AssertValidArgument(
                        operand <= method.Parameters.Count +
                                   ( ( method.Attributes & MethodAttributes.Static ) != 0 ? 0 : 1 ),
                        "code", "InvalidParameterOrdinal", operand );
                }
            }
#endif

            this.InternalEmitInstructionByte( code, operand );
        }

        /// <summary>
        /// Emits an instruction with an <see cref="long"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="operand">The <see cref="long"/> operand.</param>
        public void EmitInstructionInt64( OpCodeNumber code, long operand )
        {
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineI8,
                    "code", "IncompatibleOpCode", code );
            }
#endif
            this.InternalEmitInstructionInt64( code, operand );
        }

        /// <summary>
        /// Emits an instruction with an <see cref="int"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="operand">The <see cref="int"/> operand.</param>
        public void EmitInstructionInt32( OpCodeNumber code, int operand )
        {
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineI ||
                    operandType == OperandType.InlineBrTarget ||
                    operandType == OperandType.InlineField ||
                    operandType == OperandType.InlineMethod ||
                    operandType == OperandType.InlineSig ||
                    operandType == OperandType.InlineString ||
                    operandType == OperandType.InlineTok ||
                    operandType == OperandType.InlineType,
                    "code", "IncompatibleOpCode", code );
            }
#endif


            this.InternalEmitInstructionInt32( code, operand );
        }

        /// <summary>
        /// Emits an instruction with an <see cref="short"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="operand">The <see cref="short"/> operand.</param>
        public void EmitInstructionInt16( OpCodeNumber code, short operand )
        {
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );

                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineVar,
                    "code", "IncompatibleOpCode", code );

                if ( OpCodeMap.IsParameterOperand( code ) )
                {
                    MethodDefDeclaration method = this.body.Method;
                    ExceptionHelper.Core.AssertValidArgument(
                        operand <= method.Parameters.Count +
                                   ( ( method.Attributes & MethodAttributes.Static ) != 0 ? 0 : 1 ),
                        "code", "InvalidParameterOrdinal", operand );
                }
            }
#endif

            this.InternalEmitInstructionInt16( code, operand );
        }

        /// <summary>
        /// Emits an instruction with a <see cref="float"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="operand">The <see cref="float"/> operand.</param>
        public void EmitInstructionSingle( OpCodeNumber code, float operand )
        {
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.ShortInlineR,
                    "code", "IncompatibleOpCode", code );
            }
#endif


            this.InternalEmitInstructionSingle( code, operand );
        }

        /// <summary>
        /// Emits an instruction with a <see cref="double"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="operand">The <see cref="double"/> operand.</param>
        public void EmitInstructionDouble( OpCodeNumber code, double operand )
        {
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineR,
                    "code", "IncompatibleOpCode", code );
            }
#endif


            this.InternalEmitInstructionDouble( code, operand );
        }

        /// <summary>
        /// Emits an instruction with a string operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="operand">The string operand.</param>
        public void EmitInstructionString( OpCodeNumber code, LiteralString operand )
        {
            #region Preconditions

            if ( operand.IsNull )
            {
                throw new ArgumentNullException( "operand" );
            }

            #endregion

#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineString,
                    "code", "IncompatibleOpCode", code );
            }
#endif


            this.InternalEmitInstructionString( code, operand );
        }

        /// <summary>
        /// Emits an instruction with an <see cref="IMethod"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="method">The <see cref="IMethod"/> operand.</param>
        public void EmitInstructionMethod( OpCodeNumber code, IMethod method )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( method, "method" );

            #endregion

#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineMethod ||
                    operandType == OperandType.InlineTok,
                    "code", "IncompatibleOpCode", code );
            }
#endif


            this.InternalEmitInstructionMethod( code, method );
        }

        /// <summary>
        /// Emits a <see cref="OpCodeNumber.Switch"/> instruction.
        /// </summary>
        /// <param name="switchTargets">An array of instruction sequences (<see cref="InstructionSequence"/>)
        /// which are the targets of the switch instruction.</param>
        public void EmitSwitchInstruction( InstructionSequence[] switchTargets )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( switchTargets, "switchTargets" );

            #endregion

            this.InternalEmitSwitchInstruction( switchTargets );
        }

        /// <summary>
        /// Emits an instruction with an <see cref="IType"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="type">The <see cref="IType"/> operand.</param>
        public void EmitInstructionType( OpCodeNumber code, ITypeSignature type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineType ||
                    operandType == OperandType.InlineTok,
                    "code", "IncompatibleOpCode", code );
            }
#endif

            this.InternalEmitInstructionType( code, type );
        }

        /// <summary>
        /// Emits an instruction with a <see cref="ParameterDeclaration"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="parameter">The <see cref="ParameterDeclaration"/> operand.</param>
        public void EmitInstructionParameter( OpCodeNumber code, ParameterDeclaration parameter )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( parameter, "parameter" );

            #endregion

#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    ( operandType == OperandType.InlineVar ||
                      operandType == OperandType.ShortInlineVar ) &&
                    OpCodeMap.IsParameterOperand( code ),
                    "code", "IncompatibleOpCode", code );
            }
#endif

            this.InternalEmitInstructionParameter( code,
                                                   parameter, OpCodeMap.GetOperandType( code ) );
        }

        /// <summary>
        /// Emits an instruction with a <see cref="LocalVariableSymbol"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="localVariableSymbol">The <see cref="LocalVariableSymbol"/> operand.</param>
        public void EmitInstructionLocalVariable( OpCodeNumber code, LocalVariableSymbol localVariableSymbol )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( localVariableSymbol, "localVariableSymbol" );

            #endregion

#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    ( operandType == OperandType.InlineVar ||
                      operandType == OperandType.ShortInlineVar ) &&
                    !OpCodeMap.IsParameterOperand( code ),
                    "code", "IncompatibleOpCode", code );
            }
#endif

            this.InternalEmitInstructionLocalVariable( code,
                                                       localVariableSymbol, OpCodeMap.GetOperandType( code ) );
        }

        /// <summary>
        /// Emits a branching instruction.
        /// </summary>
        /// <param name="code">The instruction code.</param>
        /// <param name="sequence">The target <see cref="InstructionSequence"/>.</param>
        /// <returns>The distance of <paramref name="sequence"/> w.r.t. the current instruction.</returns>
        public AddressDistance EmitBranchingInstruction( OpCodeNumber code, InstructionSequence sequence )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sequence, "sequence" );

#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineBrTarget ||
                    operandType == OperandType.ShortInlineBrTarget,
                    "code", "IncompatibleOpCode", code );
            }
#endif

            #endregion

            OpCodeNumber nearCode, farCode;
            FindBranchingOpCode( code, out nearCode, out farCode );

            return this.InternalEmitBranchingInstruction( nearCode, farCode, sequence );
        }

        /// <summary>
        /// Emits an instruction with a <see cref="StandaloneSignatureDeclaration"/> operand.
        /// </summary>
        /// <param name="code">The instruction opcode.</param>
        /// <param name="signature">The <see cref="StandaloneSignatureDeclaration"/> operand.</param>
        public void EmitInstructionSignature( OpCodeNumber code, StandaloneSignatureDeclaration signature )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( signature, "signature" );
#if ASSERT
            if ( checkEnabled )
            {
                OperandType operandType = OpCodeMap.GetOperandType( code );
                ExceptionHelper.Core.AssertValidArgument(
                    operandType == OperandType.InlineSig,
                    "code", "IncompatibleOpCode", code );
            }
#endif

            #endregion

            this.InternalEmitInstructionSignature( code, signature );
        }

        /// <summary>
        /// Emits a symbol sequence point.
        /// </summary>
        /// <param name="sequencePoint">A <see cref="SymbolSequencePoint"/>, or <b>null</b>.</param>
        /// <remarks>
        /// This method does nothing if <paramref name="sequencePoint"/> is null.
        /// </remarks>
        public void EmitSymbolSequencePoint( SymbolSequencePoint sequencePoint )
        {
            if ( sequencePoint != null )
            {
                this.InternalEmitSymbolSequencePoint( sequencePoint );
            }
        }

        /// <summary>
        /// Emits the proper "load indirect" instruction as a function of the operand type.
        /// </summary>
        /// <param name="type">Operand type.</param>
        public void EmitInstructionLoadIndirect( ITypeSignature type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            type = type.GetNakedType( TypeNakingOptions.IgnoreModifiers );
            IntrinsicTypeSignature intrinsicType = type as IntrinsicTypeSignature;

            if ( intrinsicType == null && type.BelongsToClassification( TypeClassifications.Enum ) )
            {
                // If the type is an enum, considerate the underlying intrinsic.
                intrinsicType = EnumHelper.GetUnderlyingType( (INamedType) type );
            }

            if ( intrinsicType != null )
            {
                switch ( intrinsicType.IntrinsicType )
                {
                    case IntrinsicType.SByte:
                        this.EmitInstruction( OpCodeNumber.Ldind_I1 );
                        break;

                    case IntrinsicType.Int16:
                        this.EmitInstruction( OpCodeNumber.Ldind_I2 );
                        break;

                    case IntrinsicType.Boolean:
                        this.EmitInstruction( OpCodeNumber.Ldind_I1 );
                        break;

                    case IntrinsicType.Int32:
                        this.EmitInstruction( OpCodeNumber.Ldind_I4 );
                        break;

                    case IntrinsicType.Int64:
                    case IntrinsicType.UInt64:
                        this.EmitInstruction( OpCodeNumber.Ldind_I8 );
                        break;

                    case IntrinsicType.Byte:
                        this.EmitInstruction( OpCodeNumber.Ldind_U1 );
                        break;

                    case IntrinsicType.UInt16:
                        this.EmitInstruction( OpCodeNumber.Ldind_U2 );
                        break;

                    case IntrinsicType.UInt32:
                        this.EmitInstruction( OpCodeNumber.Ldind_U4 );
                        break;

                    case IntrinsicType.IntPtr:
                    case IntrinsicType.UIntPtr:
                        this.EmitInstruction( OpCodeNumber.Ldind_I );
                        break;


                    case IntrinsicType.Single:
                        this.EmitInstruction( OpCodeNumber.Ldind_R4 );
                        break;

                    case IntrinsicType.Double:
                        this.EmitInstruction( OpCodeNumber.Ldind_R8 );
                        break;

                    case IntrinsicType.Object:
                    case IntrinsicType.String:
                        this.EmitInstruction( OpCodeNumber.Ldind_Ref );
                        break;

                    case IntrinsicType.Char:
                        this.EmitInstruction( OpCodeNumber.Ldind_I2 );
                        break;


                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( intrinsicType, "intrinsicType" );
                }
            }
            else if ( type is GenericParameterTypeSignature || type is GenericTypeInstanceTypeSignature )
            {
                this.EmitInstructionType( OpCodeNumber.Ldobj, type );
            }
            else if ( type is ArrayTypeSignature )
            {
                this.EmitInstruction( OpCodeNumber.Ldind_Ref );
            }
            else if ( type is TypeDefDeclaration || type is TypeRefDeclaration )
            {
                if ( type.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    this.EmitInstructionType( OpCodeNumber.Ldobj, type );
                }
                else
                {
                    this.EmitInstruction( OpCodeNumber.Ldind_Ref );
                }
            }
            else
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "CannotDereferenceThisType", type.GetType() );
            }
        }

        /// <summary>
        /// Emits the proper instruction that stores an array element for a given <see cref="ITypeSignature"/>.
        /// </summary>
        /// <param name="type">Type of elements of the array.</param>
        public void EmitInstructionStoreElement( ITypeSignature type )
        {
            type = type.GetNakedType( TypeNakingOptions.IgnoreModifiers );
            IntrinsicTypeSignature intrinsicType = type as IntrinsicTypeSignature;

            if ( intrinsicType == null && type.BelongsToClassification( TypeClassifications.Enum ) )
            {
                // Look for the underlyig intrinsic type of the enumeration.
                intrinsicType = EnumHelper.GetUnderlyingType( (INamedType) type );
            }

            if ( intrinsicType != null )
            {
                switch ( intrinsicType.IntrinsicType )
                {
                    case IntrinsicType.SByte:
                    case IntrinsicType.Byte:
                        this.EmitInstruction( OpCodeNumber.Stelem_I1 );
                        break;

                    case IntrinsicType.Int16:
                    case IntrinsicType.UInt16:
                    case IntrinsicType.Char:
                        this.EmitInstruction( OpCodeNumber.Stelem_I2 );
                        break;

                    case IntrinsicType.Boolean:
                        this.EmitInstruction( OpCodeNumber.Stelem_I1 );
                        break;

                    case IntrinsicType.Int32:
                    case IntrinsicType.UInt32:
                        this.EmitInstruction( OpCodeNumber.Stelem_I4 );
                        break;

                    case IntrinsicType.Int64:
                    case IntrinsicType.UInt64:
                        this.EmitInstruction( OpCodeNumber.Stelem_I8 );
                        break;


                    case IntrinsicType.IntPtr:
                    case IntrinsicType.UIntPtr:
                        this.EmitInstruction( OpCodeNumber.Stelem_I );
                        break;

                    case IntrinsicType.Single:
                        this.EmitInstruction( OpCodeNumber.Stelem_R4 );
                        break;

                    case IntrinsicType.Double:
                        this.EmitInstruction( OpCodeNumber.Stelem_R8 );
                        break;

                    case IntrinsicType.String:
                    case IntrinsicType.Object:

                        this.EmitInstruction( OpCodeNumber.Stelem_Ref );
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException(
                            intrinsicType, "intrinsicType" );
                }
            }
            else if ( type is GenericParameterTypeSignature || type is GenericTypeInstanceTypeSignature )
            {
                this.EmitInstructionType( OpCodeNumber.Stelem, type );
            }
            else if ( type is TypeDefDeclaration || type is TypeRefDeclaration )
            {
                if ( type.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    this.EmitInstructionType( OpCodeNumber.Stelem, type );
                }
                else
                {
                    this.EmitInstruction( OpCodeNumber.Stelem_Ref );
                }
            }
            else if ( type is ArrayTypeSignature )
            {
                this.EmitInstruction( OpCodeNumber.Stelem_Ref );
            }
            else
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException(
                    "CannotDereferenceThisType", type.GetType() );
            }
        }

        /// <summary>
        /// Emits the proper "store indirect" instruction as a function of the operand type.
        /// </summary>
        /// <param name="type">Operand type.</param>
        public void EmitInstructionStoreIndirect( ITypeSignature type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            type = type.GetNakedType( TypeNakingOptions.IgnoreModifiers );
            IntrinsicTypeSignature intrinsicType = type as IntrinsicTypeSignature;

            if ( intrinsicType == null && type.BelongsToClassification( TypeClassifications.Enum ) )
            {
                // Look for the underlyig intrinsic type of the enumeration.
                intrinsicType = EnumHelper.GetUnderlyingType( (INamedType) type );
            }

            if ( intrinsicType != null )
            {
                switch ( intrinsicType.IntrinsicType )
                {
                    case IntrinsicType.SByte:
                    case IntrinsicType.Byte:
                        this.EmitInstruction( OpCodeNumber.Stind_I1 );
                        break;

                    case IntrinsicType.Int16:
                    case IntrinsicType.UInt16:
                    case IntrinsicType.Char:
                        this.EmitInstruction( OpCodeNumber.Stind_I2 );
                        break;

                    case IntrinsicType.Boolean:
                        this.EmitInstruction( OpCodeNumber.Stind_I1 );
                        break;

                    case IntrinsicType.Int32:
                    case IntrinsicType.UInt32:
                        this.EmitInstruction( OpCodeNumber.Stind_I4 );
                        break;

                    case IntrinsicType.Int64:
                    case IntrinsicType.UInt64:
                        this.EmitInstruction( OpCodeNumber.Stind_I8 );
                        break;


                    case IntrinsicType.IntPtr:
                    case IntrinsicType.UIntPtr:
                        this.EmitInstruction( OpCodeNumber.Stind_I );
                        break;

                    case IntrinsicType.Single:
                        this.EmitInstruction( OpCodeNumber.Stind_R4 );
                        break;

                    case IntrinsicType.Double:
                        this.EmitInstruction( OpCodeNumber.Stind_R8 );
                        break;

                    case IntrinsicType.String:
                    case IntrinsicType.Object:

                        this.EmitInstruction( OpCodeNumber.Stind_Ref );
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException(
                            intrinsicType, "intrinsicType" );
                }
            }
            else if ( type is GenericParameterTypeSignature || type is GenericTypeInstanceTypeSignature )
            {
                this.EmitInstructionType( OpCodeNumber.Stobj, type );
            }
            else if ( type is TypeDefDeclaration || type is TypeRefDeclaration )
            {
                if ( type.BelongsToClassification( TypeClassifications.ValueType ) )
                {
                    this.EmitInstructionType( OpCodeNumber.Stobj, type );
                }
                else
                {
                    this.EmitInstruction( OpCodeNumber.Stind_Ref );
                }
            }
            else if ( type is ArrayTypeSignature )
            {
                this.EmitInstruction( OpCodeNumber.Stind_Ref );
            }
            else
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException(
                    "CannotDereferenceThisType", type.GetType() );
            }
        }

        /// <summary>
        /// Determines whether operand types should be checked at runtime.
        /// </summary>
        public bool CheckEnabled
        {
            get { return checkEnabled; }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
#if !DEBUG
                checkEnabled = value;
#endif
            }
        }
    }
}