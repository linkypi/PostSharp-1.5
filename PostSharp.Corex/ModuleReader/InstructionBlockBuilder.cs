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
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Runtime.InteropServices;
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Builds the tree of instruction blocks (<see cref="InstructionBlock"/>) of
    /// a method body from exception handling clauses and lexical scopes.
    /// </summary>
    internal static class InstructionBlockBuilder
    {
#if CheckInvariants
        private static void CheckInvariants(InstructionBlock parent)
        {
            if (parent.HasChildrenBlocks)
            {
                if (parent.StartOffset != parent.FirstChildBlock.StartOffset)
                {
                    throw new AssertionFailedException();
                }

                if (parent.EndOffset != parent.LastChildBlock.EndOffset)
                {
                    throw new AssertionFailedException();
                }

                InstructionBlock cursor = parent.FirstChildBlock;

                while (cursor != null)
                {
                    CheckInvariants(cursor);

                    if (cursor.NextSiblingBlock != null)
                    {
                        if (cursor.EndOffset != cursor.NextSiblingBlock.StartOffset)
                        {
                            throw new AssertionFailedException();
                        }
                    }

                    cursor = cursor.NextSiblingBlock;
                }
            }
        }
#endif

        /// <summary>
        /// Get a block given its start and end offset by splitting some blocks if necessary.
        /// </summary>
        /// <param name="parent">Parent block.</param>
        /// <param name="start">Start offset.</param>
        /// <param name="end">End offset.</param>
        /// <returns>An <see cref="InstructionBlock"/> whose start offset is 
        /// <paramref name="start"/>, end offset is <paramref name="end"/> and that is
        /// a descendant of <paramref name="parent"/>.</returns>
        private static InstructionBlock GetBlock( InstructionBlock parent, int start, int end )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( parent, "parent" );

            #endregion

            InstructionBlock container = FindDirectContainer( parent, start, end );

            if ( container.StartOffset == start && container.EndOffset == end )
            {
                // The container block is exactly the block we are looking for
                return container;
            }

            if ( container.HasChildrenBlocks )
            {
                // The container block is partitioned.
                // 1. find which of the partition parts belong to the new block
                // 2. remove these parts from the container partition to the new block

                InstructionBlock newBlock = null;
                InstructionBlock cursor = container.FirstChildBlock;

                while ( cursor != null )
                {
                    InstructionBlock next;

                    if ( newBlock == null )
                    {
                        next = cursor.NextSiblingBlock;

                        if ( cursor.EndOffset > start )
                        {
                            // The block at the cursor is the first block that should be under the new block.

                            // Create the new block
                            newBlock = parent.MethodBody.CreateInstructionBlock( start, end );

                            // Insert it after the block at cursor
                            container.AddChildBlock( newBlock, NodePosition.After, cursor );

                            if ( cursor.StartOffset != start )
                            {
                                if ( !cursor.HasExceptionHandlers &&
                                     !cursor.HasChildrenBlocks &&
                                     !cursor.HasLocalVariableSymbols &&
                                     cursor.ParentBlock != null &&
                                     cursor.ParentExceptionHandler == null )
                                {
                                    // Split the current (container) block.
                                    InstructionBlock[] splittedBlocks;
                                    InstructionBlock childBlock;

                                    // If all the above conditions are ok, the block may be splitted.
                                    SplitBlock( cursor, start, cursor.EndOffset, out splittedBlocks, out childBlock );

                                    // Insert the first splitted block OUTSIDE the new block, the second INSIDE.
                                    container.AddChildBlock( splittedBlocks[0], NodePosition.After, cursor );
                                    newBlock.AddChildBlock( splittedBlocks[1], NodePosition.After, null );

                                    // Remove the current block.
                                    cursor.Detach();
                                }
                                else
                                {
                                    // We cannot split this block.
                                    return null;
                                }
                            }
                            else //  if (cursor.StartOffset != start)
                            {
                                // Unlink the block at cursor, then insert it as a child in the new block.
                                newBlock.MoveInstructionBlock( cursor, NodePosition.Before, null );
                            }
                        } // if ( cursor.EndOffset >= start )
                    }
                    else // if ( newBlock == null )
                    {
                        if ( cursor.EndOffset <= end )
                        {
                            // The block at the cursor should be under the new block.
                            newBlock.MoveInstructionBlock( cursor, NodePosition.After, null );

                            if ( cursor.EndOffset == end )
                            {
                                // Exit the loop, since the job is done.
                                break;
                            }
                        }
                        else
                        {
                            if ( !cursor.HasExceptionHandlers &&
                                 !cursor.HasChildrenBlocks &&
                                 !cursor.HasLocalVariableSymbols &&
                                 cursor.ParentBlock != null &&
                                 cursor.ParentExceptionHandler == null )
                            {
                                // Split the current (container) block.
                                InstructionBlock[] splittedBlocks;
                                InstructionBlock childBlock;

                                // If all the above conditions are ok, the block may be splitted.
                                SplitBlock( cursor, cursor.StartOffset, end, out splittedBlocks, out childBlock );

                                // Insert the first splitted block OUTSIDE the new block, the second INSIDE.
                                newBlock.AddChildBlock( splittedBlocks[0], NodePosition.After, null );
                                container.AddChildBlock( splittedBlocks[1], NodePosition.After, newBlock );

                                // Remove the current block.
                                cursor.Detach();

                                // Exit the loop, since the job is done.
                                break;
                            }
                            else
                            {
                                // We cannot split this block.
                                return null;
                            }
                        }

                        next = newBlock.NextSiblingBlock;
                    }

                    cursor = next;
                } // while ( cursor != null )

                if ( newBlock == null )
                {
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "PostConditionFailed", "block == null" );
                }

                if ( newBlock.EndOffset != end )
                {
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "PostConditionFailed",
                                                                               "cursor.EndOffset != end" );
                }

#if CheckInvariants
                CheckInvariants( parent );
#endif

                return newBlock;
            }
            else
            {
                // The container node has no child


                // Split the current (container) block.
                InstructionBlock[] splittedBlocks;
                InstructionBlock childBlock;

                if ( !container.HasExceptionHandlers &&
                     !container.HasChildrenBlocks &&
                     !container.HasLocalVariableSymbols &&
                     container.ParentBlock != null &&
                     container.ParentExceptionHandler == null )
                {
                    // If all the above conditions are ok, the block may be splitted.

                    SplitBlock( container, start, end, out splittedBlocks, out childBlock );

                    // Insert the new blocs before the container (there are at least two fragments)
                    container.ParentBlock.AddChildBlock( splittedBlocks[0], NodePosition.After, container );
                    container.ParentBlock.AddChildBlock( splittedBlocks[1], NodePosition.After, splittedBlocks[0] );
                    if ( splittedBlocks[2] != null )
                        container.ParentBlock.AddChildBlock( splittedBlocks[2], NodePosition.After, splittedBlocks[1] );

                    // Remove the container
                    container.Detach();
                }
                else
                {
                    // We cannot delete the container node, so we create a partition with children.

                    SplitBlock( container, start, end, out splittedBlocks, out childBlock );
                    container.AddChildBlock( splittedBlocks[0], NodePosition.Before, null );
                    container.AddChildBlock( splittedBlocks[1], NodePosition.After, null );
                    if ( splittedBlocks[2] != null )
                        container.AddChildBlock( splittedBlocks[2], NodePosition.After, null );
                }

                return childBlock;
            }
        }


        /// <summary>
        /// Given a parent block containing an interleave, eventually split the block
        /// in one, two or three parts so that a new block corresponding exactly to
        /// the given interleave is created.
        /// </summary>
        /// <param name="parent">Block to be eventually splitted.</param>
        /// <param name="start">Start offset of the interleave.</param>
        /// <param name="end">End offset of the interleave.</param>
        /// <param name="splittedBlocks">An array containing the parts in which
        /// <paramref name="parent"/> has been splitted, with 1, 2 or 3 non-null elements.</param>
        /// <param name="childBlock">A block starting at <paramref name="start"/> and
        /// ending at <paramref name="end"/>.</param>
        private static void SplitBlock( InstructionBlock parent,
                                        int start, int end,
                                        out InstructionBlock[] splittedBlocks,
                                        out InstructionBlock childBlock )
        {
            splittedBlocks = new InstructionBlock[3];

            if ( parent.StartOffset == start && parent.EndOffset == end )
            {
                splittedBlocks[0] = parent;
                childBlock = parent;
            }
            else
            {
                int i = 0;

                if ( start > parent.StartOffset )
                {
                    splittedBlocks[0] = parent.MethodBody.CreateInstructionBlock( parent.StartOffset, start );
                    i++;
                }

                childBlock = parent.MethodBody.CreateInstructionBlock( start, end );
                splittedBlocks[i] = childBlock;
                i++;

                if ( end < parent.EndOffset )
                {
                    splittedBlocks[i] = parent.MethodBody.CreateInstructionBlock( end, parent.EndOffset );
                }
            }
        }

        /// <summary>
        /// Finds the block that directly contains a given interleaves.
        /// </summary>
        /// <param name="parent">Parent block.</param>
        /// <param name="start">Start offset.</param>
        /// <param name="end">End offset.</param>
        /// <returns>The outest <see cref="InstructionBlock"/> containing the interleaved
        /// formed by <paramref name="start"/> and <paramref name="end"/>, or <b>null</b>
        /// if the interleave spans two blocks.
        /// </returns>
        private static InstructionBlock FindDirectContainer( InstructionBlock parent, int start, int end )
        {
            if ( ( parent.Compares( start, end ) & InstructionBlockRelation.ContainsOrEqualsMask ) == 0 )
                throw new ArgumentException( "The block given in parameter should be contained in the current block." );

            return InternalFindDirectContainer( parent, start, end );
        }


        /// <summary>
        /// Finds the block that directly contains a given interleaves. Recursive form without
        /// precondition checking.
        /// </summary>
        /// <param name="parent">Parent block.</param>
        /// <param name="start">Start offset.</param>
        /// <param name="end">End offset.</param>
        /// <returns>The outest <see cref="InstructionBlock"/> containing the interleaved
        /// formed by <paramref name="start"/> and <paramref name="end"/>, or <b>null</b>
        /// if the interleave spans two blocks.
        /// </returns>
        private static InstructionBlock InternalFindDirectContainer( InstructionBlock parent, int start, int end )
        {
            // Enumerate blocks.
            InstructionBlock block = parent.FirstChildBlock;
            while ( block != null )
            {
                InstructionBlockRelation relation = block.Compares( start, end );
                switch ( relation )
                {
                    case InstructionBlockRelation.IsEqual:
                        return block;

                    case InstructionBlockRelation.Contains:
                        return InternalFindDirectContainer( block, start, end );

                    case InstructionBlockRelation.Intersects:
                        if ( !block.HasExceptionHandlers &&
                             !block.HasChildrenBlocks &&
                             !block.HasLocalVariableSymbols &&
                             block.ParentExceptionHandler == null )
                            return parent;
                        else
                            return null;

                    case InstructionBlockRelation.IsDisjoint:
                    case InstructionBlockRelation.IsContained:
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( relation,
                                                                                      "block.Compares(start, end)" );
                }

                block = block.NextSiblingBlock;
            }

            return parent;
        }


        /// <summary>
        /// Build the tree of instruction blocks of a method.
        /// </summary>
        /// <param name="methodDef">A <see cref="MethodDefDeclaration"/> with a valid body.</param>
        /// <returns>The root block of <see cref="MethodDefDeclaration"/>.</returns>
        public static unsafe InstructionBlock BuildInstructionBlockTree( MethodDefDeclaration methodDef )
        {
            ModuleDeclaration module = methodDef.Module;
            IntPtr ilMethod = module.Tables.RvaToPointer( methodDef.RVA );

            uint codeSize;
            CorMethodFlags methodFlags;
            ushort maxStack;
            MetadataToken localVarToken;
            CorMethodSection* pDataSection;
            byte* pInstructions;

            methodDef.MethodBody = new MethodBodyDeclaration();


            int headerFormat = ( *(byte*) ilMethod & 3 );
            switch ( headerFormat )
            {
                case 2:
                    {
                        CorIlMethodHeaderTiny* pHeader = (CorIlMethodHeaderTiny*) ilMethod;
                        codeSize = (uint) ( pHeader->Flags_CodeSize >> 2 );
                        methodFlags = CorMethodFlags.TinyFormatEven;
                        maxStack = 8;
                        localVarToken = new MetadataToken();
                        pDataSection = null;
                        pInstructions = (byte*) pHeader + sizeof(CorIlMethodHeaderTiny);
                    }
                    break;

                case 3:
                    {
                        CorMethodHeaderFat* pHeader = (CorMethodHeaderFat*) ilMethod;

                        // Check the header size.
                        if ( ( pHeader->Flags_StructureSize >> 12 ) != sizeof(CorMethodHeaderFat) >> 2 )
                        {
                            Debug.WriteLine( "Incorrect CorIlMethodHeaderFat structure size." );
                        }

                        codeSize = pHeader->CodeSize;
                        methodFlags = (CorMethodFlags) ( pHeader->Flags_StructureSize & 0x0FFF );
                        maxStack = pHeader->MaxStack;
                        localVarToken = new MetadataToken( pHeader->LocalVarSigTok );
                        pInstructions = (byte*) pHeader + sizeof(CorMethodHeaderFat);
                        if ( ( methodFlags & CorMethodFlags.MoreSections ) != 0 )
                        {
                            pDataSection =
                                (CorMethodSection*)
                                methodDef.Module.ModuleReader.AlignPointerAt4( (IntPtr) ( pInstructions + codeSize ) ).ToPointer();
                        }
                        else
                        {
                            pDataSection = null;
                        }
                    }
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidMethodHeader", methodDef,
                                                                               headerFormat );
            }

            methodDef.MethodBody.InitLocalVariables = ( methodFlags & CorMethodFlags.InitLocals ) != 0;
            methodDef.MethodBody.MaxStack = maxStack;
            methodDef.MethodBody.OriginalInstructions =
                new UnmanagedBuffer(
                    module.Tables.RvaToPointer(
                        (uint)
                        ( methodDef.RVA + ( new IntPtr( pInstructions ).ToInt64() - ilMethod.ToInt64() ) ) ),
                    (int) codeSize );

            InstructionBlock rootBlock = methodDef.MethodBody.CreateInstructionBlock( 0, (int) codeSize );

            if ( !localVarToken.IsNull && localVarToken.Value != 0xFFFF )
            {
                methodDef.MethodBody.SetLocalVariables(
                    module.Tables.GetStandaloneSignature( localVarToken ).LocalVariables );
            }

            #region Processing method headers

            while ( pDataSection != null )
            {
                bool isFat = ( pDataSection->Flags & CorMethodSectionFlags.FatFormat ) != 0;
                uint sectionSize = isFat ? pDataSection->Fat.Size : pDataSection->Small.Size;

                switch ( pDataSection->Flags & CorMethodSectionFlags.KindMask )
                {
                    case CorMethodSectionFlags.ExceptionHandlingTable:
                        {
                            uint handlersInSection = isFat
                                                         ? pDataSection->Fat.GetExceptionHandlingClauseNumber()
                                                         : pDataSection->Small.GetExceptionHandlingClauseNumber();

                            // Array of distinct protected blocks
                            //List<InstructionBlock> blocks = new List<InstructionBlock>((int)handlersInSection);

                            // Starting from the most external to the innest.
                            //for ( int i = 0 ; i < methodBody.ExceptionHandlingClauses.Count ; i++ )
                            for ( int i = (int) handlersInSection - 1 ; i >= 0 ; i-- )
                            {
                                CorExceptionHandlingClauseFat clause;
                                CorExceptionFlag clauseFlag;

                                if ( isFat )
                                {
                                    clause =
                                        *CorMethodSectionFat.GetExceptionHandlingClause( &( pDataSection->Fat ), i );
                                    clauseFlag = (CorExceptionFlag) clause.Flags;
                                }
                                else
                                {
                                    CorExceptionHandlingClauseSmall* pSmallClause =
                                        CorMethodSectionSmall.GetExceptionHandlingClause( &( pDataSection->Small ), i );
                                    clause.ClassToken = pSmallClause->ClassToken;
                                    clause.FilterOffset = pSmallClause->FilterOffset;
                                    clause.HandlerLength = pSmallClause->HandlerLength;
                                    clause.HandlerOffset = pSmallClause->HandlerOffset;
                                    clause.TryLength = pSmallClause->TryLength;
                                    clause.TryOffset = pSmallClause->TryOffset;
                                    clauseFlag = (CorExceptionFlag) pSmallClause->Flags;
                                }

                                // Gets an InstructionBlock for the Try block.
                                InstructionBlock tryBlock =
                                    GetBlock( rootBlock, (int) clause.TryOffset,
                                              (int) clause.TryOffset + (int) clause.TryLength );
                                InstructionBlock tryParentBlock = tryBlock.ParentBlock;
                                InstructionBlock handlerBlock =
                                    GetBlock( tryParentBlock, (int) clause.HandlerOffset,
                                              (int) clause.HandlerOffset + (int) clause.HandlerLength );

                                InstructionBlock filterBlock;
                                if ( clauseFlag == CorExceptionFlag.Filter )
                                {
                                    filterBlock =
                                        GetBlock( tryParentBlock, (int) clause.FilterOffset, (int) clause.HandlerOffset );
                                }
                                else
                                {
                                    filterBlock = null;
                                }

                                ITypeSignature catchType;
                                if ( clauseFlag == CorExceptionFlag.Default )
                                {
                                    catchType = module.Tables.GetType( new MetadataToken( clause.ClassToken ) );
                                }
                                else
                                {
                                    catchType = null;
                                }

                                switch ( (ExceptionHandlingClauseOptions) clauseFlag )
                                {
                                    case ExceptionHandlingClauseOptions.Clause:
                                        tryBlock.AddExceptionHandlerCatch( catchType, handlerBlock, NodePosition.Before,
                                                                           null );
                                        break;

                                    case ExceptionHandlingClauseOptions.Fault:
                                        tryBlock.AddExceptionHandlerFault( handlerBlock, NodePosition.Before, null );
                                        break;

                                    case ExceptionHandlingClauseOptions.Filter:
                                        tryBlock.AddExceptionHandlerFilter( handlerBlock, filterBlock,
                                                                            NodePosition.Before, null );
                                        break;

                                    case ExceptionHandlingClauseOptions.Finally:
                                        tryBlock.AddExceptionHandlerFinally( handlerBlock, NodePosition.Before, null );
                                        break;

                                    default:
                                        throw ExceptionHelper.CreateInvalidEnumerationValueException(
                                            (ExceptionHandlingClauseOptions) clauseFlag, "clauseFlag" );
                                }
                            }
                            break;
                        }
                    default:
                        Debug.WriteLine( "Unexpected method header." );
                        break;
                }

                if ( ( pDataSection->Flags & CorMethodSectionFlags.MoreSections ) != 0 )
                {
                    pDataSection = (CorMethodSection*) ( (byte*) pDataSection + sectionSize + 1 );
                }
                else
                {
                    pDataSection = null;
                }
            }

            #endregion

            #region Processing symbolic scopes

            ISymbolReader symbolReader = module.SymbolReader;
            SymbolSequencePoint[] sequencePoints = null;

            if ( symbolReader != null )
            {
                // Get the symbols for the current method. Catch an eventual exception.
                ISymbolMethod symbolMethod;
                try
                {
                    symbolMethod = symbolReader.GetMethod( new SymbolToken( (int) methodDef.MetadataToken.Value ) );
                }
                catch ( COMException )
                {
                    symbolMethod = null;
                }

                if ( symbolMethod != null )
                {
                    // Get the collection of instruction boundaries
                    List<int> instructionBoundaries = new List<int>( methodDef.MethodBody.OriginalInstructions.Size/2 );
                    InstructionReader reader = methodDef.MethodBody.CreateOriginalInstructionReader();
                    while ( reader.ReadInstruction() )
                    {
                        instructionBoundaries.Add( reader.OffsetBefore );
                    }
                    reader.Dispose();


                    // Processes the root scope and, recursively, all children scopes.
                    ProcessLexicalScope( symbolMethod.RootScope, rootBlock, methodDef.MethodBody,
                                         pInstructions, instructionBoundaries );

                    // Get the sequence points
                    int sequencePointCount = symbolMethod.SequencePointCount;
                    int[] offsets = new int[sequencePointCount];
                    int[] startLines = new int[sequencePointCount];
                    int[] endLines = new int[sequencePointCount];
                    int[] startColumns = new int[sequencePointCount];
                    int[] endColumns = new int[sequencePointCount];
                    ISymbolDocument[] documents = new ISymbolDocument[sequencePointCount];
                    sequencePoints = new SymbolSequencePoint[sequencePointCount];

                    symbolMethod.GetSequencePoints( offsets, documents, startLines, startColumns,
                                                    endLines, endColumns );

                    for ( int i = 0 ; i < sequencePointCount ; i++ )
                    {
                     
                        // Copy the sequence point.
                        sequencePoints[i] = new SymbolSequencePoint(
                            offsets[i], startLines[i], startColumns[i],
                            endLines[i], endColumns[i], documents[i] );
                    }
                }
            }

            #endregion

            methodDef.MethodBody.RootInstructionBlock = rootBlock;


            // Analyze the code branches and build list of instruction sequences.
            InstructionSequenceBuilder.BuildInstructionSequences( methodDef.MethodBody );

            // Browse all sequences and set the first sequence point of each.
            if ( sequencePoints != null )
            {
                methodDef.MethodBody.OriginalSymbolSequencePoints = sequencePoints;

                int instructionSequenceCount = methodDef.MethodBody.InstructionSequenceCount;
                for ( short i = 0 ; i < instructionSequenceCount ; i++ )
                {
                    InstructionSequence sequence = methodDef.MethodBody.GetInstructionSequenceByToken( i );
                    sequence.ComputeFirstOriginalSymbolSequencePoint();
                }
            }

            return rootBlock;
        }

        /// <summary>
        /// Processes a lexical scope, i.e. create children instruction blocks and register
        /// local variable symbols where necessary.
        /// </summary>
        /// <param name="scope">The lexical scope to process.</param>
        /// <param name="parentBlock">The parent <see cref="InstructionBlock"/>.</param>
        /// <param name="methodBody">Method body from which local variables are read.</param>
        /// <param name="ilBytes">A pointer to the binary stream of IL instructions.</param>
        /// <param name="instructionBoundaries">A list of offsets of instruction boundaries.</param>
        private static unsafe void ProcessLexicalScope( ISymbolScope scope, InstructionBlock parentBlock,
                                                        MethodBodyDeclaration methodBody, byte* ilBytes,
                                                        List<int> instructionBoundaries )
        {
            // CheckInvariants(parentBlock);

            ISymbolVariable[] locals = scope.GetLocals();
            InstructionBlock block;

            // Process variables
            if ( locals.Length > 0 )
            {
                int startOffset, endOffset;
                int startOffsetIndex = instructionBoundaries.BinarySearch( scope.StartOffset );
                if ( startOffsetIndex < 0 )
                {
                    startOffset = instructionBoundaries[~startOffsetIndex - 1];

                    Trace.ModuleReader.WriteLine(
                        "The start offset ({0:x}) of a lexical scope did not correspond with an instruction " +
                        "boundary. It was corrected to {1:x}.", scope.StartOffset, startOffset );
                }
                else
                {
                    startOffset = instructionBoundaries[startOffsetIndex];
                }

                if ( scope.EndOffset >= methodBody.OriginalInstructions.Size )
                {
                    endOffset = methodBody.OriginalInstructions.Size;
                }
                else
                {
                    int endOffsetIndex = instructionBoundaries.BinarySearch( scope.EndOffset );
                    if ( endOffsetIndex < 0 )
                    {
                        endOffset = instructionBoundaries[~endOffsetIndex];

                        Trace.ModuleReader.WriteLine(
                            "The end offset ({0:x}) of a lexical scope did not correspond with an instruction " +
                            "boundary. It was corrected to {1:x}.", scope.EndOffset, endOffset );
                    }
                    else
                    {
                        endOffset = instructionBoundaries[endOffsetIndex];
                    }
                }

                InstructionBlock container = FindDirectContainer( parentBlock, startOffset, endOffset );


                if ( container != null )
                {
                    if ( container.StartOffset != startOffset )
                    {
                        // These are hints to make the scope corresponding to the exception handler.

                        if ( ( startOffset == container.StartOffset + 1 &&
                               *( ilBytes + container.StartOffset ) >= (byte) OpCodeNumber.Stloc_0 &&
                               *( ilBytes + container.StartOffset ) <= (byte) OpCodeNumber.Stloc_3 ) ||
                             ( startOffset == container.StartOffset + 2 &&
                               *( ilBytes + container.StartOffset ) == (byte) OpCodeNumber.Stloc_S ) ||
                             ( startOffset == container.StartOffset + 4 &&
                               *( (ushort*) ( ilBytes + container.StartOffset ) ) == (ushort) OpCodeNumber.Stloc ) )
                        {
                            startOffset = container.StartOffset;
                        }

                        if ( ( endOffset + 3 == container.EndOffset &&
                               *( ilBytes + endOffset ) == (byte) OpCodeNumber.Leave ) ||
                             ( endOffset + 2 == container.EndOffset &&
                               *( ilBytes + endOffset ) == (byte) OpCodeNumber.Leave_S ) ||
                             ( endOffset + 1 == container.EndOffset &&
                               *( ilBytes + endOffset ) == (byte) OpCodeNumber.Nop ) )
                        {
                            endOffset = container.EndOffset;
                        }
                    }


                    block = GetBlock( container, startOffset, endOffset );

                    if ( block == null )
                    {
                        Trace.ModuleReader.WriteLine(
                            "The lexical scope 0x{0:x}-0x{1:x} spans multiple blocks (1). This lexical scope will be ignored.",
                            startOffset, endOffset );
                        block = parentBlock;
                    }
                    else
                    {
                        foreach ( ISymbolVariable local in locals )
                        {
                            int ordinal;
                            string name;
                            try
                            {
                                ordinal = local.AddressField1;
                                name = local.Name;
                            }
                            catch
                            {
                                // The COM component may be buggy.
                                continue;
                            }

                            LocalVariableSymbol symbol =
                                new LocalVariableSymbol( methodBody.GetLocalVariable( ordinal ), name );

                            block.AddLocalVariableSymbol( symbol );
                        }
                    }
                }
                else
                {
                    block = parentBlock;
                    Trace.ModuleReader.WriteLine(
                        "The lexical scope 0x{0:x}-0x{1:x} spans multiple blocks (2). This lexical scope will be ignored.",
                        startOffset, endOffset );
                }
            }
            else
            {
                block = parentBlock;
            }

            //CheckInvariants( parentBlock );

            // Process child scopes
            foreach ( ISymbolScope childScope in scope.GetChildren() )
            {
                if ( block.ParentBlock != null && block.Node.List == null )
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "InvariantFailed",
                                                                               "block.ParentBlock != null && block.Node.List == null" );


                ProcessLexicalScope( childScope, block, methodBody, ilBytes, instructionBoundaries );

                if ( block.ParentBlock != null && block.Node.List == null )
                {
                    block = block.ParentBlock;
                }
            }
        }

        #region Structures

        /// <summary>
        /// Kinds of exception handling clauses.
        /// </summary>
        private enum CorExceptionFlag : short
        {
            /// <summary>
            /// Typed handler (catch).
            /// </summary>
            Default,

            /// <summary>
            /// Filter.
            /// </summary>
            Filter = 0x0001,

            /// <summary>
            /// Finally.
            /// </summary>
            Finally = 0x0002,

            /// <summary>
            /// Fault.
            /// </summary>
            Fault = 0x0004,

            /// <summary>
            /// Duplicated.
            /// </summary>
            /// <remarks>
            /// MS says: duplicated clase..  the current clause was duplicated down to a 
            /// funclet which was pulled out of line
            /// </remarks>
            Duplicated = 0x0008
        }

        /// <summary>
        /// Fat form of a method exception handling clause.
        /// </summary>
        [StructLayout( LayoutKind.Explicit )]
        private struct CorExceptionHandlingClauseFat
        {
            /// <summary>
            /// Flags.
            /// </summary>
            [FieldOffset( 0 )] public uint Flags;

            /// <summary>
            /// Offset of the Try block.
            /// </summary>
            [FieldOffset( 4 )] public uint TryOffset;

            /// <summary>
            /// Length of the Try block.
            /// </summary>
            [FieldOffset( 8 )] public uint TryLength;

            /// <summary>
            /// Offset of the Handler block.
            /// </summary>
            [FieldOffset( 12 )] public uint HandlerOffset;

            /// <summary>
            /// Size of the Handler block.
            /// </summary>
            [FieldOffset( 16 )] public uint HandlerLength;

            /// <summary>
            /// Token of the caught for class type-based handlers.
            /// </summary>
            [FieldOffset( 20 )] public uint ClassToken;

            /// <summary>
            /// Offset of the Filter block.
            /// </summary>
            [FieldOffset( 20 )] public uint FilterOffset;
        }

        /// <summary>
        /// Small form of a method exception handling clause.
        /// </summary>
        [StructLayout( LayoutKind.Explicit )]
        private struct CorExceptionHandlingClauseSmall
        {
            /// <summary>
            /// Flags.
            /// </summary>
            [FieldOffset( 0 )] public ushort Flags;

            /// <summary>
            /// Offset of the Try block.
            /// </summary>
            [FieldOffset( 2 )] public ushort TryOffset;

            /// <summary>
            /// Length of the Try block.
            /// </summary>
            [FieldOffset( 4 )] public byte TryLength;

            /// <summary>
            /// Offset of the Handler block.
            /// </summary>
            [FieldOffset( 5 )] public ushort HandlerOffset;

            /// <summary>
            /// Length of the Handler block.
            /// </summary>
            [FieldOffset( 7 )] public byte HandlerLength;

            /// <summary>
            /// Token of the caught for class type-based handlers.
            /// </summary>
            [FieldOffset( 8 )] public uint ClassToken;

            /// <summary>
            /// Offset of the Filter block.
            /// </summary>
            [FieldOffset( 8 )] public uint FilterOffset;
        }

        /// <summary>
        /// Tiny form of the method header.
        /// </summary>
        [StructLayout( LayoutKind.Explicit )]
        private struct CorIlMethodHeaderTiny
        {
            /// <summary>
            /// Flags and code size compressed.
            /// </summary>
            [FieldOffset( 0 )] public byte Flags_CodeSize;
        }

        /// <summary>
        /// Fat form of the method header.
        /// </summary>
        [StructLayout( LayoutKind.Explicit )]
        internal struct CorMethodHeaderFat
        {
            /// <summary>
            /// Flags and structure size compressed.
            /// </summary>
            [FieldOffset( 0 )] public ushort Flags_StructureSize;

            /// <summary>
            /// Maximum number of items on the operand stack.
            /// </summary>
            [FieldOffset( 2 )] public ushort MaxStack;

            /// <summary>
            /// Size of the code.
            /// </summary>
            [FieldOffset( 4 )] public uint CodeSize;

            /// <summary>
            /// A token of type <see cref="TokenType.Signature"/> containing
            /// the signature of local variables (0 means none).
            /// </summary>
            [FieldOffset( 8 )] public uint LocalVarSigTok;
        }

        /// <summary>
        /// Flags of IL method body.
        /// </summary>
        private enum CorMethodFlags
        {
            /// <summary>
            /// Determines whether local variables should be initialized to their
            /// default values.
            /// </summary>
            InitLocals = 0x0010,

            /// <summary>
            /// Determines whether there is another section after the current one.
            /// </summary>
            MoreSections = 0x0008, // there is another attribute after the current one

            /// <summary>
            /// Undocumented.
            /// </summary>
            /// <remarks>
            /// From MS:  FIX Remove the current and do it on a per Module basis
            /// </remarks>
            CompressedIL = 0x0040,

            /// <summary>
            /// Isolates the format from a <see cref="CorMethodFlags"/>.
            /// </summary>
            FormatMask = ( ( 1 << 3 ) - 1 ),

            /// <summary>
            /// Tiny format (use the current code if the code size is even).
            /// </summary>
            TinyFormatEven = 0x0002,

            /// <summary>
            /// Small format.
            /// </summary>
            SmallFormat = 0x0000,

            /// <summary>
            /// Fat format.
            /// </summary>
            FatFormat = 0x0003,

            /// <summary>
            /// Tiny format (use the current code if the code size is odd).
            /// </summary>
            TinyFormatOdd = 0x0006,
        }

        /// <summary>
        /// Flags of a section of an IL method body.
        /// </summary>
        private enum CorMethodSectionFlags : byte // codes that identify attributes
        {
            /// <summary>
            /// Reserved.
            /// </summary>
            Default = 0,

            /// <summary>
            /// The section is an exception handling table.
            /// </summary>
            ExceptionHandlingTable = 1,

            /// <summary>
            /// The section is an optional IL table.
            /// </summary>
            OptionalILTable = 2,

            /// <summary>
            /// Isolates the section type from the <see cref="CorMethodSectionFlags"/>.
            /// </summary>
            KindMask = 0x3F,

            /// <summary>
            /// The section is in fat format.
            /// </summary>
            FatFormat = 0x40,

            /// <summary>
            /// There is another section after the current one.
            /// </summary>
            MoreSections = 0x80,
        }

        /// <summary>
        /// Represents a section of a IL method body.
        /// </summary>
        [StructLayout( LayoutKind.Explicit )]
        private struct CorMethodSection
        {
            /// <summary>
            /// Flags.
            /// </summary>
            [FieldOffset( 0 )] public CorMethodSectionFlags Flags;

            /// <summary>
            /// Small form of the method section.
            /// </summary>
            [FieldOffset( 1 )] public CorMethodSectionSmall Small;

            /// <summary>
            /// Fat form of the method section.
            /// </summary>
            [FieldOffset( 1 )] public CorMethodSectionFat Fat;
        }

        /// <summary>
        /// Small form of a method section.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct CorMethodSectionSmall
        {
            /// <summary>
            /// Size of the section in bytes.
            /// </summary>
            public byte Size;

            /// <summary>
            /// Reserved.
            /// </summary>
            public ushort Reserved;

            /// <summary>
            /// First exception handling clause.
            /// </summary>
            private CorExceptionHandlingClauseSmall firstClause;

            /// <summary>
            /// Gets the number of exception handling clauses.
            /// </summary>
            /// <returns>The number of exception handling clauses.</returns>
            public uint GetExceptionHandlingClauseNumber()
            {
                if ( Size%12 == 0 )
                {
                    return (uint) Size/12;
                }
                else if ( ( Size - 4 )%12 == 0 )
                {
                    return (uint) ( Size - 4 )/12;
                }
                else
                {
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidToken",
                                                                               this.Size,
                                                                               "Exception handling clause size (small)" );
                }
            }

            /// <summary>
            /// Gets a pointer to an exception handling clause given its index.
            /// </summary>
            /// <param name="pInstance">Pointer to a <see cref="CorExceptionHandlingClauseSmall"/>.</param>
            /// <param name="index">Index of the required clause.</param>
            /// <returns>The <see cref="CorExceptionHandlingClauseSmall"/> at position <paramref name="index"/>.</returns>
            public static unsafe CorExceptionHandlingClauseSmall* GetExceptionHandlingClause(
                CorMethodSectionSmall* pInstance, int index )
            {
                return (CorExceptionHandlingClauseSmall*) ( (byte*) &pInstance->firstClause + index*12 );
            }
        }

        /// <summary>
        /// Fat form of the method section.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct CorMethodSectionFat
        {
            /// <summary>
            /// First byte of the section size.
            /// </summary>
            private byte sectionSize0;

            /// <summary>
            /// Second and third bytes of the section size.
            /// </summary>
            private ushort sectionSize1;

            /// <summary>
            /// First exception handling clause.
            /// </summary>
            private CorExceptionHandlingClauseFat firstClause;

            /// <summary>
            /// Gets the size of the current section.
            /// </summary>
            public uint Size { get { return (uint) ( sectionSize0 | ( sectionSize1 << 8 ) ); } }

            /// <summary>
            /// Gets the number of exception handling clauses.
            /// </summary>
            /// <returns>The number of exception handling clauses.</returns>
            public uint GetExceptionHandlingClauseNumber()
            {
                if ( Size%24 == 0 )
                {
                    return Size/24;
                }
                else if ( ( Size - 4 )%24 == 0 )
                {
                    return ( Size - 4 )/24;
                }
                else
                {
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidToken",
                                                                               this.Size,
                                                                               "Exception handling clause size (large)" );
                }
            }

            /// <summary>
            /// Gets a pointer to an exception handling clause given its index.
            /// </summary>
            /// <param name="pInstance">Pointer to a <see cref="CorExceptionHandlingClauseSmall"/>.</param>
            /// <param name="index">Index of the required clause.</param>
            /// <returns>The <see cref="CorExceptionHandlingClauseSmall"/> at position <paramref name="index"/>.</returns>
            public static unsafe CorExceptionHandlingClauseFat* GetExceptionHandlingClause(
                CorMethodSectionFat* pInstance, int index )
            {
                return (CorExceptionHandlingClauseFat*) ( (byte*) &pInstance->firstClause + index*24 );
            }
        }

        #endregion
    }
}
