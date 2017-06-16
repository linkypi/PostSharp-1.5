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
using System.Collections.Specialized;
using System.IO;
using System.Security;
using System.Security.Policy;
using PostSharp.Collections;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Specifies how the PostSharp Object (<see cref="IPostSharpObject"/>) should be instantiated
    /// and should behave.
    /// </summary>
    [Serializable]
    public sealed class PostSharpObjectSettings
    {
        private readonly AppDomainSetup appDomainSetup;
        private readonly Evidence evidence = new Evidence();

        private readonly Set<string> searchDirectories =
            new Set<string>( 16, StringComparer.InvariantCultureIgnoreCase );

        private readonly TagCollection domainTags = new TagCollection();
        private readonly NameValueCollection settings = new NameValueCollection();
        private readonly Set<string> disabledMessages = new Set<string>( 4, StringComparer.InvariantCultureIgnoreCase );


        /// <summary>
        /// Initializes a new <see cref="PostSharpObjectSettings"/> and set default values.
        /// </summary>
        public PostSharpObjectSettings()
        {
            string appBase = ApplicationInfo.BaseDirectory;
            string appDomainConfigurationFile = Path.Combine( appBase, "PostSharp-AppDomain.config" );

            this.appDomainSetup = new AppDomainSetup
                                      {
                                          LoaderOptimization = LoaderOptimization.SingleDomain,
                                          DisallowApplicationBaseProbing = false,
                                          ShadowCopyFiles = "false",
                                          ApplicationBase = appBase,
                                          ApplicationName = "PostSharpWeavingDomain",
                                          ConfigurationFile = appDomainConfigurationFile
                                      };
        }


        /// <summary>
        /// Determines whether the PostSharp Object should be created in a new <see cref="AppDomain"/>.
        /// </summary>
        public bool CreatePrivateAppDomain { get; set; }


        /// <summary>
        /// The <see cref="AppDomainSetup"/> object used to create the private <see cref="AppDomain"/>.
        /// Ignored if the property <see cref="CreatePrivateAppDomain"/> is false.
        /// </summary>
        public AppDomainSetup AppDomainSetup
        {
            get { return appDomainSetup; }
        }

        /// <summary>
        /// A default permission set that is granted to all assemblies loaded 
        /// into the new application domain that do not have specific grants. 
        /// Ignored if the property <see cref="CreatePrivateAppDomain"/> is false.
        /// </summary>
        public PermissionSet PermissionSet { get; set; }

        /// <summary>
        /// Evidence mapped through the security policy to establish a top-of-stack permission set 
        /// of the new <see cref="AppDomain"/>. Ignored if the property <see cref="CreatePrivateAppDomain"/> is false.
        /// </summary>
        public Evidence Evidence
        {
            get { return evidence; }
        }

        /// <summary>
        /// Gets the set of directories in which assemblies have to be searched.
        /// </summary>
        /// <remarks>
        /// This search path is useful when assemblies and their dependencies are loaded,
        /// <i>before</i> projects are executed. In order to specify the path of
        /// PostSharp plug-ins, it is preferable to use the standard search path facility of
        /// projects.
        /// </remarks>
        public Set<string> SearchDirectories
        {
            get { return searchDirectories; }
        }

        /// <summary>
        /// Gets the set of messages that should be disabled.
        /// </summary>
        /// <remarks>
        /// <para>Populate the set with the identifier of messages to be disabled.
        /// You cannot disable errors or fatal errors.</para>
        /// </remarks>
        public Set<string> DisabledMessages
        {
            get { return this.disabledMessages; }
        }

        /// <summary>
        /// Determines whether the name of processed assemblies should be
        /// automatically modified.
        /// </summary>
        /// <remarks>
        /// <para>You will typically use <b>false</b> in the compile-time scenario
        /// and <b>true</b> in the runtime scenario.</para>
        /// <para>A <b>true</b> value instructs the PostSharp Object to suppress
        /// public keys from and to append a tilde to names of transformed
        /// assemblies. Removing public keys is required (unless you can resign
        /// assemblies after processing), otherwise the VRE will refuse to
        /// load transformed assemblies. Changing assembly names make sure
        /// that the system Assembly Binder does not load the original assembly
        /// and call 'our' custom binder.</para>
        /// </remarks>
        public bool OverwriteAssemblyNames { get; set; }


        /// <summary>
        /// Gets or sets the assembly-qualified name of the implementation
        /// of the local host. This type should be derived from <see cref="PostSharpLocalHost"/>
        /// and have a default constructor.
        /// </summary>
        public string LocalHostImplementation { get; set; }


        /// <summary>
        /// Gets or sets the project execution order. Determines whether projects
        /// should be executed sequentially or per phase.
        /// </summary>
        /// <seealso cref="PostSharp.Extensibility.ProjectExecutionOrder"/>.
        public ProjectExecutionOrder ProjectExecutionOrder { get; set; }

        /// <summary>
        /// Gets the collection of domainTags set on the PostSharp Object.
        /// </summary>
        public TagCollection DomainTags
        {
            get { return domainTags; }
        }

        /// <summary>
        /// Gets or sets the PostSharp settings (overwriting the ones in <b>PostSharp-Library.config</b>).
        /// </summary>
        public NameValueCollection Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Determines whether user code is allowed to be loaded in the CLR. 
        /// </summary>
        /// <remarks>
        /// The value should typically be <b>true</b> for assemblies linked against the full .NET Framework,
        /// and <b>false</b> for assemblies linked against the Compact Framework or Silverlight.
        /// </remarks>
        public bool DisableReflection { get; set; }
    }


    /// <summary>
    /// Determines in which order tasks of projects should be executed. This is
    /// relevant only when the <see cref="PostSharpObject.InvokeProjects"/> method
    /// is called with many projects <i>or</i> if many projects are executed
    /// together due to recursive processing.
    /// </summary>
    public enum ProjectExecutionOrder
    {
        /// <summary>
        /// Default execution order: <see cref="Sequential"/>.
        /// </summary>
        Default = Sequential,

        /// <summary>
        /// Projects are executed atomically. All tasks of all phases of one project are executed,
        /// then the next projects are executed.
        /// </summary>
        Sequential = 0,

        /// <summary>
        /// Groups of projects are executed per phase. Tasks of one phases of all projects are executed,
        /// then the next phases are are executed for all projects.
        /// </summary>
        Phased
    }
}