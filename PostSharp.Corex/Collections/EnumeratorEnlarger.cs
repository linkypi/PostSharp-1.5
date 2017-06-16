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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Provides a method that casts a enumerator of a specific type into
    /// an enumerator of a more general type.
    /// </summary>
    public static class EnumeratorEnlarger
    {
        /// <summary>
        /// Casts an enumerator <see cref="IEnumerator{T}"/>&lt;<typeparamref name="TSource"/>&gt; into
        /// <see cref="IEnumerator{T}"/>&lt;<typeparamref name="TTarget"/>&gt;, where
        /// <typeparamref name="TTarget"/> is derived from <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">Source type of enumerated items.</typeparam>
        /// <typeparam name="TTarget">Target type of enumerated items (should be derived from
        /// <typeparamref name="TSource"/>).</typeparam>
        /// <param name="sourceEnumerator">Enumerator to be enlarged.</param>
        /// <returns>An <see cref="IEnumerator{T}"/>&lt;<typeparamref name="TTarget"/>&gt;
        /// that maps <paramref name="sourceEnumerator"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter" )]
        public static IEnumerator<TTarget> EnlargeEnumerator<TSource, TTarget>( IEnumerator<TSource> sourceEnumerator )
            where TSource : TTarget
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sourceEnumerator, "sourceEnumerator" );

            #endregion

            return new EnumeratorExpanderImpl<TSource, TTarget>( sourceEnumerator );
        }

        /// <summary>
        /// Casts an enumerable <see cref="IEnumerable{T}"/>&lt;<typeparamref name="TSource"/>&gt; into
        /// <see cref="IEnumerable{T}"/>&lt;<typeparamref name="TTarget"/>&gt;, where
        /// <typeparamref name="TTarget"/> is derived from <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">Source type of enumerated items.</typeparam>
        /// <typeparam name="TTarget">Target type of enumerated items (should be derived from
        /// <typeparamref name="TSource"/>).</typeparam>
        /// <param name="sourceEnumerable">Enumerator to be enlarged.</param>
        /// <returns>An <see cref="IEnumerable{T}"/>&lt;<typeparamref name="TTarget"/>&gt;
        /// that maps <paramref name="sourceEnumerable"/>.</returns>
        public static IEnumerable<TTarget> EnlargeEnumerable<TSource, TTarget>( IEnumerable<TSource> sourceEnumerable )
            where TSource : TTarget
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sourceEnumerable, "sourceEnumerable" );

            #endregion

            return new EnumerableExpanderImpl<TSource, TTarget>( sourceEnumerable );
        }

        private sealed class EnumerableExpanderImpl<SourceType, TargetType> : IEnumerable<TargetType>
            where SourceType : TargetType
        {
            private readonly IEnumerable<SourceType> sourceEnumerable;

            public EnumerableExpanderImpl( IEnumerable<SourceType> sourceEnumerable )
            {
                this.sourceEnumerable = sourceEnumerable;
            }

            #region IEnumerable<TargetType> Members

            public IEnumerator<TargetType> GetEnumerator()
            {
                IEnumerator<SourceType> sourceEnumerator = this.sourceEnumerable.GetEnumerator();
                if ( sourceEnumerator == null )
                {
                    return null;
                }
                return new EnumeratorExpanderImpl<SourceType, TargetType>( sourceEnumerator );
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }


        private sealed class EnumeratorExpanderImpl<SourceType, TargetType> : IEnumerator<TargetType>
            where SourceType : TargetType
        {
            private readonly IEnumerator<SourceType> sourceEnumerator;

            public EnumeratorExpanderImpl( IEnumerator<SourceType> sourceEnumerator )
            {
                this.sourceEnumerator = sourceEnumerator;
            }

            #region IEnumerator<TargetType> Members

            public TargetType Current { get { return this.sourceEnumerator.Current; } }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                this.sourceEnumerator.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current { get { return this.sourceEnumerator.Current; } }

            public bool MoveNext()
            {
                return this.sourceEnumerator.MoveNext();
            }

            public void Reset()
            {
                this.sourceEnumerator.Reset();
            }

            #endregion
        }
    }
}