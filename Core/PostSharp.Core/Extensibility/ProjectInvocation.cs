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

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Request to execute a <see cref="Project"/>.
    /// </summary>
    /// <remarks>
    /// When invoking a project, one should give (1) the project to execute, (2) the module
    /// that should be processed and (3) some user-defined properties. The two last elements
    /// are encapsulates by the <see cref="ProjectInvocationParameters"/> class. The
    /// <see cref="ProjectInvocation"/> class encapsulates both the module identity and a
    /// <see cref="ProjectInvocationParameters"/> object.
    /// </remarks>
    [Serializable]
    public sealed class ProjectInvocation
    {
        private readonly ProjectInvocationParameters invocationParameters;
        private readonly ModuleLoadStrategy moduleLoadStrategy;


        /// <summary>
        /// Initializes a new <see cref="ProjectInvocation"/> and informs to process
        /// the default module (manifest module) of the given assembly.
        /// </summary>
        /// <param name="invocationParameters">Identity of the project to be executed and
        /// their parameters.</param>
        /// <param name="moduleLoadStrategy">Identity of the assembly to be loaded.</param>
        public ProjectInvocation(
            ProjectInvocationParameters invocationParameters,
            ModuleLoadStrategy moduleLoadStrategy )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( moduleLoadStrategy, "assemblyLoadArgs" );
            ExceptionHelper.AssertArgumentNotNull( invocationParameters, "invocationParameters" );

            #endregion

            this.invocationParameters = invocationParameters;
            this.moduleLoadStrategy = moduleLoadStrategy;
        }


        /// <summary>
        /// Gets the identity of the assembly and the arguments with which it should be loaded.
        /// </summary>
        public ModuleLoadStrategy ModuleLoadStrategy
        {
            get { return this.moduleLoadStrategy; }
        }

        /// <summary>
        /// Gets the identity of the project and the parameters with which it should be executed.
        /// </summary>
        public ProjectInvocationParameters InvocationParameters
        {
            get { return this.invocationParameters; }
        }
    }

    /// <summary>
    /// Determines how an already-given module should be processed. Encapsulates a project identity
    /// and the properties with which it should be executed.
    /// </summary>
    [Serializable]
    public sealed class ProjectInvocationParameters
    {
        private readonly PropertyCollection properties = new PropertyCollection();
        private readonly TagCollection tags = new TagCollection();
        private readonly string projectConfigurationFile;

        /// <summary>
        /// Initializes a new <see cref="ProjectInvocationParameters"/>.
        /// </summary>
        /// <param name="projectConfigurationFile">Location of the project file.</param>
        public ProjectInvocationParameters( string projectConfigurationFile )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( projectConfigurationFile, "projectConfigurationFile" );

            #endregion

            this.projectConfigurationFile = projectConfigurationFile;
        }

        /// <summary>
        /// Gets the collection of properties with which the project should be executed.
        /// </summary>
        public PropertyCollection Properties
        {
            get { return this.properties; }
        }

        /// <summary>
        /// Gets the location of the project configuration file.
        /// </summary>
        public string ProjectConfigurationFile
        {
            get { return this.projectConfigurationFile; }
        }

        /// <summary>
        /// Determines whether the dependencies of this module should be processed before.
        /// </summary>
        /// <remarks>
        /// If this property is <b>true</b>, the PostSharp Object will call the 
        /// <see cref="IPostSharpHost.GetProjectInvocationParameters"/> for each dependency.
        /// </remarks>
        public bool ProcessDependenciesFirst { get; set; }

        /// <summary>
        /// Gets the collection of tags that needs to be passed to the
        /// <see cref="Project"/>.
        /// </summary>
        public TagCollection Tags
        {
            get { return this.tags; }
        }

        /// <summary>
        /// If <b>true</b>, the name of this assembly won't be overwritten.
        /// </summary>
        public bool PreventOverwriteAssemblyNames { get; set; }
    }
}