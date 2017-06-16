using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.MSBuild
{
    /// <summary>
    /// <b>[MSBuild Task]</b> Execute PostSharp.
    /// </summary>
    public sealed class PostSharp15 : Task
    {
        private readonly PostSharpTaskParameters parameters = new PostSharpTaskParameters();

        /// <summary>
        /// Gets or sets the location of the assembly to be processed.
        /// </summary>
        /// <seealso cref="InputReferenceDirectory"/>
        [Required]
        public string Input
        {
            get { return this.parameters.Input; }
            set { this.parameters.Input = value; }
        }


        /// <summary>
        /// Directory according to which the <see cref="Input"/> property should be
        /// resolved, if a relative filename is given in <see cref="Input"/>.
        /// </summary>
        public string InputReferenceDirectory
        {
            get { return this.parameters.InputReferenceDirectory; }
            set { this.parameters.InputReferenceDirectory = value; }
        }


        /// <summary>
        /// Determines whether tracing is enabled.
        /// </summary>
        /// <value>
        /// A boolean. Default is <b>false</b>.
        /// </value>
        public bool Verbose
        {
            get { return this.parameters.Verbose; }
            set { this.parameters.Verbose = value; }
        }

        /// <summary>
        /// Gets or sets the PostSharp project to be executed. Required.
        /// </summary>
        [Required]
        public string Project
        {
            get { return this.parameters.Project; }
            set { this.parameters.Project = value; }
        }

        /// <summary>
        /// Gets or sets the parameters passed to the PostSharp project.
        /// </summary>
        /// <value>
        /// A string whose format is "Name1=Value1;Name2=Value2".
        /// </value>
        public string Parameters
        {
            get { return this.parameters.Parameters; }
            set { this.parameters.Parameters = value; }
        }

        /// <summary>
        /// Indicates that the PostSharp tag line should not be printed.
        /// </summary>
        public bool NoLogo
        {
            get { return this.parameters.NoLogo; }
            set { this.parameters.NoLogo = value; }
        }

        /// <summary>
        /// Indicates whether the AutoUpdate check is disabled.
        /// </summary>
        public bool AutoUpdateDisabled
        {
            get { return this.parameters.AutoUpdateDisabled; }
            set { this.parameters.AutoUpdateDisabled = value; }
        }

        /// <summary>
        /// If <b>true</b>, the method <see cref="Debugger"/>.<see cref="Debugger.Launch"/>
        /// will be invoked before the execution of PostSharp, given the opportunity to
        /// attach a debugger to the building process.
        /// </summary>
        public bool AttachDebugger
        {
            get { return this.parameters.AttachDebugger; }
            set { this.parameters.AttachDebugger = value; }
        }

        /// <summary>
        /// If <b>true</b>, user assemblies will not be loaded into the CLR. This behavior is
        /// typically desired when user assemblies are linked against the Compact Framework or Silverlight.
        /// </summary>
        public bool DisableReflection
        {
            get { return this.parameters.DisableReflection; }
            set { this.parameters.DisableReflection = value; }
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            // We need to invoke the task in a separate AppDomain because other copies of our assemblies
            // may be loaded in the current assembly, even using Assembly.LoadFrom.
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName( typeof(PostSharp15).Assembly.Location );
            setup.ApplicationName = "PostSharp";
            setup.LoaderOptimization = LoaderOptimization.SingleDomain;

            AppDomain appDomain = AppDomain.CreateDomain( "PostSharp", null, setup );
            try
            {
                PostSharpRemoteTask remote =
                    (PostSharpRemoteTask)
                    appDomain.CreateInstanceAndUnwrap( typeof(PostSharpRemoteTask).Assembly.FullName,
                                                       typeof(PostSharpRemoteTask).FullName );
                return remote.Execute( this.parameters, this.Log );
            }
            finally
            {
                AppDomain.Unload( appDomain );
            }
        }
    }
}