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

#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a sequence of instructions without branches, i.e. the control flow
    /// necessarly begins at the beginning of the sequence and ends at the end 
    /// of the sequence.
    /// </summary>
    /// <remarks>
    /// By default, PostSharp only analyzes the normal control flow on the base of
    /// branching instructions, i.e. it ignores the control flow caused by exceptions.
    /// So it is possible that, in case of exception, the sequence is left from
    /// an textWriter point instead of from the last instruction.
    /// </remarks>
    /// <devDoc>
    /// <para>
    /// The bytes of an <see cref="InstructionSequence"/> can be represented in
    /// two ways:
    /// </para>
    /// <list type="bullet">
    ///		<item>
    ///			<term>Original</term>
    ///			<description>When the instruction sequence is read from the PE file,
    ///			the instructions are stored in the original image of the PE file.
    ///			The <see cref="startOffset"/> and <see cref="endOffset"/> fields,
    ///			relative to the first instructon of the method body, indicate
    ///			the start and the end of the current sequence.</description>
    ///		</item>
    ///		<item>
    ///			<term>Modified</term>
    ///			<description>If the instruction sequence is modified or is created,
    ///			it is stored in the <see cref="modifiedInstructionBytes"/> array
    ///			of bytes.</description>
    ///		</item>
    /// </list>
    /// </devDoc>
    public sealed class InstructionSequence : IDisposable
    {
        #region Fields

        /// <summary>
        /// Parent method body.
        /// </summary>
        private MethodBodyDeclaration body;

        /// <summary>
        /// Parent block.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if the
        /// instruction sequence is detached.
        /// </value>
        private InstructionBlock block;

        /// <summary>
        /// Start offset of the sequence in the IL body.
        /// </summary>
        /// <value>
        /// An offset, or <see cref="NotSet"/> if the field has not been affected.
        /// </value>
        private int startOffset;

        /// <summary>
        /// End offset of the sequence in the IL body.
        /// </summary>
        /// <value>
        /// An offset, or <see cref="NotSet"/> if the field has not been affected.
        /// </value>
        private int endOffset;

        /// <summary>
        /// Token of the current sequence, i.e. position in the array
        /// of sequence in the method body.
        /// </summary>
        /// <value>
        /// An offset, or <see cref="NotSet"/> if the field has not been affected.
        /// </value>
        private short token;

        /// <summary>
        /// Binary content of the sequence, if modified.
        /// </summary>
        /// <value>
        /// An array of bytes, or <b>null</b> if the sequence has not been modified.
        /// </value>
        private byte[] modifiedInstructionBytes;

        /// <summary>
        /// Ordinal of the first symbol sequence point in the sequence.
        /// </summary>
        private short firstOriginalSymbolSequencePoint;

        /// <summary>
        /// Means that a field has not been set.
        /// </summary>
        internal const int NotSet = -1;

        /// <summary>
        /// Linked list node associated to this instance.
        /// </summary>
        private readonly LinkedListNode<InstructionSequence> node;

        private string comment;

        #endregion

        /// <summary>
        /// Initializes an empty instruction sequence.
        /// </summary>
        internal InstructionSequence( MethodBodyDeclaration methodBody )
        {
            this.startOffset = NotSet;
            this.endOffset = NotSet;
            this.token = NotSet;
            this.firstOriginalSymbolSequencePoint = NotSet;
            this.node = new LinkedListNode<InstructionSequence>( this );
            this.body = methodBody;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if ( this.startOffset != NotSet )
            {
                return
                    string.Format( CultureInfo.InvariantCulture,
                                   "{{InstructionSequence {2} Original, Range={0:x}-{1:x} }}",
                                   this.startOffset, this.endOffset, this.token );
            }
            else if ( this.modifiedInstructionBytes != null )
            {
                return
                    string.Format( CultureInfo.InvariantCulture, "{{InstructionSequence {1} Modified, Comment=\"{0}\"}}",
                                   this.comment, this.token );
            }
            else
            {
                return
                    string.Format( CultureInfo.InvariantCulture,
                                   "{{InstructionSequence {1} Not Commited, Comment=\"{0}\"}}",
                                   this.comment, this.token );
            }
        }


        /// <summary>
        /// Splits the current sequence after the position of an <see cref="InstructionReader"/>,
        /// and returns a new <see cref="InstructionSequence"/> containing the second
        /// part of the sequence.
        /// </summary>
        /// <param name="reader">An <see cref="InstructionReader"/> positioned 
        /// on the current <see cref="InstructionSequence"/>.</param>
        /// <returns>The <see cref="InstructionSequence"/> containing the second part
        /// of the sequence, or <b>null</b> if <paramref name="reader"/> is at
        /// the end of the sequence.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="InstructionReader"/>
        /// is not positioned at the current <see cref="InstructionSequence"/>.
        /// </exception>
        public InstructionSequence SplitAfterReaderPosition( InstructionReader reader )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( reader, "reader" );
            ExceptionHelper.Core.AssertValidOperation( reader.CurrentInstructionSequence == this,
                                                       "InstructionSequenceNotAtReader" );

            #endregion

            int byteCount = this.ByteCount;

            if ( reader.IsAtEnfOfSequence )
            {
                return null;
            }

            InstructionSequence newSequence = this.block.MethodBody.CreateInstructionSequence();
            this.block.AddInstructionSequence( newSequence, NodePosition.After, this );

            if ( !this.IsOriginal )
            {
                // Split the current array in two parts.
                byte[] firstArray = new byte[reader.OffsetAfter];
                byte[] secondArray = new byte[byteCount - reader.OffsetAfter];
                Buffer.BlockCopy( this.modifiedInstructionBytes, 0, firstArray, 0, firstArray.Length );
                Buffer.BlockCopy( this.modifiedInstructionBytes, reader.OffsetAfter, secondArray, 0, secondArray.Length );
                this.modifiedInstructionBytes = firstArray;
                newSequence.modifiedInstructionBytes = secondArray;
            }
            else
            {
                newSequence.startOffset = this.startOffset + reader.OffsetAfter;
                newSequence.endOffset = this.endOffset;
                newSequence.ComputeFirstOriginalSymbolSequencePoint();
                this.endOffset = newSequence.startOffset;
            }

            return newSequence;
        }

        /// <summary>
        /// Splits the current <see cref="InstructionSequence"/> in maximally three sequences, one
        /// containing the instructions before the current instruction of a given <see cref="InstructionReader"/>,
        /// one containing only the current
        /// instruction and one containing the instructions after the current instruction.
        /// </summary>
        /// <param name="reader"><see cref="InstructionReader"/> positioned in the current <see cref="InstructionSequence"/>.</param>
        /// <param name="sequenceBefore">Returns a new sequence containing all instructions before the current one,
        /// or <b>null</b> if the current instruction is the first one.</param>
        /// <param name="sequenceAfter">Returns a new sequence containing all instructions after the current one,
        /// or <b>null</b> if the current instruction is the last one.</param>
        /// <remarks>
        /// After having called this method, the current sequence contains only the current instruction
        /// of <see cref="InstructionReader"/>.
        /// </remarks>
        public void SplitAroundReaderPosition( InstructionReader reader,
                                               out InstructionSequence sequenceBefore,
                                               out InstructionSequence sequenceAfter )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( reader, "reader" );
            ExceptionHelper.Core.AssertValidOperation( reader.CurrentInstructionSequence == this,
                                                       "InstructionSequenceNotAtReader" );

            #endregion

            // Cache some values;
            int byteCount = this.ByteCount;

            // Create the instruction sequence Before.
            if ( reader.OffsetBefore == 0 )
            {
                sequenceBefore = null;
            }
            else
            {
                sequenceBefore = this.block.MethodBody.CreateInstructionSequence();
                this.block.AddInstructionSequence( sequenceBefore, NodePosition.Before, this );

                if ( this.modifiedInstructionBytes != null )
                {
                    byte[] arrayBefore = new byte[reader.OffsetBefore];
                    Buffer.BlockCopy( this.modifiedInstructionBytes, 0, arrayBefore, 0, arrayBefore.Length );
                    sequenceBefore.modifiedInstructionBytes = arrayBefore;
                }
                else
                {
                    sequenceBefore.startOffset = this.startOffset;
                    sequenceBefore.endOffset = this.startOffset + reader.OffsetBefore;
                    sequenceBefore.firstOriginalSymbolSequencePoint = this.firstOriginalSymbolSequencePoint;
                }

                // Redirect all branch targers to "sequenceBefore".
                this.Redirect( sequenceBefore );
            }

            // Create the instruction sequence After.
            if ( reader.IsAtEnfOfSequence )
            {
                sequenceAfter = null;
            }
            else
            {
                sequenceAfter = this.block.MethodBody.CreateInstructionSequence();
                this.block.AddInstructionSequence( sequenceAfter, NodePosition.After, this );

                if ( this.modifiedInstructionBytes != null )
                {
                    byte[] arrayAfter = new byte[byteCount - reader.OffsetAfter];
                    Buffer.BlockCopy( this.modifiedInstructionBytes, reader.OffsetAfter, arrayAfter, 0,
                                      arrayAfter.Length );
                    sequenceAfter.modifiedInstructionBytes = arrayAfter;
                }
                else
                {
                    sequenceAfter.startOffset = this.startOffset + reader.OffsetAfter;
                    sequenceAfter.endOffset = this.endOffset;
                    sequenceAfter.ComputeFirstOriginalSymbolSequencePoint();
                }
            }

            // Modify the current instruction sequence.
            if ( sequenceAfter != null || sequenceBefore != null )
            {
                if ( this.modifiedInstructionBytes != null )
                {
                    byte[] arrayInstead = new byte[reader.OffsetAfter - reader.OffsetBefore];
                    Buffer.BlockCopy( this.modifiedInstructionBytes, reader.OffsetBefore, arrayInstead,
                                      0, arrayInstead.Length );
                    this.modifiedInstructionBytes = arrayInstead;
                }
                else
                {
                    int _startOffset = this.startOffset;
                    this.startOffset = _startOffset + reader.OffsetBefore;
                    this.endOffset = _startOffset + reader.OffsetAfter;
                    this.ComputeFirstOriginalSymbolSequencePoint();
                }
            }

            
#if DEBUG
            // Check postconditions (both sequences are valid).
            if (sequenceAfter != null) 
                sequenceAfter.CheckInstructions();

            if (sequenceBefore != null) 
                sequenceBefore.CheckInstructions();

            this.CheckInstructions();
#endif
           
        }

        #region Properties

        /// <summary>
        /// Gets the array of bytes containing the modified instructions, or
        /// <b>null</b> if the sequence has not been modified.
        /// </summary>
        internal byte[] ModifiedInstructionBytes
        {
            get { return this.modifiedInstructionBytes; }
            set
            {
                if ( this.modifiedInstructionBytes != value )
                {
                    this.modifiedInstructionBytes = value;
                    this.startOffset = NotSet;
                    this.endOffset = NotSet;
                }
            }
        }

        /// <summary>
        /// Gets or sets the end offset of the current instruction sequence
        /// in the method instruction stream.
        /// </summary>
        internal int EndOffset
        {
            get { return endOffset; }
            set
            {
#if ASSERT
                if ( value < 0 )
                    throw new ArgumentOutOfRangeException( "value" );
#endif

                //if (this.startOffset != NotSet && value < this.startOffset) throw new ArgumentException();
                endOffset = value;
            }
        }


        /// <summary>
        /// Gets or sets the start offset of the current instruction sequence
        /// in the method instruction stream.
        /// </summary>
        internal int StartOffset
        {
            get { return startOffset; }
            set
            {
#if ASSERT
                if ( value < 0 )
                    throw new ArgumentOutOfRangeException( "value" );
#endif
                // if (this.endOffset != NotSet && value > this.endOffset) throw new ArgumentException();
                startOffset = value;
            }
        }

        /// <summary>
        /// Gets the linked list node associated to the current instance.
        /// </summary>
        internal LinkedListNode<InstructionSequence> Node
        {
            get { return this.node; }
        }

        /// <summary>
        /// Gets the <see cref="MethodBodyDeclaration"/> to which the current
        /// instance is related.
        /// </summary>
        public MethodBodyDeclaration MethodBody
        {
            get { return this.body; }
        }

        /// <summary>
        /// Gets the next sibling <see cref="InstructionSequence"/>.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionSequence"/> having the same parent <see cref="InstructionBlock"/>,
        /// or <b>null</b> if the current sequence is the last of its block.
        /// </value>
        [Browsable( false )]
        public InstructionSequence NextSiblingSequence
        {
            get { return this.node.Next == null ? null : this.node.Next.Value; }
        }

        /// <summary>
        /// Gets the next <see cref="InstructionSequence"/> in the current <see cref="InstructionBlock"/>
        /// or in the next sibling <see cref="InstructionBlock"/>.
        /// </summary>
        [Browsable( false )]
        public InstructionSequence NextDeepSequence
        {
            get
            {
                InstructionSequence nextSequence = this.NextSiblingSequence;

                if ( nextSequence == null )
                {
                    InstructionBlock nextBlock = this.ParentInstructionBlock.NextDeepBlock;
                    if ( nextBlock != null )
                        nextSequence = nextBlock.FindFirstInstructionSequence();
                }

                return nextSequence;
            }
        }

        /// <summary>
        /// Gets the previous sibling <see cref="InstructionSequence"/>.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionSequence"/> having the same parent <see cref="InstructionBlock"/>,
        /// or <b>null</b> if the current sequence is the first of its block.
        /// </value>
        [Browsable( false )]
        public InstructionSequence PreviousSiblingSequence
        {
            get { return this.node.Previous == null ? null : this.node.Previous.Value; }
        }


        /// <summary>
        /// Detaches the current <see cref="InstructionSequence"/> from its 
        /// parent <see cref="InstructionBlock"/>.
        /// </summary>
        /// <remarks>
        /// This method does not detaches the <see cref="InstructionSequence"/> to the
        /// <see cref="MethodBodyDeclaration"/> to which it belongs. Note that the state
        /// of the instruction stream may be inconsistent after calling this method, because
        /// instructions may still reference the current <see cref="InstructionSequence"/>
        /// as a branch target. In order to redirect branch targets, call the
        /// <see cref="Remove(InstructionSequence)"/>  or <see cref="Redirect"/> method.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The instruction sequence
        /// is not attached.</exception>
        public void Detach()
        {
            #region Preconditions

            if ( this.ParentInstructionBlock == null )
            {
                throw new InvalidOperationException();
            }
            this.AssertNotDisposed();

            #endregion

            if ( this.ParentInstructionBlock != null )
            {
                this.node.List.Remove( this.node );
            }

            this.block = null;
        }

        /// <summary>
        /// Removes the current <see cref="InstructionSequence"/> from 
        /// the <see cref="MethodBodyDeclaration"/> to which it belongs, and
        /// optionally redirects to another <see cref="InstructionSequence"/>
        /// all branching targets to the current <see cref="InstructionSequence"/>
        /// </summary>
        /// <param name="redirectInstructionSequence"><see cref="InstructionSequence"/>
        /// to which branches to the current sequence should be redirected, or <b>null</b>
        /// if branching targets should not be redirected.
        /// </param>
        /// <remarks>
        /// This method has the effect of disposing the current <see cref="InstructionSequence"/>.
        /// </remarks>
        public void Remove( InstructionSequence redirectInstructionSequence )
        {
            #region Preconditions

            this.AssertNotDisposed();
            ExceptionHelper.Core.AssertValidArgument(
                redirectInstructionSequence == null || this.body == redirectInstructionSequence.body,
                "redirectInstructionSequence", "MethodBodySameInBothSequences" );

            #endregion

            if ( this.block != null )
            {
                this.Detach();
            }

            if ( redirectInstructionSequence == null )
            {
                this.body.UnregisterInstructionSequence( this );
            }
            else
            {
                this.body.RedirectBranchTarget( this, redirectInstructionSequence );
            }

            this.token = NotSet;
            this.body = null;
        }

        /// <summary>
        /// Removes the current <see cref="InstructionSequence"/> from 
        /// the <see cref="MethodBodyDeclaration"/> to which it belongs, but does not
        /// redirect branching targets.
        /// </summary>
        public void Remove()
        {
            this.Remove( null );
        }

        /// <summary>
        /// Redirects to another <see cref="InstructionSequence"/> all branch targets
        /// currently referencing the current <see cref="InstructionSequence"/>.
        /// </summary>
        /// <param name="newSequence">The <see cref="InstructionSequence"/> that should
        /// become the new target.</param>
        public void Redirect( InstructionSequence newSequence )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( newSequence, "newSequence" );
            this.AssertNotDisposed();
            ExceptionHelper.Core.AssertValidArgument( this.body == newSequence.body, "redirectInstructionSequence",
                                                      "MethodBodySameInBothSequences" );

            #endregion

            this.body.RedirectBranchTarget( this, newSequence );
        }

        /// <summary>
        /// Throws an exception if the current object has been disposed.
        /// </summary>
        private void AssertNotDisposed()
        {
            if ( this.body == null )
            {
                throw new ObjectDisposedException( "InstructionSequence" );
            }
        }

        /// <inheritdoc />
        [SuppressMessage( "Microsoft.Design", "CA1063:ImplementIDisposableCorrectly" )]
        void IDisposable.Dispose()
        {
            if ( this.body != null )
            {
                this.Remove();
            }
        }

        /// <summary>
        /// Determines whether the current instruction sequence is original, i.e.
        /// whether it was unmodified from the original module.
        /// </summary>
        public bool IsOriginal
        {
            get { return this.modifiedInstructionBytes == null; }
        }


        /// <summary>
        /// Gets the first sequence point token.
        /// </summary>
        internal short FirstOriginalSymbolSequencePoint
        {
            get { return firstOriginalSymbolSequencePoint; }
        }

        /// <summary>
        /// Computes the <see cref="firstOriginalSymbolSequencePoint"/> field.
        /// </summary>
        internal void ComputeFirstOriginalSymbolSequencePoint()
        {
            SymbolSequencePoint searchedPoint = new SymbolSequencePoint( this.startOffset );

            if ( this.MethodBody.OriginalSymbolSequencePoints != null )
            {
                int position =
                    Array.BinarySearch( this.MethodBody.OriginalSymbolSequencePoints, searchedPoint );

                if ( position < 0 )
                {
                    position = ~position;
                }

                if ( position < this.MethodBody.OriginalSymbolSequencePoints.Length )
                {
                    this.firstOriginalSymbolSequencePoint = (short) position;
                }
                else
                {
                    this.firstOriginalSymbolSequencePoint = -1;
                }
            }
            else
            {
                this.firstOriginalSymbolSequencePoint = -1;
            }
        }

        /// <summary>
        /// Gets the number of bytes in the current instruction sequence.
        /// </summary>
        internal int ByteCount
        {
            get
            {
                if ( this.modifiedInstructionBytes != null )
                {
                    return this.modifiedInstructionBytes.Length;
                }
                else
                {
                    return this.endOffset - this.startOffset;
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the comment on this instruction sequence.
        /// </summary>
        /// <remarks>
        /// This comment will be written in MSIL output. This is for
        /// code weaver debugging only.
        /// </remarks>
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        #region Public properties

        /// <summary>
        /// Gets the instruction sequence token.
        /// </summary>
        [ReadOnly( true )]
        public short Token
        {
            get
            {
                this.AssertNotDisposed();
                return token;
            }
            internal set { this.token = value; }
        }

        /// <summary>
        /// Gets the block to which the current sequences belongs.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if the current
        /// <see cref="InstructionSequence"/> is detached.
        /// </value>
        [Browsable( false )]
        public InstructionBlock ParentInstructionBlock
        {
            get { return this.block; }
            internal set { this.block = value; }
        }

        #endregion

        #region IWriteILReference Members

        /// <summary>
        /// Writes the label corresponding to the current sequence.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        internal void WriteILReference( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.block != null && this.block.MethodBody != null,
                                                       "CannotWriteReferenceOfDetachedSequence" );

            #endregion

            if ( this.token >= 0 )
            {
                writer.WriteLabelReference( this.token );
            }
            else
            {
                writer.WriteIdentifier( "unresolved_" + this.startOffset.ToString( "x", CultureInfo.InvariantCulture ) );
            }
        }

        #endregion

        /// <summary>
        /// Checks whether the instruction stream can be read.
        /// </summary>
        /// <remarks>
        /// Should be used only in debugging scenarios. 
        /// Conditional to the DEBUG compilation symbol.
        /// </remarks>
        [Conditional( "DEBUG" )]
        public void CheckInstructions()
        {
            using ( InstructionReader reader = this.block.MethodBody.CreateInstructionReader( false ) )
            {
                reader.EnterInstructionSequence( this );
                while ( reader.ReadInstruction() )
                {
                }
            }
        }
    }
}