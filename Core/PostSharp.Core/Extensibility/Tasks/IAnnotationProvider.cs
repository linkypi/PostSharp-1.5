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
using System.Diagnostics.CodeAnalysis;
using PostSharp.CodeModel;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// When implemented by a <see cref="Task"/>, provides custom attribute instances
    /// to the <see cref="AnnotationRepositoryTask"/> task.
    /// </summary>
    public interface IAnnotationProvider
    {
        /// <summary>
        /// Enumerates the custom attribute instances (<see cref="IAnnotationInstance"/>)
        /// providen by the current task.
        /// </summary>
        /// <returns>An enumerator of custom attributes provided by the current task.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        IEnumerator<IAnnotationInstance> GetAnnotations();
    }

    /// <summary>
    /// Implementation of <see cref="IAnnotationProvider"/> that provides the custom
    /// attribute instances defined in the module (i.e. <see cref="CustomAttributeDeclaration"/>).
    /// </summary>
    public sealed class ModuleAnnotationProvider : Task, IAnnotationProvider
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            return true;
        }

        /// <inheritdoc />
        public IEnumerator<IAnnotationInstance> GetAnnotations()
        {

            IEnumerator<MetadataDeclaration> enumerator =
                this.Project.Module.Tables.GetEnumerator( TokenType.CustomAttribute );

            while ( enumerator.MoveNext() )
            {
                CustomAttributeDeclaration attribute = (CustomAttributeDeclaration) enumerator.Current;
                yield return attribute;
            }
        }
    }
}