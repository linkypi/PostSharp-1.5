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

#if !MONO

using System;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using PostSharp.CodeModel;
using PostSharp.ModuleReader;

namespace PostSharp.PlatformAbstraction.DotNet
{
    internal class DotNet20Platform : Platform
    {
        private IAssemblyCache assemblyCache;

        public DotNet20Platform()
        {
            this.Identity = PlatformIdentity.Microsoft;
            this.IntrinsicOfOppositeSignAssignable = true;
            this.DefaultTargetPlatformName = "net-2.0";
            this.ReadModuleStrategy = ReadModuleStrategy.FromMemoryImage;
        }

        /// <summary>
        /// Throws an exception in case of error code.
        /// </summary>
        /// <param name="hr">An HRESULT.</param>
        private static void VerifyHR( int hr )
        {
            if ( hr != 0 )
            {
                Marshal.ThrowExceptionForHR( hr );
            }
        }

        public override string NormalizeCilIdentifier( string name )
        {
            return name;
        }

        internal override ISymbolReader GetSymbolReader( ModuleReader.ModuleReader moduleReader )
        {
            string assemblyLocation = moduleReader.ModuleLocation;
            string pdbFileName = Path.ChangeExtension( assemblyLocation, "pdb" );

            if ( File.Exists( pdbFileName ) )
            {
                Trace.ModuleReader.WriteLine( "Found the file {0}. Trying to load it.", pdbFileName );

                Guid iidImport = Guids.IID_IMetadataImport2;


                IMetadataDispenserEx dispenser =
                    (IMetadataDispenserEx)
                    Activator.CreateInstance( Type.GetTypeFromCLSID( Guids.CLSID_CorMetadataDispenser, true ) );

                UnmanagedBuffer metadataDirectory = moduleReader.ImageReader.MetadataDirectory;

                IntPtr pImport;
                VerifyHR( dispenser.OpenScopeOnMemory( metadataDirectory.Origin, (uint) metadataDirectory.Size, 0,
                                                       ref iidImport, out pImport ) );


                SymBinder binder = new SymBinder();
                ISymbolReader symbolReader;
                try
                {
                    symbolReader =
                        binder.GetReader( pImport, assemblyLocation,
                                          Path.GetDirectoryName( assemblyLocation ) );
                }
                catch ( COMException )
                {
                    symbolReader = null;
                }

                Marshal.Release( pImport );
                Marshal.ReleaseComObject( dispenser );

                return symbolReader;
            }
            else
            {
                return null;
            }
        }

        public override AppDomain CreateAppDomain( string name, Evidence evidence, AppDomainSetup setup,
                                                   PermissionSet permissions )
        {
            return AppDomain.CreateDomain( name, evidence, setup, permissions );
        }

     
        public override string FindAssemblyInCache( IAssemblyName assemblyName )
        {
            if ( assemblyName.GetPublicKeyToken() == null || assemblyName.GetPublicKeyToken().Length == 0 )
                return null;

            string name = assemblyName.FullName;

            // Try the given full name.
            string location = QueryAssemblyInfo(name);
            if (location != null) return location;

            // Try the current processor architecture.
            string processorArchitecture = IntPtr.Size == 4 ? "x86" : "AMD64";
            location = QueryAssemblyInfo(name + ", processorArchitecture=" + processorArchitecture);
            if (location != null) return location;

            // Finally try MSIL.
            location = QueryAssemblyInfo(name + ", processorArchitecture=MSIL");

            return location;

        }

        private String QueryAssemblyInfo(String assemblyName)
        {
            ASSEMBLY_INFO assembyInfo = new ASSEMBLY_INFO {cchBuf = 512};
            assembyInfo.currentAssemblyPath = new String('\0',assembyInfo.cchBuf);

            // Get IAssemblyCache pointer
            int hr;
            if (this.assemblyCache == null)
            {
                hr = GacApi.CreateAssemblyCache( out assemblyCache, 0 );
                if ( hr != 0 )
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            AssemblyLoadHelper.WriteLine( "Looking in GAC for {0}.", assemblyName );
            hr = assemblyCache.QueryAssemblyInfo(1, assemblyName, ref assembyInfo);
        
            if ( hr != 0)
            {

                if ((uint) hr == 0x80070002)
                {
                    return null;
                }
                else
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            AssemblyLoadHelper.WriteLine("Found in GAC!");
            return assembyInfo.currentAssemblyPath;
        }
        class GacApi
        {
            [DllImport("fusion.dll")]
            internal static extern int CreateAssemblyCache(
                    out IAssemblyCache ppAsmCache,
                    int reserved);

        }
        // GAC Interfaces - IAssemblyCache. As a sample, non used vtable entries declared as dummy.
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        interface IAssemblyCache
        {
            int Dummy1();
            [PreserveSig()]
            int QueryAssemblyInfo(
                                int flags,
                                [MarshalAs(UnmanagedType.LPWStr)]
                            String assemblyName,
                                ref ASSEMBLY_INFO assemblyInfo);
            int Dummy2();
            int Dummy3();
            int Dummy4();
        }
        [StructLayout(LayoutKind.Sequential)]
        struct ASSEMBLY_INFO
        {
            public int cbAssemblyInfo;
            public int assemblyFlags;
            public long assemblySizeInKB;
            [MarshalAs(UnmanagedType.LPWStr)]
            public String currentAssemblyPath;
            public int cchBuf;
        }
    }
}

#endif