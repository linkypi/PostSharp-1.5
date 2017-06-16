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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using PostSharp.CodeModel.Collections;
using PostSharp.ModuleReader;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents the IL body of a method. 
    /// </summary>
    /// <remarks>
    /// Method bodies are owned by methods 
    /// (<see cref="MethodDefDeclaration"/>).
    /// </remarks>
    public sealed class MethodBodyDeclaration : Declaration, IWriteILDefinition, IDisposable
    {
        #region Fields

        /// <summary>
        /// Value of the <see cref="MethodBodyDeclaration.MaxStack"/> property
        /// that specifies that the current property should be computed.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704",
            Justification = "Spelling is correct." )] public const int RecomputeMaxStack = -1;

        /// <summary>
        /// Collection of local variables.
        /// </summary>
        private LocalVariableDeclarationCollection localVariables;

        /// <summary>
        /// Determines whether local variables should be initialized to their default value.
        /// </summary>
        private bool initLocalVariables = true;

        /// <summary>
        /// Root instruction block.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if the method body
        /// has not yet been resolved or is empty.
        /// </value>
        private InstructionBlock rootInstructionBlock;

        /// <summary>
        /// Maximum stack depth needed to execute this method.
        /// </summary>
        private int maxStack = RecomputeMaxStack;

        /// <summary>
        /// Maps <see cref="InstructionSequence"/> offset in the IL body
        /// to tokens, i.e. positions in the <see cref="sequences"/> list.
        /// </summary>
        private Dictionary<int, int> mapInstructionSequenceOffsetToToken;

        /// <summary>
        /// List of sequences.
        /// </summary>
        private List<InstructionSequence> sequences;

        private readonly List<InstructionBlock> blocks = new List<InstructionBlock>();

        /// <summary>
        /// List of original symbol sequence points.
        /// </summary>
        private SymbolSequencePoint[] originalSymbolSequencePoints;

        private List<SymbolSequencePoint> additionalSymbolSequencePoints;

        private bool isModified;

        #endregion

        #region Instruction sequences

        internal void SetInstructionSequenceCapacity( int capacity )
        {
            this.sequences = new List<InstructionSequence>( capacity );
            this.mapInstructionSequenceOffsetToToken = new Dictionary<int, int>( capacity );
        }

        /// <summary>
        /// Gets the number of sequences in the current method body.
        /// </summary>
        /// <remarks>
        /// This property returns the size of the vector needed to store additional
        /// information about sequences, while using the sequence token as the array index.
        /// </remarks>
        public int InstructionSequenceCount
        {
            get { return this.sequences == null ? 0 : this.sequences.Count; }
        }

        /// <summary>
        /// Gets an enumator for all the instruction sequences (<see cref="InstructionSequence"/>)
        /// contained in the current method body.
        /// </summary>
        /// <returns>An enumerator of the sequences contained in the current method body.</returns>
        public IEnumerator<InstructionSequence> GetInstructionSequenceEnumerator()
        {
            for ( int i = 0; i < this.sequences.Count; i++ )
            {
                InstructionSequence sequence = this.sequences[i];
                if ( sequence != null )
                    yield return sequence;
            }
        }

        /// <summary>
        /// Creates an <see cref="InstructionSequence"/> for the current method body.
        /// </summary>
        /// <returns>The new <see cref="InstructionSequence"/>.</returns>
        /// <remarks>
        /// This method does not add the new <see cref="InstructionSequence"/>
        /// to any block. This should be done in user code.
        /// </remarks>
        public InstructionSequence CreateInstructionSequence()
        {
            InstructionSequence sequence = new InstructionSequence( this );
            this.RegisterInstructionSequence( sequence );
            this.isModified = true;
            return sequence;
        }

        /// <summary>
        /// Creates an <see cref="InstructionBlock"/> for the current method body.
        /// </summary>
        /// <returns>The new <see cref="InstructionBlock"/>.</returns>
        public InstructionBlock CreateInstructionBlock()
        {
            InstructionBlock block = new InstructionBlock( (short) this.blocks.Count, this );
            this.blocks.Add( block );
            return block;
        }

        internal InstructionBlock CreateInstructionBlock( int startOffset, int endOffset )
        {
            InstructionBlock block = new InstructionBlock( (short) this.blocks.Count, this,
                                                           startOffset, endOffset );
            this.blocks.Add( block );
            return block;
        }

        /// <summary>
        /// Gets an <see cref="InstructionSequence"/> given its token.
        /// </summary>
        /// <param name="token">An <see cref="InstructionSequence"/> token.</param>
        /// <returns>The <see cref="InstructionSequence"/> whose token
        /// is <paramref name="token"/>.</returns>
        /// <remarks>
        /// Many tokens may resolve to the same <see cref="InstructionSequence"/>.
        /// This method may return an <see cref="InstructionSequence"/> whose token
        /// is different than <paramref name="token"/>.
        /// </remarks>
        public InstructionSequence GetInstructionSequenceByToken( int token )
        {
            return this.sequences[token];
        }

        /// <summary>
        /// Gets an <see cref="InstructionBlock"/> given its token.
        /// </summary>
        /// <param name="token">An <see cref="InstructionBlock"/> token.</param>
        /// <returns>The <see cref="InstructionBlock"/> whose token
        /// is <paramref name="token"/>.</returns>
        public InstructionBlock GetInstructionBlockByToken( short token )
        {
            return this.blocks[token];
        }

        /// <summary>
        /// Gets an <see cref="InstructionSequence"/> given its offset in the 
        /// binary IL method body.
        /// </summary>
        /// <param name="offset">The offset in the binary IL method body.</param>
        /// <returns>The <see cref="InstructionSequence"/> whose offset
        /// is <paramref name="offset"/>.</returns>
        internal InstructionSequence GetInstructionSequenceByOffset( int offset )
        {
            return this.GetInstructionSequenceByToken( this.mapInstructionSequenceOffsetToToken[offset] );
        }

        /// <summary>
        /// Registers an existing <see cref="InstructionSequence"/> in the method 
        /// body and assigns it a token.
        /// </summary>
        /// <param name="sequence">An <see cref="InstructionSequence"/>.</param>
        internal void RegisterInstructionSequence( InstructionSequence sequence )
        {
            if ( this.sequences == null )
            {
                this.sequences = new List<InstructionSequence>( 16 );
            }

            sequence.Token = (short) this.sequences.Count;
            this.sequences.Add( sequence );
            if ( sequence.StartOffset != InstructionSequence.NotSet )
            {
                this.mapInstructionSequenceOffsetToToken.Add( sequence.StartOffset, sequence.Token );
            }
        }

        internal void UnregisterInstructionSequence( InstructionSequence sequence )
        {
            this.RedirectBranchTarget( sequence, null );
        }

        internal void RedirectBranchTarget( InstructionSequence oldTarget, InstructionSequence newTarget )
        {
            for ( int i = 0; i < this.sequences.Count; i++ )
            {
                InstructionSequence currentSequence = this.sequences[i];
                if ( currentSequence != null && currentSequence.Token == oldTarget.Token )
                {
                    this.sequences[i] = newTarget;
                }
            }
        }

        #endregion

        #region Local Variables


        /// <summary>
        /// Gets or sets the collection of local variables (<see cref="LocalVariableDeclaration"/>).
        /// </summary>
        internal void SetLocalVariables( LocalVariableDeclarationCollection value )
        {
            this.localVariables = value;
        }

        /// <summary>
        /// Gets the number of local variables.
        /// </summary>
        public int LocalVariableCount
        {
            get { return this.localVariables == null ? 0 : this.localVariables.Count; }
        }

        

        /// <summary>
        /// Ensure that the collection of local variables are writable.
        /// </summary>
        /// <remarks>This method is called automatically when you try to add a new local
        /// variable. You should call it explicitely in the rare case where you want
        /// to remove a local variable or change its type.</remarks>
        public void EnsureWritableLocalVariables()
        {
            if ( this.localVariables == null )
            {
                this.localVariables = new LocalVariableDeclarationCollection( this, "localVariables" );
                this.localVariables.EnsureCapacity( 4 );
            }
            else if ( this.localVariables.IsReadOnly )
            {
                this.localVariables = this.localVariables.GetLocalCopy( this, "localVariables" );
            }
        }

        internal void AddLocalVariable( LocalVariableDeclaration localVariable )
        {
            this.EnsureWritableLocalVariables();
            this.localVariables.Add( localVariable );
        }

        /// <summary>
        /// Removes a local variable.
        /// </summary>
        /// <param name="index">Index of the local variable.</param>
        /// <remarks>
        /// This method does not remove symbols (see <see cref="LocalVariableSymbol"/>) associated
        /// to the current local variable. Be sure to remove all associated symbols before removing
        /// the current local variable.
        /// </remarks>
        public void RemoveLocalVariable(int index)
        {
            this.EnsureWritableLocalVariables();
            if ( this.localVariables != null )
            {
                this.localVariables.Remove( this.localVariables[index] );
            }
        }

        /// <summary>
        /// Gets a <see cref="LocalVariableDeclaration"/> given its index.
        /// </summary>
        /// <param name="index">The local variable index.</param>
        /// <returns>The <see cref="LocalVariableDeclaration"/> whose
        /// position is <paramref name="index"/>.</returns>
        public LocalVariableDeclaration GetLocalVariable( int index )
        {
            #region Preconditions

            if ( index < 0 || index >= this.LocalVariableCount )
            {
                throw new ArgumentOutOfRangeException( "index" );
            }

            #endregion

            return this.localVariables[index];
        }

        /// <summary>
        /// Determines whether local variables should be initialized 
        /// by the runtime to their default value.
        /// </summary>
        public bool InitLocalVariables
        {
            get { return initLocalVariables; }
            set { initLocalVariables = value; }
        }

        #endregion

        #region Other Properties

        /// <summary>
        /// Gets or sets the <see cref="UnmanagedBuffer"/> containing the
        /// original instructions.
        /// </summary>
        internal UnmanagedBuffer OriginalInstructions { get; set; }


        /// <summary>
        /// Gets or sets the maximal number of items that the runtime evaluation
        /// stack should contain.
        /// </summary>
        /// <value>
        /// Any positive integer, or <see cref="RecomputeMaxStack"/> if the
        /// property should be recomputed by the library.
        /// </value>
        [ReadOnly( true )]
        public int MaxStack
        {
            get { return this.maxStack; }
            set { this.maxStack = value; }
        }

        /// <summary>
        /// Gets or sets the root <see cref="InstructionBlock"/>.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if no
        /// root block is specified (in the current case the method body is empty).
        /// </value>
        [Browsable( false )]
        public InstructionBlock RootInstructionBlock
        {
            get { return rootInstructionBlock; }
            set
            {
                ExceptionHelper.Core.AssertValidOperation( value == null || value.MethodBody == this,
                                                           "CannotAddBlockToOtherMethodBody" );
                if ( this.rootInstructionBlock != null )
                {
                    this.rootInstructionBlock.OnRemovingFromParent();
                }

                this.rootInstructionBlock = value;

                if ( value != null )
                {
                    value.OnAddingToParent( this, "rootInstructionBlock" );
                }
            }
        }

        /// <summary>
        /// Gets the number of instruction blocks (<see cref="InstructionBlock"/>)
        /// created for the current method body.
        /// </summary>
        public int InstructionBlockCount
        {
            get { return this.blocks.Count; }
        }

        /// <summary>
        /// Gets the <see cref="MethodDefDeclaration"/> to which the current 
        /// body belongs.
        /// </summary>
        [Browsable( false )]
        public MethodDefDeclaration Method
        {
            get { return (MethodDefDeclaration) this.Parent; }
        }

        internal ISymbolDocument AnySymbolDocument
        {
            get
            {
                if ( this.originalSymbolSequencePoints != null )
                {
                    for ( int i = 0; i < this.originalSymbolSequencePoints.Length; i++ )
                    {
                        if ( this.originalSymbolSequencePoints[i].Document != null )
                            return this.originalSymbolSequencePoints[i].Document;
                    }
                }

                if ( this.additionalSymbolSequencePoints != null )
                {
                    for ( int i = 0; i < this.additionalSymbolSequencePoints.Count; i++ )
                    {
                        if ( this.additionalSymbolSequencePoints[i].Document != null )
                            return this.additionalSymbolSequencePoints[i].Document;
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// Gets or sets the list of symbolic sequence points in the original form of current method.
        /// </summary>
        internal SymbolSequencePoint[] OriginalSymbolSequencePoints
        {
            get { return originalSymbolSequencePoints; }
            set { originalSymbolSequencePoints = value; }
        }

        internal int NextAdditionalSymbolSequencePointPosition
        {
            get
            {
                if ( this.additionalSymbolSequencePoints == null )
                {
                    return 0;
                }
                else
                {
                    return this.additionalSymbolSequencePoints.Count;
                }
            }
        }

        /// <summary>
        /// Determines whether the current method body has been modified.
        /// </summary>
        public bool IsModified
        {
            get { return isModified; }
            internal set { isModified = value; }
        }


        internal void AddSymbolSequencePoints( List<SymbolSequencePoint> sequencePoints )
        {
            if ( this.additionalSymbolSequencePoints == null )
            {
                this.additionalSymbolSequencePoints = new List<SymbolSequencePoint>(
                    Math.Min( sequencePoints.Count, 8 ) );
            }
            else
            {
                this.additionalSymbolSequencePoints.Capacity =
                    this.additionalSymbolSequencePoints.Count + sequencePoints.Count;
            }

            this.additionalSymbolSequencePoints.AddRange( sequencePoints );
        }

        internal SymbolSequencePoint GetAdditionalSequencePoint( int position )
        {
            return this.additionalSymbolSequencePoints[position];
        }

        #endregion

        #region IWriteILDefinition Members

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            if ( this.rootInstructionBlock != null )
            {
                writer.Options.InMethodBody = true;
                PreparingInstructionEmitter computeAddressEmitter = new PreparingInstructionEmitter( this );
                AddressRange[] addresses;
                bool[] localHasReference;
                bool[] localHasMissingSymbol;
                computeAddressEmitter.Prepare( out addresses, out localHasMissingSymbol, out localHasReference );

                writer.WriteKeyword( ".maxstack" );
                writer.WriteInteger( this.maxStack, IntegerFormat.Decimal );
                writer.WriteLineBreak();


                WriteILInstructionEmitter writeIlEmitter = new WriteILInstructionEmitter(
                    this, writer, addresses, localHasMissingSymbol, localHasReference );
                writeIlEmitter.WriteIL();
                writer.Options.InMethodBody = false;
            }
            else
            {
                writer.WriteCommentLine( "Error: this method body has no root instruction block!" );
            }
        }

        #endregion

        #region Instruction iterator

        /// <summary>
        /// Calls a specified delegate for each instruction of the current method body.
        /// </summary>
        /// <param name="action">A delegate of type <see cref="InstructionAction"/>
        /// that will be called for each instruction.</param>
        /// <remarks>
        /// This method offers an easy way to iterate the instructions
        /// of a method.
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// MethodDefDeclaration.MethodBody.ForEachInstruction(
        ///		delegate(InstructionReader instructionReader)
        ///		{
        ///			// To something.
        ///     }
        /// ]]>
        /// </example>
        public void ForEachInstruction( InstructionAction action )
        {
            InstructionReader reader = this.CreateInstructionReader();
            ForEachInstructionProcessBlock( this.rootInstructionBlock, action, reader );
        }

        /// <summary>
        /// Calls a specified delegate for each instruction of a given block.
        /// </summary>
        /// <param name="block">An <see cref="InstructionBlock"/>.</param>
        /// <param name="action">A delegate of type <see cref="InstructionAction"/>
        /// that will be called for each instruction.</param>
        /// <param name="reader">The <see cref="InstructionReader"/> that should
        /// be used to read the instructions.</param>
        private static void ForEachInstructionProcessBlock( InstructionBlock block,
                                                            InstructionAction action, InstructionReader reader )
        {
            reader.EnterInstructionBlock( block );

            if ( block.HasChildrenBlocks )
            {
                InstructionBlock child = block.FirstChildBlock;
                while ( child != null )
                {
                    ForEachInstructionProcessBlock( child, action, reader );

                    child = child.NextSiblingBlock;
                }
            }
            else
            {
                InstructionSequence sequence = block.FirstInstructionSequence;

                while ( sequence != null )
                {
                    reader.EnterInstructionSequence( sequence );

                    while ( reader.ReadInstruction() )
                    {
                        action( reader );
                    }

                    reader.LeaveInstructionSequence();

                    sequence = sequence.NextSiblingSequence;
                }
            }


            reader.LeaveInstructionBlock();
        }

        #endregion

        /// <summary>
        /// Gets an <see cref="InstructionReader"/> for the current method body, which will
        /// resolve local variable symbols.
        /// </summary>
        /// <returns>An <see cref="InstructionReader"/> assigned to the current 
        /// <see cref="MethodBodyDeclaration"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This is not conceptually a property. It creates a new instance of the InstructionReader." )
        ]
        public InstructionReader CreateInstructionReader()
        {
            return new InstructionReader( this, true );
        }


        /// <summary>
        /// Gets an <see cref="InstructionReader"/> for the current method body and specify
        /// whether it should resolve local variable symbols.
        /// </summary>
        /// <param name="resolveSymbols">Whether the <see cref="InstructionReader"/> should resolve
        /// local variable symbols.</param>
        /// <returns>An <see cref="InstructionReader"/> assigned to the current 
        /// <see cref="MethodBodyDeclaration"/>.</returns>
        public InstructionReader CreateInstructionReader( bool resolveSymbols )
        {
            return new InstructionReader( this, resolveSymbols );
        }

        /// <summary>
        /// Gets an <see cref="InstructionReader"/> over the <i>original</i> and <i>unstructured</i>
        /// instruction stream.
        /// </summary>
        /// <returns>An <see cref="InstructionReader"/> that does not resolve lexical scopes.</returns>
        public InstructionReader CreateOriginalInstructionReader()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( !this.isModified, "MethodBodyIsModified" );

            #endregion

            InstructionSequence sequence = new InstructionSequence( this )
                                               {
                                                   StartOffset = 0,
                                                   EndOffset = this.OriginalInstructions.Size
                                               };

            InstructionReader reader = this.CreateInstructionReader( false );
            reader.EnterInstructionSequence( sequence );

            return reader;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if ( this.Parent == null )
            {
                return "Unattached Method Body";
            }
            else
            {
                return "Body of " + this.Method.ToString();
            }
        }

        /// <summary>
        /// Write the current method body (as MSIL) to the console.
        /// </summary>
        public void DebugWriteIL()
        {
            IndentedTextWriter writer = new IndentedTextWriter( Console.Out );
            ILWriter ilWriter = new ILWriter( writer );
            ilWriter.Indent++;
            this.WriteILDefinition( ilWriter );
            ilWriter.Indent--;
        }

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            if ( !this.localVariables.IsReadOnly && !this.localVariables.IsDisposed )
            {
                this.localVariables.Dispose();
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents an action that performs an action on the current instruction
    /// of a specified <see cref="InstructionReader"/>.
    /// </summary>
    /// <param name="reader">An <see cref="InstructionReader"/> positioned at an instruction.</param>
    public delegate void InstructionAction( InstructionReader reader );
}