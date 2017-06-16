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
using PostSharp.CodeModel;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Context of the current join point.
    /// </summary>
    public sealed class WeavingContext : IDisposable
    {
        #region Fields

        /// <summary>
        /// Instance of <see cref="InstructionWriter"/> shared by all advices.
        /// </summary>
        private InstructionWriter instructionWriter = new InstructionWriter();

        private readonly JoinPoint joinPoint = new JoinPoint();

        #endregion

        /// <summary>
        /// Initializes a new <see cref="WeavingContext"/>.
        /// </summary>
        internal WeavingContext()
        {
        }


        /// <summary>
        /// Determines whether the <b>this</b> pointer is initialized.
        /// </summary>
        public bool IsThisInitialized { get; internal set; }


        /// <summary>
        /// Gets the field being woven.
        /// </summary>
        /// <remarks>
        /// This property only meaningful in the context of
        /// <b>field-level</b> advices, i.e. advices that
        /// have been added with the <see cref="Weaver.AddFieldLevelAdvice"/>.
        /// </remarks>
        public FieldDefDeclaration Field { get; internal set; }


        /// <summary>
        /// Gets the local variable in which the field value is stored.
        /// </summary>
        /// <remarks>
        /// This property only meaningful in the context of
        /// <b>field-level</b> advices, i.e. advices that
        /// have been added with the <see cref="Weaver.AddFieldLevelAdvice"/>.
        /// </remarks>
        public LocalVariableSymbol FieldValueLocal { get; internal set; }


        /// <summary>
        /// Gets the current method.
        /// </summary>
        public MethodDefDeclaration Method { get; internal set; }

        /// <summary>
        /// Gets the current instruction block.
        /// </summary>
        public InstructionBlock InstructionBlock { get; internal set; }

        /// <summary>
        /// Gets the variable containing at runtime the return value, in join points of type
        /// <see cref="JoinPointKinds.AfterMethodBodyAlways"/> and 
        /// <see cref="JoinPointKinds.AfterMethodBodyException"/>
        /// </summary>
        public LocalVariableSymbol ReturnValueVariable { get; internal set; }

        /// <summary>
        /// Gets the <see cref="InstructionSequence"/> containing the current on-success handler
        /// in the chain.
        /// </summary>
        /// <seealso cref="LeaveBranchTarget"/>
        public InstructionSequence OnSuccessBranchTarget { get; internal set; }


        /// <summary>
        /// Gets the <see cref="InstructionSequence"/> containing the next (i.e. immediately outer)
        /// on-success handler in the chain.
        /// </summary>
        /// <seealso cref="OnSuccessBranchTarget"/>
        public InstructionSequence LeaveBranchTarget { get; internal set; }

        /// <summary>
        /// Gets the current exception handler.
        /// </summary>
        public ExceptionHandler ExceptionHandler { get; internal set; }

        /// <summary>
        /// Gets an instance of <see cref="InstructionWriter"/> shared by all advices.
        /// </summary>
        public InstructionWriter InstructionWriter
        {
            get { return instructionWriter; }
        }

        /// <summary>
        /// Gets the current join point.
        /// </summary>
        public JoinPoint JoinPoint
        {
            get { return joinPoint; }
        }


        /// <summary>
        /// Gets a <see cref="WeavingHelper"/>, which provides utility functions
        /// typically used during weaving.
        /// </summary>
        public WeavingHelper WeavingHelper { get; internal set; }

        /// <summary>
        /// Gets the <see cref="PostSharp.CodeModel.InstructionReader"/> located at
        /// the current instruction.
        /// </summary>
        /// <remarks>
        /// This property is exposed to implement some unrecommendable workarounds.
        /// Prefer the <b>JoinPoint.Instruction</b> property to get the current instruction.
        /// <b>You are supposed to leave the <see cref="InstructionReader"/> at the
        /// same position as you received it!</b>
        /// </remarks>
        public InstructionReader InstructionReader { get; internal set; }


        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.instructionWriter == null ) return;

            this.instructionWriter.Dispose();
            this.instructionWriter = null;
        }
    }
}