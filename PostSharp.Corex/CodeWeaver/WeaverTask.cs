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
using PostSharp.Extensibility;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// Pluggable task that executes the <see cref="CodeWeaver"/>.
    /// </summary>
    /// <remarks>
    /// This task first builds a collection of advices from other tasks implementing
    /// the <see cref="IAdviceProvider"/> interface. Then it weaves each method
    /// using the <see cref="CodeWeaver"/> class.
    /// </remarks>
    public sealed class WeaverTask : Task
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            using ( Weaver weaver = new Weaver( this.Project ) )
            {
                // Indexing advices.
                using ( IEnumerator<IAdviceProvider> providers = this.Project.Tasks.GetInterfaces<IAdviceProvider>() )
                {
                    while ( providers.MoveNext() )
                    {
                        providers.Current.ProvideAdvices( weaver );
                    }
                }
                weaver.Weave();

                return true;
            }
        }
    }
}