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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Weaves advices with main source code.
    /// </summary>
    public sealed partial class Weaver : IDisposable
    {
        /// <summary>
        /// GUID of a declaration tag (see <see cref="MetadataDeclaration.GetTag"/> and
        /// <see cref="MetadataDeclaration.SetTag"/>). The tag contains a boolean 
        /// indicating whether the method should be ignored by the code weaver.
        /// </summary>
        private static readonly Guid ignoreMethodTag =
            new Guid( "{4DB33AEC-6065-4b65-BE9F-0E968E081258}" );

        private readonly WeavingHelper weavingHelper;

        private readonly Dictionary<MethodDefDeclaration, MethodLevelAdvices> methodLevelAdvices =
            new Dictionary<MethodDefDeclaration, MethodLevelAdvices>();

        private readonly Dictionary<IField, AdvicePerKindCollection> fieldLevelAdvices =
            new Dictionary<IField, AdvicePerKindCollection>();

        private readonly ModuleDeclaration module;

        private InstructionWriter instructionWriter = new InstructionWriter();

        private readonly Set<FieldDefDeclaration> fieldsChangedToProperties = new Set<FieldDefDeclaration>();

        private readonly Project project;


        /// <summary>
        /// Initializes a new <see cref="Weaver"/>.
        /// </summary>
        /// <param name="project">Project to which the new <see cref="Weaver"/> is assigned.</param>
        public Weaver( Project project )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( project, "project" );

            #endregion

            this.project = project;
            this.module = project.Module;
            this.weavingHelper = new WeavingHelper( module );
        }

        /// <summary>
        /// Specifies that a method should be ignored (skipped) by the <see cref="Weaver"/>.
        /// </summary>
        /// <param name="method">A method.</param>
        public static void IgnoreMethod( MethodDefDeclaration method )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( method, "method" );

            #endregion

            method.SetTag( ignoreMethodTag, true );
        }

        private void InternalAddAdvice( IAdvice advice, MethodDefDeclaration method, JoinPointKinds joinPointKinds,
                                        MetadataDeclaration operand )
        {
#if TRACE
            if ( operand != null )
            {
                Trace.CodeWeaver.WriteLine( "Adding the advice {{{0}}} to method {{{1}}} with operand {{{2}}}.",
                                            advice.GetType(), method, operand );
            }
            else
            {
                Trace.CodeWeaver.WriteLine( "Adding the advice {{{0}}} to method {{{1}}} without operand.",
                                            advice.GetType(), method, operand );
            }
#endif

            MethodLevelAdvices advices;
            if ( !this.methodLevelAdvices.TryGetValue( method, out advices ) )
            {
                advices = new MethodLevelAdvices();
                this.methodLevelAdvices.Add( method, advices );
            }
            advices.Add( advice, joinPointKinds, operand );
        }

        private void RecursiveAddAdviceWithOperand( IAdvice advice, Set<MethodDefDeclaration> methods,
                                                    JoinPointKinds joinPointKinds, MetadataDeclaration operand )
        {
            Trace.CodeWeaver.WriteLine( "Advice {0}: finding methods using the method {1}.",
                                        advice, operand );

            // Add to methods that use the current operand.
            if ( operand.Module == this.module )
            {
                // Resolves references internal to the module. This is stupid,
                // but some compilers use a MemberRef to access a MethodDef.
                IMember memberOperand = operand as IMember;

                if ( memberOperand != null )
                {
                    foreach (
                        MemberRefDeclaration memberRef in IndexTypeDefMemberRefsTask.GetReferences( memberOperand ) )
                    {
                        this.RecursiveAddAdviceWithOperand( advice, methods, joinPointKinds, memberRef );
                    }
                }

                // Add the advice to methods using the operand.
                ICollection<MethodDefDeclaration> usedBy = IndexUsagesTask.GetUsedBy( operand );
                if ( usedBy != null )
                {
                    foreach ( MethodDefDeclaration usedByMethod in usedBy )
                    {
                        if ( methods == null || methods.Contains( usedByMethod ) )
                        {
                            this.InternalAddAdvice( advice, usedByMethod, joinPointKinds, operand );
                        }
                    }
                }
            }
            else
            {
                MethodDefDeclaration methodDef;
                FieldDefDeclaration fieldDef;

                if ( ( methodDef = operand as MethodDefDeclaration ) != null )
                {
                    MethodRefDeclaration methodRef = (MethodRefDeclaration) methodDef.Translate( this.module );

                    this.RecursiveAddAdviceWithOperand( advice, methods, joinPointKinds,
                                                        methodRef );
                }
                else if ( ( fieldDef = operand as FieldDefDeclaration ) != null )
                {
                    FieldRefDeclaration fieldRef = (FieldRefDeclaration) fieldDef.Translate( this.module );

                    this.RecursiveAddAdviceWithOperand( advice, methods, joinPointKinds,
                                                        fieldRef );
                }
            }

            // Add recursively to generic instances of the operand.
            ICollection<MetadataDeclaration> references =
                IndexGenericInstancesTask.GetGenericInstances( operand );
            if ( references != null )
            {
                Trace.CodeWeaver.WriteLine( "Inspecting {1} generic instance(s) of {0}...", operand, references.Count );
                foreach ( MetadataDeclaration reference in references )
                {
                    this.RecursiveAddAdviceWithOperand( advice, methods, joinPointKinds, reference );
                }
            }
            else
            {
                Trace.CodeWeaver.WriteLine( "No generic instance for {0}.", operand );
            }
        }

        /// <summary>
        /// Adds a method-level advice to the current <see cref="Weaver"/>.
        /// </summary>
        /// <param name="advice">An advice.</param>
        /// <param name="methods">The set of methods to which the advice applies, or
        /// <b>null</b> if the advice applies to all methods.</param>
        /// <param name="joinPointKinds">The kinds of join points to which the advice applies.</param>
        /// <param name="operands">The set of operands to which the advice applies,
        /// or <b>null</b> if the selected kinds of join points have no operand <i>or</i> if
        /// the advice applies to all operands.</param>
        public void AddMethodLevelAdvice( IAdvice advice,
                                          IEnumerable<MethodDefDeclaration> methods,
                                          JoinPointKinds joinPointKinds,
                                          IEnumerable<MetadataDeclaration> operands )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( advice, "advice" );

            #endregion

            Set<MethodDefDeclaration> methodSet = null;

            // Index the target methods.
            if ( methods != null )
            {
                methodSet = new Set<MethodDefDeclaration>();
                foreach ( MethodDefDeclaration method in methods )
                {
                    methodSet.Add( method );
                }
            }

            if ( operands == null )
            {
                IEnumerator methodsEnumerator;

                // No operand. The advice applies on all methods on which the advice is applied.
                if ( methods == null )
                {
                    // The advice is applied on all methods of the module.
                    methodsEnumerator = this.module.GetDeclarationEnumerator( TokenType.MethodDef );
                }
                else
                {
                    // The advice is applied on a subset of methods.
                    methodsEnumerator = methodSet.GetEnumerator();
                }

                while ( methodsEnumerator.MoveNext() )
                {
                    this.InternalAddAdvice( advice, (MethodDefDeclaration) methodsEnumerator.Current, joinPointKinds,
                                            null );
                }
            }
            else
            {
                // We have an operand. We need only to weave methods that use the operand.

                foreach ( MetadataDeclaration operand in operands )
                {
                    this.RecursiveAddAdviceWithOperand( advice, methodSet, joinPointKinds, operand );
                }
            }
        }

        /// <summary>
        /// Adds a field-level advice to the current <see cref="Weaver"/>.
        /// </summary>
        /// <param name="advice">An advice.</param>
        /// <param name="joinPointKinds">The kinds of join points to which the advice applies.</param>
        /// <param name="fields">The set of fields to which the advice applies.</param>
        public void AddFieldLevelAdvice( IAdvice advice, JoinPointKinds joinPointKinds, IEnumerable<IField> fields )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( advice, "advice" );
            ExceptionHelper.AssertArgumentNotNull( fields, "fields" );

            #endregion

            foreach ( IField field in fields )
            {
                AdvicePerKindCollection adviceCollection;
                if ( !this.fieldLevelAdvices.TryGetValue( field, out adviceCollection ) )
                {
                    adviceCollection = new AdvicePerKindCollection();
                    this.fieldLevelAdvices.Add( field, adviceCollection );
                }
                adviceCollection.Add( advice, joinPointKinds );
            }
        }

        /// <summary>
        /// Informs the weaver that a property should be generated around a field.
        /// </summary>
        /// <param name="fieldDef">Field that should be wrapped by a property.</param>
        /// <remarks>
        /// If this property is set, the field is made private and renamed, and a property
        /// with the original name and visibility of the field is created.
        /// </remarks>
        public void GeneratePropertyAroundField( FieldDefDeclaration fieldDef )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( fieldDef, "fieldDef" );

            #endregion

            this.fieldsChangedToProperties.AddIfAbsent( fieldDef );
        }


        /// <summary>
        /// Adds a type-level advice to the current <see cref="Weaver"/>.
        /// </summary>
        /// <param name="advice">An advice.</param>
        /// <param name="joinPointKinds">The kinds of join points to which the advice applies.</param>
        /// <param name="types">The set of types to which the advice applies.</param>
        public void AddTypeLevelAdvice( IAdvice advice, JoinPointKinds joinPointKinds,
                                        IEnumerable<TypeDefDeclaration> types )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( advice, "advice" );
            ExceptionHelper.AssertArgumentNotNull( types, "types" );

            #endregion

            foreach ( TypeDefDeclaration typeDef in types )
            {
                if ( ( joinPointKinds & JoinPointKinds.AfterInstanceInitialization ) != 0 )
                {
                    IEnumerable<MethodDefDeclaration> methods = typeDef.Methods.GetByName( ".ctor" );
                    if ( methods != null )
                    {
                        this.AddMethodLevelAdvice(advice, methods, JoinPointKinds.AfterInstanceInitialization, null);
                    }
                }

                if ( ( joinPointKinds & JoinPointKinds.BeforeStaticConstructor ) != 0 )
                {
                    MethodDefDeclaration staticConstructor = typeDef.Methods.GetOneByName( ".cctor" );

                    IBeforeStaticConstructorAdvice beforeStaticConstructorAdvice =
                        advice as IBeforeStaticConstructorAdvice;

                    bool beforeFieldInitSupported = beforeStaticConstructorAdvice == null
                                                        ? false
                                                        :
                                                            beforeStaticConstructorAdvice.IsBeforeFieldInitSupported;

                    if ( staticConstructor == null )
                    {
                        // We have to create a static constructor.
                        staticConstructor = new MethodDefDeclaration
                                                {
                                                    Name = ".cctor",
                                                    Attributes = ( MethodAttributes.Private | MethodAttributes.Static |
                                                                   MethodAttributes.RTSpecialName |
                                                                   MethodAttributes.SpecialName |
                                                                   MethodAttributes.HideBySig )
                                                };

                        typeDef.Methods.Add( staticConstructor );
                        this.weavingHelper.AddCompilerGeneratedAttribute( staticConstructor.CustomAttributes );

                        staticConstructor.ReturnParameter = new ParameterDeclaration
                                                                {
                                                                    Attributes = ParameterAttributes.Retval,
                                                                    ParameterType =
                                                                        module.Cache.GetIntrinsic( IntrinsicType.Void )
                                                                };
                        InstructionSequence sequence = staticConstructor.MethodBody.CreateInstructionSequence();
                        staticConstructor.MethodBody.RootInstructionBlock =
                            staticConstructor.MethodBody.CreateInstructionBlock();
                        staticConstructor.MethodBody.RootInstructionBlock.AddInstructionSequence( sequence,
                                                                                                  NodePosition.Before,
                                                                                                  null );
                        this.instructionWriter.AttachInstructionSequence( sequence );
                        this.instructionWriter.EmitInstruction( OpCodeNumber.Ret );
                        this.instructionWriter.DetachInstructionSequence();
                    }
                    else
                    {
                        beforeFieldInitSupported &= ( typeDef.Attributes & TypeAttributes.BeforeFieldInit ) != 0;
                    }

                    typeDef.Attributes = ( typeDef.Attributes & ~TypeAttributes.BeforeFieldInit ) |
                                         ( beforeFieldInitSupported ? TypeAttributes.BeforeFieldInit : 0 );

                    this.AddMethodLevelAdvice( advice, new Singleton<MethodDefDeclaration>( staticConstructor ),
                                               JoinPointKinds.BeforeMethodBody, null );
                }
            }
        }

        #region State Machine

        /// <summary>
        /// Main loop of the state machine. Calls the transition processing
        /// method as a function of the current position in the code tree
        /// and loops until it reaches the end of the method body.
        /// </summary>
        /// <param name="state">Current state.</param>
        private static void Process( WeaverState state )
        {
            while ( true )
            {
                switch ( state.Position )
                {
                    case Position.BeforeBody:
                        ProcessBeforeBody( state );
                        break;

                    case Position.AfterBlock:
                        ProcessAfterBlock( state );
                        break;

                    case Position.AfterBody:
                        ExceptionHelper.Core.Assert( state.BlockStack.Count == 0, "Block stack not empty." );
                        return;

                    case Position.AfterSequence:
                        ProcessAfterSequence( state );
                        break;

                    case Position.BeforeBlock:
                        ProcessBeforeBlock( state );
                        break;

                    case Position.BeforeInstruction:
                        ProcessBeforeInstruction( state );
                        break;

                    case Position.BeforeSequence:
                        ProcessBeforeSequence( state );
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( state.Position,
                                                                                      "state.Position" );
                }
            }
        }

        internal static JoinPointKinds ConvertJoinPointKind( JoinPointKindIndex joinPointKind )
        {
            if ( joinPointKind == JoinPointKindIndex.None )
            {
                return JoinPointKinds.None;
            }
            else
            {
                return (JoinPointKinds) ( (ulong) 1 << (int) joinPointKind );
            }
        }

        /// <summary>
        /// Weave all advices around a location.
        /// </summary>
        /// <param name="joinPointKindBefore">Kind of join point before the location.</param>
        /// <param name="joinPointKindInsteadOf">Kind of join point instead of the location.</param>
        /// <param name="joinPointKindAfter">Kind of join point after the location.</param>
        /// <param name="state">Weaving state.</param>
        /// <param name="operand">Operand of the current instruction.</param>
        private static void ProcessJoinPoint(
            JoinPointKindIndex joinPointKindBefore,
            JoinPointKindIndex joinPointKindInsteadOf,
            JoinPointKindIndex joinPointKindAfter,
            WeaverState state, MetadataDeclaration operand )
        {
            MethodBodyDeclaration methodBody = state.Context.Method.MethodBody;

            AdviceCollection joinPointsBefore = state.Advices.GetAdvices( joinPointKindBefore, operand );
            if ( !CheckAdviceCount( joinPointsBefore, state.Context.Method ))
                return;

            AdviceCollection joinPointsInsteadOf = state.Advices.GetAdvices( joinPointKindInsteadOf, operand );
            if (!CheckAdviceCount(joinPointsInsteadOf, state.Context.Method))
                return;

            AdviceCollection joinPointsAfter = state.Advices.GetAdvices( joinPointKindAfter, operand );
            if (!CheckAdviceCount(joinPointsAfter, state.Context.Method))
                return;


            if ( joinPointsAfter != null || joinPointsBefore != null || joinPointsInsteadOf != null )
            {
                int requiresWeavingBefore, requiresWeavingAfter, requiresWeavingInsteadOf;

                JoinPoint joinPoint = state.Context.JoinPoint;

                joinPoint.Instruction = state.Reader.CurrentInstruction;

                #region Determines if weaving is required

                if ( joinPointsBefore != null )
                {
                    joinPoint.Position = JoinPointPosition.Before;
                    joinPoint.JoinPointKind = ConvertJoinPointKind( joinPointKindBefore );
                    requiresWeavingBefore = joinPointsBefore.RequiresWeave( state.Context );
                }
                else
                {
                    requiresWeavingBefore = 0;
                }

                if ( joinPointsAfter != null )
                {
                    joinPoint.Position = JoinPointPosition.After;
                    joinPoint.JoinPointKind = ConvertJoinPointKind( joinPointKindAfter );
                    requiresWeavingAfter = joinPointsAfter.RequiresWeave( state.Context );
                }
                else
                {
                    requiresWeavingAfter = 0;
                }

                if ( joinPointsInsteadOf != null )
                {
                    joinPoint.Position = JoinPointPosition.InsteadOf;
                    joinPoint.JoinPointKind = ConvertJoinPointKind( joinPointKindInsteadOf );
                    requiresWeavingInsteadOf = joinPointsInsteadOf.RequiresWeave( state.Context );
                }
                else
                {
                    requiresWeavingInsteadOf = 0;
                }

                #endregion

                if ( requiresWeavingAfter != 0 || requiresWeavingBefore != 0 || requiresWeavingInsteadOf != 0 )
                {
                    // Weaving is required.
                    state.HasChange = true;

                    Trace.CodeWeaver.WriteLine( "Weaving instruction {0}", state.Context.JoinPoint.Instruction );

                    #region  Weave

                    InstructionBlock blockBefore = null,
                                     blockAfter = null,
                                     blockInsteadOf,
                                     blockWeaveBefore,
                                     blockWeaveAfter,
                                     blockWeaveInsteadOf;

                    InstructionBlock currentBlock = state.Context.InstructionBlock;

                    #region Split the current block

                    // Split the sequence
                    InstructionSequence sequenceBefore, sequenceAfter;
                    InstructionSequence sequenceInsteadOf = state.Reader.CurrentInstructionSequence;
                    sequenceInsteadOf.SplitAroundReaderPosition( state.Reader,
                                                                 out sequenceBefore, out sequenceAfter );


                    // Build the BEFORE block.
                    InstructionSequence cursor = sequenceInsteadOf.PreviousSiblingSequence;

                    while ( cursor != null )
                    {
                        if ( blockBefore == null )
                        {
                            blockBefore = methodBody.CreateInstructionBlock();
                            blockBefore.Comment = string.Format(
                                CultureInfo.InvariantCulture,
                                "Before instruction {{{0}}}.", joinPoint.Instruction );
                        }

                        InstructionSequence previousSequence = cursor.PreviousSiblingSequence;
                        blockBefore.MoveInstructionSequence( cursor, NodePosition.Before, null );
                        cursor = previousSequence;
                    }


                    // Build the AFTER block.
                    cursor = sequenceInsteadOf.NextSiblingSequence;

                    while ( cursor != null )
                    {
                        if ( blockAfter == null )
                        {
                            blockAfter = methodBody.CreateInstructionBlock();
                            blockAfter.Comment = string.Format(
                                CultureInfo.InvariantCulture,
                                "After instruction {{{0}}}.", joinPoint.Instruction );
                        }

                        InstructionSequence nextSequence = cursor.NextSiblingSequence;
                        blockAfter.MoveInstructionSequence( cursor, NodePosition.After, null );
                        cursor = nextSequence;
                    }

                    // Build the INSTEAD OF block.
                    if ( requiresWeavingInsteadOf == 0 )
                    {
                        blockInsteadOf = methodBody.CreateInstructionBlock();
                        blockInsteadOf.Comment = string.Format(
                            CultureInfo.InvariantCulture,
                            "Instead of instruction {{{0}}}.", joinPoint.Instruction );
                        blockInsteadOf.MoveInstructionSequence( sequenceInsteadOf, NodePosition.After, null );
                    }
                    else
                    {
                        sequenceInsteadOf.Detach();
                        blockInsteadOf = null;
                    }

                    #endregion

                    #region Restructure the current block

                    // At this point, the initial block should be empty.
                    if ( currentBlock.HasInstructionSequences )
                    {
                        throw new AssertionFailedException( "The current block should not have any " +
                                                            "instruction sequence after splitting." );
                    }

                    state.Reader.LeaveInstructionSequence();


                    InstructionBlock lastBlock, parentBlock;
                    NodePosition nextBlockPosition;
                    if ( currentBlock.HasExceptionHandlers ||
                         currentBlock.HasLocalVariableSymbols ||
                         currentBlock.ParentBlock == null ||
                         currentBlock.ParentExceptionHandler != null )
                    {
                        // The block cannot be removed. Add children to it.
                        lastBlock = null;
                        nextBlockPosition = NodePosition.After;
                        parentBlock = currentBlock;
                    }
                    else
                    {
                        // The block can be removed. Add new blocks at parent level.
                        parentBlock = currentBlock.ParentBlock;
                        lastBlock = currentBlock.PreviousSiblingBlock;
                        nextBlockPosition = lastBlock == null ? NodePosition.Before : NodePosition.After;
                        if ( state.Reader.CurrentInstructionBlock != currentBlock )
                            throw new AssertionFailedException();
                        state.Reader.LeaveInstructionBlock();
                        state.BlockStack.Pop();
                        currentBlock.Detach();
                    }

                    if ( blockBefore != null )
                    {
                        parentBlock.AddChildBlock( blockBefore, nextBlockPosition, lastBlock );
                        lastBlock = blockBefore;
                        nextBlockPosition = NodePosition.After;
                    }

                    if ( requiresWeavingBefore != 0 )
                    {
                        blockWeaveBefore = methodBody.CreateInstructionBlock();
                        blockWeaveBefore.Comment = string.Format(
                            CultureInfo.InvariantCulture,
                            "Advice {0} of {1}.",
                            joinPointKindBefore, joinPoint.Instruction );

                        parentBlock.AddChildBlock( blockWeaveBefore, nextBlockPosition, lastBlock );
                        lastBlock = blockWeaveBefore;
                        nextBlockPosition = NodePosition.After;
                    }
                    else
                    {
                        blockWeaveBefore = null;
                    }

                    if ( requiresWeavingInsteadOf != 0 )
                    {
                        blockWeaveInsteadOf = methodBody.CreateInstructionBlock();
                        blockWeaveInsteadOf.Comment = string.Format(
                            CultureInfo.InvariantCulture,
                            "Advice {0} of {1}.",
                            joinPointKindInsteadOf, joinPoint.Instruction );

                        parentBlock.AddChildBlock( blockWeaveInsteadOf, nextBlockPosition, lastBlock );
                        lastBlock = blockWeaveInsteadOf;
                        nextBlockPosition = NodePosition.After;
                    }
                    else
                    {
                        parentBlock.AddChildBlock( blockInsteadOf, nextBlockPosition, lastBlock );
                        lastBlock = blockInsteadOf;
                        blockWeaveInsteadOf = null;
                        nextBlockPosition = NodePosition.After;
                    }

                    if ( requiresWeavingAfter != 0 )
                    {
                        blockWeaveAfter = methodBody.CreateInstructionBlock();
                        blockWeaveBefore = methodBody.CreateInstructionBlock();
                        blockWeaveBefore.Comment = string.Format(
                            CultureInfo.InvariantCulture,
                            "Advice {0} of {1}.",
                            joinPointKindInsteadOf, joinPoint.Instruction );

                        parentBlock.AddChildBlock( blockWeaveAfter, nextBlockPosition, lastBlock );
                        lastBlock = blockWeaveAfter;
                        nextBlockPosition = NodePosition.After;
                    }
                    else
                    {
                        blockWeaveAfter = null;
                    }

                    if ( blockAfter != null )
                    {
                        parentBlock.AddChildBlock( blockAfter, nextBlockPosition, lastBlock );
                        //lastBlock = blockAfter;
                        //nextBlockPosition = NodePosition.After;
                    }

                    #endregion

                    #region Weave

                    if ( requiresWeavingBefore != 0 )
                    {
                        ExceptionHelper.AssumeNotNull( joinPointsBefore );
                        joinPoint.Position = JoinPointPosition.Before;
                        joinPoint.JoinPointKind = ConvertJoinPointKind( joinPointKindBefore );

                        joinPointsBefore.Weave( state.Context, requiresWeavingBefore, blockWeaveBefore, false );
                    }

                    if ( requiresWeavingInsteadOf != 0 )
                    {
                        ExceptionHelper.AssumeNotNull( joinPointsInsteadOf );

                        joinPoint.Position = JoinPointPosition.InsteadOf;
                        joinPoint.JoinPointKind = ConvertJoinPointKind( joinPointKindInsteadOf );

                        joinPointsInsteadOf.Weave( state.Context, requiresWeavingInsteadOf, blockWeaveInsteadOf, false );
                    }

                    if ( requiresWeavingAfter != 0 )
                    {
                        ExceptionHelper.AssumeNotNull( joinPointsAfter );

                        joinPoint.Position = JoinPointPosition.After;
                        joinPoint.JoinPointKind = ConvertJoinPointKind( joinPointKindAfter );

                        joinPointsAfter.Weave( state.Context, requiresWeavingAfter, blockWeaveAfter, true );
                    }

                    #endregion

                    #region Redirect branch targets

                    // If there is a sequence before the current instruction,
                    // branch targets have already been redirected to this sequence.
                    if ( sequenceBefore == null )
                    {
                        InstructionSequence newFirstSequence;

                        if ( blockWeaveBefore != null )
                        {
                            newFirstSequence = blockWeaveBefore.FindFirstInstructionSequence();
                        }
                        else if ( blockWeaveInsteadOf != null )
                        {
                            newFirstSequence = blockWeaveInsteadOf.FindFirstInstructionSequence();
                        }
                        else
                        {
                            newFirstSequence = sequenceInsteadOf;
                        }

                        if ( newFirstSequence != sequenceInsteadOf && newFirstSequence != null )
                        {
                            if ( sequenceInsteadOf.ParentInstructionBlock != null )
                            {
                                sequenceInsteadOf.Redirect( newFirstSequence );
                            }
                            else
                            {
                                sequenceInsteadOf.Remove( newFirstSequence );
                            }
                        }
                    }

                    #endregion

                    #endregion

                    #region Reposition the cursor

                    InstructionBlock nextBlock;

                    if ( blockAfter != null )
                    {
                        nextBlock = blockAfter;
                    }
                    else if ( blockWeaveAfter != null )
                    {
                        nextBlock = blockWeaveAfter.NextSiblingBlock;
                    }
                    else if ( blockWeaveInsteadOf != null )
                    {
                        nextBlock = blockWeaveInsteadOf.NextSiblingBlock;
                    }
                    else
                    {
                        nextBlock = blockInsteadOf.NextSiblingBlock;
                    }


                    if ( nextBlock == null )
                    {
                        // The block we've woven was at the end of its parent. Exit its parent.
                        state.Context.InstructionBlock = parentBlock;
                        state.Position = Position.AfterBlock;
                    }
                    else
                    {
                        state.Context.InstructionBlock = nextBlock;
                        state.Position = Position.BeforeBlock;
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Processes transitions from the <see cref="Position.BeforeSequence"/>
        /// state in the weaving state machine.
        /// </summary>
        /// <param name="state">Current state.</param>
        private static void ProcessBeforeSequence( WeaverState state )
        {
            state.Reader.EnterInstructionSequence( state.CurrentSequence );
            state.Position = Position.BeforeInstruction;
        }

        /// <summary>
        /// Processes transitions from the <see cref="Position.AfterSequence"/>
        /// state in the weaving state machine.
        /// </summary>
        /// <param name="state">Current state.</param>
        private static void ProcessAfterSequence( WeaverState state )
        {
            state.Reader.LeaveInstructionSequence();
            state.Context.JoinPoint.Instruction = null;

            InstructionSequence nextSequence = state.CurrentSequence.NextSiblingSequence;
            if ( nextSequence != null )
            {
                state.CurrentSequence = nextSequence;
                state.Position = Position.BeforeSequence;
            }
            else
            {
                state.Position = Position.AfterBlock;
            }
        }

        /// <summary>
        /// Processes transitions from the <see cref="Position.BeforeInstruction"/>
        /// state in the weaving state machine.
        /// </summary>
        /// <param name="state">Current state.</param>
        private static void ProcessBeforeInstruction( WeaverState state )
        {
            if ( !state.Reader.ReadInstruction() )
            {
                state.Position = Position.AfterSequence;
            }
            else
            {
                switch ( state.Reader.CurrentInstruction.OpCodeNumber )
                {
                    case OpCodeNumber.Ldfld:
                    case OpCodeNumber.Ldsfld:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeGetField,
                            JoinPointKindIndex.InsteadOfGetField,
                            JoinPointKindIndex.AfterGetField,
                            state, (MetadataDeclaration) state.Reader.CurrentInstruction.FieldOperand );
                        break;

                    case OpCodeNumber.Stfld:
                    case OpCodeNumber.Stsfld:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeSetField,
                            JoinPointKindIndex.InsteadOfSetField,
                            JoinPointKindIndex.AfterSetField,
                            state, (MetadataDeclaration) state.Reader.CurrentInstruction.FieldOperand );
                        break;

                    case OpCodeNumber.Ldflda:
                    case OpCodeNumber.Ldsflda:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeGetFieldAddress,
                            JoinPointKindIndex.InsteadOfGetFieldAddress,
                            JoinPointKindIndex.AfterGetFieldAddress,
                            state, (MetadataDeclaration) state.Reader.CurrentInstruction.FieldOperand );
                        break;


                    case OpCodeNumber.Ldelem:
                    case OpCodeNumber.Ldelem_I:
                    case OpCodeNumber.Ldelem_I1:
                    case OpCodeNumber.Ldelem_I2:
                    case OpCodeNumber.Ldelem_I4:
                    case OpCodeNumber.Ldelem_I8:
                    case OpCodeNumber.Ldelem_R4:
                    case OpCodeNumber.Ldelem_R8:
                    case OpCodeNumber.Ldelem_Ref:
                    case OpCodeNumber.Ldelem_U1:
                    case OpCodeNumber.Ldelem_U2:
                    case OpCodeNumber.Ldelem_U4:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeGetArray,
                            JoinPointKindIndex.InsteadOfGetArray,
                            JoinPointKindIndex.AfterGetArray,
                            state, null );
                        break;

                    case OpCodeNumber.Stelem:
                    case OpCodeNumber.Stelem_I:
                    case OpCodeNumber.Stelem_I1:
                    case OpCodeNumber.Stelem_I2:
                    case OpCodeNumber.Stelem_I4:
                    case OpCodeNumber.Stelem_I8:
                    case OpCodeNumber.Stelem_R4:
                    case OpCodeNumber.Stelem_R8:
                    case OpCodeNumber.Stelem_Ref:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeSetArray,
                            JoinPointKindIndex.InsteadOfSetArray,
                            JoinPointKindIndex.AfterSetArray,
                            state, null );
                        break;

                    case OpCodeNumber.Call:
                    case OpCodeNumber.Callvirt:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeCall,
                            JoinPointKindIndex.InsteadOfCall,
                            JoinPointKindIndex.AfterCall,
                            state, (MetadataDeclaration) state.Reader.CurrentInstruction.MethodOperand );
                        break;

                    case OpCodeNumber.Newobj:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeNewObject,
                            JoinPointKindIndex.InsteadOfNewObject,
                            JoinPointKindIndex.AfterNewObject,
                            state, (MetadataDeclaration) state.Reader.CurrentInstruction.MethodOperand );
                        break;

                    case OpCodeNumber.Throw:
                    case OpCodeNumber.Rethrow:
                        ProcessJoinPoint(
                            JoinPointKindIndex.BeforeThrow,
                            JoinPointKindIndex.None,
                            JoinPointKindIndex.None,
                            state, null );
                        break;

                    case OpCodeNumber.Ldarg:
                    case OpCodeNumber.Ldarg_0:
                    case OpCodeNumber.Ldarg_1:
                    case OpCodeNumber.Ldarg_2:
                    case OpCodeNumber.Ldarg_3:
                    case OpCodeNumber.Ldarg_S:
                        ProcessJoinPoint( JoinPointKindIndex.BeforeLoadArgument,
                                          JoinPointKindIndex.InsteadOfLoadArgument,
                                          JoinPointKindIndex.AfterLoadArgument, state,
                                          null );

                        break;

                    case OpCodeNumber.Starg:
                    case OpCodeNumber.Starg_S:
                        ProcessJoinPoint( JoinPointKindIndex.BeforeStoreArgument,
                                          JoinPointKindIndex.InsteadOfStoreArgument,
                                          JoinPointKindIndex.AfterStoreArgument, state,
                                          null );

                        break;

                    case OpCodeNumber.Ldarga:
                    case OpCodeNumber.Ldarga_S:
                        ProcessJoinPoint( JoinPointKindIndex.BeforeLoadArgumentAddress,
                                          JoinPointKindIndex.InsteadOfLoadArgumentAddress,
                                          JoinPointKindIndex.AfterLoadArgumentAddress, state,
                                          null );

                        break;


                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Processes transitions from the <see cref="Position.BeforeBody"/>
        /// state in the weaving state machine.
        /// </summary>
        /// <param name="state">Current state.</param>
        private static void ProcessBeforeBody( WeaverState state )
        {
            state.Context.InstructionBlock = state.Context.Method.MethodBody.RootInstructionBlock;
            state.Position = Position.BeforeBlock;
        }

        /// <summary>
        /// Processes transitions from the <see cref="Position.AfterBlock"/>
        /// state in the weaving state machine.
        /// </summary>
        /// <param name="state">Current state.</param>
        private static void ProcessAfterBlock( WeaverState state )
        {
            Trace.CodeWeaver.WriteLine( "After Block {0}", state.Context.InstructionBlock );


            if ( !state.SkippedBlocks.Contains( state.Context.InstructionBlock ) )
            {
                ExceptionHelper.Core.Assert( state.Reader.CurrentInstructionBlock == state.Context.InstructionBlock,
                                             "Leaving a block that is not the current one." );

                ExceptionHelper.Core.Assert( state.BlockStack.Peek() == state.Context.InstructionBlock,
                                             "Inconsistent stack of blocks." );

                if ( state.Context.InstructionBlock == state.InitializationBlock )
                {
                    bool isThisInitialized = !state.Context.Method.IsStatic;
                    Trace.CodeWeaver.WriteLine(
                        "state.Context.IsThisInitialized = !state.Context.Method.IsStatic = {0}",
                        state.Context.InstructionBlock, isThisInitialized );
                    state.Context.IsThisInitialized = isThisInitialized;
                }

                state.BlockStack.Pop();
                state.Reader.LeaveInstructionBlock();
                Trace.CodeWeaver.WriteLine( "The reader is now in block {{{0}}}.", state.Reader.CurrentInstructionBlock );
            }
            else
            {
                Trace.CodeWeaver.WriteLine( "  This block was marked for skipping." );
            }

            InstructionBlock nextBlock = state.Context.InstructionBlock.NextSiblingBlock;
            if ( nextBlock != null )
            {
                state.Context.InstructionBlock = nextBlock;
                state.Position = Position.BeforeBlock;
            }
            else if ( state.Context.InstructionBlock.ParentBlock != null )
            {
                state.Context.InstructionBlock = state.Context.InstructionBlock.ParentBlock;
                state.Position = Position.AfterBlock;
            }
            else
            {
                state.Context.InstructionBlock = null;
                state.Position = Position.AfterBody;
            }
        }

        /// <summary>
        /// Processes transitions from the <see cref="Position.BeforeBlock"/>
        /// state in the weaving state machine.
        /// </summary>
        /// <param name="state">Current state.</param>
        private static void ProcessBeforeBlock( WeaverState state )
        {
            Trace.CodeWeaver.WriteLine( "Before Block {0}", state.Context.InstructionBlock );

            if ( !state.SkippedBlocks.Contains( state.Context.InstructionBlock ) )
            {
                if ( ( state.Context.InstructionBlock.ParentBlock == null && state.BlockStack.Count != 0 ) ||
                     state.Context.InstructionBlock.ParentBlock != null &&
                     state.Context.InstructionBlock.ParentBlock != state.BlockStack.Peek() )
                {
                    throw new AssertionFailedException();
                }
                state.BlockStack.Push( state.Context.InstructionBlock );


                if ( state.Context.InstructionBlock == state.InitializationBlock )
                {
                    Trace.CodeWeaver.WriteLine( "state.Context.IsThisInitialized = false" );
                    state.Context.IsThisInitialized = false;
                }

                state.Reader.EnterInstructionBlock( state.Context.InstructionBlock );

                if ( state.Context.InstructionBlock.HasChildrenBlocks )
                {
                    state.Context.InstructionBlock = state.Context.InstructionBlock.FirstChildBlock;
                }
                else if ( state.Context.InstructionBlock.HasInstructionSequences )
                {
                    state.CurrentSequence = state.Context.InstructionBlock.FirstInstructionSequence;
                    state.Position = Position.BeforeSequence;
                }
                else
                {
                    state.Position = Position.AfterBlock;
                }
            }
            else
            {
                Trace.CodeWeaver.WriteLine( "  This block was marked for skipping." );
                state.Position = Position.AfterBlock;
            }
        }

        #endregion

        /// <summary>
        /// Weave the module to which the current <see cref="Weaver"/> is related
        /// using the current set of advices.
        /// </summary>
        /// <see cref="AddMethodLevelAdvice"/>
        /// <see cref="AddTypeLevelAdvice"/>
        public void Weave()
        {
            foreach ( KeyValuePair<IField, AdvicePerKindCollection> pair in this.fieldLevelAdvices )
            {
                // TODO: Support for IField, not only FieldDefDeclaration.
                this.WeaveField( (FieldDefDeclaration) pair.Key, pair.Value );
            }

            foreach ( MethodDefDeclaration method in this.methodLevelAdvices.Keys )
            {
                this.WeaveMethod( method );
            }
        }

        private void WeaveField( FieldDefDeclaration field, AdvicePerKindCollection advices )
        {
            bool changeToProperty = this.fieldsChangedToProperties.Contains( field );

            Trace.CodeWeaver.WriteLine( "Weaving field {{{0}}}, change to property = {1}.", field,
                                        changeToProperty );


            WeavingContext weavingContext = new WeavingContext {Field = field, WeavingHelper = this.weavingHelper};
            weavingContext.JoinPoint.Position = JoinPointPosition.InsteadOf;
            weavingContext.JoinPoint.JoinPointKind = JoinPointKinds.InsteadOfGetField;


            AdviceCollection onGetAdvices = advices.GetAdvices( JoinPointKindIndex.InsteadOfGetField );
            if ( onGetAdvices != null && onGetAdvices.Count > 1 )
            {
                onGetAdvices.Sort();
            }
            int onGetAdviceMask = onGetAdvices == null ? 0 : onGetAdvices.RequiresWeave( weavingContext );

            weavingContext.JoinPoint.JoinPointKind = JoinPointKinds.InsteadOfSetField;
            AdviceCollection onSetAdvices = advices.GetAdvices( JoinPointKindIndex.InsteadOfSetField );
            if ( onSetAdvices != null && onSetAdvices.Count > 1 )
            {
                onSetAdvices.Sort();
            }
            int onSetAdviceMask = onSetAdvices == null ? 0 : onSetAdvices.RequiresWeave( weavingContext );

            bool removeField = RemoveTask.IsMarkedForRemoval( field );
            bool isStatic = ( field.Attributes & FieldAttributes.Static ) != 0;
            string originalFieldName = field.Name;

            // Rename the field now so that the references we create now
            // have proper name.
            if ( changeToProperty )
            {
                field.Name = "~" + field.Name;
                foreach ( FieldRefDeclaration fieldRef in IndexGenericInstancesTask.GetGenericInstances( field ) )
                {
                    fieldRef.Name = field.Name;
                }
            }

            // Remove the InitOnly flag of the field
            field.Attributes &= ~FieldAttributes.InitOnly;

            // Finds the generic instance of the target field.
            IField targetGenericFieldInstance = GenericHelper.GetFieldCanonicalGenericInstance( field );

            // Find get and set methods.

            #region Determine method attributes

            MethodAttributes methodAttributes;
            CallingConvention callingConvention;

            switch ( field.Attributes & FieldAttributes.FieldAccessMask )
            {
                case FieldAttributes.Assembly:
                    methodAttributes = MethodAttributes.Assembly;
                    break;

                case FieldAttributes.FamANDAssem:
                    methodAttributes = MethodAttributes.FamANDAssem;
                    break;

                case FieldAttributes.Family:
                    methodAttributes = MethodAttributes.Family;
                    break;

                case FieldAttributes.FamORAssem:
                    methodAttributes = MethodAttributes.FamORAssem;
                    break;

                case FieldAttributes.Private:
                    methodAttributes = MethodAttributes.Private;
                    break;

                case FieldAttributes.Public:
                    methodAttributes = MethodAttributes.Public;
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException(
                        field.Attributes, "field.Attributes" );
            }

            methodAttributes |= MethodAttributes.HideBySig;

            if ( ( field.Attributes & FieldAttributes.Static ) != 0 )
            {
                methodAttributes |= MethodAttributes.Static;
                callingConvention = CallingConvention.Default;
            }
            else
            {
                callingConvention = CallingConvention.HasThis;
            }

            #endregion

            #region Generate the Get method

            MethodDefDeclaration getMethod;
            bool hasGetAdvice = onGetAdvices != null && onGetAdvices.Count > 0;
            if ( changeToProperty || hasGetAdvice )
            {
                getMethod = new MethodDefDeclaration
                                {
                                    Name = ( ( changeToProperty ? "get_" : "~get~" ) + originalFieldName )
                                };
                if ( ( field.Attributes & FieldAttributes.InitOnly ) != 0 )
                {
                    getMethod.Attributes = ( methodAttributes & ~MethodAttributes.MemberAccessMask )
                                           | MethodAttributes.Private;
                }
                else
                {
                    getMethod.Attributes = methodAttributes;
                }
                getMethod.CallingConvention = callingConvention;

                field.DeclaringType.Methods.Add( getMethod );
                IgnoreMethod( getMethod );
                getMethod.CustomAttributes.Add( this.weavingHelper.GetDebuggerNonUserCodeAttribute() );
                this.weavingHelper.AddCompilerGeneratedAttribute( getMethod.CustomAttributes );

                getMethod.ReturnParameter = new ParameterDeclaration
                                                {
                                                    Attributes = ParameterAttributes.Retval,
                                                    ParameterType = field.FieldType
                                                };
                MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
                getMethod.MethodBody = methodBody;
                methodBody.RootInstructionBlock = methodBody.CreateInstructionBlock();

                LocalVariableSymbol fieldValueLocal = methodBody.RootInstructionBlock.DefineLocalVariable(
                    targetGenericFieldInstance.FieldType, "fieldValue" );

                InstructionBlock entryBlock = methodBody.CreateInstructionBlock();
                InstructionBlock adviceBlock = methodBody.CreateInstructionBlock();
                InstructionBlock exitBlock = methodBody.CreateInstructionBlock();
                methodBody.RootInstructionBlock.AddChildBlock( entryBlock, NodePosition.After, null );
                methodBody.RootInstructionBlock.AddChildBlock( adviceBlock, NodePosition.After, null );
                methodBody.RootInstructionBlock.AddChildBlock( exitBlock, NodePosition.After, null );

                InstructionSequence entryInstructionSequence = methodBody.CreateInstructionSequence();
                entryBlock.AddInstructionSequence( entryInstructionSequence, NodePosition.Before, null );
                instructionWriter.AttachInstructionSequence( entryInstructionSequence );

                if ( !removeField )
                {
                    if ( !isStatic )
                    {
                        instructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        instructionWriter.EmitInstructionField( OpCodeNumber.Ldfld, targetGenericFieldInstance );
                    }
                    else
                    {
                        instructionWriter.EmitInstructionField( OpCodeNumber.Ldsfld, targetGenericFieldInstance );
                    }

                    instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, fieldValueLocal );
                }

                instructionWriter.DetachInstructionSequence();

                InstructionSequence exitSequence = methodBody.CreateInstructionSequence();
                exitBlock.AddInstructionSequence( exitSequence, NodePosition.After, null );
                instructionWriter.AttachInstructionSequence( exitSequence );
                instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, fieldValueLocal );
                instructionWriter.EmitInstruction( OpCodeNumber.Ret );
                instructionWriter.DetachInstructionSequence();

                if ( hasGetAdvice )
                {
                    weavingContext.JoinPoint.JoinPointKind = JoinPointKinds.InsteadOfGetField;
                    weavingContext.JoinPoint.Position = JoinPointPosition.InsteadOf;
                    weavingContext.FieldValueLocal = fieldValueLocal;
                    onGetAdvices.Weave( weavingContext, onGetAdviceMask, adviceBlock, true );
                }
            }
            else
            {
                getMethod = null;
            }

            #endregion

            #region Generate the Set method

            MethodDefDeclaration setMethod;
            bool hasSetAdvice = onSetAdvices != null && onSetAdvices.Count > 0;
            if ( changeToProperty | hasSetAdvice )
            {
                ExceptionHelper.AssumeNotNull( onSetAdvices );

                setMethod = new MethodDefDeclaration
                                {
                                    Name = ( ( changeToProperty ? "set_" : "~set~" ) + originalFieldName ),
                                    Attributes = methodAttributes,
                                    CallingConvention = callingConvention
                                };

                field.DeclaringType.Methods.Add( setMethod );
                IgnoreMethod( setMethod );
                setMethod.CustomAttributes.Add( this.weavingHelper.GetDebuggerNonUserCodeAttribute() );
                this.weavingHelper.AddCompilerGeneratedAttribute( setMethod.CustomAttributes );

                setMethod.ReturnParameter = new ParameterDeclaration
                                                {
                                                    Attributes = ParameterAttributes.Retval,
                                                    ParameterType = module.Cache.GetIntrinsic( IntrinsicType.Void )
                                                };
                ParameterDeclaration valueParameter = new ParameterDeclaration
                                                          {
                                                              Name = "value",
                                                              ParameterType = field.FieldType,
                                                              Ordinal = 0
                                                          };
                setMethod.Parameters.Add( valueParameter );
                MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
                setMethod.MethodBody = methodBody;
                methodBody.RootInstructionBlock = methodBody.CreateInstructionBlock();

                LocalVariableSymbol fieldValueLocal = methodBody.RootInstructionBlock.DefineLocalVariable(
                    targetGenericFieldInstance.FieldType, "fieldValue" );

                InstructionBlock entryBlock = methodBody.CreateInstructionBlock();
                InstructionBlock adviceBlock = methodBody.CreateInstructionBlock();
                InstructionBlock exitBlock = methodBody.CreateInstructionBlock();
                methodBody.RootInstructionBlock.AddChildBlock( entryBlock, NodePosition.After, null );
                methodBody.RootInstructionBlock.AddChildBlock( adviceBlock, NodePosition.After, null );
                methodBody.RootInstructionBlock.AddChildBlock( exitBlock, NodePosition.After, null );


                if ( !removeField )
                {
                    InstructionSequence entryInstructionSequence = methodBody.CreateInstructionSequence();
                    entryBlock.AddInstructionSequence( entryInstructionSequence, NodePosition.Before, null );
                    instructionWriter.AttachInstructionSequence( entryInstructionSequence );

                    if ( isStatic )
                    {
                        instructionWriter.EmitInstructionField( OpCodeNumber.Ldsfld, targetGenericFieldInstance );
                    }
                    else
                    {
                        instructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        instructionWriter.EmitInstructionField( OpCodeNumber.Ldfld, targetGenericFieldInstance );
                    }

                    instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, fieldValueLocal );
                    instructionWriter.DetachInstructionSequence();
                }


                InstructionSequence exitSequence = methodBody.CreateInstructionSequence();
                exitBlock.AddInstructionSequence( exitSequence, NodePosition.After, null );
                instructionWriter.AttachInstructionSequence( exitSequence );

                if ( !removeField )
                {
                    if ( !isStatic )
                    {
                        instructionWriter.EmitInstruction( OpCodeNumber.Ldarg_0 );
                        instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, fieldValueLocal );
                        instructionWriter.EmitInstructionField( OpCodeNumber.Stfld, targetGenericFieldInstance );
                    }
                    else
                    {
                        instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, fieldValueLocal );
                        instructionWriter.EmitInstructionField( OpCodeNumber.Stsfld, targetGenericFieldInstance );
                    }
                }

                instructionWriter.EmitInstruction( OpCodeNumber.Ret );
                instructionWriter.DetachInstructionSequence();

                weavingContext.JoinPoint.JoinPointKind = JoinPointKinds.InsteadOfSetField;
                weavingContext.JoinPoint.Position = JoinPointPosition.InsteadOf;
                weavingContext.FieldValueLocal = fieldValueLocal;
                onSetAdvices.Weave( weavingContext, onSetAdviceMask, adviceBlock, false );
            }
            else
            {
                setMethod = null;
            }

            #endregion

            this.AddMethodLevelAdvice( new FieldLevelAdvice( field, getMethod, setMethod, removeField ), null,
                                       JoinPointKinds.InsteadOfGetField | JoinPointKinds.InsteadOfSetField |
                                       JoinPointKinds.InsteadOfGetFieldAddress,
                                       new Singleton<MetadataDeclaration>( field ) );

            if ( changeToProperty )
            {
                PropertyDeclaration property = new PropertyDeclaration {Name = originalFieldName};
                field.Attributes = ( ( field.Attributes & ~FieldAttributes.FieldAccessMask ) & ~FieldAttributes.InitOnly ) |
                                   FieldAttributes.Private;
                field.DeclaringType.Properties.Add( property );

                property.PropertyType = field.FieldType;

                if ( getMethod != null )
                {
                    property.Members.Add( new MethodSemanticDeclaration( MethodSemantics.Getter, getMethod ) );
                }

                if ( setMethod != null && ( field.Attributes & FieldAttributes.InitOnly ) == 0 )
                {
                    property.Members.Add( new MethodSemanticDeclaration( MethodSemantics.Setter, setMethod ) );
                }

                // Move custom attributes from the field to the property.
                if ( field.CustomAttributes.Count > 0 )
                {
                    ImplementationBoundAttributesTask implementationBoundAttributesTask =
                        ImplementationBoundAttributesTask.GetTask( this.project );

                    List<CustomAttributeDeclaration> moveableAttributes =
                        new List<CustomAttributeDeclaration>( field.CustomAttributes.Count );
                    foreach ( CustomAttributeDeclaration customAttribute in field.CustomAttributes )
                    {
                        if (
                            !implementationBoundAttributesTask.IsImplementationBound(
                                 customAttribute.Constructor.DeclaringType ) )
                            moveableAttributes.Add( customAttribute );
                    }

                    foreach ( CustomAttributeDeclaration customAttribute in moveableAttributes )
                    {
                        field.CustomAttributes.Remove( customAttribute );
                        property.CustomAttributes.Add( customAttribute );
                    }
                }
            }
        }

        /// <summary>
        /// Weave a method.
        /// </summary>
        /// <param name="method">The method to weave.</param>
        private void WeaveMethod( MethodDefDeclaration method )
        {
            Trace.CodeWeaver.WriteLine( "Weaving method {{{0}}}.", method );

            if ( !method.HasBody )
            {
                Trace.CodeWeaver.WriteLine( "Skipping this method because it has no body." );
                return;
            }

            // Test whether the method is tagged as ignored. If yes, skip it.
            object ignore = method.GetTag( ignoreMethodTag );
            if ( ignore != null && (bool) ignore )
            {
                Trace.CodeWeaver.WriteLine( "Skipping this method because it has the 'IgnoreMethodTag' tag." );
            }

            MethodLevelAdvices advices;
            if ( !this.methodLevelAdvices.TryGetValue( method, out advices ) )
            {
                // Return immediately if nothing is required.
                Trace.CodeWeaver.WriteLine( "Skipping this method because it has no advice." );
                return;
            }

            // Sort the advices.
            advices.MergeAndSort();


            // Build a state object.
            using ( WeaverState state = new WeaverState( method, advices ) )
            {
                state.Context.WeavingHelper = this.weavingHelper;
                state.Context.Method = method;
                state.Context.InstructionReader = state.Reader;

                #region Get weaving requirements

                int beforeInstanceConstructorMask = 0;
                AdviceCollection beforeInstanceConstructorAdvices;
                bool isContructor = method.Name == ".ctor";
                if ( isContructor )
                {
                    beforeInstanceConstructorAdvices =
                        state.Advices.GetAdvices( JoinPointKindIndex.BeforeInstanceConstructor, null );
                    
                    if (!CheckAdviceCount(beforeInstanceConstructorAdvices, state.Context.Method))
                        return;

                    if ( beforeInstanceConstructorAdvices != null )
                    {
                        state.Context.JoinPoint.Position = JoinPointPosition.Before;
                        state.Context.JoinPoint.JoinPointKind = JoinPointKinds.BeforeInstanceConstructor;

                        beforeInstanceConstructorMask = beforeInstanceConstructorAdvices.RequiresWeave( state.Context );
                    }
                }
                else
                {
                    beforeInstanceConstructorAdvices = null;
                }

                int afterMethodBodyAlwaysMask;
                int afterMethodBodyExceptionMask;
                int afterMethodBodySuccessMask;
                int afterInstanceInitializationMask;
                int beforeMethodBodyMask;

                AdviceCollection aroundMethodBodyAdvices =
                    state.Advices.GetAdvices( JoinPointKindIndex.AfterMethodBodyAlways, null );

                if (!CheckAdviceCount(aroundMethodBodyAdvices, state.Context.Method))
                    return;


                if ( aroundMethodBodyAdvices != null )
                {
                    state.Context.JoinPoint.Position = JoinPointPosition.After;

                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.BeforeMethodBody;
                    beforeMethodBodyMask = aroundMethodBodyAdvices.RequiresWeave( state.Context );

                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterMethodBodyAlways;
                    afterMethodBodyAlwaysMask = aroundMethodBodyAdvices.RequiresWeave( state.Context );

                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterMethodBodyException;
                    afterMethodBodyExceptionMask = aroundMethodBodyAdvices.RequiresWeave( state.Context );

                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterMethodBodySuccess;
                    afterMethodBodySuccessMask = aroundMethodBodyAdvices.RequiresWeave( state.Context );

                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterInstanceInitialization;
                    afterInstanceInitializationMask = aroundMethodBodyAdvices.RequiresWeave(state.Context);
                }
                else
                {
                    afterMethodBodyAlwaysMask = 0;
                    afterMethodBodyExceptionMask = 0;
                    afterMethodBodySuccessMask = 0;
                    beforeMethodBodyMask = 0;
                    afterInstanceInitializationMask = 0;
                }

                #endregion

                #region Weave 'around method body' advices

                // Prepare the structure of the method body for AfterMethodBody/BeforeMethodBody/AfterInstanceInitialization advices.

                if ( afterMethodBodyExceptionMask != 0 ||
                     afterMethodBodyAlwaysMask != 0 ||
                     beforeMethodBodyMask != 0 ||
                     afterMethodBodySuccessMask != 0 ||
                     beforeInstanceConstructorMask != 0 ||
                     isContructor )
                {
                    MethodBodyDeclaration methodBody = method.MethodBody;

                    state.HasChange = true;

                    MethodBodyRestructurerOptions restructureOptions = MethodBodyRestructurerOptions.None;
                    if ( afterMethodBodyExceptionMask != 0 ||
                         afterMethodBodyAlwaysMask != 0 ||
                         afterMethodBodySuccessMask != 0 )
                    {
                        restructureOptions |= MethodBodyRestructurerOptions.ChangeReturnInstructions;
                    }


                    // Restructure the method body.
                    MethodBodyRestructurer methodBodyRestructurer = new MethodBodyRestructurer( method,
                                                                                                restructureOptions,
                                                                                                this.weavingHelper );

                    methodBodyRestructurer.Restructure( this.instructionWriter );

                    state.InitializationBlock = methodBodyRestructurer.InitializationBlock;
                    state.Context.ReturnValueVariable = methodBodyRestructurer.ReturnValueVariable;
                    InstructionBlock userEntryBlock = methodBodyRestructurer.EntryBlock;


                    // Prepare instruction sequence targets for the chain of advices.
                    InstructionSequence currentLeaveBranchTarget = methodBodyRestructurer.ReturnBranchTarget;
                    InstructionSequence nextLeaveBranchTarget = method.MethodBody.CreateInstructionSequence();


                    // Apply BeforeInstanceConstructor advice.
                    if ( beforeInstanceConstructorMask != 0  )
                    {
                        ExceptionHelper.AssumeNotNull( beforeInstanceConstructorAdvices );

                        state.Context.JoinPoint.Position = JoinPointPosition.Before;
                        state.Context.JoinPoint.JoinPointKind = JoinPointKinds.BeforeInstanceConstructor;
                        beforeInstanceConstructorAdvices.Weave( state.Context, beforeInstanceConstructorMask,
                                                                userEntryBlock, false );
                        state.SkippedBlocks.Add( userEntryBlock );
                    }

                    // Apply AfterInitialization advices.
                    if ( afterInstanceInitializationMask != 0 && methodBodyRestructurer.ConstructorType == ConstructorType.CallBase )
                    {
                        state.Context.JoinPoint.Position = JoinPointPosition.After;
                        state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterInstanceInitialization;

                        
                        InstructionBlock afterInstanceInitializationBlock = methodBody.CreateInstructionBlock();
                        methodBodyRestructurer.PrincipalBlock.AddChildBlock( afterInstanceInitializationBlock, NodePosition.Before, null );

                        aroundMethodBodyAdvices.Weave(state.Context, afterInstanceInitializationMask, afterInstanceInitializationBlock, false);
                    }


                    if ( afterMethodBodyExceptionMask != 0 ||
                         afterMethodBodyAlwaysMask != 0 ||
                         afterMethodBodySuccessMask != 0 ||
                         beforeMethodBodyMask != 0 )
                    {
                        int currentBeforeMethodBodyMask = beforeMethodBodyMask;
                        int currentAfterMethodBodyExceptionMask = afterMethodBodyExceptionMask;
                        int currentAfterMethodBodyAlwaysMask = afterMethodBodyAlwaysMask;
                        int currentAfterMethodBodySuccessMask = afterMethodBodySuccessMask;
                        int currentIndex = 0;
                        InstructionBlock currentProtectedBlock = methodBodyRestructurer.PrincipalBlock;
                        ITypeSignature[] catchTypes = new IType[1];

                        // Add nested exception handlers.
                        while ( (currentBeforeMethodBodyMask != 0 ||
                                currentAfterMethodBodyAlwaysMask != 0 ||
                                currentAfterMethodBodyExceptionMask != 0 ||
                                currentAfterMethodBodySuccessMask != 0) &&
                                currentIndex < aroundMethodBodyAdvices.Count)
                        {
                            // Get the exception type.
                            IAdvice currentAdvice = aroundMethodBodyAdvices[currentIndex].Advice;

                            bool hasBefore = ( currentBeforeMethodBodyMask & 1 ) != 0;
                            bool hasFinally = ( currentAfterMethodBodyAlwaysMask & 1 ) != 0;
                            bool hasCatch = ( currentAfterMethodBodyExceptionMask & 1 ) != 0;
                            bool hasOnSuccess = ( currentAfterMethodBodySuccessMask & 1 ) != 0;

                            state.Context.LeaveBranchTarget = nextLeaveBranchTarget;
                            state.Context.OnSuccessBranchTarget = currentLeaveBranchTarget;


                            if ( hasBefore )
                            {
                                if ( currentProtectedBlock.HasInstructionSequences )
                                {
                                    currentProtectedBlock = currentProtectedBlock.Nest();
                                }

                                InstructionBlock beforeBlock = methodBody.CreateInstructionBlock();
                                currentProtectedBlock.AddChildBlock( beforeBlock, NodePosition.Before, null );

                                state.Context.JoinPoint.JoinPointKind = JoinPointKinds.BeforeMethodBody;
                                currentAdvice.Weave( state.Context, beforeBlock );
                                state.SkippedBlocks.Add( beforeBlock );
                            }

                            // Consider the current bit.
                            if ( hasFinally || hasCatch || hasOnSuccess )
                            {
                                // Set the context to the exception handler.
                                state.Context.JoinPoint.Position = JoinPointPosition.After;


                                InstructionBlock newProtectedBlock;
                                InstructionBlock[] catchBlocks;
                                InstructionBlock finallyBlock;

                                ITypedExceptionAdvice typedExceptionAdvice = currentAdvice as ITypedExceptionAdvice;

                                #region Add the OnSuccess block.

                                if ( hasOnSuccess )
                                {
                                    if ( currentProtectedBlock.HasInstructionSequences )
                                    {
                                        currentProtectedBlock = currentProtectedBlock.Nest();
                                    }

                                    InstructionBlock onSuccessBlock = methodBody.CreateInstructionBlock();
                                    onSuccessBlock.Comment = "On Success Block";
                                    currentProtectedBlock.AddChildBlock( onSuccessBlock, NodePosition.After, null );

                                    InstructionBlock onSuccessLabelBlock = methodBody.CreateInstructionBlock();
                                    onSuccessLabelBlock.Comment = "On Success Label Block";
                                    onSuccessBlock.AddChildBlock( onSuccessLabelBlock, NodePosition.After, null );
                                    onSuccessLabelBlock.AddInstructionSequence( currentLeaveBranchTarget,
                                                                                NodePosition.After, null );
                                    InstructionBlock onSuccessUserBlock = methodBody.CreateInstructionBlock();

                                    onSuccessUserBlock.Comment = "On Success User Block";
                                    onSuccessBlock.AddChildBlock( onSuccessUserBlock, NodePosition.After, null );

                                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterMethodBodySuccess;
                                    currentAdvice.Weave( state.Context, onSuccessUserBlock );

                                    InstructionBlock onSuccessLeaveBlock = methodBody.CreateInstructionBlock();
                                    onSuccessLeaveBlock.Comment = "On Success Leave Block";
                                    onSuccessBlock.AddChildBlock( onSuccessLeaveBlock, NodePosition.After, null );

                                    InstructionSequence onSuccessLeaveSequence =
                                        method.MethodBody.CreateInstructionSequence();
                                    onSuccessLeaveBlock.AddInstructionSequence( onSuccessLeaveSequence,
                                                                                NodePosition.After, null );
                                    instructionWriter.AttachInstructionSequence( onSuccessLeaveSequence );
                                    instructionWriter.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );
                                    instructionWriter.EmitBranchingInstruction( OpCodeNumber.Leave,
                                                                                nextLeaveBranchTarget );
                                    instructionWriter.DetachInstructionSequence();

                                    state.SkippedBlocks.Add( onSuccessUserBlock );
                                    state.SkippedBlocks.Add( onSuccessLeaveBlock );
                                }
                                else
                                {
                                    InstructionBlock leaveBlock;

                                    if ( currentProtectedBlock.HasInstructionSequences )
                                    {
                                        leaveBlock = currentProtectedBlock;
                                    }
                                    else
                                    {
                                        leaveBlock = methodBody.CreateInstructionBlock();
                                        currentProtectedBlock.AddChildBlock( leaveBlock, NodePosition.After, null );
                                        leaveBlock.Comment = "Leave Block";
                                    }

                                    leaveBlock.AddInstructionSequence( currentLeaveBranchTarget, NodePosition.After,
                                                                       null );
                                    instructionWriter.AttachInstructionSequence( currentLeaveBranchTarget );
                                    instructionWriter.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );
                                    instructionWriter.EmitBranchingInstruction( OpCodeNumber.Leave,
                                                                                nextLeaveBranchTarget );
                                    instructionWriter.DetachInstructionSequence();
                                }

                                #endregion

                                #region Catch and Finally

                                if ( hasCatch )
                                {
                                    catchTypes[0] = null;
                                    if ( typedExceptionAdvice != null )
                                    {
                                        catchTypes[0] = typedExceptionAdvice.GetExceptionType( state.Context );
                                    }

                                    if ( catchTypes[0] == null )
                                    {
                                        catchTypes[0] = this.module.Cache.GetType( "System.Exception, mscorlib" );
                                    }
                                }

                                // Add the current exception handlers.
                                this.weavingHelper.AddExceptionHandlers(
                                    this.instructionWriter,
                                    currentProtectedBlock,
                                    nextLeaveBranchTarget,
                                    hasCatch ? catchTypes : null,
                                    hasFinally,
                                    out newProtectedBlock,
                                    out catchBlocks,
                                    out finallyBlock );


                                // Weave
                                if ( hasCatch )
                                {
                                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterMethodBodyException;
                                    state.SkippedBlocks.Add( catchBlocks[0] );
                                    currentAdvice.Weave( state.Context, catchBlocks[0] );
                                }

                                if ( hasFinally )
                                {
                                    state.Context.JoinPoint.JoinPointKind = JoinPointKinds.AfterMethodBodyAlways;
                                    state.SkippedBlocks.Add( finallyBlock );
                                    currentAdvice.Weave( state.Context, finallyBlock );
                                }

                                #endregion

                                currentProtectedBlock = newProtectedBlock;
                            } // if (hasFinally || hasCatch || hasOnSuccess)

                            // Go to the next bit.
                            currentBeforeMethodBodyMask = currentBeforeMethodBodyMask >> 1;
                            currentAfterMethodBodyAlwaysMask = currentAfterMethodBodyAlwaysMask >> 1;
                            currentAfterMethodBodyExceptionMask = currentAfterMethodBodyExceptionMask >> 1;
                            currentAfterMethodBodySuccessMask = currentAfterMethodBodySuccessMask >> 1;
                            if ( hasFinally || hasCatch || hasOnSuccess )
                            {
                                currentLeaveBranchTarget = nextLeaveBranchTarget;
                                nextLeaveBranchTarget = method.MethodBody.CreateInstructionSequence();
                            }
                            currentIndex++;
                        } // while 
                    }
                    // if (afterMethodBodyExceptionMask != 0 || afterMethodBodyAlwaysMask != 0 || afterMethodBodySuccessMask != 0)

                    // Create the last block with the return instruction.
                    if ( ( restructureOptions & MethodBodyRestructurerOptions.ChangeReturnInstructions ) != 0 )
                    {
                        InstructionBlock returnBlock = methodBody.CreateInstructionBlock();
                        returnBlock.Comment = "Return Block";
                        method.MethodBody.RootInstructionBlock.AddChildBlock( returnBlock, NodePosition.After, null );

                        if ( currentLeaveBranchTarget.ParentInstructionBlock == null )
                        {
                            returnBlock.AddInstructionSequence( currentLeaveBranchTarget, NodePosition.After, null );
                        }
                        returnBlock.AddInstructionSequence( nextLeaveBranchTarget, NodePosition.After, null );
                        instructionWriter.AttachInstructionSequence( currentLeaveBranchTarget );
                        if ( methodBodyRestructurer.ReturnValueVariable != null )
                        {
                            instructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc,
                                                                            methodBodyRestructurer.ReturnValueVariable );
                        }
                        instructionWriter.EmitInstruction( OpCodeNumber.Ret );
                        instructionWriter.DetachInstructionSequence();
                    }
                }
                // if (afterMethodBodyExceptionMask != 0 || afterMethodBodyAlwaysMask != 0 || beforeMethodBodyMask != 0 || afterMethodBodySuccessMask != 0)

                #endregion

                // Process instruction-level advices.
                state.Context.IsThisInitialized = !state.Context.Method.IsStatic;
                Process( state );

                // Mark the MaxStack to be recomputed.
                if ( state.HasChange )
                {
                    method.MethodBody.MaxStack = MethodBodyDeclaration.RecomputeMaxStack;
                }
            }
        }

        private static bool CheckAdviceCount(AdviceCollection advices, MethodDefDeclaration method)
        {
            if (advices != null && advices.Count > 32)
            {
                CoreMessageSource.Instance.Write(SeverityType.Error, "PS0110", new object[] {method});
                return false;
            }

            return true;
        }

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.instructionWriter != null )
            {
                this.instructionWriter.Dispose();
                this.instructionWriter = null;
            }
        }

        #endregion

        private class MethodLevelAdvices
        {
            private AdvicePerKindCollection operandLessAdvices;
            private Dictionary<MetadataToken, AdvicePerKindCollection> operandAdvices;

            public void Add( IAdvice advice, JoinPointKinds joinPointKinds, MetadataDeclaration operand )
            {
                AdvicePerKindCollection adviceCollection;

                if ( operand == null )
                {
                    if ( this.operandLessAdvices == null )
                    {
                        this.operandLessAdvices = new AdvicePerKindCollection();
                    }
                    adviceCollection = this.operandLessAdvices;
                }
                else
                {
                    if ( this.operandAdvices == null )
                    {
                        this.operandAdvices = new Dictionary<MetadataToken, AdvicePerKindCollection>();
                        adviceCollection = new AdvicePerKindCollection();
                        this.operandAdvices.Add( operand.MetadataToken, adviceCollection );
                    }
                    else if ( !this.operandAdvices.TryGetValue( operand.MetadataToken, out adviceCollection ) )
                    {
                        adviceCollection = new AdvicePerKindCollection();
                        this.operandAdvices.Add( operand.MetadataToken, adviceCollection );
                    }
                }

                adviceCollection.Add( advice, joinPointKinds );
            }

            public void MergeAndSort()
            {
                // We have now to merge operand-less advices into advices with operands,
                // then to sort all collections.
                if ( this.operandAdvices != null )
                {
                    foreach ( KeyValuePair<MetadataToken, AdvicePerKindCollection> pair in this.operandAdvices )
                    {
                        pair.Value.MergeAndSort( this.operandLessAdvices );
                    }
                }

                if ( this.operandLessAdvices != null )
                {
                    this.operandLessAdvices.MergeAndSort( null );
                }
            }

            public AdviceCollection GetAdvices( JoinPointKindIndex joinPointKind, MetadataDeclaration operand )
            {
                AdvicePerKindCollection adviceCollection;

                if ( joinPointKind == JoinPointKindIndex.None )
                {
                    return null;
                }

                if ( operand != null )
                {
                    if ( this.operandAdvices == null ||
                         !this.operandAdvices.TryGetValue( operand.MetadataToken, out adviceCollection ) )
                    {
                        adviceCollection = this.operandLessAdvices;
                    }
                }
                else
                {
                    adviceCollection = this.operandLessAdvices;
                }

                if ( adviceCollection != null )
                {
                    return adviceCollection.GetAdvices( joinPointKind );
                }
                else
                {
                    return null;
                }
            }
        }


        private class FieldLevelAdvice : IAdvice
        {
            private readonly MethodDefDeclaration getMethod;
            private readonly MethodDefDeclaration setMethod;
            private readonly FieldDefDeclaration targetField;
            private readonly bool remove;
            private OpCodeNumber nextOpcode;

            public FieldLevelAdvice(
                FieldDefDeclaration targetField,
                MethodDefDeclaration getMethod,
                MethodDefDeclaration setMethod,
                bool remove )
            {
                this.targetField = targetField;
                this.getMethod = getMethod;
                this.setMethod = setMethod;
                this.remove = remove;
            }

            #region IAdvice Members

            public int Priority
            {
                get { return int.MinValue; }
            }

            public bool RequiresWeave( WeavingContext context )
            {
                if ( context.JoinPoint.JoinPointKind == JoinPointKinds.InsteadOfGetFieldAddress )
                {
                    // We should look at the next instruction: if it is an "initobj",
                    // the compiler tried to assign the value-type field to its zero
                    // value. This happens with nullable value types (Nullable<>).
                    InstructionReaderBookmark bookmark = context.InstructionReader.CreateBookmark();
                    if ( context.InstructionReader.ReadInstruction() )
                    {
                        this.nextOpcode = context.InstructionReader.CurrentInstruction.OpCodeNumber;

                        if ( this.nextOpcode == OpCodeNumber.Initobj )
                        {
                            context.InstructionReader.GoToBookmark( bookmark );
                            return true;
                        }
                            // This is a dirty trick to better support value types in the System namespace.
                        else if ( this.nextOpcode == OpCodeNumber.Call || this.nextOpcode == OpCodeNumber.Callvirt )
                        {
                            IMethod method = context.InstructionReader.CurrentInstruction.MethodOperand;
                            MethodDefDeclaration methodDef = method.GetMethodDefinition();

                            int dot = 0;
                            string typeName = methodDef.DeclaringType.Name;

                            if ( methodDef.Module.IsMscorlib && ( dot = typeName.LastIndexOf( '.' ) ) > 0 &&
                                 typeName.Substring(
                                     0, dot ) == "System" )
                            {
                                context.InstructionReader.GoToBookmark( bookmark );
                                return true;
                            }

                            if ( dot > 0 )
                                Console.WriteLine( typeName.Substring(
                                                       0, dot ) );
                        }
                    }
                    context.InstructionReader.GoToBookmark( bookmark );


                    CoreMessageSource.Instance.Write(
                        this.remove
                            ?
                                SeverityType.Error
                            : SeverityType.Warning,
                        this.remove ? "PS0088" : "PS0089", new object[]
                                                               {
                                                                   this.targetField.ToString(),
                                                                   context.Method.ToString()
                                                               },
                        context.JoinPoint.Instruction.LastSymbolSequencePoint );
                    return false;
                }
                else
                {
                    return true;
                }
            }

            private bool TestThisInitialization( WeavingContext context )
            {
                // Require that the this pointer is initialized (unless the field is static).
                if ( !this.setMethod.IsStatic )
                {
                    if ( !context.IsThisInitialized && context.Method.Name == ".ctor" )
                    {
                        SymbolSequencePoint sequencePoint = context.JoinPoint.Instruction.LastSymbolSequencePoint;

                        if ( this.remove )
                        {
                            // Write an error message.
                            CoreMessageSource.Instance.Write(
                                SeverityType.Error, "PS0070",
                                new object[]
                                    {
                                        this.targetField.DeclaringType.Name,
                                        this.targetField.Name
                                    },
                                sequencePoint
                                );
                        }
                        else
                        {
                            CoreMessageSource.Instance.Write(
                                SeverityType.Warning, "PS0069",
                                new object[]
                                    {
                                        this.targetField.DeclaringType.Name,
                                        this.targetField.Name
                                    },
                                sequencePoint );
                        }

                        return false;
                    }
                }

                return true;
            }

            public void Weave( WeavingContext context, InstructionBlock block )
            {
                InstructionSequence sequence = block.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence( sequence, NodePosition.After, null );
                IField field = context.JoinPoint.Instruction.FieldOperand;

                context.InstructionWriter.AttachInstructionSequence( sequence );
                context.InstructionWriter.EmitSymbolSequencePoint( context.JoinPoint.Instruction.SymbolSequencePoint );


                switch ( context.JoinPoint.Instruction.OpCodeNumber )
                {
                    case OpCodeNumber.Ldfld:
                    case OpCodeNumber.Ldsfld:
                        {
                            IMethod method =
                                field.DeclaringType.Methods.GetMethod( this.getMethod.Name, this.getMethod,
                                                                       BindingOptions.Default );
                            context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, method );
                        }
                        break;


                    case OpCodeNumber.Stfld:
                    case OpCodeNumber.Stsfld:
                        {
                            if ( !this.TestThisInitialization( context ) )
                            {
                                // Cannot weave because the 'this' pointer is not yet initialized.
                                // Emit the original instruction.
                                context.InstructionWriter.EmitSymbolSequencePoint(
                                    context.JoinPoint.Instruction.SymbolSequencePoint );
                                context.InstructionWriter.EmitInstructionField( OpCodeNumber.Stfld, field );
                                context.InstructionWriter.DetachInstructionSequence();
                                return;
                            }

                            IMethod method =
                                field.DeclaringType.Methods.GetMethod( this.setMethod.Name, this.setMethod,
                                                                       BindingOptions.Default );
                            context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, method );
                        }
                        break;

                    case OpCodeNumber.Ldflda:
                    case OpCodeNumber.Ldsflda:
                        {
                            // Define a new local variable and initializes it.
                            context.Method.MethodBody.InitLocalVariables = true;
                            LocalVariableSymbol variable = block.DefineLocalVariable( field.FieldType, "~tmp~{0}" );
                            if ( this.nextOpcode == OpCodeNumber.Initobj )
                            {
                                // If we are here, this is because the next instruction is 'initobj'.
                                // We have to use alternative instructions to reset the object.

                                context.InstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, variable );

                                context.InstructionWriter.EmitInstructionType( OpCodeNumber.Initobj, field.FieldType );
                                context.InstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, variable );
                                IMethod method =
                                    field.DeclaringType.Methods.GetMethod( this.setMethod.Name, this.setMethod,
                                                                           BindingOptions.Default );
                                context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, method );

                                // Now we load again the address of the tmp variable on the stack,
                                // It's useless but we have to put a dummy address on the stack for
                                // the next "initobj" instruction.
                                context.InstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, variable );
                            }
                            else
                            {
                                // Here we allow instance calls to types of the System namespace.
                                IMethod method =
                                    field.DeclaringType.Methods.GetMethod( this.getMethod.Name, this.getMethod,
                                                                           BindingOptions.Default );
                                context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, method );
                                context.InstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Stloc, variable );
                                context.InstructionWriter.EmitInstructionLocalVariable( OpCodeNumber.Ldloca, variable );
                            }
                        }
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException(
                            context.JoinPoint.Instruction.OpCodeNumber,
                            "context.JoinPoint.Instruction.OpCodeNumber" );
                }

                context.InstructionWriter.DetachInstructionSequence();
            }

            #endregion
        }
    }
}