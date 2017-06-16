using System;
using System.Reflection;
using System.Security.Policy;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Helpers;
using PostSharp.Utilities;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Abstract strategy determining how a module should be loaded
    /// (typically by name, <see cref="ModuleLoadReflectionFromNameStrategy"/>;
    /// or by file path, <see cref="ModuleLoadReflectionFromFileStrategy"/>).
    /// </summary>
    /// <seealso cref="Assembly.LoadFrom(string,Evidence)"/>
    /// <seealso cref="Assembly.Load(string,Evidence)"/>
    [Serializable]
    public abstract class ModuleLoadStrategy
    {
        /// <summary>
        /// Loads the assembly into the current <see cref="AppDomain"/>.
        /// </summary>
        /// <returns>The loaded assembly.</returns>
        public abstract ModuleDeclaration Load( Domain domain );

        public abstract bool Matches( Domain domain, IAssemblyName assemblyName );

        public abstract bool LazyLoading { get; }
    }

    /// <summary>
    /// Abstract implementation of <see cref="ModuleLoadStrategy"/> that first 
    /// loads the assembly using reflection.
    /// </summary>
    [Serializable]
    public abstract class ModuleLoadReflectionStrategy : ModuleLoadStrategy
    {
        /// <summary>
        /// Gets or sets the <see cref="Evidence"/> with which the assembly should be loaded.
        /// </summary>
        public Evidence Evidence { get; set; }

        /// <summary>
        /// Loads the assembly using reflection.
        /// </summary>
        /// <returns></returns>
        public abstract Assembly LoadAssembly();

        /// <inheritdoc />
        public override ModuleDeclaration Load( Domain domain )
        {
            using ( HighPrecisionTimer reflectionAssemblyLoadTimer = new HighPrecisionTimer() )
            {
                // Load the assembly in the domain.
                Assembly assembly = this.LoadAssembly();

                Trace.Timings.WriteLine( "Reflection assembly {{{0}}} loaded in {1} ms.",
                                         assembly.FullName,
                                         reflectionAssemblyLoadTimer.CurrentTime );

                AssemblyEnvelope assemblyEnvelope = domain.GetAssembly( assembly );
                if ( assemblyEnvelope == null )
                {
                    assemblyEnvelope = domain.LoadAssembly( assembly, false );
                }

                return assemblyEnvelope.ManifestModule;
            }
        }

        public override bool LazyLoading
        {
            get { return false; }
        }
    }
}