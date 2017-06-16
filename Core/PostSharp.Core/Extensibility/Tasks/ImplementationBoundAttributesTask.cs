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
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Determines whether a custom attribute is bound to its implementation,
    /// i.e. provides an index of instances of the <see cref="ImplementationBoundAttributeAttribute"/>
    /// custom attribute.
    /// </summary>
    public sealed class ImplementationBoundAttributesTask : Task
    {
        private readonly Set<IType> implementationBoundAttributes = new Set<IType>();

        /// <summary>
        /// Gets the <see cref="ImplementationBoundAttributesTask"/> task in a project.
        /// </summary>
        /// <param name="project">Project.</param>
        /// <returns>The <see cref="ImplementationBoundAttributesTask"/> if one was found in
        /// the project, or <b>null</b> otherwise.</returns>
        public static ImplementationBoundAttributesTask GetTask( Project project )
        {
            return (ImplementationBoundAttributesTask) project.Tasks["ImplementationBoundAttributes"];
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            IEnumerator<IAnnotationInstance> caEnumerator =
                AnnotationRepositoryTask.GetTask( this.Project ).GetAnnotationsOfType(
                    this.Project.Module.GetTypeForFrameworkVariant( typeof(ImplementationBoundAttributeAttribute) ).
                        GetTypeDefinition(),
                    false );

            while ( caEnumerator.MoveNext() )
            {
                this.implementationBoundAttributes.AddIfAbsent(
                    (IType) caEnumerator.Current.Value.ConstructorArguments[0].Value.Value );
            }

            return true;
        }

        /// <summary>
        /// Determines whether a custom attribute of a given type is bound to the implementation of the
        /// semantic to which it was applied.
        /// </summary>
        /// <param name="attributeType">Type of the custom attribute.</param>
        /// <returns><b>true</b> if custom attributes of type <paramref name="attributeType"/> are bound 
        /// to the implementation of the semantic to which they are applied, or <b>false</b>
        /// if these custom attributes should be moved to the semantic when semantics are split
        /// from implementations.</returns>
        /// <see cref="ImplementationBoundAttributeAttribute"/>
        public bool IsImplementationBound( IType attributeType )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( attributeType, "attributeType" );

            #endregion

            return this.implementationBoundAttributes.Contains( attributeType );
        }
    }
}