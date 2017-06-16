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

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Executes PEVERIFY on the compilation output.
    /// </summary>
    /// <remarks>
    /// This task is not very useful, because PEVERIFY needs dependencies to be in the
    /// same directory as the verified assemblies, which is typically not the case
    /// when PostSharp is executed. It is instead preferable to call PEVERIFY from
    /// the MSBuild task.
    /// </remarks>
    public sealed class VerifyTask : Task
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            this.Project.Platform.Verify( this.Project.Properties["CompileTargetFileName"] );
            return true;
        }
    }
}