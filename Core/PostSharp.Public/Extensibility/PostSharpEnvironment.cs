#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion


#if !SMALL

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Provides information about the currently executing project.
    /// </summary>
    public interface IProject
    {
        /// <summary>
        /// Evaluates an expression (that is, replace parameters by their actual value).
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <returns>The evaluated expression, or <b>null</b> if one parameter could not be
        /// resolved.</returns>
        string EvaluateExpression( string expression );

        /// <summary>
        /// Gets the variant of the .NET Framework against which the .NET assembly
        /// currently processed is linked.
        /// </summary>
        /// <returns>A string uniquely identifying the framework variant, to be compared
        /// to the constants defined in the <see cref="FrameworkVariants"/> class.
        /// Actually, the public key token of <b>mscorlib</b> is returned.</returns>
        string GetFrameworkVariant();
    }

    /// <summary>
    /// Provides information about the current PostSharp environment.
    /// </summary>
    public interface IPostSharpEnvironment
    {
        /// <summary>
        /// Gets the currently executing project.
        /// </summary>
        /// <value>
        /// The current project, or <b>null</b> if there is no current project.
        /// </value>
        IProject CurrentProject { get; }
    }

    /// <summary>
    /// Provides access to the current PostSharp environment (<see cref="IPostSharpEnvironment"/>).
    /// </summary>
    public static class PostSharpEnvironment
    {
        private static IPostSharpEnvironment currentEnvironment;

        /// <summary>
        /// Gets the current PostSharp environment, or <b>null</b>
        /// if the PostSharp Platform is not loaded in the current
        /// context.
        /// </summary>
        public static IPostSharpEnvironment Current
        {
            get { return currentEnvironment; }
            set { currentEnvironment = value; }
        }

        /// <summary>
        /// Determines whether the PostSharp Platform is currently loaded.
        /// </summary>
        public static bool IsPostSharpRunning
        {
            get { return currentEnvironment != null; }
        }
    }
}

#endif