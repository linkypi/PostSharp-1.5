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
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Provides a method that enumerates generic arguments (i.e. references to generic parameters)
    /// in type of method signatures.
    /// </summary>
    public sealed class GenericParameterReferenceFinder
    {
        private readonly SortedList<int, bool> referencedGenericTypeParameters = new SortedList<int, bool>( 4 );
        private readonly SortedList<int, bool> referencedGenericMethodParameters = new SortedList<int, bool>( 4 );
        private ModuleDeclaration module;

        private GenericParameterReferenceFinder()
        {
        }

        /// <summary>
        /// Enumerates generic arguments (i.e. references to generic parameters)
        /// in type of method signatures.
        /// </summary>
        /// <param name="visitable">The signature to traverse.</param>
        /// <returns>An enumerator over <see cref="GenericParameterTypeSignature"/> occurrences in <paramref name="visitable"/>.</returns>
        public static IEnumerator<GenericParameterTypeSignature> GetGenericParameterReferenceEnumerator(
            IVisitable<ITypeSignature> visitable )
        {
            ExceptionHelper.AssertArgumentNotNull( visitable, "signature" );
            GenericParameterReferenceFinder instance = new GenericParameterReferenceFinder();
            ITypeSignature signature = visitable as ITypeSignature;

            if ( signature != null )
            {
                instance.VisitTypeSignature( null, null, signature );
            }
            visitable.Visit( null, instance.VisitTypeSignature );

            foreach ( KeyValuePair<int, bool> pair in instance.referencedGenericTypeParameters )
            {
                yield return instance.module.Cache.GetGenericParameter( pair.Key, GenericParameterKind.Type );
            }

            foreach ( KeyValuePair<int, bool> pair in instance.referencedGenericMethodParameters )
            {
                yield return instance.module.Cache.GetGenericParameter( pair.Key, GenericParameterKind.Method );
            }
        }

        private void VisitTypeSignature( IVisitable<ITypeSignature> owner, string role, ITypeSignature element )
        {
            IGenericParameter genericParameter = element as IGenericParameter;
            if ( genericParameter != null )
            {
                SortedList<int, bool> list = genericParameter.Kind == GenericParameterKind.Method
                                                 ?
                                             referencedGenericMethodParameters
                                                 : referencedGenericTypeParameters;

                if ( !list.ContainsKey( genericParameter.Ordinal ) )
                {
                    list.Add( genericParameter.Ordinal, true );
                }

                this.module = element.Module;
            }
        }
    }
}