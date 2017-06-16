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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// writer a linear stream of instructions into an <see cref="InstructionSequence"/>.
    /// </summary>
    /// <remarks>
    /// After having been instantiated, the <see cref="InstructionWriter"/> should
    /// be associated to an <see cref="InstructionSequence"/> using the 
    /// <see cref="AttachInstructionSequence"/>. After all instructions have been written,
    /// the <see cref="DetachInstructionSequence()"/> method should be called. 
    /// </remarks>
    public sealed class InstructionWriter : InstructionEmitter, IDisposable
    {
        #region Fields

        /// <summary>
        /// A <see cref="MemoryStream"/> used during the complete lifecycle
        /// of this instance to writer bytes.
        /// </summary>
        private readonly MemoryStream stream;

        /// <summary>
        /// A <see cref="BinaryWriter"/> writing into <see cref="stream"/>.
        /// </summary>
        private readonly BinaryWriter writer;

        /// <summary>
        /// The method to which the instruction writer is attached.
        /// </summary>
        /// <value>
        /// A <see cref="MethodDefDeclaration"/>, or <b>null</b> if the 
        /// <see cref="InstructionWriter"/> is detached.
        /// </value>
        private MethodDefDeclaration method;

        /// <summary>
        /// The instruction sequence to which the instruction writer is attached.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionWriter"/>, or <b>null</b> if the 
        /// <see cref="InstructionWriter"/> is detached.
        /// </value>
        private InstructionSequence sequence;

        private List<SymbolSequencePoint> additionalSequencePoints;

        private int nextSequencePointToken;


        /// <summary>
        /// Whether <see cref="method"/> is static.
        /// </summary>
        private bool isStatic;

#if DEBUG
        private bool incomplete;
#endif

        #endregion

        /// <summary>
        /// Initializes a new <see cref="InstructionWriter"/>.
        /// </summary>
        public InstructionWriter()
        {
            this.stream = new MemoryStream( 128 );
            this.writer = new BinaryWriter( this.stream );
        }

        [Conditional("DEBUG")]
        private void SetComplete()
        {
#if DEBUG
            this.incomplete = false;
#endif
        }
        [Conditional("DEBUG")]
        private void SetIncomplete()
        {
#if DEBUG
            this.incomplete = true;
#endif
        }



        #region Begin/end sequence

        /// <summary>
        /// Gets the current instruction sequence.
        /// </summary>
        /// <value>
        /// The current instruction sequence, or <b>null</b> if the current writer
        /// is not attached to any sequence.
        /// </value>
        public InstructionSequence CurrentInstructionSequence
        {
            get { return this.sequence; }
        }

        /// <summary>
        /// Attaches the current <see cref="InstructionWriter"/> to an <see cref="InstructionSequence"/>.
        /// </summary>
        /// <param name="sequence">An <see cref="InstructionSequence"/> that is attached
        /// to a method.</param>
        /// <exception cref="ArgumentNullException">
        ///		The <paramref name="sequence"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///		The <paramref name="sequence"/> is not attached to a method.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// This <see cref="InstructionWriter"/> has already an active <see cref="InstructionSequence"/>.
        /// </exception>
        public void AttachInstructionSequence( InstructionSequence sequence )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sequence, "sequence" );
            ExceptionHelper.Core.AssertValidOperation( this.sequence == null, "InstructionWriterHasSequence" );
            ExceptionHelper.Core.AssertValidArgument(
                sequence.ParentInstructionBlock != null && sequence.ParentInstructionBlock.MethodBody != null,
                "sequence", "InstructionSequenceIsNotAttachedToMethod" );

            #endregion

            this.sequence = sequence;
            this.method = sequence.ParentInstructionBlock.MethodBody.Method;
            this.isStatic = ( method.Attributes & MethodAttributes.Static ) != 0;
            this.MethodBody = this.method.MethodBody;
            this.nextSequencePointToken = this.method.MethodBody.NextAdditionalSymbolSequencePointPosition;
        }

        /// <overloads>Detaches the current <see cref="InstructionWriter"/> from
        /// the active <see cref="InstructionSequence"/>.</overloads>
        /// <summary>
        /// Detaches the current <see cref="InstructionWriter"/> from the active
        /// <see cref="InstructionSequence"/> and commits the changes.
        /// </summary>
        public void DetachInstructionSequence()
        {
            this.DetachInstructionSequence( true );
        }

        /// <summary>
        /// Detaches the current <see cref="InstructionWriter"/> form the active
        /// <see cref="InstructionSequence"/> by specifying whether the
        /// changes should be committed or rolled back.
        /// </summary>
        /// <param name="commit"><b>true</b> if the changes should ne
        /// committed, or <b>false</b> of they should be rolled back.</param>
        /// <exception cref="InvalidOperationException">	
        ///		This <see cref="InstructionWriter"/> has no active
        ///		<see cref="InstructionSequence"/>.
        /// </exception>
        public void DetachInstructionSequence( bool commit )
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.sequence != null, "InstructionWriterHasNoSequence" );

#if DEBUG
            if ( this.incomplete )
                throw new InvalidOperationException("A sequence cannot end with a prefix or a symbol sequence point.");
#endif
            #endregion

            if ( commit )
            {
                this.writer.Flush();
                byte[] buffer = this.stream.GetBuffer();
                byte[] bytes = new byte[this.stream.Position];
                Array.Copy( buffer, bytes, this.stream.Position );
                this.sequence.ModifiedInstructionBytes = bytes;
                if ( this.additionalSequencePoints != null )
                {
                    this.MethodBody.AddSymbolSequencePoints( this.additionalSequencePoints );
                    this.additionalSequencePoints.Clear();
                }
            }
            else
            {
                if ( this.additionalSequencePoints != null )
                {
                    this.additionalSequencePoints.Clear();
                }
            }

            this.stream.Seek( 0, SeekOrigin.Begin );
            this.sequence = null;
            this.MethodBody = null;
        }

        #endregion

        #region implementation of InstructionEmitter

        /// <summary>
        /// Writes an opcode.
        /// </summary>
        /// <param name="opCode">An <see cref="OpCodeNumber"/>.</param>
        private void EmitOpCode( OpCodeNumber opCode )
        {
            #region Preconditions

            if ( this.sequence == null )
            {
                throw new InvalidOperationException();
            }

            #endregion

            if ( ( (ushort) opCode & 0xFF00 ) != 0 )
            {
                this.writer.Write( (byte) ( (ushort) opCode >> 8 ) );
            }

            this.writer.Write( (byte) ( (ushort) opCode & 0xFF ) );
        }

        /// <inheritdoc />
        protected override void InternalEmitPrefix( InstructionPrefixes prefix )
        {
            if ( ( prefix & InstructionPrefixes.ReadOnly ) != 0 )
            {
                this.EmitOpCode( OpCodeNumber.Readonly );
            }

            if ( ( prefix & InstructionPrefixes.Tail ) != 0 )
            {
                this.EmitOpCode( OpCodeNumber.Tail );
            }

            if ( ( prefix & InstructionPrefixes.Volatile ) != 0 )
            {
                this.EmitOpCode( OpCodeNumber.Volatile );
            }


            if ( ( prefix & InstructionPrefixes.UnalignedMask ) != 0 )
            {
                this.EmitOpCode( OpCodeNumber.Unaligned );
                switch ( prefix & InstructionPrefixes.UnalignedMask )
                {
                    case InstructionPrefixes.Unaligned1:
                        this.writer.Write( (byte) 1 );
                        break;

                    case InstructionPrefixes.Unaligned2:
                        this.writer.Write( (byte) 2 );
                        break;

                    case InstructionPrefixes.Unaligned4:
                        this.writer.Write( (byte) 4 );
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( prefix, "prefix" );
                }
            }

            this.SetIncomplete();
        }

        /// <summary>
        /// Emits an opcode that takes a declaration (represented as a <see cref="MetadataDeclaration"/>)
        /// as operand.
        /// </summary>
        /// <param name="opCode">An <see cref="OpCodeNumber"/>.</param>
        /// <param name="declaration">The operand.</param>
        private void EmitOpCodeMetadataToken( OpCodeNumber opCode, MetadataDeclaration declaration )
        {
            ExceptionHelper.Core.AssertValidArgument( declaration.Module == this.method.Module,
                                                      "declaration", "ModulesDoNotMatch" );
            declaration.InternalIsWeaklyReferenced = false;
            this.EmitOpCodeMetadataToken( opCode, declaration.MetadataToken );
        }

        /// <summary>
        /// Emits an opcode that takes a declaration (represented as a <see cref="MetadataToken"/>)
        /// as operand.
        /// </summary>
        /// <param name="opCode">An <see cref="OpCodeNumber"/>.</param>
        /// <param name="token">The operand.</param>
        private void EmitOpCodeMetadataToken( OpCodeNumber opCode, MetadataToken token )
        {
            #region Preconditions

            if ( token.IsNull )
            {
                throw new ArgumentNullException( "token" );
            }

            #endregion

            this.EmitOpCode( opCode );
            this.writer.Write( token.Value );
        }

        /// <inheritdoc />
        protected override void InternalEmitPrefixType( OpCodeNumber prefix, ITypeSignature type )
        {
            this.EmitOpCodeMetadataToken( prefix, (MetadataDeclaration) type );
            this.SetIncomplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstruction( OpCodeNumber code )
        {
            this.EmitOpCode( code );
            this.SetComplete();

        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionField( OpCodeNumber code, IField field )
        {
            this.EmitOpCodeMetadataToken( code, (MetadataDeclaration) field );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionByte( OpCodeNumber code, byte operand )
        {
            this.EmitOpCode( code );
            this.writer.Write( operand );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionInt64( OpCodeNumber code, long operand )
        {
            this.EmitOpCode( code );
            this.writer.Write( operand );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionInt32( OpCodeNumber code, int operand )
        {
            this.EmitOpCode( code );
            this.writer.Write( operand );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionInt16( OpCodeNumber code, short operand )
        {
            this.EmitOpCode( code );
            this.writer.Write( operand );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionSingle( OpCodeNumber code, float operand )
        {
            this.EmitOpCode( code );
            this.writer.Write( operand );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionDouble( OpCodeNumber code, double operand )
        {
            this.EmitOpCode( code );
            this.writer.Write( operand );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionString( OpCodeNumber code, LiteralString operand )
        {
            MetadataToken customStringToken = this.method.Module.Tables.AddCustomString( operand );
            this.EmitOpCodeMetadataToken( code, customStringToken );
            this.SetComplete();

        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionMethod( OpCodeNumber code, IMethod method )
        {
            this.EmitOpCodeMetadataToken( code, (MetadataDeclaration) method );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitSwitchInstruction( InstructionSequence[] switchTargets )
        {
            this.EmitOpCode( OpCodeNumber.Switch );
            this.writer.Write( (uint) switchTargets.Length );
            for ( int i = 0; i < switchTargets.Length; i++ )
            {
                this.writer.Write( (int) switchTargets[i].Token );
            }
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionType( OpCodeNumber code, ITypeSignature type )
        {
            MetadataDeclaration metadataDeclaration;
            TypeSignature typeSignature = type as TypeSignature;
            if ( typeSignature != null )
            {
                metadataDeclaration = this.MethodBody.Module.TypeSpecs.GetBySignature( typeSignature, true );
            }
            else
            {
                metadataDeclaration = (MetadataDeclaration) type;
            }
            this.EmitOpCodeMetadataToken( code, metadataDeclaration );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionParameter( OpCodeNumber code, ParameterDeclaration parameter,
                                                                  OperandType operandType )
        {
            this.EmitOpCode( code );
            this.writer.Write( (short) parameter.Ordinal + ( this.isStatic ? 0 : 1 ) );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionLocalVariable( OpCodeNumber code,
                                                                      LocalVariableSymbol localVariableSymbol,
                                                                      OperandType operandType )
        {
            this.EmitOpCode( code );
            this.writer.Write( (short) localVariableSymbol.LocalVariable.Ordinal );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override void InternalEmitInstructionSignature( OpCodeNumber code,
                                                                  StandaloneSignatureDeclaration signature )
        {
            this.EmitOpCodeMetadataToken( code, signature );
            this.SetComplete();
        }

        /// <inheritdoc />
        protected override AddressDistance InternalEmitBranchingInstruction( OpCodeNumber nearCode, OpCodeNumber farCode,
                                                                             InstructionSequence sequence )
        {
            this.EmitOpCode( farCode );
            this.writer.Write( (int) sequence.Token );
            this.SetComplete();
            return AddressDistance.Far;
        }

        /// <inheritdoc />
        protected override void InternalEmitSymbolSequencePoint( SymbolSequencePoint sequencePoint )
        {
            base.InternalEmitSymbolSequencePoint( sequencePoint );
            if ( sequencePoint.IsHidden )
            {
                this.InternalEmitInstructionInt16( OpCodeNumber._SequencePoint, -1 );
            }
            else
            {
                if ( this.additionalSequencePoints == null )
                {
                    this.additionalSequencePoints = new List<SymbolSequencePoint>( 8 );
                }
                this.additionalSequencePoints.Add( sequencePoint );
                this.InternalEmitInstructionInt16( OpCodeNumber._SequencePoint, (short) this.nextSequencePointToken );
                this.nextSequencePointToken++;
            }

            this.SetIncomplete();
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            this.stream.Close();
            this.writer.Close();
        }

        #endregion
    }
}