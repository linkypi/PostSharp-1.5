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
using PostSharp.Collections;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Tasks that references the uses of fields and methods by methods.
    /// </summary>
    /// <remarks>
    /// Note that this task only indexes references found in IL instructions, not
    /// in metadata tables.
    /// </remarks>
    public sealed class IndexUsagesTask : Task
    {
        private static readonly Guid usesTag = new Guid( "{6D19CACD-56AD-4496-8771-F1E2FCCDEBFC}" );
        private static readonly Guid usedByTag = new Guid( "{5F319300-3488-4a41-9A2C-0412A7CF1954}" );

        /// <inheritdoc />
        public static bool Execute( Project project )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( project, "project" );

            #endregion

            IndexUsagesTask instance = project.Tasks["IndexUsages"] as IndexUsagesTask;

            ExceptionHelper.Core.AssertValidOperation( instance != null, "CannotFindTask" );

            return instance.Execute();
        }

        private static void InspectSequence(
            MethodDefDeclaration methodDef,
            InstructionReader reader,
            Set<MetadataToken> processedOperands,
            List<MetadataDeclaration> uses
            )
        {
            while ( reader.ReadInstruction() )
            {
                MetadataDeclaration operand = null;

                switch ( reader.CurrentInstruction.OperandType )
                {
                    case OperandType.InlineField:
                        operand = (MetadataDeclaration) reader.CurrentInstruction.FieldOperand;
                        break;

                    case OperandType.InlineMethod:
                        operand = (MetadataDeclaration) reader.CurrentInstruction.MethodOperand;
                        break;

                    case OperandType.InlineType:
                        operand = (MetadataDeclaration) reader.CurrentInstruction.TypeOperand;
                        break;
                }

                if ( operand != null && !processedOperands.Contains( operand.MetadataToken ) )
                {
                    Trace.IndexUsagesTask.WriteLine( "Found a reference to {{{0}}}.", operand );

                    processedOperands.Add( operand.MetadataToken );
                    uses.Add( operand );

                    List<MethodDefDeclaration> usedBy =
                        (List<MethodDefDeclaration>) operand.GetTag( usedByTag );

                    if ( usedBy == null )
                    {
                        usedBy = new List<MethodDefDeclaration>();
                        operand.SetTag( usedByTag, usedBy );
                    }

                    usedBy.Add( methodDef );
                }
            }
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            // Clear the existing tags, because sometimes this task is executed many times.
            this.Project.Module.ClearAllTags( usesTag );
            this.Project.Module.ClearAllTags( usedByTag );

            IEnumerator<MetadataDeclaration> methodDefs =
                this.Project.Module.GetDeclarationEnumerator( TokenType.MethodDef );


            while ( methodDefs.MoveNext() )
            {
                MethodDefDeclaration methodDef = (MethodDefDeclaration) methodDefs.Current;

                Trace.IndexUsagesTask.WriteLine( "Inspecting method {{{0}}}.", methodDef );

                List<MetadataDeclaration> uses = new List<MetadataDeclaration>();
                Set<MetadataToken> processedOperands = new Set<MetadataToken>();


                if ( methodDef.HasBody )
                {
                    if ( methodDef.MethodBody.IsModified )
                    {
                        InstructionReader reader = methodDef.MethodBody.CreateInstructionReader( false );
                        IEnumerator<InstructionSequence> sequenceEnumerator =
                            methodDef.MethodBody.GetInstructionSequenceEnumerator();
                        while ( sequenceEnumerator.MoveNext() )
                        {
                            reader.EnterInstructionSequence( sequenceEnumerator.Current );
                            InspectSequence( methodDef, reader, processedOperands, uses );
                            reader.LeaveInstructionSequence();
                        }
                    }
                    else
                    {
                        InstructionReader reader = methodDef.MethodBody.CreateOriginalInstructionReader();
                        InspectSequence( methodDef, reader, processedOperands, uses );
                    }


                    if ( uses.Count > 0 )
                    {
                        methodDef.SetTag( usesTag, uses );
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the collection of methods that use a given declaration.
        /// </summary>
        /// <param name="declaration">A field, method or type.</param>
        /// <returns>The collection of methods that use <paramref name="declaration"/>.</returns>
        public static ICollection<MethodDefDeclaration> GetUsedBy( MetadataDeclaration declaration )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( declaration, "declaration" );

            #endregion

            return (ICollection<MethodDefDeclaration>) declaration.GetTag( usedByTag ) ??
                   EmptyCollection<MethodDefDeclaration>.GetInstance();
        }

        /// <summary>
        /// Gets the collection of declarations (fields, methods, types) that are used
        /// by a method.
        /// </summary>
        /// <param name="method">A method.</param>
        /// <returns>The collection of fields, methods and types used by <paramref name="method"/>.</returns>
        public static ICollection<MetadataDeclaration> GetUses( MethodDefDeclaration method )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( method, "method" );

            #endregion

            return (ICollection<MetadataDeclaration>) method.GetTag( usesTag ) ??
                   EmptyCollection<MetadataDeclaration>.GetInstance();
        }

        /// <summary>
        /// Method to be called when the body of a method has been moved into another. This methods updates
        /// the indexes of uses-used by.
        /// </summary>
        /// <param name="sourceMethod">Method to which the body orgininally belonged.</param>
        /// <param name="targetMethod">Method to which the body now belongs.</param>
        public static void MoveMethodBody( MethodDefDeclaration sourceMethod, MethodDefDeclaration targetMethod )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sourceMethod, "sourceMethod" );
            ExceptionHelper.AssertArgumentNotNull( targetMethod, "targetMethod" );

            #endregion

            ICollection<MetadataDeclaration> usedMethods =
                (ICollection<MetadataDeclaration>) sourceMethod.GetTag( usesTag );

            if ( usedMethods != null )
            {
                foreach ( MetadataDeclaration usedDeclaration in usedMethods )
                {
                    ICollection<MethodDefDeclaration> usedMethodUsedBy =
                        (ICollection<MethodDefDeclaration>) usedDeclaration.GetTag( usedByTag );

                    usedMethodUsedBy.Remove( sourceMethod );
                    usedMethodUsedBy.Add( targetMethod );
                }

                targetMethod.SetTag( usesTag, usedMethods );
                sourceMethod.SetTag( usesTag, null );
            }
        }
    }
}