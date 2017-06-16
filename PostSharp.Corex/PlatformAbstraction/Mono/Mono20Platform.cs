using System;
using System.Diagnostics.SymbolStore;
using System.Security;
using System.Security.Policy;
using PostSharp.CodeModel;
using PostSharp.ModuleReader;

namespace PostSharp.PlatformAbstraction.Mono
{
    internal class Mono20Platform : Platform
    {
        public Mono20Platform()
        {
            this.Identity = PlatformIdentity.Mono;
            this.IntrinsicOfOppositeSignAssignable = true;
            this.DefaultTargetPlatformName = "mono-2.0";
            this.ReadModuleStrategy = ReadModuleStrategy.FromDisk;
        }

        public override string NormalizeCilIdentifier( string name )
        {
            return name.Replace( "~", "__" ).Replace( '.', '_' );
        }

        internal override ISymbolReader GetSymbolReader( ModuleReader.ModuleReader moduleReader )
        {
            return null;
        }

        public override AppDomain CreateAppDomain( string name, Evidence evidence, AppDomainSetup setup,
                                                   PermissionSet permissions )
        {
            return AppDomain.CreateDomain( name, evidence, setup );
        }

        public override string FindAssemblyInCache( IAssemblyName assemblyName )
        {
            return null;
        }
    }
}