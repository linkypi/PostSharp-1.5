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
using System.Reflection.Emit;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Restructures a method body so that the original code can be
    /// wrapped into a try...catch block.
    /// </summary>
    public sealed class MethodBodyRestructurer
    {
        private readonly WeavingHelper weavingHelper;
        private readonly MethodDefDeclaration method;
        private readonly MethodBodyRestructurerOptions options;
        private readonly Stack<InstructionSequence> sequenceStack = new Stack<InstructionSequence>();
      

        #region Fields 

        /// <summary>
        /// Entry block.
        /// </summary>
        private InstructionBlock entryBlock;

        /// <summary>
        /// Block executed just after instance initialization
        /// (or <b>null</b> if the method is not a constructor).
        /// </summary>
        private InstructionBlock afterInitializationBlock;


        /// <summary>
        /// Block initializing the object (in case the method is a constructor).
        /// </summary>
        private InstructionBlock initializationBlock;

        /// <summary>
        /// Block containing the original code,
        /// after the object has been initialized.
        /// </summary>
        private InstructionBlock principalBlock;

        /// <summary>
        /// Local variable containing the return value.
        /// </summary>
        private LocalVariableSymbol returnValueVariable;

        private ConstructorType constructorType;

        private InstructionSequence returnInstructionSequence;

        private ThisInitializationStatus[] blockThisInitializationStatus;

        private bool[] sequencesWithThisUninitialized;

        private InstructionBlock hybridBlock;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MethodBodyRestructurer"/>.
        /// </summary>
        /// <param name="method">The method that will be restructured.</param>
        /// <param name="options">Options.</param>
        /// <param name="weavingHelper">A <see cref="WeavingHelper"/>.</param>
        public MethodBodyRestructurer(MethodDefDeclaration method, MethodBodyRestructurerOptions options,
            WeavingHelper weavingHelper)
        {   
            #region Preconditions
            ExceptionHelper.AssertArgumentNotNull(method, "method");
            #endregion

            this.method = method;
            this.options = options;
            this.weavingHelper = weavingHelper;
        }

        #region Resulting Properties
        /// <summary>
        /// Gets the block initializing the object (in case the method is a constructor).
        /// </summary>
        public InstructionBlock InitializationBlock { get { return initializationBlock; } }

        /// <summary>
        /// Gets the block execute just after the instance is initialized.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if the
        /// method is not a constructor.
        /// </value>
        public InstructionBlock AfterInitializationBlock { get { return this.afterInitializationBlock; } }

        /// <summary>
        /// Gets the block containing the original code,
        /// after the object has been initialized.
        /// </summary>
        public InstructionBlock PrincipalBlock { get { return principalBlock; } }

        /// <summary>
        /// Gets the entry block.
        /// </summary>
        /// <remarks>
        /// The entry block is a block that will be executed <i>before</i>
        /// any other in the method.
        /// </remarks>
        public InstructionBlock EntryBlock { get { return this.entryBlock; } }

        /// <summary>
        /// Gets the variable containing the return value in the exit block.
        /// </summary>
        public LocalVariableSymbol ReturnValueVariable { get { return this.returnValueVariable; } }

        /// <summary>
        /// Gets the kind of constructor that was passed.
        /// </summary>
        public ConstructorType ConstructorType { get { return this.constructorType; } }

        /// <summary>
        /// Gets the <see cref="InstructionSequence"/> containing the <b>ret</b> instruction.
        /// </summary>
        /// <remarks>
        /// This instruction sequence loads the value of <see cref="ReturnValueVariable"/>, then
        /// returns to the calling context. When some code in the protected block
        /// (<see cref="PrincipalBlock"/>) wants to return to the calling context, it cannot
        /// use directly the <b>ret</b> instruction but should first set the 
        /// <see cref="ReturnValueVariable"/> variable (unless the method returns <b>void</b>),
        /// then use the <b>leave</b> instruction to the <see cref="ReturnBranchTarget"/> instruction
        /// sequence.
        /// </remarks>
        public InstructionSequence ReturnBranchTarget { get { return returnInstructionSequence; } }
        #endregion

        /// <summary>
        /// Proceeds with the method body restructuration process.
        /// </summary>
        /// <param name="writer">An <see cref="InstructionWriter"/>.</param>
        public void Restructure(InstructionWriter writer)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(method, "method");
            if (!method.HasBody)
                throw new ArgumentException("The method has no body.");

            #endregion

            MethodBodyDeclaration methodBody = method.MethodBody;

            int blockCount = methodBody.InstructionBlockCount;

            // Embed the old root block into a new one.
            InstructionBlock oldRootBlock = methodBody.RootInstructionBlock;
            oldRootBlock.Comment = "Old Root Block";
            oldRootBlock.Detach();
            
            this.principalBlock = oldRootBlock;

                  
            InstructionBlock newRootBlock = methodBody.CreateInstructionBlock();
            newRootBlock.Comment = "New Root Block";
            methodBody.RootInstructionBlock = newRootBlock;
           
            this.principalBlock = methodBody.CreateInstructionBlock();
            this.principalBlock.Comment = "Principal Block";
            newRootBlock.AddChildBlock(this.principalBlock, NodePosition.After, null);
            this.principalBlock.AddChildBlock(oldRootBlock, NodePosition.After, null);


            
            this.initializationBlock = null;
            this.constructorType = ConstructorType.NotAnInstanceConstructor;

            InstructionBlock entrySymbolBlock;

            #region If contructor, isolate the uninitialized part

            if (method.Name == ".ctor")
            {
                // It is a constructor. We have to put in the non-protected
                // block all the code that is before
                // the parent constructor call.
                this.AnalyzeSequences(oldRootBlock);

                if (constructorType != ConstructorType.CallNothing)
                {
                    initializationBlock = methodBody.CreateInstructionBlock();
                    initializationBlock.Comment = "Initialization Block";
                    newRootBlock.AddChildBlock(initializationBlock, NodePosition.Before, null);

                    this.blockThisInitializationStatus = new ThisInitializationStatus[blockCount];
                    AnalyzeBlockRecursive(oldRootBlock);

                    MoveUninitializedBlockRecursive(oldRootBlock, initializationBlock);

                    entrySymbolBlock = SplitEntrySymbolSequencePoint(initializationBlock, writer);
                } // if ( constructorType != ConstructorType.CallNothing )
                else
                {
                    // This is a constructor, but it does not call the cascade constructor
                    // (we are in a struct).
                    entrySymbolBlock = SplitEntrySymbolSequencePoint(principalBlock, writer);
                }
            }
            else
            {
                // Not a constructor
                principalBlock = oldRootBlock;
                entrySymbolBlock = SplitEntrySymbolSequencePoint(principalBlock, writer);
            }

            #endregion



            // Complete the structure.
            this.entryBlock = methodBody.CreateInstructionBlock();
            entryBlock.Comment = "Entry Block";
           
            newRootBlock.AddChildBlock(entryBlock, NodePosition.Before, null);
            if (entrySymbolBlock != null)
            {
                newRootBlock.AddChildBlock(entrySymbolBlock, NodePosition.Before, null);
            }

            if (initializationBlock != null)
            {
                this.afterInitializationBlock = methodBody.CreateInstructionBlock();
                this.afterInitializationBlock.Comment = "After Initialization Block";
                newRootBlock.AddChildBlock(afterInitializationBlock, NodePosition.After, initializationBlock);
            }

            method.MethodBody.RootInstructionBlock = newRootBlock;

            if ((options & (MethodBodyRestructurerOptions.ChangeReturnInstructions)) != 0)
            {
                // Define the required variables.
                bool returnsVoid = IntrinsicTypeSignature.Is(method.ReturnParameter.ParameterType, IntrinsicType.Void);

                this.returnValueVariable = !returnsVoid ? 
                    newRootBlock.DefineLocalVariable(method.ReturnParameter.ParameterType,
                                                     "~returnValue~{0}") : 
                    null;

                // We create a branch target but we do not assign it to
                // a block. The caller is responsible to do it.
                returnInstructionSequence = method.MethodBody.CreateInstructionSequence();
                returnInstructionSequence.Comment = "Initial Return Branch Target";


                // Transform the protected block to use LEAVE instructions.
                InstructionReader reader = method.MethodBody.CreateInstructionReader();

                this.weavingHelper.RedirectReturnInstructions(reader, writer,
                                                 principalBlock, returnInstructionSequence, returnValueVariable,
                                                 false);
            }
            else
            {
                returnValueVariable = null;
            }

        }

         /// <summary>
        /// Gets a value indicating whether the <b>this</b> pointer is initialized in a given
        /// block.
        /// </summary>
        /// <param name="block">The block to analyze.</param>
        private void AnalyzeBlockRecursive(InstructionBlock block)
        {
            // Contains the result of the analysis up to the current execution point.
            ThisInitializationStatus result = ThisInitializationStatus.None;

          
            if ( block.HasChildrenBlocks )
            {
                // If the block has children, works recursively.
                InstructionBlock child = block.FirstChildBlock;
                while ( child != null )
                {
                    AnalyzeBlockRecursive( child );
                    ThisInitializationStatus childResult = this.blockThisInitializationStatus[child.Token];

                    if (childResult != ThisInitializationStatus.None)
                    {
                        if ( childResult < 0 /* Error */)
                        {
                            blockThisInitializationStatus[block.Token] = childResult;
                            return;
                        }
                        else if ( childResult == ThisInitializationStatus.Hybrid )
                        {
                            result = ThisInitializationStatus.Hybrid;
                        }
                        else if ( result == ThisInitializationStatus.None )
                        {
                            result = childResult;
                        }
                        else if ( result != childResult )
                        {
                            result = ThisInitializationStatus.Hybrid;
                        }
                    }

                    child = child.NextSiblingBlock;
                }

            }
            else
            {
                // If the block has sequences, iterate them.

                InstructionSequence sequence = block.FirstInstructionSequence;

                while ( sequence != null )
                {
                    if ( sequencesWithThisUninitialized[sequence.Token] )
                    {
                        switch ( result )
                        {
                            case ThisInitializationStatus.None:
                                result = ThisInitializationStatus.Uninitialized;
                                break;

                            case ThisInitializationStatus.Uninitialized:
                                break;

                            case ThisInitializationStatus.Initialized:
                                blockThisInitializationStatus[block.Token] = ThisInitializationStatus.Hybrid;
                                if (this.hybridBlock == null)
                                    this.hybridBlock = block;
                                return;
                        }

                        if (block.HasExceptionHandlers)
                        {
                            blockThisInitializationStatus[block.Token] = ThisInitializationStatus.ErrorHasHandler;
                            return;
                        }

                    }
                    else
                    {
                        switch ( result )
                        {
                            case ThisInitializationStatus.None:
                                result = ThisInitializationStatus.Initialized;
                                break;

                            case ThisInitializationStatus.Initialized:
                                break;

                            case ThisInitializationStatus.Uninitialized:
                                blockThisInitializationStatus[block.Token] = ThisInitializationStatus.Hybrid;
                                return;
                        }
                    }

                    sequence = sequence.NextSiblingSequence;
                }

            }

            blockThisInitializationStatus[block.Token] = result;
            return;
        }


        /// <summary>
        /// Analyzes the method control flow and determines which sequences have
        /// an uninitialized <b>this</b> pointer.
        /// </summary>
        private void AnalyzeSequences(InstructionBlock rootBlock)
        {
            /* 
             * The initial state is that all sequences have initialized this pointer. Then
             * this method starts with the method entry point and explore possible paths
             * of the control flow until it reaches a point where the this pointer is
             * initialized. Then it stops the exploration of this branche.
             * Paths to be explored are pushed on a stack and the
             * stack is processed progressively.
             * 
             */

            constructorType = ConstructorType.Default;
            MethodBodyDeclaration methodBody = this.method.MethodBody;

            // We create an array for the result set, but we allocate one more position
            // because we could split a sequence.
            this.sequencesWithThisUninitialized = new bool[methodBody.InstructionSequenceCount + 1];

            // Mark all the sequences that have uninitialized the current pointer.
            InstructionReader reader = methodBody.CreateInstructionReader( false );
            InstructionSequence sequence = rootBlock.FindFirstInstructionSequence();
            this.sequenceStack.Push( sequence );

            while ( this.sequenceStack.Count > 0 )
            {
                sequence = this.sequenceStack.Pop();
                if (this.sequencesWithThisUninitialized[sequence.Token])
                {
                    // Sequence already processed.
                    continue;
                }

                // If the current sequence was on the stack, it is because it was reachable
                // from the method entry point BEFORE the base constructor was called.
                this.sequencesWithThisUninitialized[sequence.Token] = true;

                if ( reader.CurrentInstructionSequence != null )
                    reader.LeaveInstructionSequence();

                reader.EnterInstructionSequence( sequence );

                bool fallToNextSequence = true;

                while ( reader.ReadInstruction() )
                {
                    if ( reader.CurrentInstruction.OpCodeNumber == OpCodeNumber.Call &&
                         reader.CurrentInstruction.MethodOperand.Name == ".ctor" )
                    {
                        if ( reader.CurrentInstruction.MethodOperand.DeclaringType ==
                             methodBody.Method.DeclaringType )
                        {
                            constructorType = ConstructorType.CallThis;
                        }
                        else
                        {
                            constructorType = ConstructorType.CallBase;
                        }

                        // This is the call to the parent constructor. We can split
                        // the sequence at the current point
                        if ( !reader.IsAtEnfOfSequence )
                        {
                            sequence.SplitAfterReaderPosition( reader );

                            // We do not put on the stack the sequence containing the instructions after,
                            // because they are initialized.
                        }

                        fallToNextSequence = false;
                        break;
                    }
                    else
                    {
                        bool exitSequence;

                        switch ( OpCodeMap.GetFlowControl( reader.CurrentInstruction.OpCodeNumber ) )
                        {
                            case FlowControl.Branch:
                                this.sequenceStack.Push( reader.CurrentInstruction.BranchTargetOperand );
                                exitSequence = true;
                                break;

                            case FlowControl.Cond_Branch:
                                this.sequenceStack.Push( reader.CurrentInstruction.BranchTargetOperand );
                                exitSequence = false;
                                break;

                            case FlowControl.Return:
                                exitSequence = true;
                                fallToNextSequence = false;
                                break;

                            default:
                                exitSequence = false;
                                break;
                        }

                        if ( exitSequence )
                            break;
                    }
                }

                // We 'falled' naturally into the next sequence.
                if ( fallToNextSequence )
                {
                    InstructionSequence nextSequence = sequence.NextDeepSequence;

                    if ( nextSequence != null )
                        this.sequenceStack.Push( nextSequence );
                }
            }

            // Done.
            if ( constructorType == ConstructorType.Default )
            {
                constructorType = ConstructorType.CallNothing;
                this.sequencesWithThisUninitialized = null;
            }
        }

        private void MoveUninitializedBlockRecursive(InstructionBlock originalBlock, InstructionBlock targetParentBlock)
        {
            // If the block is empty, we cannot determine whether it is initialized or not.
            // So we simply detach it.
            if ( !originalBlock.HasChildrenBlocks && !originalBlock.HasInstructionSequences)
            {
                originalBlock.Detach();
                return;
            }

            switch ( this.blockThisInitializationStatus[originalBlock.Token])
            {
                case ThisInitializationStatus.Initialized:
                    // Nothing to do.
                    return;

                case ThisInitializationStatus.Uninitialized:
                    // We simply move it.
                    targetParentBlock.MoveInstructionBlock(originalBlock, NodePosition.After, null);
                    return;

                case ThisInitializationStatus.Hybrid:
                    // Here it's difficult.
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException(
                        this.blockThisInitializationStatus[originalBlock.Token],
                        "this.blockThisInitializationStatus[originalBlock.Token]");
            }

            if ( originalBlock.HasExceptionHandlers )
                throw new AssertionFailedException(
                    string.Format("A hybrid block has exception handlers in method {0}.",
                    this.method));

           
            if (originalBlock.HasInstructionSequences)
            {
                    // Search the first marked sequence.
                InstructionSequence splitSequence = null;
                InstructionSequence currentSequence = originalBlock.FirstInstructionSequence;
                while (currentSequence != null)
                {
                    if (!sequencesWithThisUninitialized[currentSequence.Token])
                    {
                        splitSequence = currentSequence;
                        break;
                    }

                    currentSequence = currentSequence.NextSiblingSequence;
                }

                if (splitSequence == null)
                    throw new AssertionFailedException(
                        string.Format(
                            "We did not find the first initialized sequence in method {0}.",
                            method));

                InstructionBlock initializedBlock =
                    originalBlock.SplitBlockAfterSequence(splitSequence.PreviousSiblingSequence);
                initializedBlock.Comment = "Originally Just After Initilization";
                originalBlock.Comment = originalBlock.Comment == null ?
                    "Containing Initialization" : 
                    originalBlock.Comment + "; Containing Initialization";
                targetParentBlock.MoveInstructionBlock(originalBlock, NodePosition.After, null);
            }
            else if ( originalBlock.HasChildrenBlocks )
            {
                InstructionBlock newBlock = this.method.MethodBody.CreateInstructionBlock();
                newBlock.Comment = "Uninitialized Split Block";
                targetParentBlock.AddChildBlock(newBlock, NodePosition.After, null);

                InstructionBlock child = originalBlock.FirstChildBlock;

                while ( child != null )
                {
                    this.MoveUninitializedBlockRecursive(child, newBlock);
                    child = child.NextSiblingBlock;
                }
                
            }
        }

        private static InstructionBlock SplitEntrySymbolSequencePoint(InstructionBlock block, InstructionWriter writer)
        {
            MethodBodyDeclaration methodBody = block.MethodBody;

            InstructionSequence sequence = block.FindFirstInstructionSequence();

            if (sequence == null)
                return null;

            InstructionReader reader = methodBody.CreateInstructionReader(false);
            InstructionBlock nopBlock = null;

            reader.EnterInstructionSequence(sequence);
            if (reader.ReadInstruction())
            {
                if (reader.CurrentInstruction.OpCodeNumber == OpCodeNumber.Nop)
                {
                    InstructionSequence afterNopSequence = sequence.SplitAfterReaderPosition(reader);
                    if (afterNopSequence != null)
                    {
                        afterNopSequence.Comment = sequence.Comment;
                    }
                    sequence.Comment = "Initial Symbol Sequence Point Sequence";
                    sequence.Detach();

                    nopBlock = methodBody.CreateInstructionBlock();
                    nopBlock.Comment = "Initial Symbol Sequence Point Block";
                    nopBlock.AddInstructionSequence(sequence, NodePosition.Before, null);
                }
                else if (reader.CurrentInstruction.SymbolSequencePoint != null)
                {
                    SymbolSequencePoint currentPoint = reader.CurrentInstruction.SymbolSequencePoint;
                    SymbolSequencePoint entryPoint =
                        new SymbolSequencePoint(currentPoint.StartLine, currentPoint.StartColumn - 2,
                                                 currentPoint.StartLine, currentPoint.StartColumn - 1,
                                                 currentPoint.Document);

                    InstructionSequence nopSequence = methodBody.CreateInstructionSequence();
                    nopSequence.Comment = "Initial Symbol Sequence Point Sequence";

                    nopBlock = methodBody.CreateInstructionBlock();
                    nopBlock.Comment = "Initial Symbol Sequence Point Block";
                    nopBlock.AddInstructionSequence(nopSequence, NodePosition.Before, null);

                    writer.AttachInstructionSequence(nopSequence);
                    writer.EmitSymbolSequencePoint(entryPoint);
                    writer.EmitInstruction(OpCodeNumber.Nop);
                    writer.DetachInstructionSequence();
                }
            }

            return nopBlock;
        }


        
        private enum ThisInitializationStatus
        {
            /// <summary>
            /// Not yet computed.
            /// </summary>
            None = 0,

            /// <summary>
            /// Has initialized <b>this</b> pointer.
            /// </summary>
            Initialized = 1,

            /// <summary>
            /// Has uninitialized <b>this</b> pointer.
            /// </summary>
            Uninitialized = 2,

            /// <summary>
            /// Hash some sequences with initialized <b>this</b> pointer, some
            /// with uninitialized <b>this</b> pointer.
            /// </summary>
            Hybrid = 3,

            /// <summary>
            /// Error: some blocks have handlers
            /// </summary>
            ErrorHasHandler = -1
        }

    }

    /// <summary>
    /// Options for the <see cref="MethodBodyRestructurer"/> class.
    /// </summary>
    [Flags]
    public enum MethodBodyRestructurerOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None,

        /// <summary>
        /// Change <b>ret</b> instructions into branching instructions.
        /// </summary>
        ChangeReturnInstructions = 1
    }



    /// <summary>
    /// Determines whether the instance constructor calls a constructor its own
    /// class (<b>this</b>) or in the base class (<b>base</b>).
    /// </summary>
    public enum ConstructorType
    {
        /// <summary>
        /// Default: <see cref="NotAnInstanceConstructor"/>.
        /// </summary>
        Default = NotAnInstanceConstructor,

        /// <summary>
        /// The method is not an instance constructor.
        /// </summary>
        NotAnInstanceConstructor = 0,

        /// <summary>
        /// The constructor calls the base class.
        /// </summary>
        CallBase,

        /// <summary>
        /// The constructor calls the current class.
        /// </summary>
        CallThis,

        /// <summary>
        /// The constructor does not call any base constructor (<b>struct</b> constructor).
        /// </summary>
        CallNothing
    }
}
