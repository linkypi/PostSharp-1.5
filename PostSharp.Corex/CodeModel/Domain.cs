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
using System.Reflection;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.Helpers;
using PostSharp.Extensibility;
using PostSharp.ModuleReader;
using PostSharp.PlatformAbstraction;
using PostSharp.Utilities;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// A domain is a scope, i.e. a context in which external references 
    /// are resolved. A domain cntains assemblies.
    /// </summary>
    public sealed class Domain : Element, IDisposable, ITaggable
    {
        private readonly AssemblyEnvelopeCollection assemblies;
        private readonly TagCollection tags = new TagCollection();
        private Guid domainGuid = Guid.NewGuid();

        private readonly AssemblyRedirectionPolicyManager assemblyRedirectionPolicy =
            new AssemblyRedirectionPolicyManager();

        private readonly bool reflectionDisabled;
        private readonly AssemblyLocator assemblyLocator;

        /// <summary>
        /// Initializes a new <see cref="Domain"/>.
        /// </summary>
        public Domain() : this( false )
        {
        }


        /// <summary>
        /// Initializes a new <see cref="Domain"/> and specifies whether 
        /// assemblies are allowed to be loaded into the CLR.
        /// </summary>
        /// <param name="disableReflection"><b>true</b> if assemblies are loaded
        /// in the CLR, otherwise <b>false</b>.</param>
        public Domain( bool disableReflection )
        {
            this.reflectionDisabled = disableReflection;
            this.assemblies = new AssemblyEnvelopeCollection( this, "assembly" );
            this.assemblyRedirectionPolicy = new AssemblyRedirectionPolicyManager();
            this.assemblyLocator = new AssemblyLocator( this );
        }

        /// <inheritdoc />
        protected override bool NotifyChildPropertyChanged(Element child, string property, object oldValue, object newValue)
        {
            if ( child.Role == "assembly" && property == "Name" )
            {
                this.assemblies.OnItemNameChanged((AssemblyEnvelope)child, (string) oldValue);
                return true;
            }

            return false;
        }

        public event EventHandler<AssemblyLoadingEventArgs> AssemblyLoading;

     
        /// <summary>
        /// The <see cref="AssemblyLocator"/> for the current <see cref="Domain"/>.
        /// </summary>
        public AssemblyLocator AssemblyLocator { get { return assemblyLocator;}}

        /// <summary>
        /// Determines whether assemblies of the current <see cref="Domain"/> are loaded into the CLR.
        /// </summary>
        public bool ReflectionDisabled
        {
            get { return this.reflectionDisabled; }
        }

        /// <summary>
        /// Gets the GUID of this domain.
        /// </summary>
        public Guid Guid
        {
            get { return this.domainGuid; }
        }

        /// <summary>
        /// Gets the collection of assemblies loaded in the domain.
        /// </summary>
        public AssemblyEnvelopeCollection Assemblies
        {
            get { return this.assemblies; }
        }


        /// <summary>
        /// Loads an assembly into the current domain (without lazy loaded).
        /// </summary>
        /// <param name="reflectionAssembly">A reflection <see cref="Assembly"/>.</param>
        /// <returns>The <see cref="AssemblyEnvelope"/> representing the loaded
        /// assembly.</returns>
        public AssemblyEnvelope LoadAssembly( Assembly reflectionAssembly )
        {
            return this.LoadAssembly( reflectionAssembly, false );
        }

        /// <summary>
        /// Determines whether an assembly (given the reflection <see cref="Assembly"/>) has
        /// already been loaded in the current <see cref="Domain"/>.
        /// </summary>
        /// <param name="reflectionAssembly">The <see cref="Assembly"/> whose presence in the current
        /// <see cref="Domain"/> should be determined.</param>
        /// <returns><b>true</b> if <paramref name="reflectionAssembly"/> has already, otherwise <b>false</b>.
        /// </returns>
        public bool IsAssemblyLoaded( Assembly reflectionAssembly )
        {
            if ( this.reflectionDisabled )
                throw new InvalidOperationException( "This domain is not bound to reflection." );

            foreach ( AssemblyEnvelope assembly in this.assemblies )
            {
                if ( assembly.GetSystemAssembly() == reflectionAssembly )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="AssemblyEnvelope"/> of the current <see cref="Domain"/>
        /// corresponding to a given <see cref="Assembly"/>, but does not load the assembly
        /// into the domain if it is not found.
        /// </summary>
        /// <param name="reflectionAssembly">The <see cref="Assembly"/> to be located.</param>
        /// <returns>The <see cref="AssemblyEnvelope"/> corresponding to <paramref name="reflectionAssembly"/>,
        /// or <b>null</b> if the current <see cref="Domain"/> does not contain <paramref name="reflectionAssembly"/>.</returns>
        public AssemblyEnvelope GetAssembly( Assembly reflectionAssembly )
        {
            foreach ( AssemblyEnvelope assembly in this.assemblies )
            {
                if ( assembly.GetSystemAssembly() == reflectionAssembly )
                {
                    return assembly;
                }
            }

            return null;
        }


        /// <summary>
        /// Loads an assembly (given as an <see cref="Assembly"/>) into the current domain and specifies whether the assembly should 
        /// be lazily loaded or not.
        /// </summary>
        /// <param name="reflectionAssembly">A reflection <see cref="Assembly"/>.</param>
        /// <param name="lazyLoading">Whether the assembly should be lazily loaded.</param>
        /// <returns>The <see cref="AssemblyEnvelope"/> representing the loaded
        /// assembly.</returns>
        /// <remarks>
        /// Typically, the principal assembly is loaded completely at first time and
        /// referenced assemblies are loaded lazily.
        /// </remarks>
        public AssemblyEnvelope LoadAssembly( Assembly reflectionAssembly, bool lazyLoading )
        {
            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
            {
                // Check that this assembly is not already loaded.
                foreach ( AssemblyEnvelope assembly in this.assemblies )
                {
                    if ( assembly.GetSystemAssembly() == reflectionAssembly )
                    {
                        Trace.Domain.WriteLine( "The assembly {{{0}}} was already loaded in the domain.",
                                                reflectionAssembly );
                        return assembly;
                    }
                }

                Trace.Domain.WriteLine( "Loading reflection assembly {{{0}}} into domain.", reflectionAssembly );

                AssemblyEnvelope assemblyEnvelope = new AssemblyEnvelope( reflectionAssembly.Location );
                this.assemblies.Add( assemblyEnvelope );

                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( reflectionAssembly, "reflectionAssembly" );

                #endregion

                foreach ( Module module in reflectionAssembly.GetModules( true ) )
                {
                    if ( module.MDStreamVersion != 0 )
                    {
                        ModuleReader.ModuleReader moduleReader =
                            new ModuleReader.ModuleReader( module, assemblyEnvelope, lazyLoading );
                        moduleReader.ReadModule( Platform.Current.ReadModuleStrategy );
                    }
                }

                Trace.Timings.WriteLine( "Assembly {{{0}}} loaded in {1} ms (lazyLoading={2})",
                                         reflectionAssembly, timer.CurrentTime, lazyLoading );

                return assemblyEnvelope;
            }
        }

        /// <summary>
        /// Loads an assembly (given by file name) into the current domain and specifies whether the assembly should 
        /// be lazily loaded or not.
        /// </summary>
        /// <param name="assemblyLocation">Full path of the physical location of the assembly to be loaded.</param>
        /// <param name="lazyLoading">Whether the assembly should be lazily loaded.</param>
        /// <returns>The <see cref="AssemblyEnvelope"/> representing the loaded
        /// assembly.</returns>
        /// <remarks>
        /// Typically, the principal assembly is loaded completely at first time and
        /// referenced assemblies are loaded lazily.
        /// </remarks>
        public AssemblyEnvelope LoadAssembly( string assemblyLocation, bool lazyLoading )
        {
            if ( !reflectionDisabled )
            {
                Assembly assembly = Assembly.LoadFrom( assemblyLocation );
                return this.LoadAssembly( assembly, lazyLoading );
            }

            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
            {
                AssemblyEnvelope assemblyEnvelope = new AssemblyEnvelope( assemblyLocation );
                this.assemblies.Add( assemblyEnvelope );

                ModuleReader.ModuleReader moduleReader = new ModuleReader.ModuleReader( assemblyLocation,
                                                                                        assemblyEnvelope, lazyLoading );
                moduleReader.ReadModule( ReadModuleStrategy.FromDisk );

                Trace.Timings.WriteLine( "Assembly {{{0}}} loaded in {1} ms (lazyLoading={2})",
                                         assemblyLocation, timer.CurrentTime, lazyLoading );

                return assemblyEnvelope;
            }
        }

        /// <summary>
        /// Find the <see cref="TypeDefDeclaration"/> of a given <see cref="Type"/> in the current <see cref="Domain"/>
        /// with default binding options.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> whose definition is requested.</param>
        /// <returns>The <see cref="TypeDefDeclaration"/> corresponding to <paramref name="type"/>.</returns>
        public TypeDefDeclaration FindTypeDefinition( Type type )
        {
            return FindTypeDefinition( type, BindingOptions.Default );
        }

        /// <summary>
        /// Find the <see cref="TypeDefDeclaration"/> of a given <see cref="Type"/> in the current <see cref="Domain"/>
        /// and specifies binding options.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> whose definition is requested.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="TypeDefDeclaration"/> corresponding to <paramref name="type"/>.</returns>
        public TypeDefDeclaration FindTypeDefinition( Type type, BindingOptions bindingOptions )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            AssemblyEnvelope assembly = this.GetAssembly( AssemblyNameHelper.GetAssemblyName( type ), bindingOptions );
            if ( assembly == null )
                return null;

            return (TypeDefDeclaration) assembly.ManifestModule.FindType( type.FullName, BindingOptions.OnlyExisting );
        }

        /// <summary>
        /// Gets an assembly given its name with default binding options.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <returns>The assembly named <paramref name="assemblyName"/>.</returns>
        public AssemblyEnvelope GetAssembly( string assemblyName )
        {
            return GetAssembly( assemblyName, BindingOptions.Default );
        }

        /// <summary>
        /// Gets an assembly given its name and specifies binding options.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The assembly named <paramref name="assemblyName"/>.</returns>
        public AssemblyEnvelope GetAssembly( string assemblyName, BindingOptions bindingOptions )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( assemblyName, "assemblyName" );

            #endregion

            return GetAssembly( AssemblyNameWrapper.GetWrapper( assemblyName ), bindingOptions );
        }

        /// <summary>
        /// Find the <see cref="TypeDefDeclaration"/> of a type given its name in the current <see cref="Domain"/>
        /// and specifies binding options.
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">Assembly-qualified name of the type to be found.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="TypeDefDeclaration"/> corresponding to <paramref name="assemblyQualifiedTypeName"/>.</returns>
        public TypeDefDeclaration FindTypeDefinition( string assemblyQualifiedTypeName, BindingOptions bindingOptions )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotEmptyOrNull( assemblyQualifiedTypeName, "assemblyQualifiedTypeName" );

            #endregion

            int comma = assemblyQualifiedTypeName.IndexOf( ',' );
            ExceptionHelper.Core.AssertValidArgument( comma > 0, "assemblyQualifiedTypeName",
                                                      "ExpectedAssemblyQualifiedTypeName", assemblyQualifiedTypeName );

            string assemblyName = assemblyQualifiedTypeName.Substring( comma + 1 );
            string typeName = assemblyQualifiedTypeName.Substring( 0, comma );

            AssemblyEnvelope assembly = this.GetAssembly( assemblyName, bindingOptions );
            if ( assembly == null ) return null;
            return assembly.GetTypeDefinition( typeName, bindingOptions | BindingOptions.OnlyExisting );
        }

        /// <summary>
        /// Find the <see cref="TypeDefDeclaration"/> of a type given its name in the current <see cref="Domain"/>
        /// with default binding options.
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">Assembly-qualified name of the type to be found.</param>
        /// <returns>The <see cref="TypeDefDeclaration"/> corresponding to <paramref name="assemblyQualifiedTypeName"/>.</returns>
        public TypeDefDeclaration FindTypeDefinition( string assemblyQualifiedTypeName )
        {
            return this.FindTypeDefinition( assemblyQualifiedTypeName, BindingOptions.Default );
        }

        /// <summary>
        /// Gets an assembly from the current domain given its name or loads it
        /// into the domain if not yet present, with default binding options.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to load.</param>
        /// <returns>The <see cref="AssemblyEnvelope"/> corresponding to <paramref name="assemblyName"/>.</returns>
        public AssemblyEnvelope GetAssembly( IAssemblyName assemblyName )
        {
            return GetAssembly( assemblyName, BindingOptions.Default );
        }

        /// <summary>
        /// Gets an assembly from the current domain given its name or loads it
        /// into the domain if not yet present, and specifies binding options.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to load.</param>
        /// <param name="bindingOptions">Binding options.</param>
        /// <returns>The <see cref="AssemblyEnvelope"/> corresponding to <paramref name="assemblyName"/>.</returns>
        public AssemblyEnvelope GetAssembly( IAssemblyName assemblyName, BindingOptions bindingOptions )
        {
            IAssemblyName canonicalAssemblyName = this.assemblyRedirectionPolicy.GetCanonicalAssemblyName( assemblyName );

            // Look in existing assemblies.
            IEnumerable<AssemblyEnvelope> candidateAssemblies = this.assemblies.GetByName( canonicalAssemblyName.Name );
            if ( candidateAssemblies != null )
            {
                foreach ( AssemblyEnvelope candidateAssembly in candidateAssemblies )
                {
                    if ( candidateAssembly.MatchesReference( canonicalAssemblyName ) )
                    {
                        return candidateAssembly;
                    }
                }
            }

            // The assembly was not found. We should load it.


            AssemblyLoadHelper.ClearLog();
            string location = this.assemblyLocator.FindAssembly( assemblyName );
            string findLog = AssemblyLoadHelper.GetLog();

            if ( location == null )
            {
                // Still not found.

                if ( ( bindingOptions & BindingOptions.OnlyExisting ) != 0 )
                {
                    if ( ( bindingOptions & BindingOptions.DontThrowException ) != 0 )
                        return null;
                    else
                        throw new BindingException( string.Format( "Assembly {0} is not loaded in the domain.",
                                                                   assemblyName.FullName ) );
                }

            }

            if ( !this.reflectionDisabled )
            {
                // Load the reflection assembly.
                Assembly reflectionAssembly;
                if ( location == null )
                {
                    // Use normal CLR binder to find the assembly.
                    reflectionAssembly =
                        AssemblyLoadHelper.LoadAssemblyFromName(
                            this.assemblyRedirectionPolicy.GetCanonicalAssemblyName( canonicalAssemblyName.FullName ) );
                }
                else
                {
                    reflectionAssembly = AssemblyLoadHelper.LoadAssemblyFromFile( location );
                }

                AssemblyLoadingEventArgs assemblyLoadingEventArgs = new AssemblyLoadingEventArgs( location, true, canonicalAssemblyName, reflectionAssembly );
                if ( this.AssemblyLoading != null )
                {
                    this.AssemblyLoading(this, assemblyLoadingEventArgs);
                }


                return this.LoadAssembly( reflectionAssembly, assemblyLoadingEventArgs.LazyLoading );
            }
            else
            {
                if ( location == null )
                    throw ExceptionHelper.Core.CreateBindingException( "CannotLoadAssembly",
                                                                       assemblyName, "Assembly not found.", findLog );

                AssemblyLoadingEventArgs assemblyLoadingEventArgs = new AssemblyLoadingEventArgs(location, true, canonicalAssemblyName, null);
                if (this.AssemblyLoading != null)
                {
                    this.AssemblyLoading(this, assemblyLoadingEventArgs);
                }
                return this.LoadAssembly(location, assemblyLoadingEventArgs.LazyLoading);
            }
        }

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            this.assemblies.Dispose();
        }

        #endregion

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
        /// Gets the <see cref="AssemblyRedirectionPolicyManager"/> for the current domain.
        /// </summary>
        public AssemblyRedirectionPolicyManager AssemblyRedirectionPolicies
        {
            get { return assemblyRedirectionPolicy; }
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return "Domain " + this.domainGuid.ToString();
        }
    }

    public sealed class AssemblyLoadingEventArgs : EventArgs
    {
        internal AssemblyLoadingEventArgs(string assemblyLocation, bool lazyLoading,
            IAssemblyName assemblyName, Assembly assembly)
        {
            this.AssemblyLocation = assemblyLocation;
            this.LazyLoading = lazyLoading;
            this.AssemblyName = assemblyName;
            this.Assembly = assembly;
        }

        public Assembly Assembly { get; private set; }
        public IAssemblyName AssemblyName { get; private set; }
        public string AssemblyLocation { get; private set; }
        public bool LazyLoading { get; set; }
    }

}