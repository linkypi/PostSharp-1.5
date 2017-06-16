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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections.Specialized;
using PostSharp.Extensibility.Configuration;
using PostSharp.PlatformAbstraction;
using PostSharp.Utilities;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Contains the actual configuration of the application or project at a
    /// given time in the project loading process, i.e. with resolved
    /// properties and references.
    /// </summary>
    [Serializable]
    public sealed class Project : MarshalByRefObject, IDisposable, ITaggable, IProject
    {
        #region Fields

        private static readonly char[] versionRangeSeparators = new[] {'-'};

        /// <summary>
        /// Regular expression parsing property references.
        /// </summary>
        private static readonly Regex expressionRegex = new Regex( @"\{\$(?<name>[^\}]*)\}" );

        /// <summary>
        /// Collection of directories forming the path. The dictionary value
        /// is a boolean indicating whether the path was added to the Assembly
        /// Binder. If yes, it should be removed when the project is disposed.
        /// </summary>
        private readonly Dictionary<string, bool> path =
            new Dictionary<string, bool>( StringComparer.InvariantCultureIgnoreCase );

        /// <summary>
        /// Platforms by name.
        /// </summary>
        private readonly PlatformConfigurationDictionary platforms = new PlatformConfigurationDictionary();

        /// <summary>
        /// Properties.
        /// </summary>
        private readonly PropertyCollection properties = new PropertyCollection();

        /// <summary>
        /// Task types by name.
        /// </summary>
        private readonly TaskTypeConfigurationDictionary taskTypes = new TaskTypeConfigurationDictionary();


        /// <summary>
        /// Set of included plug-ins (to avoid cycles).
        /// </summary>
        private readonly StringSet plugIns = new StringSet();

        /// <summary>
        /// Project-level configuration, or <b>null</b> if no project has been loaded.
        /// </summary>
        private ProjectConfiguration projectConfiguration;

        private bool disposed;

        private ModuleDeclaration module;

        private Domain domain;


        /// <summary>
        /// Target platform.
        /// </summary>
        private TargetPlatformAdapter platform;

        /// <summary>
        /// Collection of task instances.
        /// </summary>
        private readonly TaskCollection tasks;

        private readonly TagCollection tags = new TagCollection();

        private static ApplicationConfiguration applicationConfiguration;

        private static readonly PhaseConfigurationDictionary phasesbyName = new PhaseConfigurationDictionary();

        private string referenceDirectory;

        private readonly Dictionary<string, AssemblyName> strongNames =
            new Dictionary<string, AssemblyName>( StringComparer.InvariantCultureIgnoreCase );

        #endregion

        #region Properties

        /// <summary>
        /// Gets the directory of platforms indexed by name.
        /// </summary>
        public PlatformConfigurationDictionary Platforms
        {
            get { return this.platforms; }
        }

        /// <summary>
        /// Gets the collection of properties.
        /// </summary>
        public PropertyCollection Properties
        {
            get { return this.properties; }
        }

        /// <summary>
        /// Gets the directory of task types indexed by name.
        /// </summary>
        public TaskTypeConfigurationDictionary TaskTypes
        {
            get { return this.taskTypes; }
        }

        /// <summary>
        /// Gets the collection of phases.
        /// </summary>
        public static PhaseConfigurationDictionary Phases
        {
            get { return phasesbyName; }
        }

        /// <summary>
        /// Gets the collection of plug-ins included at this point.
        /// </summary>
        /// <remarks>
        /// This collection contains the full paths of loaded plug-ins. This is to avoid
        /// cycles in references.
        /// </remarks>
        public StringSet PlugIns
        {
            get { return this.plugIns; }
        }

        private bool AddPlugIn( string shortName, string declaringContextName )
        {
            Trace.ProjectLoader.WriteLine( "Adding the plug-in {0}.", shortName );

            string fullName = shortName + ".psplugin";
            if ( this.FindFile( ref fullName, null ) )
            {
                fullName = CanonizePath( fullName );
                if ( !this.plugIns.Contains( fullName ) )
                {
                    this.PlugIns.Add( fullName );
                    this.LoadPlugInConfiguration( fullName, declaringContextName );
                }
                else
                {
                    Trace.ProjectLoader.WriteLine( "The plug-in {0} was already loaded.", shortName );
                }

                return true;
            }
            else
            {
                Trace.ProjectLoader.WriteLine( "The plug-in {0} was not found.", shortName );
                return false;
            }
        }


        /// <summary>
        /// Adds and load a plugin in the current project.
        /// </summary>
        /// <param name="shortName">Name of the plug-in (without directory or extension)</param>
        /// <returns><b>true</b> if the plug-in was just added, <b>false</b> if it was
        /// already loaded.</returns>
        public bool AddPlugIn( string shortName )
        {
            return this.AddPlugIn( shortName, null );
        }

        /// <summary>
        /// Gets the project configuration element.
        /// </summary>
        /// <value>
        /// A <see cref="ProjectConfiguration"/>, or <b>null</b> if no project has been loaded.
        /// </value>
        public ProjectConfiguration ProjectConfiguration
        {
            get
            {
                this.AssertNotDisposed();
                return this.projectConfiguration;
            }
        }

        /// <summary>
        /// Gets the target <see cref="ModuleDeclaration"/> in its current state.
        /// </summary>
        public ModuleDeclaration Module
        {
            get { return this.module; }
        }

        /// <summary>
        /// Gets the collection of project tasks,
        /// </summary>
        public TaskCollection Tasks
        {
            get { return this.tasks; }
        }

        /// <summary>
        /// Gets the target platform.
        /// </summary>
        public TargetPlatformAdapter Platform
        {
            get
            {
                if ( this.platform == null )
                {
                    this.platform = this.GetPlatformAdapter();
                }

                return this.platform;
            }
        }

        /// <summary>
        /// Gets the mappings between assembly friendly names and the corresponding strong names.
        /// </summary>
        /// <remarks>
        /// This dictionary is used to automatically map short names to strong names in case
        /// a short name is found in a configuration file.
        /// </remarks>
        public IDictionary<string, AssemblyName> StrongNames
        {
            get { return this.strongNames; }
        }

        #endregion

        #region Construction

        private Project()
        {
            ExceptionHelper.Core.AssertValidOperation( CustomAssemblyBinder.Instance != null,
                                                       "ProjectRequiresCustomAssemblyBinder" );

            this.tasks = new TaskCollection( this );
        }

        private void LoadSourceModule( ModuleLoadStrategy moduleLoadStrategy )
        {
            // Add the directory containing the source module in the search path.
            ModuleLoadReflectionFromFileStrategy moduleLoadReflectionFromFileStrategy =
                moduleLoadStrategy as ModuleLoadReflectionFromFileStrategy;
            if ( moduleLoadReflectionFromFileStrategy != null )
            {
                string sourceDirectory = Path.GetDirectoryName( moduleLoadReflectionFromFileStrategy.FileName );
                Trace.ProjectLoader.WriteLine(
                    "Adding {{{0}}} to the search path because it contains the source assembly.",
                    sourceDirectory );
                this.AddPath( sourceDirectory );
            }

            this.module = moduleLoadStrategy.Load( domain );
        }

        /// <summary>
        /// Gets the application-level configuration.
        /// </summary>
        public static ApplicationConfiguration ApplicationConfiguration
        {
            get
            {
                if ( applicationConfiguration == null )
                {
                    // First try the configuration file in the same file as the library.
                    string fileName = Path.Combine( ApplicationInfo.BaseDirectory, "PostSharp-Platform.config" );
                    string lookdIntoLocations = fileName;

                    if ( !File.Exists( fileName ) )
                    {
                        // Then fall back on the primary location.
                        if ( ApplicationInfo.BaseDirectory != null )
                        {
                            fileName =
                                Path.Combine( ApplicationInfo.BaseDirectory, "PostSharp-Platform.config" );
                            lookdIntoLocations += ", " + fileName;
                            if ( !File.Exists( fileName ) )
                            {
                                fileName = null;
                            }
                        }
                        else
                        {
                            fileName = null;
                        }
                    }

                    if ( fileName == null )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0002",
                                                          new object[] {lookdIntoLocations} );
                    }

                    applicationConfiguration =
                        (ApplicationConfiguration)
                        ConfigurationHelper.DeserializeDocument( fileName, ConfigurationDocumentKind.Application );
                    applicationConfiguration.FileName = fileName;

                    for ( int i = 0; i < ApplicationConfiguration.Phases.Count; i++ )
                    {
                        PhaseConfiguration phase = ApplicationConfiguration.Phases[i];
                        phasesbyName.Add( phase );
                    }
                }

                return applicationConfiguration;
            }
        }

        /// <summary>
        /// Loads the application-level configuration in the current instance.
        /// </summary>
        private void LoadApplicationConfiguration()
        {
            Trace.ProjectLoader.WriteLine( "Loading the application confiration." );

            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
            {
                this.LoadBaseConfiguration( ApplicationConfiguration, ApplicationConfiguration.FileName, null );


                Trace.Timings.WriteLine( "Application-level configuration loaded in {0} ms.", timer.CurrentTime );
            }
        }

        /// <summary>
        /// Loads the configuration of a plug-in in the current instance.
        /// </summary>
        /// <param name="plugInFileName">Full plug-in file name.</param>
        /// <param name="declaringFileName">Name of the file that includes the plug-in 
        /// (for error reporting).</param>
        private void LoadPlugInConfiguration( string plugInFileName, string declaringFileName )
        {
            Trace.ProjectLoader.WriteLine( "Loading the plugin configuration {0}.", plugInFileName );

            if ( !File.Exists( plugInFileName ) )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0010", new object[] {plugInFileName},
                                                  declaringFileName );
            }

            this.LoadBaseConfiguration(
                ConfigurationHelper.DeserializeDocument( plugInFileName, ConfigurationDocumentKind.PlugIn ),
                plugInFileName, null );

            Trace.ProjectLoader.WriteLine( "Back to {0}.", declaringFileName );
        }

        private static Project LoadProject( Domain domain, string fileName, PropertyCollection properties )
        {
            return LoadProject( domain, ProjectConfiguration.LoadProject( fileName ), properties );
        }


        private static Project LoadProject( Domain domain, ProjectConfiguration projectConfiguration,
                                            PropertyCollection properties )
        {
            Project project = new Project {domain = domain, projectConfiguration = projectConfiguration};

            project.properties.Merge( properties );

            // Add the source module directory to the path.
            string directory = Path.GetDirectoryName( projectConfiguration.FileName );

            project.properties["ProjectDirectory"] = directory;
            project.properties["WorkingDirectory"] = Environment.CurrentDirectory;

            // If the "ReferenceDirectory" property has not been set from outside, set it to the current directory.
            if ( project.properties["ReferenceDirectory"] == null )
            {
                Trace.ProjectLoader.WriteLine(
                    "Setting the ReferenceDirectory property to the working directory {{{0}}} " +
                    "because it was not defined externally.", Environment.CurrentDirectory );
                project.properties["ReferenceDirectory"] = Environment.CurrentDirectory;
            }
            else
            {
                Trace.ProjectLoader.WriteLine( "The ReferenceDirectory property was set externally to {{{0}}}.",
                                               project.properties["ReferenceDirectory"] );
            }


            // Set up default properties
            project.properties["PostSharpDirectory"] = ApplicationInfo.BaseDirectory;
            project.properties["PostSharpVersion"] = ApplicationInfo.Version.ToString();
            project.properties["ApplicationDataDirectory"] =
                Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

            if ( project.referenceDirectory != null )
            {
                project.referenceDirectory = project.Evaluate( projectConfiguration.ReferenceDirectory );

                if ( project.referenceDirectory == null )
                {
                    CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0085", null );
                }
            }

            if ( String.IsNullOrEmpty( project.referenceDirectory ) )
            {
                project.referenceDirectory = project.properties["ReferenceDirectory"];
            }
            else
            {
                project.referenceDirectory = Path.GetFullPath( project.referenceDirectory );
            }


            Trace.ProjectLoader.WriteLine( "The project reference directory is {{{0}}}.", project.referenceDirectory );

            // Load the project configuration files.
            project.LoadApplicationConfiguration();
            Trace.ProjectLoader.WriteLine( "Loading the project {0}.", projectConfiguration.FileName );
            project.LoadBaseConfiguration( projectConfiguration, projectConfiguration.FileName,
                                           projectConfiguration.ReferenceDirectory );


            // Deserialize tasks.
            project.DeserializeTasks();

            return project;
        }


        /// <summary>
        /// Creates a new <see cref="Project"/> instance and assigns it to
        /// a module that has not already been loaded.
        /// </summary>
        /// <param name="projectFileName">The location of the project file to be executed.</param>
        /// <param name="domain">The <see cref="Domain"/> into which the module has to be loaded.</param>
        /// <param name="moduleLoadStrategy">Specifies how the assembly should be loaded.</param>
        /// <param name="properties">Collection of properties for the project execution.</param>
        /// <param name="tags">Collection of tags to be applied to the project, or <b>null</b>
        /// if no tag has to be applied.</param>
        /// <returns><b>true</b> if the project was successful, otherwise <b>false</b>.</returns>
        public static Project CreateInstance( string projectFileName, Domain domain,
                                              ModuleLoadStrategy moduleLoadStrategy, PropertyCollection properties,
                                              TagCollection tags )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( domain, "domain" );
            ExceptionHelper.AssertArgumentNotNull( moduleLoadStrategy, "assemblyLoadArgs" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( projectFileName, "projectFileName" );

            #endregion

            Project project = LoadProject( domain, projectFileName, properties );
            if ( tags != null )
            {
                tags.CopyTo( project );
            }

            project.LoadSourceModule( moduleLoadStrategy );

            return project;
        }

        /// <summary>
        /// Creates a new <see cref="Project"/> and assigns it to
        /// a module has already been loaded as a <see cref="ModuleDeclaration"/>.
        /// </summary>
        /// <param name="projectFileName">Location of the project file to be executed.</param>
        /// <param name="properties">Collection of properties for the project execution.</param>
        /// <param name="module">Module that needs to be processed.</param>
        /// <param name="tags">Collection of tags to be applied to the project, or <b>null</b>
        /// if no tag has to be applied.</param>
        /// <returns><b>true</b> if the project was successful, otherwise <b>false</b>.</returns>
        public static Project CreateInstance( string projectFileName,
                                              ModuleDeclaration module, PropertyCollection properties,
                                              TagCollection tags )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( module, "module" );
            ExceptionHelper.AssertArgumentNotEmptyOrNull( projectFileName, "projectFileName" );

            #endregion

            Project project = LoadProject( module.Domain, projectFileName, properties );
            if ( tags != null )
            {
                tags.CopyTo( project );
            }
            project.module = module;

            string moduleDirectory = Path.GetDirectoryName( module.FileName );
            Trace.ProjectLoader.WriteLine( "Adding {{{0}}} to the search path because it contains the source module.",
                                           moduleDirectory );
            project.AddPath( moduleDirectory );

            return project;
        }

        /// <summary>
        /// Add a directory to the project search path.
        /// </summary>
        /// <param name="path">A directory name.</param>
        public void AddPath( string path )
        {
            ExceptionHelper.AssertArgumentNotEmptyOrNull( path, "path" );

            if ( !String.IsNullOrEmpty( path ) && !this.path.ContainsKey( path ) )
            {
                bool addedInBinder = this.domain.AssemblyLocator.AddDirectory( path );
                this.path.Add( path, addedInBinder );
            }
        }


        /// <summary>
        /// Loads in the current instance all configuration elements that are
        /// common to projects, plug-in and application.
        /// </summary>
        /// <param name="source">The source configuration.</param>
        /// <param name="fileName">The name of the file from where the <paramref name="source"/>
        /// comes from (for error reporting).</param>
        /// <param name="referenceDirectory">Directory according to which relative paths are resolved,
        /// or <b>null</b> if relative paths have to be resolved according to the directory containing 
        /// the file <paramref name="fileName"/>.</param>
        private void LoadBaseConfiguration( BaseConfiguration source, string fileName, string referenceDirectory )
        {
            Trace.ProjectLoader.WriteLine( "Loading configuration from file {{{0}}}, reference directory = {{{1}}}.",
                                           fileName, referenceDirectory );

            ValidatePath( fileName );
            
            source.FileName = fileName;

            #region Properties

            if ( source.Properties != null )
            {
                foreach ( PropertyConfiguration property in source.Properties )
                {
                    property.Parent = source;

                    string newValue = this.Evaluate( property.Value );

                    this.properties[property.Name] = newValue;
                }
            }

            #endregion

            #region Path

            string fileDirectory = Path.GetDirectoryName( Path.GetFullPath( fileName ) );
            this.AddPath( fileDirectory );

            Trace.ProjectLoader.WriteLine( "Adding {{{0}}} to the search path because it contains the file {{{1}}}.",
                                           fileDirectory, Path.GetFileName( fileName ) );

            string resolvedReferenceDirectory;

            if ( !String.IsNullOrEmpty( referenceDirectory ) )
            {
                resolvedReferenceDirectory = this.Evaluate( referenceDirectory );

                ValidatePath( referenceDirectory );

                if ( resolvedReferenceDirectory == null )
                {
                    CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0076", null, fileName );
                }

                if ( String.Compare( fileDirectory, resolvedReferenceDirectory ) != 0 )
                {
                    Trace.ProjectLoader.WriteLine(
                        "Adding {{{0}}} to the search path because it is the reference directory for the file {{{1}}}.",
                        fileDirectory, Path.GetFileName( fileName ) );
                    this.AddPath( referenceDirectory );
                }
            }
            else
            {
                resolvedReferenceDirectory = fileDirectory;
            }


            if ( source.SearchPath != null )
            {
                foreach ( SearchPathConfiguration searchPath in source.SearchPath )
                {
                    if ( !String.IsNullOrEmpty( searchPath.Directory ) )
                    {
                        string resolvedPath = this.Evaluate( searchPath.Directory );
                        if ( resolvedPath != null )
                        {
                            foreach ( string pathElement in resolvedPath.Split( ';', ',' ) )
                            {
                                if ( pathElement.Trim().Length > 0 )
                                {
                                    string directory = CanonizePath( pathElement );

                                    Trace.ProjectLoader.WriteLine(
                                        "Adding {{{0}}} to the search path because of a SearchPath element (relatively to directory {{{1}}})..",
                                        directory, resolvedReferenceDirectory );

                                    string fullPath =
                                        Path.GetFullPath( Path.Combine( resolvedReferenceDirectory, directory ) );


                                    this.AddPath( fullPath );
                                }
                            }
                        }
                    }

                    if ( !String.IsNullOrEmpty( searchPath.File ) )
                    {
                        string resolvedFile = this.Evaluate( searchPath.File );
                        if ( resolvedFile != null )
                        {
                            foreach ( string fileElement in resolvedFile.Split( ';', ',' ) )
                            {
                                if ( fileElement.Trim().Length > 0 )
                                {
                                    string file = CanonizePath( fileElement );

                                    Trace.ProjectLoader.WriteLine(
                                        "Adding the file {{{0}}} to the search path because of a SearchPath element (relatively to directory {{{1}}})..",
                                        file, resolvedReferenceDirectory );

                                    string fullPath =
                                        Path.GetFullPath( Path.Combine( resolvedReferenceDirectory, file ) );


                                    this.domain.AssemblyLocator.AddFile( fullPath );
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Include

            if ( source.UsedPlugIns != null )
            {
                foreach ( UsingConfiguration include in source.UsedPlugIns )
                {
                    string includeFilePath = CanonizePath( this.Evaluate( include.PlugInFile ) );

                    if ( includeFilePath == null )
                    {
                        continue;
                    }

                    if ( !this.FindFile( ref includeFilePath, resolvedReferenceDirectory ) )
                    {
                        // The file was not found.
                        CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0008", new object[] {includeFilePath},
                                                          fileName );
                    }
                    else
                    {
                        // Check if the include was already loaded.
                        if ( !this.PlugIns.Contains( includeFilePath ) )
                        {
                            // Not yet loaded.
                            this.PlugIns.Add( includeFilePath );
                            this.LoadPlugInConfiguration( includeFilePath, fileName );
                        }
                    }
                }
            }

            #endregion

            #region Platforms

            if ( source.Platforms != null )
            {
                foreach ( PlatformConfiguration platformConfiguration in source.Platforms )
                {
                    platformConfiguration.Parent = source;

                    this.Platforms[platformConfiguration.Name] = platformConfiguration;
                }
            }

            #endregion

            #region Task Types

            if ( source.TaskTypes != null )
            {
                foreach ( TaskTypeConfiguration taskType in source.TaskTypes )
                {
                    taskType.Parent = source;

                    this.TaskTypes[taskType.Name] = taskType;

                    if ( taskType.Dependencies != null )
                    {
                        foreach ( DependencyConfiguration dependency in taskType.Dependencies )
                        {
                            dependency.Parent = taskType;
                        }
                    }
                }
            }

            #endregion

            #region Redirections

            if ( source.AssemblyBinding != null )
            {
                if ( !( source is ApplicationConfiguration ) )
                    ProcessAssemblyRedirections( this.domain, source.AssemblyBinding.DependentAssemblies );

                for ( int i = 0; i < source.AssemblyBinding.ImportAssemblyBindings.Count; i++ )
                {
                    ImportAssemblyBindingsConfiguration import = source.AssemblyBinding.ImportAssemblyBindings[i];

                    string href = this.Evaluate( import.File );

                    if ( href == null )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Error,
                                                          "PS0098", new object[] {i + 1, "File"}, fileName );
                        continue;
                    }

                    string select = this.Evaluate( import.Select );

                    if ( select == null )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Error,
                                                          "PS0098", new object[] {i + 1, "Select"}, fileName );
                        continue;
                    }

                    XmlDocument document = new XmlDocument();
                    try
                    {
                        Trace.ProjectLoader.WriteLine( "Processing file {0}.", href );

                        document.Load( href );
                        XmlNamespaceManager nsManager = new XmlNamespaceManager( document.NameTable );
                        nsManager.AddNamespace( "bind", "urn:schemas-microsoft-com:asm.v1" );
                        XmlElement fragment = document.SelectSingleNode( select, nsManager ) as XmlElement;

                        if ( fragment == null )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error,
                                                              "PS0097", new object[] {select, href}, fileName );
                            continue;
                        }

                        AssemblyBindingExternalConfiguration externalBindings = (AssemblyBindingExternalConfiguration)
                                                                                ConfigurationHelper.DeserializeFragment(
                                                                                    fragment,
                                                                                    ConfigurationDocumentKind.ExternalAssemblyBinding );

                        ProcessAssemblyRedirections( this.domain, externalBindings.DependentAssemblies );
                    }
                    catch ( XmlException e )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Error,
                                                          "PS0095", new object[] {e.Message}, href,
                                                          e.LineNumber, e.LinePosition );
                        continue;
                    }
                }
            }

            #endregion
        }

        internal static void ProcessAssemblyRedirections( Domain domain,
                                                          DependentAssemblyConfigurationCollection redirections )
        {
            foreach ( DependentAssemblyConfiguration dependentAssemblyConfiguration in redirections )
            {
                // TODO: better input checks.
                byte[] publicKeyToken =
                    AssemblyNameHelper.ParseBytes( dependentAssemblyConfiguration.AssemblyIdentity.PublicKeyToken );


                // Parse versions.
                Version oldVersionLowerBound, oldVersionUpperBound, newVersion;

                try
                {
                    newVersion =
                        String.IsNullOrEmpty( dependentAssemblyConfiguration.BindingRedirect.NewVersion )
                            ? null
                            :
                                new Version( dependentAssemblyConfiguration.BindingRedirect.NewVersion );

                    if ( String.IsNullOrEmpty( dependentAssemblyConfiguration.BindingRedirect.OldVersion ) )
                    {
                        oldVersionLowerBound = null;
                        oldVersionUpperBound = null;
                    }
                    else
                    {
                        string[] versionStrings =
                            dependentAssemblyConfiguration.BindingRedirect.OldVersion.Split( versionRangeSeparators );

                        switch ( versionStrings.Length )
                        {
                            case 1:
                                oldVersionLowerBound = new Version( versionStrings[0] );
                                oldVersionUpperBound = oldVersionLowerBound;
                                break;

                            case 2:
                                oldVersionLowerBound = new Version( versionStrings[0] );
                                oldVersionUpperBound = new Version( versionStrings[1] );
                                break;

                            default:
                                throw new AssertionFailedException();
                        }
                    }
                }
                catch ( Exception e )
                {
                    CoreMessageSource.Instance.Write( SeverityType.Fatal,
                                                      "PS0096",
                                                      new object[]
                                                          {
                                                              dependentAssemblyConfiguration.AssemblyIdentity.Name,
                                                              e.Message
                                                          } );

                    continue;
                }


                domain.AssemblyRedirectionPolicies.AddPolicy(
                    new AssemblyRedirectionPolicy(
                        dependentAssemblyConfiguration.AssemblyIdentity.Name,
                        publicKeyToken,
                        dependentAssemblyConfiguration.AssemblyIdentity.Culture,
                        oldVersionLowerBound,
                        oldVersionUpperBound,
                        dependentAssemblyConfiguration.BindingRedirect.NewName,
                        AssemblyNameHelper.ParseBytes( dependentAssemblyConfiguration.BindingRedirect.NewPublicKeyToken ),
                        newVersion
                        ) );
            }
        }

        private void DeserializeTasks()
        {
            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
            {
                if ( this.projectConfiguration.TasksElement != null )
                {
                    bool hasErrors = false;
                    List<Task> newTasks = new List<Task>();

                    foreach ( XmlNode node in this.projectConfiguration.TasksElement.ChildNodes )
                    {
                        switch ( node.NodeType )
                        {
                            case XmlNodeType.Comment:
                            case XmlNodeType.Whitespace:
                                // Ignored.
                                continue;

                            case XmlNodeType.Element:
                                // Will be processed.
                                break;

                            default:
                                CoreMessageSource.Instance.Write( SeverityType.Error,
                                                                  "PS0036", new object[] {node.NodeType, node.Name},
                                                                  this.projectConfiguration.FileName );
                                continue;
                        }

                        XmlElement element = (XmlElement) node;

                        // Get the task type.
                        TaskTypeConfiguration taskTypeConfiguration;
                        if ( !this.TaskTypes.TryGetValue( node.Name, out taskTypeConfiguration ) )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Error, "PS0079", new object[] {node.Name},
                                                              this.projectConfiguration.FileName );
                            hasErrors = true;
                            continue;
                        }
                        else
                        {
                            Task task = taskTypeConfiguration.CreateInstance( element, this );
                            if ( task != null )
                            {
                                newTasks.Add( task );
                            }
                            else
                            {
                                hasErrors = true;
                            }
                        }
                    }

                    if ( hasErrors )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Fatal,
                                                          "PS0084", new object[0] );
                        return;
                    }

                    this.tasks.AddRange( newTasks );
                }

                Trace.Timings.WriteLine( "Tasks deserialized in {0} ms.", timer.CurrentTime );

                return;
            }
        }

        /// <summary>
        /// Gets the <see cref="TargetPlatformAdapter"/> for the current project.
        /// </summary>
        /// <returns>The <see cref="TargetPlatformAdapter"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        private TargetPlatformAdapter GetPlatformAdapter()
        {
            PlatformConfiguration platformConfiguration;

            string platformName = this.properties["TargetPlatform"] ??
                                  PlatformAbstraction.Platform.Current.DefaultTargetPlatformName;

            if ( !platforms.TryGetValue( platformName, out platformConfiguration ) )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0029",
                                                  new object[] {platformName} );
                return null; // Unreacheable.
            }


            // Get the platform type.
            Type platformType = Type.GetType( platformConfiguration.Implementation, false );

            if ( platformType == null )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0031", new object[]
                                                                                    {
                                                                                        platformConfiguration.Name,
                                                                                        platformConfiguration.
                                                                                            Implementation
                                                                                    } );
                // Unreachabe.
                return null;
            }

            // Get the platform constructor.
            ConstructorInfo platformConstructor = platformType.GetConstructor(
                new[] {typeof(NameValueCollection)} );

            return (TargetPlatformAdapter) platformConstructor.Invoke(
                                               new object[]
                                                   {
                                                       ConfigurationHelper.ConvertNameValueCollection(
                                                           platformConfiguration.Parameters )
                                                   } );
        }

        #endregion

        #region Working with paths and properties

        /// <summary>
        /// Gets the full path of a path that may be given either absolutely either relatively.
        /// </summary>
        /// <param name="path">Path that may be given either absolutely, either relatively to the reference directory
        /// of the current project. This path may contain expressions.</param>
        /// <returns>The full path corresponding to <paramref name="path"/>, or <b>null</b> if <paramref name="path"/>
        /// contained an expression that could not be resolved.</returns>
        public string GetFullPath( string path )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( path, "path" );

            #endregion

            string resolvedPath = this.Evaluate( path );
            if ( resolvedPath == null )
                return null;

            return Path.GetFullPath( Path.Combine( this.referenceDirectory, resolvedPath ) );
        }

        /// <summary>
        /// Converts to directory separators to the one of the current platform.
        /// </summary>
        /// <param name="path">The path to canonize.</param>
        /// <returns>A canonical path.</returns>
        internal static string CanonizePath( string path )
        {
            if ( path == null )
            {
                return null;
            }

            string result = path.Replace( '/', Path.DirectorySeparatorChar );
            result = result.Replace( '\\', Path.DirectorySeparatorChar );
            result = result.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );
            result = result.Trim();

            ValidatePath( result );

            return result;
        }


        /// <summary>
        /// Finds a file in the current search path.
        /// </summary>
        /// <param name="fileName">A file name, either given absolutely, either without path,
        /// either relatively to the declaring file. Written back with the final file location.</param>
        /// <param name="referenceDirectory">Directory according to which relative paths should be resolved.</param>
        /// <returns><b>true</b> if the file was found, otherwise <b>false</b>.</returns>
        private bool FindFile( ref string fileName, string referenceDirectory )
        {
            fileName = CanonizePath( fileName );

            Trace.ProjectLoader.WriteLine( "Finding file {{{0}}}.", fileName );

            // Test whether the path was given.
            if ( fileName.IndexOf( Path.DirectorySeparatorChar ) >= 0 )
            {
                // Yes, the path was given.
                string filePath;

                if ( Path.IsPathRooted( fileName ) )
                {
                    filePath = fileName;
                }
                else
                {
                    filePath = Path.GetFullPath( Path.Combine( referenceDirectory, fileName ) );
                }

                fileName = filePath;

                Trace.ProjectLoader.WriteLine( "Probing {{{0}}}.", fileName );
                if ( File.Exists( filePath ) )
                {
                    Trace.ProjectLoader.WriteLine( "File found at this location." );
                    return true;
                }
                else
                {
                    Trace.ProjectLoader.WriteLine(
                        "File not found. No other location is considered because an absolute path was given." );
                    return false;
                }
            }
            else
            {
                // The path was not given. Look for the included file in the path.
                foreach ( string directory in this.path.Keys )
                {
                    string filePath = Path.GetFullPath( Path.Combine( this.Evaluate( directory ), fileName ) );
                    Trace.ProjectLoader.WriteLine( "Probing {{{0}}}.", filePath );
                    if ( File.Exists( filePath ) )
                    {
                        Trace.ProjectLoader.WriteLine( "File found at this location." );
                        fileName = filePath;
                        return true;
                    }
                }

                Trace.ProjectLoader.WriteLine( "File not found." );
                return false;
            }
        }

        /// <inheritdoc />
        string IProject.EvaluateExpression( string expression )
        {
            return this.Evaluate( expression );
        }

        string IProject.GetFrameworkVariant()
        {
            return this.module.GetFrameworkVariant();
        }


        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <returns>The expression value.</returns>
        public string Evaluate( string expression )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( expression, "expression" );

            #endregion

            string value = this.InternalEvaluate( expression, expression, 0 );

            return value == null ? null : Environment.ExpandEnvironmentVariables( value );
        }

        /// <summary>
        /// Evaluates an expression (called recursively).
        /// </summary>
        /// <param name="initialExpression">The initial expression, before any resolution.</param>
        /// <param name="expression">The currently resolved expression.</param>
        /// <param name="recursionLevel">The current recursion level.</param>
        /// <returns>The expression value.</returns>
        private string InternalEvaluate( string initialExpression, string expression, int recursionLevel )
        {
            if ( recursionLevel > 32 )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0023", new object[] {initialExpression} );
                return "!Error";
            }

            if ( expressionRegex.IsMatch( expression ) )
            {
                StringBuilder currentExpression = new StringBuilder( expression );
                Match match;
                while ( ( match = expressionRegex.Match( currentExpression.ToString() ) ).Success )
                {
                    string propertyName = match.Groups["name"].Value;
                    string propertyValue = this.properties[propertyName];
                    if ( propertyValue == null )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Error, "PS0024",
                                                          new object[] {initialExpression, propertyName} );
                        return null;
                    }

                    currentExpression.Remove( match.Captures[0].Index, match.Captures[0].Length );
                    currentExpression.Insert( match.Captures[0].Index,
                                              this.InternalEvaluate( initialExpression, propertyValue,
                                                                     recursionLevel + 1 ) );
                }

                return currentExpression.ToString();
            }
            else
            {
                return expression;
            }
        }

        #endregion

        /// <summary>
        /// Executes a single phase of the project.
        /// </summary>
        /// <param name="phase">Name of the phase to be executed.</param>
        /// <returns><b>true</b> if the execution was successfull, otherwise <b>false</b>.</returns>
        public bool ExecutePhase( string phase )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( phase, "phase" );

            #endregion

            Trace.ProjectLoader.WriteLine( "Executing phase {{{0}}} on module {{{1}}}.", phase, this.module.Name );

            using ( HighPrecisionTimer phaseTimer = new HighPrecisionTimer() )
            {
                using ( IEnumerator<Task> enumerator = this.tasks.GetPendingTaskEnumerator( phase ) )
                {
                    while ( enumerator.MoveNext() )
                    {
                        Task task = enumerator.Current;

                        if ( enumerator.Current.Disabled )
                        {
                            Trace.ProjectLoader.WriteLine( "Skipping task {{{0}}} because it is disabled.",
                                                           task.TaskName );
                            continue;
                        }

                        using ( HighPrecisionTimer taskTimer = new HighPrecisionTimer() )
                        {
                            Trace.ProjectLoader.WriteLine( "Executing task {{{0}}}.", task.TaskName );

                            task.State = TaskState.Executing;
                            bool success = task.Execute();
                            task.State = TaskState.Executed;

                            Trace.ProjectLoader.WriteLine( "Task {{{0}}} completed.", task.TaskName );

                            Trace.Timings.WriteLine( "Task {{{0}}} executed in {1} ms.",
                                                     task.TaskName, taskTimer.CurrentTime );

                            if ( !success )
                            {
                                Trace.ProjectLoader.WriteLine( "Task {{{0}}} returned a failure return code.",
                                                               task.TaskName );
                                return false;
                            }
                            else if ( Messenger.Current.ErrorCount > 0 )
                            {
                                Trace.ProjectLoader.WriteLine( "Task {{{0}}} emitted {1} errors.",
                                                               task.TaskName, Messenger.Current.ErrorCount );
                                return false;
                            }
                        }
                    }
                }

                Trace.Timings.WriteLine( "Phase {{{0}}} executed in {1} ms.", phase, phaseTimer.CurrentTime );
            }


            Trace.ProjectLoader.WriteLine( "Phase {{{0}}} on module {{{1}}} completed.", phase, this.module.Name );

            return true;
        }


        /// <summary>
        /// Executes the current project.
        /// </summary>
        public bool Execute()
        {
            using ( HighPrecisionTimer projectTimer = new HighPrecisionTimer() )
            {
                foreach ( PhaseConfiguration phase in phasesbyName )
                {
                    if ( !this.ExecutePhase( phase.Name ) )
                    {
                        return false;
                    }
                }


                Trace.Timings.WriteLine( "Project executed in {0} ms.", projectTimer.CurrentTime );
            }

            Trace.ProjectLoader.WriteLine( "Project completed." );

            return true;
        }

        #region ITaggable Members

        /// <inheritdoc />
        public object GetTag( Guid guid )
        {
            return this.tags.GetTag( guid );
        }

        /// <inheritdoc />
        public void SetTag( Guid guid, object value )
        {
            this.tags.SetTag( guid, value );
        }

        #endregion

        /// <summary>
        /// Determines whether the current instance has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.disposed; }
        }

        /// <summary>
        /// Throws an exception if the current instance has been disposed.
        /// </summary>
        [Conditional( "ASSERT" )]
        private void AssertNotDisposed()
        {
            if ( this.IsDisposed )
            {
                throw new ObjectDisposedException( this.GetType().Name );
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( !this.disposed )
            {
                this.tasks.Dispose();
                foreach ( KeyValuePair<string, bool> pair in this.path )
                {
                    if ( pair.Value )
                    {
                        this.domain.AssemblyLocator.RemoveDirectory( pair.Key );
                    }
                }
                this.disposed = true;
            }
        }

        public static void ValidatePath(string path)
        {
            try
            {
                Path.GetFullPath( path );
            }
            catch ( Exception e )
            {
                CoreMessageSource.Instance.Write(SeverityType.Fatal, "PS0045", new object[] { path, e.Message});
            }

            
        }
    }
}