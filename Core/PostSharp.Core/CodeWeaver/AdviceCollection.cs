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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Collection of advices (<see cref="IAdvice"/>).
    /// </summary>
    internal sealed class AdviceCollection : Collection<AdviceJoinPointKindsPair>
    {
        /// <summary>
        /// Initializes a new empty <see cref="AdviceCollection"/> with default capacity.
        /// </summary>
        public AdviceCollection()
            : base( new List<AdviceJoinPointKindsPair>() )
        {
        }

        /// <summary>
        /// Merge another collection of advices into the current collection.
        /// </summary>
        /// <param name="advices">A collection of advices.</param>
        public void Merge( ICollection<AdviceJoinPointKindsPair> advices )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( advices, "advices" );

            #endregion

            ( (List<IAdvice>) this.Items ).Capacity = this.Count + advices.Count;
            foreach ( AdviceJoinPointKindsPair advice in advices )
            {
                this.Items.Add( advice );
            }
        }

        /// <summary>
        /// Sort the current collection by advice priority.
        /// </summary>
        internal void Sort()
        {
            ( (List<AdviceJoinPointKindsPair>) this.Items ).Sort( AdviceComparer.Instance );
        }

        /// <summary>
        /// Determines whether some advices in the current collection requires to
        /// be woven on a given join point.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>0 if no advice requires to be woven in this <paramref name="context"/>,
        /// otherwise a bit mask to be used with the <see cref="Weave"/> method.</returns>
        internal int RequiresWeave( WeavingContext context )
        {
            int result = 0;
            int bit = 1;

            for ( int i = 0 ; i < this.Count ; i++ )
            {
                AdviceJoinPointKindsPair pair = this[i];

                if ( ( pair.Kinds & context.JoinPoint.JoinPointKind ) != 0 &&
                     pair.Advice.RequiresWeave( context ) )
                {
                    Trace.CodeWeaver.WriteLine( "Advice {{{0}}} requires weaving {1}",
                                                pair.Advice, context.JoinPoint );
                    result |= bit;
                }

                bit = bit << 1;
            }

            return result;
        }

        /// <summary>
        /// Weave the advices of the current collection on a given join point.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="mask">The value returned by <see cref="RequiresWeave"/>.</param>
        /// <param name="block">The <see cref="InstructionBlock"/> into which advices
        /// have to generate code.</param>
        /// <param name="reverse">Whether the advices should be injected in reverse order.</param>
        internal void Weave( WeavingContext context, int mask, InstructionBlock block, bool reverse )
        {
            // Determines whether there is more than a single bit set.
            int bit = 1;
            int count = 0;
            for ( int i = 0 ; i < this.Count ; i++ )
            {
                if ( (mask & bit) == 0 ) continue;
                count++;
                if ( count > 1 )
                {
                    break;
                }
            }


            bit = 1;
            for ( int i = 0 ; i < this.Count ; i++ )
            {
                if ( ( mask & bit ) != 0 )
                {
                    IAdvice advice = this[i].Advice;

                    if ( Trace.CodeWeaver.Enabled )
                    {
                        Trace.CodeWeaver.WriteLine( "In method {{{0}}}, apply {1} on {2}.",
                                                    context.Method, advice, context.JoinPoint );
                    }

                    string comment = string.Format(
                        CultureInfo.InvariantCulture,
                        "Advice: {0} on {1}", advice, context.JoinPoint );

                    if ( count == 1 )
                    {
                        // This is the unique advice. Use the main block.
                        block.Comment = comment;
                        advice.Weave( context, block );
                      }
                    else
                    {
                        // Create a new instruction block and attach it.
                        InstructionBlock newBlock = block.MethodBody.CreateInstructionBlock();
                        newBlock.Comment = comment;

                        if ( reverse )
                        {
                            block.AddChildBlock( newBlock, NodePosition.After, null );
                        }
                        else
                        {
                            block.AddChildBlock( newBlock, NodePosition.Before, null );
                        }

                        // Weave the advice.
                        advice.Weave( context, newBlock );
                    }

                    ExceptionHelper.Core.Assert(context.InstructionWriter.CurrentInstructionSequence == null,
                                              "AdviceWeaveDidNotReleaseInstructionWriter",
                                              advice.GetType().FullName);
               
                }

                bit = bit << 1;
            }
        }
    }
}