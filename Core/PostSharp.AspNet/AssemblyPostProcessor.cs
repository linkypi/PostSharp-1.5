using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;

namespace PostSharp.AspNet
{
    /// <summary>
    /// Hooks into the ASP.NET compilation process by post-processing
    /// assemblies using PostSharp (implementation of <see cref="IAssemblyPostProcessor"/>).
    /// </summary>
    /// <remarks>
    /// <para>In order to use PostSharp in a web project, specify this class
    /// as an assembly post-processor in <b>web.config</b>:</para>
    /// <code>
    /// &lt;configuration&gt;
    ///      &lt;system.web&gt;
    ///        &lt;compilation debug="true" assemblyPostProcessorType="PostSharp.AspNet.AssemblyPostProcessor, PostSharp.AspNet"/&gt;
    ///      &lt;/system.web&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// <para>Additionally, you have to add the <b>&lt;postsharp ... /&gt;</b>
    /// section in the configuration file:
    /// </para>
    /// <code>
    ///&lt;?xml version="1.0"?&gt;
    ///&lt;configuration&gt;
    ///	&lt;!-- Add a configuration handler for PostSharp. --&gt;
    ///	&lt;configSections&gt;
    ///		&lt;section name="postsharp" type="PostSharp.AspNet.PostSharpConfiguration, PostSharp.AspNet"/&gt;
    ///	&lt;/configSections&gt;
    ///	&lt;!-- PostSharp configuration --&gt;
    ///	&lt;postsharp directory="P:\open\branches\1.0\Core\PostSharp.MSBuild\bin\Debug" trace="true"&gt;
    ///		&lt;parameters&gt;
    ///			&lt;!--&lt;add name="parameter-name" value="parameter-value"/&gt;--&gt;
    ///		&lt;/parameters&gt;
    ///		&lt;searchPath&gt;
    ///			&lt;!-- Always add the binary folder to the search path. --&gt;
    ///			&lt;add name="bin" value="~\bin"/&gt;
    ///			&lt;!-- Then add the location of plug-ins that are not installed in standard locations. --&gt;
    ///			&lt;add name="laos-weaver" value="P:\open\branches\1.0\Laos\PostSharp.Laos.Weaver\bin\Debug"/&gt;
    ///		&lt;/searchPath&gt;
    ///	&lt;/postsharp&gt;
    ///	&lt;appSettings/&gt;
    ///	&lt;connectionStrings/&gt;
    ///	&lt;system.web&gt;
    ///		&lt;!-- Note the 'assemblyPostProcessorType' attribute. --&gt;
    ///		&lt;compilation debug="true" assemblyPostProcessorType="PostSharp.AspNet.AssemblyPostProcessor, PostSharp.AspNet"&gt;
    ///		&lt;authentication mode="None"/&gt;
    ///		&lt;trace enabled="true" pageOutput="true"/&gt;
    ///	&lt;/system.web&gt;
    ///&lt;/configuration&gt;
    /// </code>
    /// <para>
    /// In all configuration parameters and in search path elements, the tilde character (~) is
    /// replaced by the physical path of the application.
    /// </para>
    /// </remarks>
    /// <seealso cref="PostSharpConfiguration"/>
    public class AssemblyPostProcessor : IAssemblyPostProcessor
    {
        private readonly PostSharpConfiguration configuration;
        private readonly string project;
        private readonly string applicationPath;
        private readonly string postsharpPath;

        /// <summary>
        /// Initializes a new <see cref="AssemblyPostProcessor"/>.
        /// </summary>
        public AssemblyPostProcessor()
        {
            this.configuration = WebConfigurationManager.GetSection( "postsharp" ) as PostSharpConfiguration ?? new PostSharpConfiguration();

            if ( string.IsNullOrEmpty( HttpRuntime.AppDomainAppPath ) )
                throw new InvalidOperationException( "Cannot invoke PostSharp.AspNet outside of an ASP.NET context." );


            this.applicationPath = HttpRuntime.AppDomainAppPath;
            string directory = configuration.Directory;
            if ( string.IsNullOrEmpty( directory ))
                directory = "%POSTSHARP15%";
            directory = directory.Replace( "~", this.applicationPath );
            this.postsharpPath = Environment.ExpandEnvironmentVariables( directory );

            if ( string.IsNullOrEmpty( configuration.Project ) )
                project = Path.Combine( this.postsharpPath, "Default.psproj" );
            else
                project = Path.Combine( this.applicationPath, configuration.Project );
        }

        /// <summary>
        /// Method invoked after the C# or VB compiler has done its job.
        /// </summary>
        /// <param name="path">Full path to the newly created assembly.</param>
        public void PostProcessAssembly( string path )
        {
            try
            {
                // If the C# compiler fails, we get an inexisting path here. But we should
                // not throw an exception, otherwise the user will not see C# error messages.
                if ( !File.Exists( path ) )
                    return;

                
                string commandLine = Path.Combine( Environment.ExpandEnvironmentVariables( this.postsharpPath.Replace( "~", this.applicationPath ) ), "postsharp.exe" );
                if ( !File.Exists( commandLine ) )
                {
                    throw new ConfigurationErrorsException(
                        string.Format( "Cannot find the file: '{0}'.", commandLine ) );
                }

                // We create our directories there and take a copy of the input assembly.
                string shadowDirectory = Path.Combine( Path.GetDirectoryName( path ), "before-postsharp" );
                if ( !Directory.Exists( shadowDirectory ) )
                    Directory.CreateDirectory( shadowDirectory );
                string shadowAssembly = Path.Combine( shadowDirectory, Path.GetFileName( path ) );
                File.Copy( path, shadowAssembly, true );
                string pdbFile = Path.ChangeExtension( path, ".pdb" );
                string shadowPdb = Path.Combine( shadowDirectory, Path.GetFileName( pdbFile ) );
                if ( File.Exists( pdbFile ) )
                    File.Copy( pdbFile, shadowPdb, true );
                string intermediateDirectory = Path.Combine( Path.GetDirectoryName( path ), "postsharp" );
                if ( !Directory.Exists( intermediateDirectory ) )
                    Directory.CreateDirectory( intermediateDirectory );
                string outputAssembly = Path.Combine( intermediateDirectory, Path.GetFileName( path ) );
                string outputPdb = Path.Combine( intermediateDirectory, Path.GetFileName( pdbFile ) );

                Process process = new Process
                                      {
                                          StartInfo =
                                              {
                                                  FileName = commandLine,
                                                  ErrorDialog = false,
                                                  CreateNoWindow = true,
                                                  UseShellExecute = false,
                                                  RedirectStandardError = true,
                                                  RedirectStandardOutput = true
                                              }
                                      };


                StringBuilder arguments = new StringBuilder();
                arguments.AppendFormat(
                    "\"/P:Output={0}\" \"/P:IntermediateDirectory={1} \"  /P:CleanIntermediate=False /P:ReferenceDirectory=. /P:SignAssembly=False /P:PrivateKeyLocation= /P:ResolvedReferences= \"/P:SearchPath=",
                    outputAssembly, intermediateDirectory );

                foreach ( NameValueConfigurationElement searchPath in this.configuration.SearchPath )
                {
                    string value = Environment.ExpandEnvironmentVariables(  searchPath.Value.Replace( "~", this.applicationPath ) );

                    
                    arguments.Append( value );
                    arguments.Append( "," );
                }

                arguments.Append( "\" " );

                foreach ( NameValueConfigurationElement parameter in this.configuration.Parameters )
                {
                    arguments.Append( "\"/P:" );
                    arguments.Append( parameter.Name );
                    arguments.Append( '=' );
                    arguments.Append( parameter.Value.Replace( "~", applicationPath ) );
                    arguments.Append( "\" " );
                }

                if ( this.configuration.AttachDebugger )
                {
                    arguments.Append( "/Attach " );
                }

                if ( this.configuration.Trace )
                {
                    arguments.Append( "/V " );
                }

                arguments.Append( "/SkipAutoUpdate \"" );
                arguments.Append( this.project );
                arguments.Append( "\" \"" );
                arguments.Append( shadowAssembly );
                arguments.Append( '\"' );

                process.StartInfo.Arguments = arguments.ToString();

                LogWriter logWriter;
                string logMessage;
                if ( this.configuration.Trace )
                {
                    string logFile = Path.Combine( Path.GetTempPath(),
                                                   string.Format( "postsharp-aspnet-{0:yyyy-MM-dd}.log", DateTime.Now ) );
                    logMessage = string.Format( " See file '{0}' for details.", logFile );
                    logWriter = new LogWriter( logFile );
                    logWriter.Writer.WriteLine( "--------------------------------------------------------------" );
                    logWriter.Writer.WriteLine( process.StartInfo.FileName + " " + process.StartInfo.Arguments );
                    process.ErrorDataReceived += logWriter.OnPostSharpDataReceived;
                    process.OutputDataReceived += logWriter.OnPostSharpDataReceived;
                }
                else
                {
                    logWriter = null;
                    logMessage = " Enable PostSharp tracing to see details.";
                }

                process.ErrorDataReceived += ( ( sender, e ) => Console.WriteLine( e.Data ) );
                process.OutputDataReceived += ( ( sender, e ) => Console.WriteLine( e.Data ) );

                Console.WriteLine( "Starting: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments );
                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                bool success = process.WaitForExit( 60000 );

                if ( logWriter != null )
                    logWriter.Dispose();

                if ( !success )
                    throw new ApplicationException( "PostSharp did not finish in due time." + logMessage );

                if ( process.ExitCode != 0 )
                    throw new ApplicationException( "PostSharp did not complete successfully." + logMessage );

                File.Copy( outputAssembly, path, true );
                if ( File.Exists( outputPdb ) )
                    File.Copy( outputPdb, pdbFile, true );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.ToString() );
                throw;
            }
        }

        /// <summary>
        /// Disposes the current object.
        /// </summary>
        public void Dispose()
        {
        }

        private class LogWriter : IDisposable
        {
            private StreamWriter writer;

            public LogWriter( string fileName )
            {
                this.writer = new StreamWriter( fileName, true, Encoding.UTF8 );
            }

            public StreamWriter Writer
            {
                get { return this.writer; }
            }


            public void OnPostSharpDataReceived( object sender, DataReceivedEventArgs e )
            {
                if ( writer != null )
                {
                    writer.WriteLine( e.Data );
                }
            }

            public void Dispose()
            {
                if ( this.writer != null )
                {
                    this.writer.Flush();
                    this.writer.Dispose();
                    this.writer = null;
                }
            }
        }
    }
}