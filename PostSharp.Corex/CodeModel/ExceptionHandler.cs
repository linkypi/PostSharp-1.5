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
using System.Reflection;
using PostSharp.Collections;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents an exception handler. 
    /// </summary>
    /// <remarks>
    /// An <see cref="ExceptionHandler"/> is
    /// contained in an <see cref="InstructionBlock"/>, which is the protected 
    /// block (i.e. the <i>try</i> block).
    /// </remarks>
    public sealed class ExceptionHandler : Declaration
    {
        #region Fields

        /// <summary>
        /// Filter block.
        /// </summary>
        /// <remarks>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if the handler of Filter type.
        /// </remarks>
        private readonly InstructionBlock filterBlock;

        /// <summary>
        /// Handler block.
        /// </summary>
        private readonly InstructionBlock handlerBlock;

        private readonly LinkedListNode<ExceptionHandler> node;

        #endregion

        /// <summary>
        /// Initializes a <see cref="ExceptionHandler"/>.
        /// </summary>
        /// <param name="tryBlock">The block being protected by the exception handler.</param>
        /// <param name="options">Determines the kind of handler.</param>
        /// <param name="handlerBlock">Handler instruction block.</param>
        /// <param name="filterBlock">Filter instruction block if <paramref name="options"/>
        /// is set to <see cref="ExceptionHandlingClauseOptions.Filter"/>, otherwise <b>null</b>.</param>
        /// <param name="catchType">The type to be caught, or <b>null</b> if
        /// <paramref name="options"/> is not set to <see cref="ExceptionHandlingClauseOptions.Clause"/>.</param>
        internal ExceptionHandler( InstructionBlock tryBlock, ExceptionHandlingClauseOptions options,
                                   InstructionBlock handlerBlock, InstructionBlock filterBlock, ITypeSignature catchType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( handlerBlock, "handlerBlock" );
            if ( options == ExceptionHandlingClauseOptions.Filter && filterBlock == null )
            {
                throw new ArgumentNullException( "filterBlock" );
            }
            if ( options == ExceptionHandlingClauseOptions.Clause && catchType == null )
            {
                throw new ArgumentNullException( "catchType" );
            }

            #endregion

            this.OnAddingToParent( tryBlock, "exceptionHandler" );
            this.Options = options;
            this.filterBlock = filterBlock;
            this.handlerBlock = handlerBlock;
            this.CatchType = catchType;
            this.node = new LinkedListNode<ExceptionHandler>( this );
        }

        /// <summary>
        /// Gets or sets the kind of exception handling clause.
        /// </summary>
        /// <remarks>
        /// You have to set the properties <see cref="ExceptionHandler.CatchType"/>,
        /// and <see cref="ExceptionHandler.FilterBlock"/> according to the kind
        /// of exception handler. This rule is not enforced programmatically.
        /// </remarks>
        [ReadOnly( true )]
        public ExceptionHandlingClauseOptions Options { get; set; }

        /// <summary>
        /// Gets the parent try block.
        /// </summary>
        [Browsable( false )]
        public new InstructionBlock Parent
        {
            get { return (InstructionBlock) base.Parent; }
        }

        /// <summary>
        /// Gets the filter block.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>, or <b>null</b> if the handler kind
        /// is not <see cref="ExceptionHandlingClauseOptions.Filter"/>.
        /// </value>
        [Browsable( false )]
        public InstructionBlock FilterBlock
        {
            get { return filterBlock; }
        }

        /// <summary>
        /// Gets the handler block.
        /// </summary>
        /// <value>
        /// An <see cref="InstructionBlock"/>.
        /// </value>
        [Browsable( false )]
        public InstructionBlock HandlerBlock
        {
            get { return handlerBlock; }
        }

        /// <summary>
        /// Gets or sets the catch type.
        /// </summary>
        /// <value>
        /// A reference to an <see cref="IType"/>, or <b>null</b>
        /// if the handler type is not <see cref="ExceptionHandlingClauseOptions.Clause"/>.
        /// </value>
        [ReadOnly( true )]
        public ITypeSignature CatchType { get; set; }

        internal LinkedListNode<ExceptionHandler> Node
        {
            get { return this.node; }
        }

        /// <summary>
        /// Gets the next exception handler of the parent block.
        /// </summary>
        [Browsable( false )]
        public ExceptionHandler NextSiblingExceptionHandler
        {
            get { return this.node.Next == null ? null : this.node.Next.Value; }
        }

        /// <summary>
        /// Gets the previous exception handler of the parent block.
        /// </summary>
        [Browsable( false )]
        public ExceptionHandler PreviousSiblingExceptionHandler
        {
            get { return this.node.Previous == null ? null : this.node.Previous.Value; }
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of exception handlers (<see cref="ExceptionHandler"/>).
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        [SuppressMessage( "Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable" )]
        public sealed class ExceptionHandlerCollection : LinkedList<ExceptionHandler>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExceptionHandlerCollection"/> type.
            /// </summary>
            internal ExceptionHandlerCollection()
            {
            }
        }
    }
}