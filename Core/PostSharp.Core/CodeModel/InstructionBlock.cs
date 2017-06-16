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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using PostSharp.CodeModel.Collections;
using PostSharp.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// A single node of the hierarchical structure of a method body. 
    /// </summary>
    /// <remarks>
    /// A method body (<see cref="MethodBodyDeclaration"/>)
    /// is represented as a tree of instruction blocks. Lexical scopes (definition
    /// of local variable symbols) and exception handlers are defined at
    /// <see cref="InstructionBlock"/> level. Instruction blocks may contain
    /// either children blocks, either instruction sequences (<see cref="InstructionSequence"/>).
    /// Independently, every <see cref="InstructionBlock"/> can contain
    /// exception handlers and local variable symbols.
    /// </remarks>
    public sealed class InstructionBlock : Element
    {
        #region Fields

        /// <summary>
        /// Collection of children blocks.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlockCollection"/>, or <b>null</b> if
        /// the current block has no child block.
        /// </value>
        private InstructionBlockCollection children;

        /// <summary>
        /// Start offset of the block in the IL body (used during method structure construction).
        /// </summary>
        private readonly int startOffset;

        /// <summary>
        /// End offset of the block in the IL body (used during method structure construction).
        /// </summary>
        private readonly int endOffset;

        /// <summary>
        /// Collection of local variables.
        /// </summary>
        /// <value>
        /// A <see cref="LocalVariableSymbolCollection"/>, or <b>null</b> if there is
        /// no local variable.
        /// </value>
        private LocalVariableSymbolCollection localVariableSymbols;

        /// <summary>
        /// Collection of exeption handlers protecting the current block.
        /// </summary>
        /// <value>
        /// An <see cref="ExceptionHandlerCollection"/>, or <b>null</b> if the
        /// current block is not protected.
        /// </value>
        private ExceptionHandlerCollection exceptionHandlers;

        /// <summary>
        /// Collection of instruction sequences.
        /// </summary>
        /// <value>
        /// A list of instruction sequences, or <b>null</b> if the current block
        /// does not contain any instruction sequence.
        /// </value>
        private LinkedList<InstructionSequence> instructionSequences;

        /// <summary>
        /// <see cref="LinkedListNode{T}"/> associated to the current block.
        /// </summary>
        /// <value>
        /// A <see cref="LinkedListNode{T}"/>, or <b>null</b> if the current block
        /// has no parent block.
        /// </value>
        private readonly LinkedListNode<InstructionBlock> node;

        private ExceptionHandler parentExceptionHandler;

        private string comment;

        private readonly MethodBodyDeclaration methodBody;

        private readonly short token;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="InstructionBlock"/> (which does not correspond 
        /// in a block in the original stream of instructions).
        /// </summary>
        /// <param name="methodBody"><see cref="MethodBodyDeclaration"/> to which the new <see cref="InstructionBlock"/> will belong.</param>
        /// <param name="token">Token of the new <see cref="InstructionBlock"/> inside <paramref name="methodBody"/>.</param>
        internal InstructionBlock( short token, MethodBodyDeclaration methodBody )
        {
            this.token = token;
            this.methodBody = methodBody;
            this.node = new LinkedListNode<InstructionBlock>( this );
        }

        /// <summary>
        /// Initializes a new <see cref="InstructionBlock"/> and specifies
        /// the parent block and the IL bytes range covered by the current block. 
        /// </summary>
        /// <param name="methodBody"><see cref="MethodBodyDeclaration"/> to which the new <see cref="InstructionBlock"/> will belong.</param>
        /// <param name="token">Token of the new <see cref="InstructionBlock"/> inside <paramref name="methodBody"/>.</param>
        /// <param name="startOffset">Start offset of the block in the binary IL method body.</param>
        /// <param name="endOffset">End offset of the block in the binary IL method body.</param>
        /// <remarks>
        /// This constructor is used during the process of building the tree of
        /// instruction blocks based on the exception handler clauses and the lexical
        /// scopes of the module.
        /// </remarks>
        internal InstructionBlock( short token, MethodBodyDeclaration methodBody, int startOffset, int endOffset )
            : this( token, methodBody )
        {
            this.startOffset = startOffset;
            this.endOffset = endOffset;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "InstructionBlock {7} [{6}], {2} local(s), {3} handler(s), {4} child(ren), {5} sequence(s), {0:x}-{1:x}",
                this.startOffset,
                this.endOffset,
                this.localVariableSymbols == null ? 0 : this.localVariableSymbols.Count,
                this.exceptionHandlers == null ? 0 : this.exceptionHandlers.Count,
                this.children == null ? 0 : this.children.Count,
                this.instructionSequences == null ? 0 : this.instructionSequences.Count,
                this.comment,
                this.token
                );
        }

#if DEBUG

        /// <summary>
        /// Writes recursively the structure of this block and its children to the console.
        /// </summary>
        public string DebugOutput()
        {
            StringWriter stringWriter = new StringWriter();
            this.DebugOutput( new IndentedTextWriter( stringWriter ) );
            return stringWriter.ToString();
        }

        /// <summary>
        /// Writes recursively the structure of this block and its children to a writer.
        /// </summary>
        /// <param name="writer">A writer.</param>
        public void DebugOutput( IndentedTextWriter writer )
        {
            writer.WriteLine( this.ToString() );
            writer.Indent++;
            if ( this.HasLocalVariableSymbols )
            {
                writer.WriteLine( "Local Variable Symbols:" );
                writer.Indent++;
                foreach ( LocalVariableSymbol symbol in this.localVariableSymbols )
                {
                    writer.WriteLine( symbol.ToString() );
                }
                writer.Indent--;
            }
            if ( this.HasExceptionHandlers )
            {
                writer.WriteLine( "Exception Handlers:" );
                writer.Indent++;
                foreach ( ExceptionHandler handler in exceptionHandlers )
                {
                    writer.WriteLine( handler.ToString() );
                }
                writer.Indent--;
            }
            if ( this.HasChildrenBlocks )
            {
                writer.WriteLine( "Children Instruction Blocks:" );
                writer.Indent++;
                foreach ( InstructionBlock child in this.children )
                {
                    child.DebugOutput( writer );
                    if ( child.ParentBlock != this )
                    {
                        writer.WriteLine( ">>> child.ParentBlock != this <<<" );
                    }
                }
                writer.Indent--;
            }
            writer.Indent--;
        }
#endif

        #region Public properties

        /// <summary>
        /// Gets the instruction sequence token.
        /// </summary>
        [ReadOnly( true )]
        public short Token
        {
            get { return token; }
        }

        /// <summary>
        /// Gets the parent block.
        /// </summary>
        /// <value>
        /// The parent block, or <b>null</b> if (a) the current block is
        /// the root block, or (b) has simply no parent.
        /// </value>
        [Browsable( false )]
        public InstructionBlock ParentBlock
        {
            get { return this.Parent as InstructionBlock; }
        }

        /// <summary>
        /// Gets or sets the comment associated to the current block.
        /// </summary>
        /// <remarks>
        /// This comment is a purely informative string that will be rendered into the MSIL code.
        /// It should be used for diagnostics only.
        /// </remarks>
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }


        /// <summary>
        /// Determines whether the current block has children blocks.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current block has at least one children block, otherwise <b>false</b>.
        /// </value>
        /// <remarks>
        /// A block may have either children blocks either instruction sequences.
        /// </remarks>
        public bool HasChildrenBlocks
        {
            get { return this.children != null && this.children.Count > 0; }
        }

        /// <summary>
        /// Determines whether the current block has exception handlers (<see cref="ExceptionHandler"/>).
        /// </summary>
        /// <value>
        /// <b>true</b> if the current block has at least one exception handler, otherwise <b>false</b>.
        /// </value>
        public bool HasExceptionHandlers
        {
            get { return this.exceptionHandlers != null && this.exceptionHandlers.Count > 0; }
        }

        /// <summary>
        /// Determines whether the current block has local variable symbols (<see cref="LocalVariableSymbol"/>).
        /// </summary>
        /// <value>
        /// <b>true</b> if the current block has at least one local variable symbol, otherwise <b>false</b>.
        /// </value>
        public bool HasLocalVariableSymbols
        {
            get { return this.localVariableSymbols != null && this.localVariableSymbols.Count > 0; }
        }

        /// <summary>
        /// Determines whether the current block has instruction sequences (<see cref="InstructionSequence"/>).
        /// </summary>
        /// <value>
        /// <b>true</b> if the current block has at least one instruction sequence, otherwise <b>false</b>.
        /// </value>
        public bool HasInstructionSequences
        {
            get { return this.instructionSequences != null && this.instructionSequences.Count > 0; }
        }


        /// <summary>
        /// Gets the parent exception handler.
        /// </summary>
        /// <value>
        /// An <see cref="ExceptionHandler"/>, or <b>null</b> if the current block is not
        /// a direct child of an exception handler.
        /// </value>
        [Browsable( false )]
        public ExceptionHandler ParentExceptionHandler
        {
            get { return this.parentExceptionHandler; }
            internal set { this.parentExceptionHandler = value; }
        }

        /// <summary>
        /// Determines whether the current block is a direct child of an exception handler
        /// (i.e. is a handler or filter block).
        /// </summary>
        public bool IsExceptionHandler
        {
            get { return this.parentExceptionHandler != null; }
        }

        /// <summary>
        /// Gets the method body containing the current instruction block.
        /// </summary>
        /// <value>
        /// The method body (<see cref="MethodBodyDeclaration"/>) containing this
        /// block, or <b>null</b> if the current block is not attached to any
        /// method body.
        /// </value>
        /// <remarks>
        /// Consider caching the result of the current method since its implementation
        /// is recursive.
        /// </remarks>
        [Browsable( false )]
        public MethodBodyDeclaration MethodBody
        {
            get { return this.methodBody; }
        }

        #endregion

        #region Children blocks

        /// <summary>
        /// Detaches the current block from its parent.
        /// </summary>
        /// <exception cref="InvalidOperationException">The instruction block
        /// has neither parent <see cref="InstructionBlock"/> neither
        /// parent <see cref="MethodBodyDeclaration"/>.</exception>
        public void Detach()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "InstructionBlockIsDetached" );
            ExceptionHelper.Core.AssertValidOperation( this.parentExceptionHandler == null,
                                                       "CannotDetachRootBlockOfHandler" );

            #endregion

            InstructionBlock parentBlock = this.ParentBlock;
            if ( parentBlock != null )
            {
                parentBlock.children.Remove( this.node );
            }
            else
            {
                ( (MethodBodyDeclaration) this.Parent ).RootInstructionBlock = null;
            }

            this.OnRemovingFromParent();
        }

        /// <summary>
        /// Adds a child block.
        /// </summary>
        /// <param name="newBlock">Block to insert.</param>
        /// <param name="position">Relative position of the new block w.r.t. <paramref name="referenceBlock"/>.</param>
        /// <param name="referenceBlock">Block after or before which <paramref name="newBlock"/> has to be inserted,
        /// or <b>null</b> if the new block has to be inserted at the first or the last.</param>
        /// <exception cref="ArgumentException">The <paramref name="newBlock"/> 
        /// instruction block is already attached.</exception>
        /// <exception cref="InvalidOperationException">The current block
        /// is detached or has no parent block.</exception>
        public void AddChildBlock( InstructionBlock newBlock, NodePosition position, InstructionBlock referenceBlock )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( newBlock, "newBlock" );
            ExceptionHelper.Core.AssertValidArgument( newBlock.Parent == null, "newBlock", "InstructionBlockIsAttached" );
            ExceptionHelper.Core.AssertValidOperation( newBlock.methodBody == this.methodBody,
                                                       "CannotAddBlockToOtherMethodBody" );
            if ( referenceBlock != null && referenceBlock.Parent != this )
            {
                throw new ArgumentException();
            }

            #endregion

            if ( this.children == null )
            {
                ExceptionHelper.Core.AssertValidOperation( !this.HasInstructionSequences, "InstructionBlockHasSequences" );

                this.children = new InstructionBlockCollection();
            }

            newBlock.OnAddingToParent( this, "childBlock" );

            LinkedListHelper.AddNode( this.children, newBlock.node, position,
                                      referenceBlock == null ? null : referenceBlock.node );
        }


        /// <summary>
        /// Gets the next block whith the same parent as the current block.
        /// </summary>
        /// <value>
        /// The child of the parent of the current block that is just next to the
        /// current block, or <b>null</b> if the current node is detached or
        /// has no is the last child of its parent.
        /// </value>
        [Browsable( false )]
        public InstructionBlock NextSiblingBlock
        {
            get { return this.node.Next == null ? null : this.node.Next.Value; }
        }

        /// <summary>
        /// Gets the next block in the method body tree, eventually with a different parent block.
        /// </summary>
        [Browsable( false )]
        public InstructionBlock NextDeepBlock
        {
            get
            {
                // Return next sibling if possible.
                if ( this.node.Next != null )
                {
                    return this.node.Next.Value;
                }

                // If there is a parent block, return the NextDeepBlock of the parent block.
                InstructionBlock parentBlock = this.ParentBlock;
                if ( parentBlock != null )
                {
                    return parentBlock.NextDeepBlock;
                }

                // Otherwise it is really the last block.
                return null;
            }
        }

        /// <summary>
        /// Gets the previous block with the same parent as the current block.
        /// </summary>
        /// <value>
        /// The child of the parent of the current block that is just previous to the
        /// current block, or <b>null</b> if the current node is detached or
        /// has no is the first child of its parent.
        /// </value>
        [Browsable( false )]
        public InstructionBlock PreviousSiblingBlock
        {
            get { return this.node.Previous == null ? null : this.node.Previous.Value; }
        }

        /// <summary>
        /// Gets the previous block in the method body tree, eventually with a different parent block.
        /// </summary>
        [Browsable( false )]
        public InstructionBlock PreviousDeepBlock
        {
            get
            {
                // Return next sibling if possible.
                if ( this.node.Previous != null )
                {
                    return this.node.Previous.Value;
                }

                // If there is a parent block, return the NextDeepBlock of the parent block.
                InstructionBlock parentBlock = this.ParentBlock;
                if ( parentBlock != null )
                {
                    return parentBlock.PreviousDeepBlock;
                }

                // Otherwise it is really the last block.
                return null;
            }
        }


        /// <summary>
        /// Gets the first child block.
        /// </summary>
        /// <value>
        /// The first child block, or <b>null</b> if the current block has no child.
        /// </value>
        [Browsable( false )]
        public InstructionBlock FirstChildBlock
        {
            get { return LinkedListHelper.GetFirstValue( this.children ); }
        }

        /// <summary>
        /// Gets the last child block.
        /// </summary>
        /// <value>
        /// The last child block, or <b>null</b> if the current block has no child.
        /// </value>
        [Browsable( false )]
        public InstructionBlock LastChildBlock
        {
            get { return LinkedListHelper.GetLastValue( this.children ); }
        }


        /// <summary>
        /// Gets an enumerator to enumerate children blocks
        /// </summary>
        /// <returns>An <see cref="IEnumerator{InstructionBlock}"/>.
        /// </returns>
        /// <remarks>
        /// For performance reason, it is better to enumerate the children blocks
        /// using the linked list formed by the <see cref="FirstChildBlock"/> and
        /// <see cref="NextSiblingBlock"/> properties.
        /// </remarks>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This method returns every time a new instance." )]
        public IEnumerator<InstructionBlock> GetChildrenEnumerator()
        {
            return this.GetChildrenEnumerator( false );
        }

        /// <summary>
        /// Gets an enumerator to enumerate children blocks and allow to enumerate
        /// them children of children recursively.
        /// </summary>
        /// <param name="deep"><b>true</b> if children have to be enumerated
        /// recursively, otherwise <b>false</b>.</param>
        /// <returns>An <see cref="IEnumerator{InstructionBlock}"/>.
        /// </returns>
        public IEnumerator<InstructionBlock> GetChildrenEnumerator( bool deep )
        {
            return new ChildrenEnumerator( this.FirstChildBlock, deep );
        }

        /// <summary>
        /// Finds the deepest common ancestor of two blocks in the method body tree.
        /// </summary>
        /// <param name="first">An <see cref="InstructionBlock"/>.</param>
        /// <param name="second">An <see cref="InstructionBlock"/>.</param>
        /// <returns>An <see cref="InstructionBlock"/>, or <b>null</b> if no common
        /// ancestor was found.</returns>
        public static InstructionBlock FindCommonAncestor( InstructionBlock first, InstructionBlock second )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( first, "first" );
            ExceptionHelper.AssertArgumentNotNull( second, "second" );

            #endregion

            Stack<InstructionBlock> parentsA = first.GetParentStack();
            Stack<InstructionBlock> parentsB = second.GetParentStack();

            InstructionBlock commonAncestor = null;

            while ( parentsA.Count > 0 && parentsB.Count > 0 )
            {
                InstructionBlock cursorA = parentsA.Pop();
                InstructionBlock cursorB = parentsB.Pop();
                if ( cursorA == cursorB )
                {
                    commonAncestor = cursorA;
                }
                else
                {
                    break;
                }
            }

            return commonAncestor;
        }

        /// <summary>
        /// Gets a <see cref="Stack{T}"/> of parent blocks, with the root block
        /// at the top of the stack and the current block at the bottom.
        /// </summary>
        /// <returns>
        /// A <see cref="Stack{T}"/> of instruction blocks (<see cref="InstructionBlock"/>).
        /// </returns>
        private Stack<InstructionBlock> GetParentStack()
        {
            Stack<InstructionBlock> stack = new Stack<InstructionBlock>();
            InstructionBlock cursor = this;
            while ( cursor != null )
            {
                stack.Push( cursor );
                cursor = cursor.ParentBlock;
            }

            return stack;
        }

        /// <summary>
        /// Inserts a new <see cref="InstructionBlock"/> between the current block
        /// and its parent.
        /// </summary>
        /// <returns>The new <see cref="InstructionBlock"/>.</returns>
        public InstructionBlock Nest()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.MethodBody != null,
                                                       "BlockNotAttached" );

            #endregion

            InstructionBlock parentBlock = this.ParentBlock;

            InstructionBlock block = this.MethodBody.CreateInstructionBlock();

            if ( parentBlock != null )
            {
                parentBlock.AddChildBlock( block, NodePosition.After, this );
            }
            else
            {
                this.MethodBody.RootInstructionBlock = block;
            }

            this.Detach();
            block.AddChildBlock( this, NodePosition.After, null );

            block.localVariableSymbols = this.localVariableSymbols;
            this.localVariableSymbols = null;
            block.exceptionHandlers = this.exceptionHandlers;
            this.exceptionHandlers = null;

            return block;
        }

        #endregion

        #region Exception Handlers

        /// <summary>
        /// Gets the first exception handler protecting the current block.
        /// </summary>
        [Browsable( false )]
        public ExceptionHandler FirstExceptionHandler
        {
            get { return this.exceptionHandlers == null ? null : this.exceptionHandlers.First.Value; }
        }

        /// <summary>
        /// Gets the last exception handler protecting the current block.
        /// </summary>
        [Browsable( false )]
        public ExceptionHandler LastExceptionHandler
        {
            get { return this.exceptionHandlers == null ? null : this.exceptionHandlers.Last.Value; }
        }


        /// <summary>
        /// Constructs an <see cref="ExceptionHandler"/> and add it to the current block.
        /// </summary>
        /// <param name="position">Position of the new <see cref="ExceptionHandler"/> w.r.t.
        /// <paramref name="referenceHandler"/>.</param>
        /// <param name="referenceHandler">Handler before or after which the new handler has to be
        /// inserted, or <b>null</b> if the new handler has to be the first or the last.</param>
        /// <param name="flags">Kind of exception handling clause.</param>
        /// <param name="handlerBlock">Handler block.</param>
        /// <param name="filterBlock">Filter block, or <b>null</b> if the exception
        /// handling clause is not <see cref="ExceptionHandlingClauseOptions.Filter"/>.</param>
        /// <param name="catchType">Type caught, or <b>null</b> if the exception
        /// handling clause is not <see cref="ExceptionHandlingClauseOptions.Clause"/>.</param>
        /// <returns>The new <see cref="ExceptionHandler"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="handlerBlock"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="catchType"/> is null when <paramref name="flags"/> equals
        ///		<see cref="ExceptionHandlingClauseOptions.Clause"/>.
        ///	</exception>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="filterBlock"/> is null when <paramref name="flags"/> equals
        ///		<see cref="ExceptionHandlingClauseOptions.Filter"/>.
        ///	</exception>
        ///	<exception cref="InvalidOperationException">
        ///		The current block is detached or is the root.
        ///	</exception>
        ///	<exception cref="ArgumentException">
        ///		<paramref name="handlerBlock"/> is attached.
        ///	</exception>
        ///	<exception cref="ArgumentException">
        ///		<paramref name="filterBlock"/> is attached.
        ///	</exception>
        private ExceptionHandler AddExceptionHandler( NodePosition position, ExceptionHandler referenceHandler,
                                                      ExceptionHandlingClauseOptions flags,
                                                      InstructionBlock handlerBlock, InstructionBlock filterBlock,
                                                      ITypeSignature catchType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( handlerBlock, "handlerBlock" );
            ExceptionHelper.Core.AssertValidOperation( handlerBlock.methodBody == this.methodBody,
                                                       "CannotAddBlockToOtherMethodBody" );
            ExceptionHelper.Core.AssertValidOperation(
                handlerBlock.Parent == null || handlerBlock.ParentBlock == this.Parent,
                "ExceptionHandlerNotInInstructionBlock" );
            ExceptionHelper.Core.AssertValidOperation(
                filterBlock == null || filterBlock.Parent == null || filterBlock.ParentBlock == this.Parent,
                "ExceptionHandlerNotInInstructionBlock" );
            ExceptionHelper.Core.AssertValidArgument( referenceHandler == null || referenceHandler.Parent == this,
                                                      "referenceHandler",
                                                      "ReferenceExceptionHandlerNotInInstructionBlock" );

            #endregion

            if ( handlerBlock.Parent == null )
            {
                this.ParentBlock.AddChildBlock( handlerBlock, NodePosition.After, this );
            }

            switch ( flags )
            {
                case ExceptionHandlingClauseOptions.Clause:
                    ExceptionHelper.AssertArgumentNotNull( catchType, "catchType" );
                    break;

                case ExceptionHandlingClauseOptions.Fault:
                case ExceptionHandlingClauseOptions.Finally:
                    // Nothing else to do.
                    break;

                case ExceptionHandlingClauseOptions.Filter:
                    ExceptionHelper.AssertArgumentNotNull( filterBlock, "filterBlock" );
                    if ( filterBlock.Parent != null && filterBlock.ParentBlock != this.Parent )
                    {
                        throw new ArgumentException();
                    }
                    if ( filterBlock.Parent == null )
                    {
                        this.ParentBlock.AddChildBlock( filterBlock, NodePosition.After, null );
                    }
                    break;
            }


            ExceptionHandler handler = new ExceptionHandler(
                this, flags, handlerBlock, filterBlock, catchType );

            if ( this.exceptionHandlers == null )
            {
                this.exceptionHandlers = new ExceptionHandlerCollection();
            }

            LinkedListHelper.AddNode( this.exceptionHandlers, handler.Node, position,
                                      referenceHandler == null ? null : referenceHandler.Node );

            handlerBlock.ParentExceptionHandler = handler;

            if ( filterBlock != null )
            {
                filterBlock.ParentExceptionHandler = handler;
            }

            return handler;
        }

        /// <summary>
        /// Constructs an <b>Catch</b> <see cref="ExceptionHandler"/> and add it to the current block.
        /// </summary>
        /// <param name="position">Position of the new <see cref="ExceptionHandler"/> w.r.t.
        /// <paramref name="referenceHandler"/>.</param>
        /// <param name="referenceHandler">Handler before or after which the new handler has to be
        /// inserted, or <b>null</b> if the new handler has to be the first or the last.</param>
        /// <param name="handlerBlock">Handler block.</param>
        /// <param name="catchType">Type caught.</param>
        /// <returns>The new <see cref="ExceptionHandler"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="handlerBlock"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="catchType"/> is null.
        ///	</exception>
        ///	<exception cref="InvalidOperationException">
        ///		The current block is detached or is the root.
        ///	</exception>
        ///	<exception cref="ArgumentException">
        ///		<paramref name="handlerBlock"/> is attached.
        ///	</exception>
        public ExceptionHandler AddExceptionHandlerCatch( ITypeSignature catchType, InstructionBlock handlerBlock,
                                                          NodePosition position, ExceptionHandler referenceHandler )
        {
            return this.AddExceptionHandler( position, referenceHandler, ExceptionHandlingClauseOptions.Clause,
                                             handlerBlock, null, catchType );
        }

        /// <summary>
        /// Constructs a <b>Finally</b> <see cref="ExceptionHandler"/> and add it to the current block.
        /// </summary>
        /// <param name="position">Position of the new <see cref="ExceptionHandler"/> w.r.t.
        /// <paramref name="referenceHandler"/>.</param>
        /// <param name="referenceHandler">Handler before or after which the new handler has to be
        /// inserted, or <b>null</b> if the new handler has to be the first or the last.</param>
        /// <param name="handlerBlock">Handler block.</param>
        /// <returns>The new <see cref="ExceptionHandler"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="handlerBlock"/> is null.
        /// </exception>
        ///	<exception cref="InvalidOperationException">
        ///		The current block is detached or is the root.
        ///	</exception>
        ///	<exception cref="ArgumentException">
        ///		<paramref name="handlerBlock"/> is attached.
        ///	</exception>
        public ExceptionHandler AddExceptionHandlerFinally( InstructionBlock handlerBlock, NodePosition position,
                                                            ExceptionHandler referenceHandler )
        {
            return this.AddExceptionHandler( position, referenceHandler, ExceptionHandlingClauseOptions.Finally,
                                             handlerBlock, null, null );
        }

        /// <summary>
        /// Constructs a <b>Fault</b> <see cref="ExceptionHandler"/> and add it to the current block.
        /// </summary>
        /// <param name="position">Position of the new <see cref="ExceptionHandler"/> w.r.t.
        /// <paramref name="referenceHandler"/>.</param>
        /// <param name="referenceHandler">Handler before or after which the new handler has to be
        /// inserted, or <b>null</b> if the new handler has to be the first or the last.</param>
        /// <param name="handlerBlock">Handler block.</param>
        /// <returns>The new <see cref="ExceptionHandler"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="handlerBlock"/> is null.
        /// </exception>
        ///	<exception cref="InvalidOperationException">
        ///		The current block is detached or is the root.
        ///	</exception>
        ///	<exception cref="ArgumentException">
        ///		<paramref name="handlerBlock"/> is attached.
        ///	</exception>
        public ExceptionHandler AddExceptionHandlerFault( InstructionBlock handlerBlock, NodePosition position,
                                                          ExceptionHandler referenceHandler )
        {
            return this.AddExceptionHandler( position, referenceHandler, ExceptionHandlingClauseOptions.Fault,
                                             handlerBlock, null, null );
        }

        /// <summary>
        /// Constructs a <b>Filter</b> <see cref="ExceptionHandler"/> and add it to the current block.
        /// </summary>
        /// <param name="handlerBlock">Handler block.</param>
        /// <param name="position">Position of the new <see cref="ExceptionHandler"/> w.r.t.
        /// <paramref name="referenceHandler"/>.</param>
        /// <param name="referenceHandler">Handler before or after which the new handler has to be
        /// inserted, or <b>null</b> if the new handler has to be the first or the last.</param>
        /// <param name="filterBlock">Filter block.</param>
        /// <returns>The new <see cref="ExceptionHandler"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="handlerBlock"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///		<paramref name="filterBlock"/> is null.
        ///	</exception>
        ///	<exception cref="InvalidOperationException">
        ///		The current block is detached or is the root.
        ///	</exception>
        ///	<exception cref="ArgumentException">
        ///		<paramref name="handlerBlock"/> is attached.
        ///	</exception>
        ///	<exception cref="ArgumentException">
        ///		<paramref name="filterBlock"/> is attached.
        ///	</exception>
        public ExceptionHandler AddExceptionHandlerFilter( InstructionBlock handlerBlock, InstructionBlock filterBlock,
                                                           NodePosition position, ExceptionHandler referenceHandler )
        {
            return this.AddExceptionHandler( position, referenceHandler, ExceptionHandlingClauseOptions.Filter,
                                             handlerBlock, filterBlock, null );
        }

        #endregion

        #region Local variables

        /// <summary>
        /// Gets the collection of local variable symbols (<see cref="LocalVariableSymbol"/>).
        /// </summary>
        internal LocalVariableSymbolCollection LocalVariableSymbols
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.localVariableSymbols != null, "PropertyNotEnabled" );

                #endregion

                return this.localVariableSymbols;
            }
        }

        /// <summary>
        /// Gets the number of local variable symbols (<see cref="LocalVariableSymbol"/>).
        /// </summary>
        public int LocalVariableSymbolCount
        {
            get { return this.localVariableSymbols == null ? 0 : this.localVariableSymbols.Count; }
        }

        /// <summary>
        /// Gets a <see cref="LocalVariableSymbol"/> given its position in the current block.
        /// </summary>
        /// <param name="index">Index of the required symbol in the collection of symbols
        /// of the current block. This is <i>not</i> the ordinal of the local variable
        /// to which it refers.</param>
        /// <returns>The <see cref="LocalVariableSymbol"/> at position <paramref name="index"/>.</returns>
        public LocalVariableSymbol GetLocalVariableSymbol( int index )
        {
            #region Preconditions

            if ( index < 0 || index >= this.LocalVariableSymbolCount )
            {
                throw new ArgumentOutOfRangeException( "index" );
            }

            #endregion

            return this.localVariableSymbols[index];
        }

        /// <summary>
        /// Creates a new local variable in the current block.
        /// </summary>
        /// <param name="type">Variable type.</param>
        /// <param name="name">Variable name, or <b>null</b> if the
        /// local variable is anonym. The variable name may contain the 
        /// placeholder <c>{0}</c>. It will be replaced by the local
        /// variable ordinal at runtime (useful to create unique names).</param>
        /// <returns>The <see cref="LocalVariableSymbol"/> referencing the new
        /// local variable.</returns>
        public LocalVariableSymbol DefineLocalVariable( ITypeSignature type, string name )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            LocalVariableDeclaration localVariable = new LocalVariableDeclaration(
                type, this.MethodBody.LocalVariableCount );
            this.MethodBody.AddLocalVariable( localVariable );
            LocalVariableSymbol symbol = new LocalVariableSymbol( localVariable,
                                                                  name == null
                                                                      ? null
                                                                      : string.Format( CultureInfo.InvariantCulture,
                                                                                       name,
                                                                                       localVariable.Ordinal ) );

            this.AddLocalVariableSymbol( symbol );
            return symbol;
        }

        internal void AddLocalVariableSymbol( LocalVariableSymbol symbol )
        {
            if ( this.localVariableSymbols == null )
            {
                this.localVariableSymbols = new LocalVariableSymbolCollection( 4 );
            }

            this.localVariableSymbols.Add( symbol );
        }

        #endregion

        #region InstructionSequences

        /// <summary>
        /// Gets the number first <see cref="InstructionSequence"/> of the current block.
        /// </summary>
        [Browsable( false )]
        public InstructionSequence FirstInstructionSequence
        {
            get { return LinkedListHelper.GetFirstValue( this.instructionSequences ); }
        }

        /// <summary>
        /// Gets the number last <see cref="InstructionSequence"/> of the current block.
        /// </summary>
        [Browsable( false )]
        public InstructionSequence LastInstructionSequence
        {
            get { return LinkedListHelper.GetLastValue( this.instructionSequences ); }
        }

        /// <summary>
        /// Adds an <see cref="InstructionSequence"/> after a given sequence in the current the block.
        /// </summary>
        /// <param name="position">Position of the new <see cref="InstructionSequence"/> w.r.t.
        /// <paramref name="referenceSequence"/>.</param>
        /// <param name="referenceSequence">Sequence before or after which <paramref name="newSequence"/> has to be
        /// inserted, or <b>null</b> if <paramref name="newSequence"/>has to be the first or the last.</param>
        /// <param name="newSequence">A detached <see cref="InstructionSequence"/>.</param>
        public void AddInstructionSequence( InstructionSequence newSequence, NodePosition position,
                                            InstructionSequence referenceSequence )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( newSequence, "newSequence" );
            ExceptionHelper.Core.AssertValidArgument(
                referenceSequence == null || referenceSequence.ParentInstructionBlock == this,
                "referenceSequence",
                "ReferenceSequenceNotInCurrentBlock" );
            ExceptionHelper.Core.AssertValidArgument( newSequence.ParentInstructionBlock == null, "newSequence",
                                                      "InstructionSequenceNotDetached" );

            #endregion

            this.InternalAddInstructionSequence( newSequence, position, referenceSequence );
        }

        private void InternalAddInstructionSequence( InstructionSequence newSequence, NodePosition position,
                                                     InstructionSequence referenceSequence )
        {
            newSequence.ParentInstructionBlock = this;

            if ( this.instructionSequences == null )
            {
                ExceptionHelper.Core.AssertValidOperation( !this.HasChildrenBlocks, "BlockHasSequencesAndBlock" );

                this.instructionSequences = new LinkedList<InstructionSequence>();
            }

            LinkedListHelper.AddNode( this.instructionSequences,
                                      newSequence.Node, position,
                                      referenceSequence == null ? null : referenceSequence.Node );
        }

        /// <summary>
        /// Moves an <see cref="InstructionBlock"/> from another block into the current one.
        /// </summary>
        /// <param name="movedBlock">Block to be moved.</param>
        /// <param name="position">Relative position of the new block w.r.t. <paramref name="referenceBlock"/>.</param>
        /// <param name="referenceBlock">Block after or before which <paramref name="movedBlock"/> has to be inserted,
        /// or <b>null</b> if the new block has to be inserted at the first or the last.</param>
        public void MoveInstructionBlock( InstructionBlock movedBlock, NodePosition position,
                                          InstructionBlock referenceBlock )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( movedBlock, "movedBlock" );

            #endregion

            // Detach the block.
            InstructionBlock parentBlock = movedBlock.ParentBlock;
            MethodBodyDeclaration parentMethodBody;
            if ( parentBlock != null )
            {
                parentBlock.children.Remove( movedBlock.node );
            }
            else if ( ( parentMethodBody = movedBlock.Parent as MethodBodyDeclaration ) != null )
            {
                parentMethodBody.RootInstructionBlock = null;
            }

            movedBlock.OnRemovingFromParent();

            // Add it to the new parent.
            this.AddChildBlock( movedBlock, position, referenceBlock );

            // If the block was has exception handlers, we have to move exception
            // handler blocks as well.
            if ( movedBlock.HasExceptionHandlers )
            {
                ExceptionHandler exceptionHandler = movedBlock.FirstExceptionHandler;

                while ( exceptionHandler != null )
                {
                    this.MoveInstructionBlock( exceptionHandler.HandlerBlock, NodePosition.After, movedBlock );
                    if ( exceptionHandler.FilterBlock != null )
                    {
                        this.MoveInstructionBlock( exceptionHandler.FilterBlock, NodePosition.After, movedBlock );
                    }

                    exceptionHandler = exceptionHandler.NextSiblingExceptionHandler;
                }
            }
        }

        /// <summary>
        /// Adds an <see cref="InstructionSequence"/> after a given sequence in the current the block.
        /// </summary>
        /// <param name="position">Position of the new <see cref="InstructionSequence"/> w.r.t.
        /// <paramref name="referenceSequence"/>.</param>
        /// <param name="referenceSequence">Sequence before or after which <paramref name="newSequence"/> has to be
        /// inserted, or <b>null</b> if <paramref name="newSequence"/>has to be the first or the last.</param>
        /// <param name="newSequence">A detached <see cref="InstructionSequence"/>.</param>
        public void MoveInstructionSequence( InstructionSequence newSequence, NodePosition position,
                                             InstructionSequence referenceSequence )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( newSequence, "newSequence" );
            if ( referenceSequence != null && referenceSequence.ParentInstructionBlock != this )
            {
                throw new ArgumentException();
            }
            //ExceptionHelper.Core.AssertValidArgument(newSequence.MethodBody == this.MethodBody, "newSequence", "MethodBodyInSequenceSameAsInBlock");

            #endregion

            if ( newSequence.ParentInstructionBlock != null )
            {
                newSequence.Node.List.Remove( newSequence.Node );
            }

            this.InternalAddInstructionSequence( newSequence, position, referenceSequence );
        }


        /// <summary>
        /// Finds the first <see cref="InstructionSequence"/> of the tree
        /// whose current <see cref="InstructionBlock"/> is the root.
        /// </summary>
        /// <returns>An <see cref="InstructionSequence"/>, or <b>null</b>
        /// if the block do not contain any <see cref="InstructionSequence"/>.
        /// </returns>
        [SuppressMessage( "Microsoft.Design", "CA1024",
            Justification = "This is not conceptually a property." )]
        public InstructionSequence FindFirstInstructionSequence()
        {
            if ( this.HasInstructionSequences )
            {
                return this.FirstInstructionSequence;
            }
            else
            {
                IEnumerator<InstructionBlock> enumerator = this.GetChildrenEnumerator( true );
                while ( enumerator.MoveNext() )
                {
                    if ( enumerator.Current.HasInstructionSequences )
                    {
                        return enumerator.Current.FirstInstructionSequence;
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// Splits the current <see cref="InstructionBlock"/> in two blocks after
        /// an <see cref="InstructionSequence"/>.
        /// </summary>
        /// <param name="sequence">An <see cref="InstructionSequence"/> belonging
        /// to the current <see cref="InstructionBlock"/>.</param>
        /// <returns>A new <see cref="InstructionBlock"/> containing the second
        /// part of the split block.</returns>
        /// <exception cref="ArgumentNullException">
        ///	The <paramref name="sequence"/> argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="sequence"/> does not belong to the current 
        /// <see cref="InstructionBlock"/>.
        /// </exception>
        public InstructionBlock SplitBlockAfterSequence( InstructionSequence sequence )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sequence, "sequence" );
            ExceptionHelper.Core.AssertValidArgument( sequence.ParentInstructionBlock == this, "sequence",
                                                      "InstructionSequenceNotInBlock" );
            ExceptionHelper.Core.AssertValidOperation( this.ParentBlock != null,
                                                       "BlockShouldHaveParent" );

            #endregion

            InstructionBlock newBlock = this.methodBody.CreateInstructionBlock();

            this.ParentBlock.AddChildBlock( newBlock, NodePosition.After, this );

            InstructionSequence currentSequence = this.FirstInstructionSequence;

            bool sequenceFound = false;
            while ( currentSequence != null )
            {
                InstructionSequence nextSequence = currentSequence.NextSiblingSequence;

                if ( sequenceFound )
                {
                    currentSequence.Detach();
                    newBlock.MoveInstructionSequence( currentSequence, NodePosition.After, null );
                }
                else
                {
                    if ( sequence == currentSequence )
                    {
                        sequenceFound = true;
                    }
                }

                currentSequence = nextSequence;
            }

            return newBlock;
        }

        #endregion

        #region Internal properties

        /// <summary>
        /// Gets the start offset of the block with respect to the first byte of
        /// the method instruction stream. Used during the construction of the
        /// <see cref="InstructionBlock"/> tree while reading the PE image.
        /// </summary>
        internal int StartOffset
        {
            get { return this.startOffset; }
        }

        /// <summary>
        /// Gets the end offset of the block with respect to the first byte of
        /// the method instruction stream. Used during the construction of the
        /// <see cref="InstructionBlock"/> tree while reading the PE image.
        /// </summary>
        internal int EndOffset
        {
            get { return this.endOffset; }
        }

        /// <summary>
        /// Get the node associated to the current instance in the parent linked list.
        /// </summary>
        internal LinkedListNode<InstructionBlock> Node
        {
            get { return this.node; }
        }

        #endregion

        #region Binary block relations

        /// <summary>
        /// Determines how the current block intersects a range of binary instructions.
        /// </summary>
        /// <param name="startOffset">Start offset in the stream of method instructions.</param>
        /// <param name="endOffset">End offset in the stream of method instructions.</param>
        /// <returns>An <see cref="InstructionBlockRelation"/>.</returns>
        /// <remarks>
        /// This method is used during the construction of the <see cref="InstructionBlock"/>
        /// hierarchy.
        /// </remarks>
        internal InstructionBlockRelation Compares( int startOffset, int endOffset )
        {
            if ( this.startOffset == startOffset && this.endOffset == endOffset )
            {
                return InstructionBlockRelation.IsEqual;
            }

            if ( this.startOffset <= startOffset )
            {
                //      ++++++++++++++++
                //      ???????????????????????

                if ( this.endOffset <= startOffset )
                {
                    //      ++++++++++++++++
                    //                      --------

                    return InstructionBlockRelation.IsDisjoint;
                }
                else
                {
                    //  +++++++++++++++++++
                    //     ??????????????????????

                    if ( this.endOffset < endOffset )
                    {
                        // ++++++++++++++++++
                        // ?????---------------

                        if ( this.startOffset == startOffset )
                        {
                            // ++++++++++++++++++
                            // --------------------
                            return InstructionBlockRelation.IsContained;
                        }
                        else
                        {
                            // ++++++++++++++++++
                            //   -------------------
                            return InstructionBlockRelation.Intersects;
                        }
                    }
                    else
                    {
                        // ++++++++++++++++
                        //   --------------
                        return InstructionBlockRelation.Contains;
                    }
                }
            }
            else
            {
                //      +++++++++++++++++++++++
                // ??????????????????????????????????????

                if ( this.startOffset >= endOffset )
                {
                    //      +++++++++++++++++++
                    //------
                    return InstructionBlockRelation.IsDisjoint;
                }
                else
                {
                    //   +++++++++++
                    // ----?????????????
                    if ( this.endOffset > endOffset )
                    {
                        //    +++++++++++
                        //  ------------
                        return InstructionBlockRelation.Intersects;
                    }
                    else
                    {
                        //    ++++++++++++
                        // ------------------
                        return InstructionBlockRelation.IsContained;
                    }
                }
            }
        }

        #endregion

        private class ChildrenEnumerator : IEnumerator<InstructionBlock>
        {
            private readonly InstructionBlock origin;
            private InstructionBlock current;
            private readonly bool dig;

            internal ChildrenEnumerator( InstructionBlock origin, bool dig )
            {
                this.origin = origin;
                this.dig = dig;
            }

            [Conditional( "ASSERT" )]
            private void AssertPositioned()
            {
                ExceptionHelper.Core.AssertValidOperation( this.current != null,
                                                           "EnumeratorNotPositioned" );
            }


            public InstructionBlock Current
            {
                get { return this.current; }
            }


            public void Dispose()
            {
            }


            object IEnumerator.Current
            {
                get { return this.current; }
            }

            public bool MoveNext()
            {
                if ( this.current == null )
                {
                    this.current = this.origin;
                }
                else
                {
                    if ( !this.dig )
                    {
                        this.current = this.current.NextSiblingBlock;
                    }
                    else
                    {
                        if ( this.current.HasChildrenBlocks )
                        {
                            this.current = this.current.FirstChildBlock;
                        }
                        else
                        {
                            this.current = this.current.NextDeepBlock;
                        }
                    }
                }

                return this.current != null;
            }

            public void Reset()
            {
                this.current = null;
            }
        }
    }

    /// <summary>
    /// Enumerates the possible ways how blocks can intersect themselves.
    /// </summary>
    internal enum InstructionBlockRelation : byte
    {
        /// <summary>
        /// Both blocks are equal.
        /// </summary>
        IsEqual = 1,

        /// <summary>
        /// The first block is contained in the other, but are no equal.
        /// </summary>
        IsContained = 2,

        /// <summary>
        /// The first block contains the other, but are no equal.
        /// </summary>
        Contains = 4,

        /// <summary>
        /// Blocks have a common intersection, but are no equal.
        /// </summary>
        Intersects = 8,

        /// <summary>
        /// Blocks are disjoint.
        /// </summary>
        IsDisjoint = 16,

        /// <summary>
        /// Any bit of the current mask means that the first block contains
        /// or is equal to the second.
        /// </summary>
        ContainsOrEqualsMask = IsEqual | Contains,

        /// <summary>
        /// Any bit of the current mask means that the first block is contained in
        /// or is equal to the second.
        /// </summary>
        IsContainedOrEqualsMask = IsEqual | IsContained,
    }


    namespace Collections
    {
        /// <summary>
        /// Linked list of instruction blocks (<see cref="InstructionBlock"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        [SuppressMessage( "Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable" )]
        public sealed class InstructionBlockCollection : LinkedList<InstructionBlock>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InstructionBlockCollection"/>
            /// type.
            /// </summary>
            internal InstructionBlockCollection()
            {
            }
        }
    }
}