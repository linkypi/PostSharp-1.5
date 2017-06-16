using System;
using System.IO;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Helpers;
using PostSharp.PlatformAbstraction;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Implementation of <see cref="ModuleLoadStrategy"/> that loads a module
    /// directly from disk, without using Reflection before.
    /// </summary>
    [Serializable]
    public class ModuleLoadDirectFromFileStrategy : ModuleLoadStrategy
    {
        private readonly string fileName;
        private readonly bool lazyLoading;
        private IAssemblyName assemblyName;

        /// <summary>
        /// Initializes a new <see cref="ModuleLoadDirectFromFileStrategy"/>
        /// and specifies whether the module should be loaded using lazy loading.
        /// </summary>
        /// <param name="fileName">Full path of the module.</param>
        /// <param name="lazyLoading"><b>true</b> if the module should be loaded using lazy loading,
        /// otherwise <b>false</b>.</param>
        public ModuleLoadDirectFromFileStrategy( string fileName, bool lazyLoading )
        {
            this.fileName = Path.GetFullPath( fileName );
            this.lazyLoading = lazyLoading;
        }

        /// <summary>
        /// Initializes a new <see cref="ModuleLoadDirectFromFileStrategy"/>
        /// that will not load the module using lazy loading.
        /// </summary>
        /// <param name="fileName">Full path of the module.</param>
        public ModuleLoadDirectFromFileStrategy( string fileName ) : this(fileName, false)
        {
            
        }

        /// <inheritdoc />
        public override ModuleDeclaration Load( Domain domain )
        {
            // Look in the domain if the same module has already been loaded.
            foreach ( AssemblyEnvelope assembly in domain.Assemblies )
            {
                foreach ( ModuleDeclaration module in assembly.Modules )
                {
                    if ( module.FileName.Equals( this.fileName, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        return module;
                    }
                }
            }

            // Not yet loaded. We have to load it now. We will suppose the module is the main module of an assembly.
            return domain.LoadAssembly( this.fileName, this.lazyLoading ).ManifestModule;
        }

        public override bool Matches( Domain domain, IAssemblyName assemblyName )
        {
            if ( this.assemblyName == null )
            {
                this.assemblyName = domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName( 
                    AssemblyNameWrapper.GetWrapper( AssemblyName.GetAssemblyName( this.fileName ) ));
            }

            return AssemblyComparer.GetInstance().Equals( this.assemblyName, assemblyName );
            
        }

        public override bool LazyLoading
        {
            get { return this.lazyLoading; }
        }
    }
}