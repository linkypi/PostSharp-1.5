#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.IO;
using System.Reflection;
using PostSharp.Extensibility;

namespace PostSharp.Samples.Host
{
    /// <summary>
    /// Implementation of <see cref="IPostSharpHost"/>. Resides
    /// in the system <see cref="AppDomain"/>.
    /// </summary>
    internal class Host : MarshalByRefObject, IPostSharpHost
    {
        #region Fields

        /// <summary>
        /// Location of the program assembly (the entry point of the client application).
        /// </summary>
        private readonly string programFileName;

        /// <summary>
        /// Directory containing <see cref="programFileName"/>.
        /// </summary>
        private readonly string programDirectory;

        /// <summary>
        /// Location of the PostSharp project file.
        /// </summary>
        private readonly string projectFileName;

        /// <summary>
        /// Location of the shadow directory, where weaved assemblies are stored.
        /// </summary>
        private string shadowDirectory;

        /// <summary>
        /// Collection of properties passed to the project.
        /// </summary>
        private readonly PropertyCollection projectParameters;

        /// <summary>
        /// Array of arguments that need to be passed to the client program's main entry point.
        /// </summary>
        private readonly string[] programArguments;

        private RemotingAccessor<ClientDomainManager> clientDomainManager;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="Host"/> instance.
        /// </summary>
        /// <param name="programFileName">Location of the program assembly (the entry point of the client application).</param>
        /// <param name="programArguments">Array of arguments that need to be passed to the client program's main entry point.</param>
        /// <param name="projectFileName">Location of the PostSharp project file.</param>
        /// <param name="projectParameters">Collection of properties passed to the project.</param>
        public Host(
            string programFileName,
            string[] programArguments,
            string projectFileName,
            PropertyCollection projectParameters )
        {
            // We just store the parameter as instance fields.
            this.programFileName = programFileName;
            this.programDirectory = Path.GetDirectoryName( programFileName );
            this.projectFileName = projectFileName;
            this.projectParameters = projectParameters;
            this.programArguments = programArguments;
        }

        /// <summary>
        /// Executes the load and transformation process and finally executes the woven client
        /// program. The current method is the real entry point of this sample.
        /// </summary>
        public void Execute()
        {
            // Create a shadow directory.
            shadowDirectory = Path.Combine( Path.GetTempPath(),
                                            Guid.NewGuid().ToString() );
            Console.WriteLine( "Using the shadow directory {{{0}}}.", shadowDirectory );
            Directory.CreateDirectory( shadowDirectory );

            try
            {
                // Set up the client AppDomain.
                AppDomainSetup clientAppDomainSetup = new AppDomainSetup
                                                          {
                                                              ApplicationName =
                                                                  Path.GetFileNameWithoutExtension( programFileName ),
                                                              ConfigurationFile = ( programFileName + ".config" )
                                                          };

                AppDomain clientAppDomain = AppDomain.CreateDomain( clientAppDomainSetup.ApplicationName,
                                                                    null, clientAppDomainSetup );

                clientDomainManager =
                    new RemotingAccessor<ClientDomainManager>(
                        (ClientDomainManager) clientAppDomain.CreateInstanceAndUnwrap(
                                                  typeof(ClientDomainManager).Assembly.FullName,
                                                  typeof(ClientDomainManager).FullName ), true );

                try
                {
                    #region Setup PostSharp then execute the program

                    // Register to messages, so we can print them on the console.
                    Messenger.Current.Message += OnMessage;

                    // Set up the project invocation for the program assembly.
                    // Other assemblies will go through the 'discovery' mechanism.
                    ProjectInvocation projectInvocation = new ProjectInvocation(
                        GetInvocationParameters( programFileName ), new ModuleLoadReflectionFromFileStrategy( programFileName ) );

                    // Prepare the PostSharp settings object.
                    PostSharpObjectSettings settings = new PostSharpObjectSettings
                                                           {
                                                               CreatePrivateAppDomain = true,
                                                               OverwriteAssemblyNames = true,
                                                               ProjectExecutionOrder = ProjectExecutionOrder.Phased,
                                                               LocalHostImplementation =
                                                                   typeof(LocalHost).AssemblyQualifiedName
                                                           };

                    // Create the PostSharp Object.
                    using ( IPostSharpObject postSharpObject = PostSharpObject.CreateInstance( settings, this ) )
                    {
                        // Invoke the processing of the project on the entry-point client assembly.
                        postSharpObject.InvokeProjects( new[] {projectInvocation} );

                        // When this is done, tell the ClientDomainManager to execute the
                        // woven assembly with given arguments.
                        clientDomainManager.Value.Execute(
                            projectInvocation.InvocationParameters.Properties["Output"],
                            this.programArguments );
                    }

                    #endregion
                }
                finally
                {
                    clientDomainManager.Dispose();
                    // Unload the client AppDomain at the end.
                    AppDomain.Unload( clientAppDomain );
                }
            }
            finally
            {
                try
                {
                    // Delete the shadow directory and it's content.
                    Directory.Delete( shadowDirectory, true );
                }
                catch
                {
                    Console.Error.WriteLine( "Warning. Could not clean the shadow directory." );
                }
            }
        }


        /// <summary>
        /// Builds a <see cref="ProjectInvocationParameters"/> object for a given source
        /// assembly file.
        /// </summary>
        /// <param name="sourceAssemblyFile">The source assembly file.</param>
        /// <returns>A <see cref="ProjectInvocationParameters"/> object requesting to process
        /// using our project file and our project parameters.</returns>
        private ProjectInvocationParameters GetInvocationParameters( string sourceAssemblyFile )
        {
            // Compute the location of the woven assembly. We put it in the shadow directory.
            string output = Path.Combine( shadowDirectory, Path.GetFileName( sourceAssemblyFile ) );

            ProjectInvocationParameters parameters = new ProjectInvocationParameters( projectFileName );
            parameters.Properties.Merge( projectParameters );
            parameters.Properties["Output"] = output;
            parameters.Properties["ResolvedReferences"] = "";
            parameters.ProcessDependenciesFirst = true;
            parameters.Tags.SetTag( SampleTask.ProjectTag, this );

            // Notify the ClientDomainManager where it will find this assembly.
            // This information is used by the ClientDomainManager's implementation
            // of AppDomain.AssemblyResolve.
            this.clientDomainManager.Value.SetAssemblyLocation(
                AssemblyName.GetAssemblyName( sourceAssemblyFile ),
                output );

            return parameters;
        }

        #region Implementation of IPostSharpHost and other callbacks

        /// <summary>
        /// Called by the <see cref="Messenger"/> class when PostSharp emits a message.
        /// Should just print the message on the console. Note that this method is not
        /// a part of the <see cref="IPostSharpHost"/> interface.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Message arguments.</param>
        private static void OnMessage( object sender, MessageEventArgs e )
        {
            string format;

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
                               e.Message.Severity.ToString().ToLower(), e.Message.MessageId, e.Message.MessageText );

            ConsoleColor oldColor = Console.ForegroundColor;
            ConsoleColor color;
            TextWriter output;

            switch ( e.Message.Severity )
            {
                case SeverityType.Error:
                case SeverityType.Fatal:
                    color = ConsoleColor.Red;
                    output = Console.Error;
                    break;

                case SeverityType.Warning:
                    color = ConsoleColor.DarkYellow;
                    output = Console.Out;
                    break;

                case SeverityType.Verbose:
                case SeverityType.Debug:
                    color = ConsoleColor.DarkGray;
                    output = Console.Out;
                    break;

                case SeverityType.ImportantInfo:
                    color = ConsoleColor.White;
                    output = Console.Out;
                    break;


                default:
                    color = oldColor;
                    output = Console.Out;
                    break;
            }

            System.Diagnostics.Trace.Flush();
            Console.ForegroundColor = color;
            output.WriteLine( message );
            Console.ForegroundColor = oldColor;
        }


        /// <summary>
        /// Method invoked by the PostSharp Object in order to determine how to process an
        /// assembly. Note that this assembly has already been resolved by the 
        /// <see cref="ResolveAssemblyReference"/> method.
        /// </summary>
        /// <param name="assemblyFileName">Location of the assembly.</param>
        /// <param name="moduleName">Name of the module to be processed.</param>
        /// <param name="assemblyName">Full assembly name.</param>
        /// <returns>A <see cref="ProjectInvocationParameters"/> object when the assembly needs
        /// to be processed (i.e. when it is located under the directory of the program assembly),
        /// or <b>null</b> otherwise.</returns>
        public ProjectInvocationParameters GetProjectInvocationParameters( string assemblyFileName, string moduleName,
                                                                           AssemblyName assemblyName )
        {
            // Check that the assembly is under the directory of the program assembly.
            if (
                Path.GetFullPath( assemblyFileName ).ToLowerInvariant().StartsWith(
                    this.programDirectory.ToLowerInvariant() ) )
            {
                // If yes, tell that we want it to be transformed using our project.
                return GetInvocationParameters( assemblyFileName );
            }
            else
            {
                // If no, the assembly should not be transformed.
                return null;
            }
        }

        /// <summary>
        /// Small utility method that tests whether a file exist in the program directory
        /// and, if yes, tests whether the assembly versions match.
        /// </summary>
        /// <param name="file">File name with extension but without directory.</param>
        /// <param name="assemblyName">Requested assembly name.</param>
        /// <returns>A full path if the file was found and assembly names match, otherwise <b>null</b>.</returns>
        private string ProbeFile( string file, AssemblyName assemblyName )
        {
            string fullPath = Path.Combine( this.programDirectory, file );

            if ( File.Exists( fullPath ) )
            {
                AssemblyName candidateAssemblyName = AssemblyName.GetAssemblyName( fullPath );
                if ( AssemblyName.ReferenceMatchesDefinition( assemblyName, candidateAssemblyName ) )
                    return fullPath;
            }
            return null;
        }

        /// <summary>
        /// Method called by the host when an assembly reference should be resolved for
        /// the first time. We look in our program directory if we don't find the requested
        /// file.
        /// </summary>
        /// <param name="assemblyName">Requested assembly name.</param>
        /// <returns>The full path of the file if we found the file, otherwise <b>false</b>.</returns>
        public string ResolveAssemblyReference( AssemblyName assemblyName )
        {
            // We just look in the application directory for the exact match.

            return ProbeFile( assemblyName.Name + ".dll", assemblyName )
                        ?? ProbeFile( assemblyName.Name + ".exe", assemblyName );
        }

        /// <summary>
        /// Method called when the host when an assembly name is overwritten. We have
        /// to inform the <see cref="ClientDomainManager"/> that it happened, so that
        /// it can update its repository of assembly locations.
        /// </summary>
        /// <param name="oldAssemblyName">Old name.</param>
        /// <param name="newAssemblyName">New name.</param>
        public void RenameAssembly( AssemblyName oldAssemblyName, AssemblyName newAssemblyName )
        {
            // Inform the ClientDomainManager so that
            // it can update its repository of assembly locations.
            this.clientDomainManager.Value.RenameAssembly( oldAssemblyName, newAssemblyName );
        }

        #endregion

        /// <summary>
        /// Sample method that is called back from a task (<see cref="SampleTask"/>),
        /// from the PostSharp AppDomain.
        /// </summary>
// ReSharper disable MemberCanBeMadeStatic
        internal void SampleCallback()
// ReSharper restore MemberCanBeMadeStatic
        {
            Console.WriteLine( "In SampleCallback()" );
        }
    }
}
