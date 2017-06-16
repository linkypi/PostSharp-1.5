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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using PostSharp.CodeModel;
using PostSharp.Extensibility.Configuration;
using PostSharp.PlatformAbstraction;
using PostSharp.Utilities;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Implementation of <see cref="IPostSharpObject"/>. The principal entry point of the
    /// PostSharp Platform.
    /// </summary>
    public sealed class PostSharpObject : MarshalByRefObject, IPostSharpObject, IPostSharpEnvironment
    {
        #region Fields

        /// <summary>
        /// Instance of the <see cref="PostSharpObject"/> in the current <see cref="AppDomain"/>.
        /// </summary>
        private static PostSharpObject theInstance;

        /// <summary>
        /// Determines whether the current instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// <see cref="Domain"/> containing all objects of the Code Object Model created
        /// in the scope of this <see cref="PostSharpObject"/>.
        /// </summary>
        private Domain domain;

        /// <summary>
        /// Host of the current object.
        /// </summary>
        private PostSharpLocalHost host;

     
        /// <summary>
        /// Set of modules that have already been processed. The dictionary value determines whether
        /// the module has been renamed.
        /// </summary>
        private readonly Dictionary<ModuleDeclaration, ModuleProcessingStatus> processedModules =
            new Dictionary<ModuleDeclaration, ModuleProcessingStatus>();

        /// <summary>
        /// Settings.
        /// </summary>
        private PostSharpObjectSettings settings;

        private Project currentProject;

        /// <summary>
        /// Lists of projects that should then be executed, in deep-first order.
        /// </summary>
        private readonly List<Project> projects = new List<Project>();

        // This is a dummy field to turn off a warning in InitializeAppDomainWhenRemote.
        private int dummy;

        private bool customAssemblyBinderOwned;

        private List<ModuleLoadStrategy> moduleLoadStrategies;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new instance. Note that, in a proper initialization, the
        /// <see cref="InitializeAppDomainWhenRemote"/> and <see cref="Initialize"/>
        /// should also be called.
        /// </summary>
        private PostSharpObject()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( theInstance == null, "DomainHasAlreadyPostSharpObject" );

            #endregion

            theInstance = this;
        }

        /// <summary>
        /// Initialize the current <see cref="AppDomain"/>. This method should be called onlu
        /// when this <see cref="PostSharpObject"/> is called in a private <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="messenger">Messenger in the <i>current</i> <see cref="AppDomain"/>.</param>
        private void InitializeAppDomainWhenRemote(
            Messenger messenger )
        {
            // Force the method to be non-static.
            this.dummy++;

            // Set up the messaging subsystem.
            Messenger.Current = messenger;
        }

        /// <summary>
        /// Initialize the current instance and the current <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="remoteHost">Host.</param>
        /// <param name="settings">Settings.</param>
        private void Initialize( IPostSharpHost remoteHost, PostSharpObjectSettings settings )
        {
            // Set the flag telling that we are running 'in' PostSharp.
            PostSharpEnvironment.Current = this;

            this.settings = settings;
            this.settings.DomainTags.CopyTo( this.domain );

            // Create a Domain and subscribes to the AssemblyResolve event.
            this.domain = new Domain( settings.DisableReflection );
            this.domain.AssemblyLocator.LocatingAssembly += OnDomainAssemblyResolve;
            this.domain.AssemblyLoading += OnAssemblyLoading;

            // Disable messages.
            foreach ( string messageId in settings.DisabledMessages )
            {
                Messenger.Current.DisableMessage( messageId );
            }

            // Apply given settings.
            foreach ( string name in settings.Settings.Keys )
            {
                ApplicationInfo.SetSetting( name, settings.Settings[name] );
            }


            // Initialize the custom assembly binder.
            this.customAssemblyBinderOwned = CustomAssemblyBinder.Initialize( true );
            this.domain.AssemblyLocator.AddDirectories( settings.SearchDirectories );

            // Load assembly redirection policies as soon as possible.
            Project.ProcessAssemblyRedirections( this.domain,
                                                 Project.ApplicationConfiguration.AssemblyBinding.DependentAssemblies );


            // Create the local host.
            if ( string.IsNullOrEmpty( settings.LocalHostImplementation ) )
            {
                this.host = new PostSharpLocalHost();
            }
            else
            {
                // Get the host type.
                Type hostType = Type.GetType( settings.LocalHostImplementation, false );
                if ( hostType == null )
                {
                    throw ExceptionHelper.Core.CreateArgumentException(
                        "settings.LocalHostImplementation", "InvalidLocalHostImplementation" );
                }

                // Get the default constructor.
                ConstructorInfo hostConstructor = hostType.GetConstructor( Type.EmptyTypes );
                if ( hostConstructor == null )
                {
                    throw ExceptionHelper.Core.CreateArgumentException(
                        "settings.LocalHostImplementation", "LocalHostTypeHasNoDefaultConstructor",
                        hostType.FullName );
                }

                // Construct the type.
                this.host = hostConstructor.Invoke( null ) as PostSharpLocalHost;
                if ( this.host == null )
                {
                    throw ExceptionHelper.Core.CreateArgumentException(
                        "settings.LocalHostImplementation", "LocalHostTypeDoesNotDerive",
                        hostType.FullName );
                }
            }

            this.host.InternalInitialize( this, remoteHost );
            CustomAssemblyBinder.Instance.SetAssemblyLocator (this.domain.AssemblyLocator);
        }

        private void OnAssemblyLoading( object sender, AssemblyLoadingEventArgs e )
        {
            if (this.moduleLoadStrategies == null)
                return;

            foreach ( ModuleLoadStrategy moduleLoadStrategy in this.moduleLoadStrategies )
            {
                if ( moduleLoadStrategy.Matches( this.domain, e.AssemblyName ))
                {
                    e.LazyLoading = moduleLoadStrategy.LazyLoading;
                    return;
                }
            }

        }

        /// <summary>
        /// Creates a PostSharp Object using the default host implementation.
        /// </summary>
        /// <param name="settings">Settings of the PostSharp Object.</param>
        /// <returns>A PostSharp Object, the entry point of the PostSharp Platform Infrastructure.</returns>
        /// <remarks>
        /// <para>The default host implementation covers the compile-time usage scenario: projects are executed
        /// only when explicitely invoked by the <see cref="IPostSharpObject.InvokeProjects"/> methods,
        /// never when they are 'discovered' at runtime.
        /// </para>
        /// <para>
        /// Note that, in every case, the host should register to the <see cref="Messenger.Message"/>
        /// event of the <see cref="Messenger"/> object, in the current <see cref="AppDomain"/>.
        /// </para>
        /// </remarks>
        public static IPostSharpObject CreateInstance( PostSharpObjectSettings settings )
        {
            return CreateInstance( settings, null );
        }

        /// <summary>
        /// Creates a PostSharp Object and specifies the host implementation.
        /// </summary>
        /// <param name="settings">Settings of the PostSharp Object.</param>
        /// <param name="host">An implementation of <see cref="IPostSharpHost"/> derived from
        /// <see cref="MarshalByRefObject"/>, or <b>null</b> to use the default host implementation.
        /// </param>
        /// <returns>A PostSharp Object, the entry point of the PostSharp Platform Infrastructure.</returns>
        /// <remarks>
        /// <para>The default host implementation covers the compile-time usage scenario: projects are executed
        /// only when explicitely invoked by the <see cref="IPostSharpObject.InvokeProjects"/> methods,
        /// never when they are 'discovered' at runtime.
        /// </para>
        /// <para>
        /// Note that, in every case, the host should register to the <see cref="Messenger.Message"/>
        /// event of the <see cref="Messenger"/> object, in the current <see cref="AppDomain"/>.
        /// </para>
        /// </remarks>
        public static IPostSharpObject CreateInstance( PostSharpObjectSettings settings, IPostSharpHost host )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( settings, "settings" );

            #endregion

            IPostSharpHost theHost = host ?? DefaultPostSharpHost.Instance;


            if ( settings.CreatePrivateAppDomain )
            {
                return CreateInstanceInPrivateAppDomain( settings, theHost );
            }
            else
            {
                Trace.Initialize();
                PostSharpObject obj = new PostSharpObject();
                obj.Initialize( theHost, settings );
                return obj;
            }
        }

        /// <summary>
        /// Initialize a <see cref="PostSharpObject"/> in a private <see cref="AppDomain"/>
        /// and returns an accessor for the <i>current</i> <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="host">Host.</param>
        /// <returns>A <see cref="PostSharpObjectAccessor"/>.</returns>
        private static IPostSharpObject CreateInstanceInPrivateAppDomain(
            PostSharpObjectSettings settings, IPostSharpHost host )
        {
            // Set up the a PermissionSet and Evidence.
            Evidence evidence = settings.Evidence != null ? null : new Evidence( AppDomain.CurrentDomain.Evidence );

            AppDomain appDomain;

            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
            {
                PermissionSet permissions = settings.PermissionSet ?? new NamedPermissionSet( "FullTrust" );
                appDomain = Platform.Current.CreateAppDomain( settings.AppDomainSetup.ApplicationName,
                                                              evidence, settings.AppDomainSetup, permissions );

                Trace.PostSharpObject.WriteLine( "Created the AppDomain {0}.", appDomain.Id );
                Trace.Timings.WriteLine( "AppDomain created in {0} ms.", timer.CurrentTime );
            }

            try
            {
                // Create the Messenger in the project application domain.
                Messenger remoteMessenger =
                    (Messenger)
                    appDomain.CreateInstanceAndUnwrap( typeof(Messenger).Assembly.FullName, typeof(Messenger).FullName,
                                                       false, BindingFlags.Instance | BindingFlags.Public,
                                                       null,
                                                       null, null, null, null );

                remoteMessenger.AddRemoteSink( Messenger.Current, false );

                // Finally create the project.
                PostSharpObject instance = (PostSharpObject) appDomain.CreateInstanceAndUnwrap(
                                                                 typeof(PostSharpObject).Assembly.FullName,
                                                                 typeof(PostSharpObject).FullName,
                                                                 false, BindingFlags.Instance | BindingFlags.NonPublic,
                                                                 null,
                                                                 null, null, null, null );
                instance.InitializeAppDomainWhenRemote( remoteMessenger );
                instance.Initialize( host, settings );

                return new PostSharpObjectAccessor( instance );
            }
            catch
            {
                AppDomain.Unload( appDomain );
                throw;
            }
        }


        /// <inheritdoc />
        public void Dispose()
        {
            if ( !this.disposed )
            {
                using ( HighPrecisionTimer domainDisposeTimer = new HighPrecisionTimer() )
                {
                    if ( this.customAssemblyBinderOwned )
                        CustomAssemblyBinder.Dispose();

                    this.host.Dispose();
                    this.domain.Dispose();
                    Trace.Timings.WriteLine( "Domain disposed in {0} ms.", domainDisposeTimer.CurrentTime );
                }


                theInstance = null;

                PostSharpEnvironment.Current = null;

                this.disposed = true;
            }
        }

        [Conditional( "ASSERT" )]
        private void AssertNotDisposed()
        {
            if ( this.disposed )
            {
                throw new ObjectDisposedException( "PostSharpObject" );
            }
        }

        #endregion

        #region Execution

        /// <summary>
        /// Handles the <see cref="AssemblyLocator.LocatingAssembly"/> event. 
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnDomainAssemblyResolve( object sender, AssemblyLocateEventArgs e )
        {
            #region Preconditions

            this.AssertNotDisposed();

            #endregion

            // Ask the host what to do.
            string location = this.host.ResolveAssemblyReference(e.AssemblyName );

            if (location != null)
            {
                e.AssemblyLocation = location;
            }
        }

      
        /// <summary>
        /// Load module dependencies recursively and process them.
        /// </summary>
        /// <param name="module">The module to be processed.</param>
        /// <returns><b>true</b> if the assembly has been renamed, otherwise <b>false</b>.</returns>
        private bool LoadModuleRecursive(ModuleDeclaration module)
        {

            ModuleProcessingStatus status;
            if ( !this.processedModules.TryGetValue( module, out status ) )
            {
                status = new ModuleProcessingStatus();
                this.processedModules.Add( module, status );
            }
         
            // If the module has already been processed, end here.
            if (status.LoadedRecursively)
            {
                Trace.PostSharpObject.WriteLine( "The module {{{0}}} has already been processed.", module.Name );
                return status.Renamed;
            }

            status.LoadedRecursively = true;

            Trace.PostSharpObject.WriteLine("Processing the module {{{0}}}.", module.Name);


            // Do we already know how to process this module?
            if (!status.InvocationParametersSet)
            {
                Trace.PostSharpObject.WriteLine("Do not know how to process the module {{{0}}}. Ask the host.",
                                                 module.Name);

                // No, we do now know yet how to process it. Ask the host.

                status.InvocationParameters = this.host.GetProjectInvocationParameters(module);

            }

            // Now we know how to process the current module. Should we process it?
            if (status.InvocationParameters == null)
            {
                // The module should not be processed.
                Trace.PostSharpObject.WriteLine("The module {{{0}}} should not be processed.", module.Name);
                return false;
            }


            // If required, recursively process dependencies.
            if (status.InvocationParameters.ProcessDependenciesFirst)
            {
                Trace.PostSharpObject.WriteLine( "Processing depencencies of {{{0}}}.", module.Name );

                // Process assembly references.
                foreach ( AssemblyRefDeclaration assemblyRef in module.AssemblyRefs )
                {
                    bool renameAssemblyRef = false;

                    AssemblyEnvelope assemblyRefEnvelope = assemblyRef.GetAssemblyEnvelope();

                    foreach ( ModuleDeclaration childModule in assemblyRefEnvelope.Modules )
                    {
                        renameAssemblyRef |= this.LoadModuleRecursive( childModule);
                    }

                    // If this assembly was processed, we need to remove the public key token
                    // in the reference.
                    if ( renameAssemblyRef && this.settings.OverwriteAssemblyNames )
                    {
                        assemblyRef.OverwrittenName = assemblyRef.Name + "~";
                    }
                }

                // Process module references.
                foreach ( ModuleRefDeclaration moduleRef in module.ModuleRefs )
                {
                    ModuleDeclaration moduleDef = module.Assembly.Modules.GetByName( moduleRef.Name );
                    if ( moduleDef != null )
                    {
                        this.LoadModuleRecursive(moduleDef);
                    }
                    else
                    {
                        // Probably an unmanaged module.
                    }
                }
            }

            // Overwrite the assembly name if necessary.
            bool renamed;
            if ( settings.OverwriteAssemblyNames &&
                 module.AssemblyManifest != null &&
                 !status.InvocationParameters.PreventOverwriteAssemblyNames )
            {
                renamed = true;
                module.AssemblyManifest.OverwrittenName =
                    module.AssemblyManifest.Name + "~";
                this.host.RenameAssembly( module.AssemblyManifest );
            }
            else
            {
                renamed = false;
            }

            // Mark that we have processed the current module.
            status.Renamed = renamed;


            projects.Add( Project.CreateInstance(
                              status.InvocationParameters.ProjectConfigurationFile, 
                              module,
                              status.InvocationParameters.Properties, 
                              status.InvocationParameters.Tags));


            return renamed;
        }

        /// <inheritdoc />
        public void ProcessAssemblies( ModuleLoadStrategy[] modules )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( modules, "assemblies" );
            this.AssertNotDisposed();

            #endregion

            this.moduleLoadStrategies = new List<ModuleLoadStrategy>(modules);
            // Load all modules in the domain and retrieve the project invocation parameters
            // from the local host.
            List<ModuleDeclaration> topModules = new List<ModuleDeclaration>();
            foreach ( ModuleLoadStrategy moduleLoadStrategy in modules )
            {
                // Load the assembly in the domain.
                ModuleDeclaration module = moduleLoadStrategy.Load( domain );
                topModules.Add( module );
            }

            foreach ( ModuleDeclaration module in topModules )
            {
                this.LoadModuleRecursive(module);
            }

            this.ExecuteProjects();
        }

        /// <inheritdoc />
        public void InvokeProject( ProjectInvocation projectInvocation )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( projectInvocation, "projectInvocation" );
            ExceptionHelper.Core.AssertValidArgument( !projectInvocation.InvocationParameters.ProcessDependenciesFirst,
                                                      "projectInvocation", "PS0101" );
            this.AssertNotDisposed();

            #endregion

            this.projects.Add( Project.CreateInstance(
                                   projectInvocation.InvocationParameters.ProjectConfigurationFile,
                                   this.domain,
                                   projectInvocation.ModuleLoadStrategy,
                                   projectInvocation.InvocationParameters.Properties,
                                   projectInvocation.InvocationParameters.Tags ) );

            this.ExecuteProjects();
        }

        /// <inheritdoc />
        public void InvokeProjects( ProjectInvocation[] projectInvocations )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( projectInvocations, "projectInvocations" );
            this.AssertNotDisposed();

            #endregion

            // If we process a single module without dependencies, we switch to a
            // a simpler invocation process that does not load all modules ahead of time.
            if ( projectInvocations.Length == 1 && !projectInvocations[0].InvocationParameters.ProcessDependenciesFirst )
            {
                this.InvokeProject( projectInvocations[0] );
                return;
            }

            this.moduleLoadStrategies = new List<ModuleLoadStrategy>(projectInvocations.Length);
            try
            {
                // Load all modules in the domain and set the invocation parameters.
                foreach ( ProjectInvocation projectInvocation in projectInvocations )
                {
                    ModuleDeclaration module = projectInvocation.ModuleLoadStrategy.Load( this.domain );
                    this.processedModules.Add( module,
                                               new ModuleProcessingStatus
                                                   {InvocationParameters = projectInvocation.InvocationParameters} );
                    moduleLoadStrategies.Add( projectInvocation.ModuleLoadStrategy );
                }

                // Load top modules and, recursively, all depending modules.
                List<ModuleDeclaration> topModules =
                    new List<ModuleDeclaration>( this.processedModules.Keys );
                foreach ( ModuleDeclaration module in topModules )
                {
                    this.LoadModuleRecursive( module );
                }

                // Now execute the projects.
                this.ExecuteProjects();
            }
            catch ( MessageException )
            {
                throw;
            }
            catch ( Exception e )
            {
                // We have to wrap all exceptions happening here into MessageException, because
                // the exception is not guaranteed to be serializable.
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0099", new object[] {e.ToString()} );
            }
        }

        private void ExecuteProjects()
        {
            // Execute projects.
            switch ( settings.ProjectExecutionOrder )
            {
                case ProjectExecutionOrder.Sequential:
                    if ( this.projects.Count == 0 )
                        return;

                    foreach ( Project project in projects )
                    {
                        currentProject = project;
                        Trace.PostSharpObject.WriteLine( "Processing the module {{{0}}} (all phases).",
                                                         project.Module.Name );
                        if ( !project.Execute() )
                        {
                            CoreMessageSource.Instance.Write(
                                SeverityType.Fatal, "PS0060", new object[] {project.Module.Name} );
                        }
                        currentProject = null;
                    }
                    break;

                case ProjectExecutionOrder.Phased:
                    PhaseExecutedEventArgs eventArgs = new PhaseExecutedEventArgs(
                        new ReadOnlyCollection<Project>( projects ) );

                    foreach ( PhaseConfiguration phase in Project.ApplicationConfiguration.Phases )
                    {
                        eventArgs.Phase = phase.Name;

                        Trace.PostSharpObject.WriteLine( "Processing phase {{{0}}}.", phase.Name );


                        if ( this.PhaseExecuting != null )
                        {
                            this.PhaseExecuting( this, eventArgs );
                        }

                        foreach ( Project project in projects )
                        {
                            currentProject = project;

                            Trace.PostSharpObject.WriteLine( "Processing the module {{{0}}}, phase {{{1}}}.",
                                                             project.Module.Name,
                                                             phase.Name );
                            if ( !project.ExecutePhase( phase.Name ) )
                            {
                                CoreMessageSource.Instance.Write(
                                    SeverityType.Fatal, "PS0061", new object[] {phase.Name, project.Module.Name} );
                            }
                        }

                        currentProject = null;

                        if ( this.PhaseExecuted != null )
                        {
                            this.PhaseExecuted( this, eventArgs );
                        }
                    }
                    break;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( settings.ProjectExecutionOrder,
                                                                                  "PostSharpObjectSettings.ProjectExecutionOrder" );
            }
        }

        #endregion

        /// <inheritdoc />
        public AppDomain AppDomain
        {
            get { return AppDomain.CurrentDomain; }
        }

        /// <summary>
        /// Event raised <i>before</i> a phase is executed on a group of projects,
        /// if the project execution mode is <see cref="ProjectExecutionOrder.Phased"/>.
        /// </summary>
        public event EventHandler<PhaseExecutedEventArgs> PhaseExecuting;

        /// <summary>
        /// Event raised <i>after</i> a phase has been executed on a group of projects,
        /// if the project execution mode is <see cref="ProjectExecutionOrder.Phased"/>.
        /// </summary>
        public event EventHandler<PhaseExecutedEventArgs> PhaseExecuted;

        /// <summary>
        /// Gets the <see cref="Domain"/> in which all modules are loaded and processed.
        /// </summary>
        public Domain Domain
        {
            get { return this.domain; }
        }

        /// <summary>
        /// Gets the currently executing project.
        /// </summary>
        public Project CurrentProject
        {
            get { return this.currentProject; }
        }

        /// <inheritdoc />
        IProject IPostSharpEnvironment.CurrentProject
        {
            get { return this.CurrentProject; }
        }

        class ModuleProcessingStatus
        {
            private ProjectInvocationParameters _invocationParameters;
            
            public bool LoadedRecursively;
            public bool Renamed;

            public ProjectInvocationParameters InvocationParameters
            {
                get { return _invocationParameters; }
                set 
                {
                    _invocationParameters = value;
                    this.InvocationParametersSet = true;
                }
            }

            public bool InvocationParametersSet { get; private set; }
        }

    }


    /// <summary>
    /// Arguments of the events <see cref="PostSharpObject.PhaseExecuting"/>
    /// and <see cref="PostSharpObject.PhaseExecuted"/>.
    /// </summary>
    public sealed class PhaseExecutedEventArgs : EventArgs
    {
        private readonly IList<Project> projects;

        internal PhaseExecutedEventArgs( IList<Project> projects )
        {
            this.projects = projects;
        }

        /// <summary>
        /// Name of the phase.
        /// </summary>
        public string Phase { get; internal set; }

        /// <summary>
        /// List of projects in the group being now executed, in 
        /// deep-first order of dependency relationships.
        /// </summary>
        public IList<Project> Projects
        {
            get { return projects; }
        }
    }
}