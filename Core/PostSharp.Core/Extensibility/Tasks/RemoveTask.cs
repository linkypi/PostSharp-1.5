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
using PostSharp.Collections;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Task that remove declarations from the module. 
    /// Typically called immediately before compilation.
    /// </summary>
    /// <remarks>
    /// Declaration removal should be two-phased: first declarations
    /// should be marked for removal using the <see cref="MarkForRemoval"/>
    /// method, then they are removed, typically immediately before
    /// compilation, during task execution.
    /// </remarks>
    public sealed class RemoveTask : Task
    {
        private static readonly Guid removalTagGuid = new Guid( "{A21E5845-021A-4ecf-A727-F8F1B358EB77}" );
        private static readonly object removalTag = new object();

        private readonly Set<IRemoveable> removeables = new Set<IRemoveable>();


        /// <summary>
        /// Finds the <see cref="RemoveTask"/> task instance in a <see cref="Project"/>.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>The <see cref="RemoveTask"/> task instance.</returns>
        /// <exception cref="InvalidOperationException">The task <b>Remove</b> is not present in this project.</exception>
        public static RemoveTask GetTask( Project project )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( project, "project" );

            #endregion

            RemoveTask task = (RemoveTask) project.Tasks["Remove"];
            ExceptionHelper.Core.AssertValidOperation( task != null, "CannotFindTask", "Remove" );
            return task;
        }


        /// <summary>
        /// Order a declaration to be removed later by this task.
        /// </summary>
        /// <param name="declaration">The declaration that should be removed.</param>
        public void MarkForRemoval( IRemoveable declaration )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( declaration, "declaration" );

            #endregion

            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( !this.removeables.Contains( declaration ),
                                                       "AlreadyMarkedForRemoval" );

            #endregion

            this.removeables.Add( declaration );
            declaration.SetTag( removalTagGuid, removalTag );
        }

        /// <summary>
        /// Determines whether a declaration has been marked for removal.
        /// </summary>
        /// <param name="declaration">A declaration.</param>
        /// <returns><b>true</b> if <paramref name="declaration"/> has been marked for
        /// removal, otherwise <b>false</b>.</returns>
        public static bool IsMarkedForRemoval( IRemoveable declaration )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( declaration, "declaration" );

            #endregion

            return declaration.GetTag( removalTagGuid ) != null;
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            foreach ( IRemoveable declaration in this.removeables )
            {
                declaration.Remove();
            }

            return true;
        }
    }
}