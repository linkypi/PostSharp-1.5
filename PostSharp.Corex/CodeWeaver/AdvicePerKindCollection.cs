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

namespace PostSharp.CodeWeaver
{
    internal sealed class AdvicePerKindCollection
    {
        private readonly AdviceCollection[] adviceCollections = new AdviceCollection[(int) JoinPointKindIndex._Count];

        public void Add( IAdvice advice, JoinPointKinds joinPointKinds )
        {
            int currentKindIndex = 0;
            ulong currentKind = 1;
            ulong remainingKinds = (ulong) joinPointKinds;
            bool afterMethodBodyAlwaysAlreadyAdded = false;

            while ( remainingKinds != 0 )
            {
                if ( ( remainingKinds & 1 ) != 0 )
                {
                    int currentKindIndexCorrected = currentKindIndex;
                    bool skip = false;

                    // Nasty trick to get the around-method advices appear
                    // all in the same collection.
                    if ( currentKindIndexCorrected == (int) JoinPointKindIndex.AfterMethodBodyException ||
                         currentKindIndexCorrected == (int) JoinPointKindIndex.AfterMethodBodySuccess ||
                         currentKindIndexCorrected == (int) JoinPointKindIndex.BeforeMethodBody ||
                        currentKindIndexCorrected == (int) JoinPointKindIndex.AfterInstanceInitialization
                        )
                    {
                        currentKindIndexCorrected = (int) JoinPointKindIndex.AfterMethodBodyAlways;
                    }

                    if ( currentKindIndexCorrected == (int) JoinPointKindIndex.AfterMethodBodyAlways )
                    {
                        if ( afterMethodBodyAlwaysAlreadyAdded )
                        {
                            skip = true;
                        }
                        else
                        {
                            afterMethodBodyAlwaysAlreadyAdded = true;
                        }
                    }

                    if ( !skip )
                    {
                        AdviceCollection adviceCollection = this.adviceCollections[currentKindIndexCorrected];
                        if ( adviceCollection == null )
                        {
                            adviceCollection = new AdviceCollection();
                            this.adviceCollections[currentKindIndexCorrected] = adviceCollection;
                        }
                        adviceCollection.Add( new AdviceJoinPointKindsPair( advice, joinPointKinds ) );

                        ExceptionHelper.Core.AssertValidOperation( adviceCollection.Count <= 32,
                                                                   "TooManySimilarAdvices" );
                    }
                }

                currentKindIndex++;
                currentKind = currentKind << 1;
                remainingKinds = remainingKinds >> 1;
            }
        }

        public AdviceCollection GetAdvices( JoinPointKindIndex joinPointKind )
        {
            return this.adviceCollections[(int) joinPointKind];
        }

        public void MergeAndSort( AdvicePerKindCollection collection )
        {
            for ( int i = 0 ; i < (int) JoinPointKindIndex._Count ; i++ )
            {
                if ( collection != null && collection.adviceCollections[i] != null )
                {
                    if ( this.adviceCollections[i] == null )
                    {
                        this.adviceCollections[i] = collection.adviceCollections[i];
                    }
                    else
                    {
                        this.adviceCollections[i].Merge( collection.adviceCollections[i] );
                    }
                }

                if ( this.adviceCollections[i] != null )
                {
                    this.adviceCollections[i].Sort();
                }
            }
        }
    }
}