using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NAnt.Core;
using NAnt.Core.Attributes;
using PostSharp.Extensibility;
using Task=NAnt.Core.Task;

namespace PostSharp.NAntTasks
{
    /// <summary>
    /// NAnt task for PostSharp.
    /// </summary>
    [TaskName( "postsharp" )]
    public class PostSharpTask : Task
    {
        private static readonly char[] trimmableChars = new[] {' ', '\t', '\f', '\n', '\r'};
        private string project;
        private string parameters;
        private bool noLogo;
        private bool traceToNAnt = true;
        private string input;
        private bool autoUpdateDisabled;
        private string inputReferenceDirectory;
        private bool attachDebugger;

        /// <summary>
        /// Gets or sets the location of the assembly to be processed.
        /// </summary>
        /// <seealso cref="InputReferenceDirectory"/>
        [TaskAttribute( "input" )]
        [StringValidator( AllowEmpty = false )]
        public string Input
        {
            get { return input; }
            set { input = value; }
        }


        /// <summary>
        /// Directory according to which the <see cref="Input"/> property should be
        /// resolved, if a relative filename is given in <see cref="Input"/>.
        /// </summary>
        [TaskAttribute( "inputReferenceDirectory" )]
        public string InputReferenceDirectory
        {
            get { return inputReferenceDirectory; }
            set { inputReferenceDirectory = value; }
        }


        /// <summary>
        /// Determines whether PostSharp tracing is redirecting to MSBuild messages.
        /// </summary>
        /// <value>
        /// A boolean. Default is <b>true</b>.
        /// </value>
        [TaskAttribute( "trace" )]
        public bool TraceToNAnt
        {
            get { return traceToNAnt; }
            set { traceToNAnt = value; }
        }

        /// <summary>
        /// Gets or sets the PostSharp project to be executed. Required.
        /// </summary>
        [TaskAttribute( "project" )]
        [StringValidator( AllowEmpty = false )]
        public string ProjectFile
        {
            get { return project; }
            set { project = value; }
        }

        /// <summary>
        /// Gets or sets the parameters passed to the PostSharp project.
        /// </summary>
        /// <value>
        /// A string whose format is "Name1=Value1;Name2=Value2".
        /// </value>
        [TaskAttribute( "parameters" )]
        public string Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        /// <summary>
        /// Indicates that the PostSharp tag line should not be printed.
        /// </summary>
        [TaskAttribute( "nologo" )]
        public bool NoLogo
        {
            get { return noLogo; }
            set { noLogo = value; }
        }

        /// <summary>
        /// Indicates whether the AutoUpdate check is disabled.
        /// </summary>
        [TaskAttribute( "autoUpdateDisabled" )]
        public bool AutoUpdateDisabled
        {
            get { return autoUpdateDisabled; }
            set { autoUpdateDisabled = value; }
        }

        /// <summary>
        /// If <b>true</b>, the method <see cref="Debugger"/>.<see cref="Debugger.Launch"/>
        /// will be invoked before the execution of PostSharp, given the opportunity to
        /// attach a debugger to the building process.
        /// </summary>
        [TaskAttribute( "attachDebugger" )]
        public bool AttachDebugger
        {
            get { return attachDebugger; }
            set { attachDebugger = value; }
        }

        /// <inheritdoc />
        protected override void ExecuteTask()
        {
      
            string inputFullPath;
            if ( !string.IsNullOrEmpty( this.inputReferenceDirectory ) )
            {
                inputFullPath = Path.Combine( this.inputReferenceDirectory, this.input );
            }
            else
            {
                inputFullPath = this.input;
            }

            if ( !this.noLogo )
            {
                this.Log( Level.Info, "PostSharp 1.5 [{0}] - Copyright (c) 2004-2010 by SharpCrafters s.r.o..",
                          ApplicationInfo.Version );
            }


            this.Log( Level.Verbose,
                      "Executing project {{{0}}} with input assembly {{{1}}}.",
                      this.project,
                      inputFullPath );

            this.Log( Level.Verbose,
                      "PostSharp implementation is in {{{0}}}.",
                      typeof(IPostSharpObject).Assembly.Location );

            if ( this.attachDebugger )
                Debugger.Launch();

            // Perform static configuration.
            Messenger.Current.Message += OnMessage;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            ModuleLoadStrategy moduleLoadStrategy = new ModuleLoadReflectionFromFileStrategy( inputFullPath );
            ProjectInvocationParameters invocationParameters = new ProjectInvocationParameters(
                this.project );

            try
            {
                // Parse properties
                if ( this.parameters != null )
                {
                    foreach ( string pair in this.parameters.Split( ';' ) )
                    {
                        string trimmedPair = pair.Trim( trimmableChars );

                        if ( trimmedPair.Length != 0 )
                        {
                            this.Log( Level.Verbose,
                                      "Parsing parameter: {{{0}}}.", trimmedPair );

                            string[] thisPair = trimmedPair.Split( '=' );
                            if ( thisPair.Length != 2 )
                            {
                                this.Log( Level.Warning,
                                          "PostSharp: ignoring this property because it is malformed: \"{0}\".",
                                          pair );
                                continue;
                            }
                            invocationParameters.Properties[thisPair[0].Trim( trimmableChars )] =
                                thisPair[1].Trim( trimmableChars );
                        }
                    }
                }

                // Start automatic updates in a second thread.
                if ( !this.autoUpdateDisabled )
                {
                    AutoUpdate.BeginRetrieveMessages();
                }


                // Execute the project.
                try
                {
                    PostSharpObjectSettings postSharpObjectSettings = new PostSharpObjectSettings();
                    postSharpObjectSettings.CreatePrivateAppDomain = true;

                    using (
                        IPostSharpObject postSharpObject =
                            PostSharpObject.CreateInstance( postSharpObjectSettings, null ) )
                    {
                        ProjectInvocation projectInvocation = new ProjectInvocation(
                            invocationParameters, moduleLoadStrategy );
                        postSharpObject.InvokeProject( projectInvocation );
                    }
                }
                catch ( MessageException e )
                {
                    this.Log( Level.Verbose, "The task failed because of a MessageException: " +
                                             e.ToString() );
                    return;
                }
                catch ( Exception e )
                {
                    this.Log( Level.Error, e.ToString() );
                    return;
                }
            }
            finally
            {
                if ( !this.autoUpdateDisabled )
                {
                    AutoUpdate.StopRetrieveMessages();
                }

                // Remove static configuration.
                Messenger.Current.Message -= OnMessage;
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }

            this.Log( Level.Verbose, "The PostSharp task terminated normally." );

            return;
        }
    

        private static Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            AssemblyName assemblyName = new AssemblyName( args.Name );

            // Look in the current list of assemblies if we have it.
            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if ( AssemblyName.ReferenceMatchesDefinition( assemblyName, assembly.GetName() ) )
                {
                    return assembly;
                }
            }

            // Not found inside.
            return null;
        }

        private void OnMessage(object sender, PostSharp.Extensibility.MessageEventArgs e)
        {
            string format;
           
            SeverityType severity = e.Message.Severity;
            string messageText = e.Message.MessageText;
            Level level;


            if ( e.Message.LocationColumn != Message.NotAvailable )
            {
                format = "{0}({1},{2}): {3} {4}: {5}";
            }
            else if ( e.Message.LocationLine != Message.NotAvailable )
            {
                format = "{0}({1}): {3} {4}: {5}";
            }
            else if ( e.Message.LocationFile != null )
            {
                format = "{0}: {3} {4}: {5}";
            }
            else
            {
                format = "{3} {4}: {5}";
            }

            string message =
                string.Format( format, e.Message.LocationFile, e.Message.LocationLine, e.Message.LocationColumn,
                               severity.ToString().ToLower(), e.Message.MessageId, messageText );


            switch ( e.Message.Severity )
            {
                case SeverityType.Error:
                case SeverityType.Fatal:
                    level = Level.Error;
                    break;

                case SeverityType.Warning:
                    level = Level.Warning;
                    break;

                case SeverityType.Debug:
                case SeverityType.Verbose:
                    level = Level.Verbose;
                    break;

                default:
                    level = Level.Info;
                    break;
            }

            this.Log( level, message );
        }
    }
}