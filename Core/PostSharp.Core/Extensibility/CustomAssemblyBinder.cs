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
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Implements custom policies for assembly binding: look in <i>all</i> directories
    /// in path and load the assembly with the highest version. 
    /// </summary>
    internal sealed class CustomAssemblyBinder
    {
     
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAssemblyBinder"/> class.
        /// </summary>
        private CustomAssemblyBinder()
        {
        }


        /// <summary>
        /// Gets the <see cref="CustomAssemblyBinder"/> associated to the current application
        /// domain, or <b>null</b> if there is no <see cref="CustomAssemblyBinder"/> associated
        /// to the current application domain.
        /// </summary>
        public static CustomAssemblyBinder Instance { get; private set; }

        public AssemblyLocator Locator { get; private set; }

        public static bool Initialize( bool resolve )
        {
            if ( Instance == null )
            {
                AppDomain appDomain = AppDomain.CurrentDomain;
                Instance = new CustomAssemblyBinder();
                Trace.AssemblyBinder.WriteLine( "Associating a CustomAssemblyBinder to the AppDomain {0}.",
                                                appDomain.FriendlyName );
                if ( resolve )
                    appDomain.AssemblyResolve += Instance.CurrentDomain_AssemblyResolve;

                appDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                return true;
            }
            else
                return false;
        }

        public static void Dispose()
        {
            if ( Instance != null )
            {
                AppDomain appDomain = AppDomain.CurrentDomain;

                Trace.AssemblyBinder.WriteLine( "Disassociating the CustomAssemblyBinder from the AppDomain {0}.",
                                                appDomain.FriendlyName );

                appDomain.AssemblyResolve -= Instance.CurrentDomain_AssemblyResolve;
                appDomain.AssemblyLoad -= CurrentDomain_AssemblyLoad;

                Instance = null;
            }
        }

        private static void CurrentDomain_AssemblyLoad( object sender, AssemblyLoadEventArgs args )
        {
            try
            {
                Trace.AssemblyBinder.WriteLine( "The assembly {{{0}}} was loaded from {1}.",
                                                args.LoadedAssembly.FullName, args.LoadedAssembly.Location );
            }
            catch ( NotSupportedException )
            {
                // Seems that Loadedassembly.Location is not supported in Mono with dynamic assemblies.
            }

            // Check that another assembly of the same name was not loaded before.
            string name = args.LoadedAssembly.GetName().Name;
            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if ( assembly != args.LoadedAssembly && assembly.GetName().Name == name )
                    CoreMessageSource.Instance.Write(
                        SeverityType.Warning, "PS0102",
                        new object[] {args.LoadedAssembly.Location, assembly.Location} );
            }
        }

        public bool FindAssembly( AssemblyName assemblyName, out string path, out Assembly alreadyLoadedAssembly )
        {
            AssemblyLoadHelper.WriteLine( "Resolving the assembly name '{0}'.", assemblyName );

            alreadyLoadedAssembly = null;

            // Apply redirection policies.
            IAssemblyName requestedAssemblyName = AssemblyNameWrapper.GetWrapper( assemblyName );
            bool hasRedirectionPolicy =
                this.Locator.Domain.AssemblyRedirectionPolicies.HasRedirectionPolicy(
                    requestedAssemblyName );
            if ( hasRedirectionPolicy )
            {
                requestedAssemblyName =
                    this.Locator.Domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName(
                        requestedAssemblyName );
            }


            foreach ( Assembly thisAssembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                AssemblyName candidateAssemblyName = thisAssembly.GetName();
                AssemblyNameWrapper candidateAssemblyNameWrapper = AssemblyNameWrapper.GetWrapper( candidateAssemblyName );

                if ( this.Locator.AssemblyReferenceMatchesDefinition( requestedAssemblyName,
                                                                      candidateAssemblyNameWrapper ) )
                {
                    AssemblyLoadHelper.WriteLine(
                        "An assembly with the same name was already found in the application domain " +
                        " and has compatible version." );
                    alreadyLoadedAssembly = thisAssembly;
                    path = thisAssembly.Location;
                        return true;
                }
            }

            path = this.Locator.FindAssembly( requestedAssemblyName );
            return path != null;
        }

        /// <summary>
        /// Implements the <see cref="AppDomain"/>.<see cref="AppDomain.AssemblyResolve"/>
        /// event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Event arguments.</param>
        /// <returns>The requested <see cref="Assembly"/>, or <b>null</b> if it could
        /// not be found.</returns>
        private Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            string path;

            AssemblyLoadHelper.WriteLine( "AppDomain.AssemblyResolve: Resolving reference '{0}'.",
                args.Name);

            // Apply redirection policies.
            AssemblyName requestedAssemblyName = new AssemblyName( args.Name );
            bool hasRedirectionPolicy =
                this.Locator.Domain.AssemblyRedirectionPolicies.HasRedirectionPolicy( requestedAssemblyName );
            if ( hasRedirectionPolicy )
            {
                requestedAssemblyName =
                    this.Locator.Domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName( requestedAssemblyName );
                AssemblyLoadHelper.WriteLine("AppDomain.AssemblyResolve: Assembly reference '{0}' has a redirection policy to '{1}'. Trying to load this assembly using Assembly.Load",
                                              args.Name, requestedAssemblyName );
                
                return Assembly.Load( requestedAssemblyName );
            }

            Assembly alreadyLoadedAssembly;
            if ( this.FindAssembly( requestedAssemblyName, out path, out alreadyLoadedAssembly ) )
            {
                if (alreadyLoadedAssembly != null)
                {
                    AssemblyLoadHelper.WriteLine("AppDomain.AssemblyResolve: assembly '{0}' already loaded in the application domain.", args.Name);
                    return alreadyLoadedAssembly;
                }
                else
                {
                    AssemblyLoadHelper.WriteLine("AppDomain.AssemblyResolve: assembly '{0}' located in '{1}'. Loading using AppDomain.LoadFrom.", args.Name, path);
                    return Assembly.LoadFrom(path);
                }
            }
            else
            {
                CoreMessageSource.Instance.Write(SeverityType.Warning, "PS0109",
                    new object[] { args.Name, Environment.NewLine + AssemblyLoadHelper.GetLog()});
                return null;
            }
        }

        public void SetAssemblyLocator( AssemblyLocator locator )
        {
            this.Locator = locator;
        }
    }
}