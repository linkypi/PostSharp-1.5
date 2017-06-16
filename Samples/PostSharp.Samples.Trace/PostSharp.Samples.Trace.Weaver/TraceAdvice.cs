#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace PostSharp.Samples.Trace
{
    /// <summary>
    /// Based on the data of a <see cref="TraceAttribute"/>, injects code in a method.
    /// Implements <see cref="IAdvice"/>.
    /// </summary>
    internal class TraceAdvice : IAdvice
    {
        private readonly TraceTask parent;
        private readonly TraceAttribute attribute;


        /// <summary>
        /// Initializes a new <see cref="TraceAdvice"/>.
        /// </summary>
        /// <param name="parent">Task to which this advice belongs.</param>
        /// <param name="attribute">Instance of the custom attribute to which this advice is related.</param>
        public TraceAdvice( TraceTask parent, TraceAttribute attribute )
        {
            this.parent = parent;
            this.attribute = attribute;
        }

        public int Priority { get { return (int) this.attribute.AttributePriority; } }

        public bool RequiresWeave( WeavingContext context )
        {
            return true;
        }

        public void Weave( WeavingContext context, InstructionBlock block )
        {
            switch ( context.JoinPoint.JoinPointKind )
            {
                case JoinPointKinds.BeforeMethodBody:
                    this.WeaveEntry( context, block );
                    break;

                case JoinPointKinds.AfterMethodBodyAlways:
                    this.WeaveExit( context, block );
                    break;

                default:
                    throw new ArgumentException(
                        string.Format( "Unexpected join point kind: {0}", context.JoinPoint.JoinPointKind ) );
            }
        }

        private void WeaveEntry( WeavingContext context, InstructionBlock block )
        {
            string methodName = context.Method.DeclaringType.ToString() + "/" + context.Method.ToString();

            // Create a new instruction sequence and add it to the block
            // dedicated to our advice. Attach the InstructionWriter.
            InstructionSequence entrySequence = context.Method.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( entrySequence, NodePosition.Before, null );
            context.InstructionWriter.AttachInstructionSequence( entrySequence );
            context.InstructionWriter.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );


            // Call Trace.WriteLine
            context.InstructionWriter.EmitInstructionString( OpCodeNumber.Ldstr,
                                                             "Entry - " + methodName );
            if ( this.attribute.Category == null )
            {
                context.InstructionWriter.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                context.InstructionWriter.EmitInstructionString( OpCodeNumber.Ldstr,
                                                                 this.attribute.Category );
            }

            context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.parent.TraceWriteLineMethod );

            // Call Trace.Indent()
            if ( context.Method.Name != ".ctor" )
            {
                context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.parent.TraceIndentMethod );
            }

            // Commit changes and detach the instruction sequence.
            context.InstructionWriter.DetachInstructionSequence();
        }

        private void WeaveExit( WeavingContext context, InstructionBlock block )
        {
            string methodName = context.Method.DeclaringType.ToString() + "/" + context.Method.ToString();

            InstructionSequence exitSequence = context.Method.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence( exitSequence, NodePosition.Before, null );
            context.InstructionWriter.AttachInstructionSequence( exitSequence );
            context.InstructionWriter.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );

            if ( context.Method.Name != ".ctor" )
            {
                context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.parent.TraceUnindentMethod );
            }
            context.InstructionWriter.EmitInstructionString( OpCodeNumber.Ldstr,
                                                             "Finally - " + methodName );
            if ( this.attribute.Category == null )
            {
                context.InstructionWriter.EmitInstruction( OpCodeNumber.Ldnull );
            }
            else
            {
                context.InstructionWriter.EmitInstructionString( OpCodeNumber.Ldstr,
                                                                 this.attribute.Category );
            }
            context.InstructionWriter.EmitInstructionMethod( OpCodeNumber.Call, this.parent.TraceWriteLineMethod );
            context.InstructionWriter.DetachInstructionSequence();
        }

        public override string ToString()
        {
            return string.Format( "{0}(Category = \"{1}\")", this.GetType().Name, this.attribute.Category );
        }
    }
}