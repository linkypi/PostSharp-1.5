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
using System.IO;
using System.Reflection;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;
using PostSharp.PlatformAbstraction;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Locates an assembly in the search path or in GAC.
    /// </summary>
    public sealed class AssemblyLocator
    {
        private static readonly string[] assemblyExtensions = new[] {"exe", "dll"};

        /// <summary>
        /// Array of directories in which assemblies have to be looked for.
        /// </summary>
        private readonly Set<string> directories = new Set<string>( 16, StringComparer.InvariantCultureIgnoreCase );

        private readonly List<string> files = new List<string>();

        internal AssemblyLocator( Domain domain )
        {
            this.Domain = domain;
        }

        /// <summary>
        /// Gets the <see cref="Domain"/> used by the <see cref="AssemblyLocator"/> to
        /// resolve assembly name redirection.
        /// </summary>
        public Domain Domain { get; private set; }

        

        /// <summary>
        /// Add a directory to the search path.
        /// </summary>
        /// <param name="directory">A directory.</param>
        public bool AddDirectory( string directory )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( directory, "directory" );

            #endregion

            string normalizedDirectory = Path.GetFullPath( directory );

            if ( !Directory.Exists( normalizedDirectory ) )
            {
                return false;
            }

            if ( !this.directories.Contains( normalizedDirectory ) )
            {
                this.directories.Add( normalizedDirectory );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a directory from the search path.
        /// </summary>
        /// <param name="directory">Directory to be removed.</param>
        /// <returns><b>true</b> if the directory was previously present and removed, <b>false</b>
        /// if the directory was previously not in the search path.</returns>
        public bool RemoveDirectory( string directory )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( directory, "directory" );

            #endregion

            string normalizedDirectory = Path.GetFullPath( directory );

            if ( !Directory.Exists( normalizedDirectory ) )
            {
                return false;
            }

            if ( !this.directories.Contains( normalizedDirectory ) )
            {
                this.directories.Remove( normalizedDirectory );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a set of directories to the search path.
        /// </summary>
        /// <param name="directories">A set of directories.</param>
        public void AddDirectories( IEnumerable<string> directories )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( directories, "directories" );

            #endregion

            foreach ( string directory in directories )
            {
                this.AddDirectory( directory );
            }
        }

        /// <summary>
        /// Add a file to the search path.
        /// </summary>
        /// <param name="fullPath">Full file path.</param>
        public void AddFile( string fullPath )
        {
            this.files.Add( fullPath );
        }

        /// <summary>
        /// Compares two instances of <see cref="AssemblyName"/>.
        /// </summary>
        /// <param name="reference">Requested assembly name.</param>
        /// <param name="definition">Proposed (candidate) assembly name.</param>
        /// <returns><b>true</b> if both names are equal, otherwise <b>false</b>.</returns>
        internal bool AssemblyReferenceMatchesDefinition( IAssemblyName reference, IAssemblyName definition )
        {
            return
                CompareHelper.Equals(
                    definition, reference,
                    this.Domain.AssemblyRedirectionPolicies, false );
        }

        /// <summary>
        /// Event raised when an assembly reference is resolved.
        /// </summary>
        public event EventHandler<AssemblyLocateEventArgs> LocatingAssembly;

        /// <summary>
        /// Finds an assembly.
        /// </summary>
        /// <param name="assemblyName">Assmebly name.</param>
        /// <returns>The full path of the assembly, or <b>null</b> if the assembly was not found.</returns>
        public string FindAssembly( IAssemblyName assemblyName )
        {
            AssemblyLoadHelper.WriteLine( "Resolving the assembly name '{0}'.", assemblyName );


            // Apply redirection policies.
            IAssemblyName requestedAssemblyName = assemblyName;
            bool hasRedirectionPolicy =
                this.Domain.AssemblyRedirectionPolicies.HasRedirectionPolicy( requestedAssemblyName );
            if ( hasRedirectionPolicy )
            {
                requestedAssemblyName =
                    this.Domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName( requestedAssemblyName );
            }

            #region 0. Look in the assemblies loaded in the AppDomain
            Set<string> priorityDirectories = new Set<string>(8, StringComparer.InvariantCultureIgnoreCase);
            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if ( assembly.GlobalAssemblyCache || string.IsNullOrEmpty( assembly.Location ) )
                    continue;

                AssemblyName candidateAssemblyName = assembly.GetName();
                if (candidateAssemblyName.Name.Equals(requestedAssemblyName.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    AssemblyLoadHelper.WriteLine( "Probing file '{0}' because it is loaded in the AppDomain.",
                                                  assembly.Location );
                    if ( AssemblyReferenceMatchesDefinition( requestedAssemblyName,
                                                             AssemblyNameHelper.Convert(candidateAssemblyName)))
                    {
                        AssemblyLoadHelper.WriteLine( "Selecting file '{0}'.", assembly.Location );
                        return assembly.Location;
                    }
                    else
                    {
                        AssemblyLoadHelper.WriteLine( "Assembly '{0}' does not match the reference.", assembly );
                    }
                }

                priorityDirectories.AddIfAbsent( Path.GetDirectoryName( assembly.Location ) );

            }
            #endregion

            #region 1. Ask the host

            if ( this.LocatingAssembly != null )
            {
                AssemblyLocateEventArgs e = new AssemblyLocateEventArgs( requestedAssemblyName );
                this.LocatingAssembly( this, e );
                if ( e.AssemblyLocation != null )
                {
                    AssemblyLoadHelper.WriteLine( "The host returned the file name {0}.",
                                                  e.AssemblyLocation);
                    // Check that the file exist.
                    if (!File.Exists(e.AssemblyLocation))
                    {
                        throw ExceptionHelper.Core.CreateFileNotFoundException(
                            e.AssemblyLocation, "HostResolvedAssemblyNotFound");
                    }

                    return e.AssemblyLocation;

                }
            }

            #endregion

            // 2. Look in GAC
            if (!this.Domain.ReflectionDisabled)
            {
                string gacLocation = Platform.Current.FindAssemblyInCache( assemblyName );
                if ( gacLocation != null )
                {
                    return gacLocation;
                }
            }

            // 3. Look in the list of files
            foreach (string file in this.files)
            {
                string location = ProbeFile(file, requestedAssemblyName, "this file was explicitely added to the search path");
                if (location != null)
                    return location;
            }


            // 4. Look in directories containing the assemblies that are loaded in the AppDomain
            foreach ( string directory in priorityDirectories )
            {
                string location = ProbeDirectory( directory, requestedAssemblyName, "this directory contains an assembly that has already been loaded" );
                if (location != null)
                    return location;
            }
       
            // 5. Looking in the directories of the search path
            foreach ( string directory in directories )
            {
                string location = ProbeDirectory( directory, requestedAssemblyName, "because this directory is in the search path" );
                if (location != null)
                    return location;
            }


            // Not found.
            AssemblyLoadHelper.WriteLine( "The assembly \"{0}\" was not found.",
                                          requestedAssemblyName.FullName );

            return null;
        }

        private string ProbeDirectory( string directory, IAssemblyName requestedAssemblyName, string reason )
        {
            foreach ( string extension in assemblyExtensions )
            {
                string fileName = Path.Combine( directory, requestedAssemblyName.Name ) + "." + extension;
                string location = ProbeFile( fileName, requestedAssemblyName, reason );
                if (location != null)
                    return location;
                       
            }

            return null;
        }

        private string ProbeFile(string file, IAssemblyName requestedAssemblyName, string reason)
        {
         
            if ( string.Compare( Path.GetFileNameWithoutExtension( file ), requestedAssemblyName.Name,
                                 StringComparison.InvariantCultureIgnoreCase ) == 0 )
            {
                AssemblyLoadHelper.WriteLine(
                    "Probing location '{0}' because {1}.", file, reason);

                if ( !File.Exists( file ) )
                {
                    return null;
                }

                AssemblyName candidateAssemblyName = AssemblyName.GetAssemblyName( file );

                AssemblyLoadHelper.WriteLine( "File '{0}' has identity '{1}'.", file, candidateAssemblyName );

                if ( AssemblyReferenceMatchesDefinition( requestedAssemblyName,
                                                         AssemblyNameWrapper.GetWrapper( candidateAssemblyName ) ) )
                {

                    if (this.Domain.AssemblyRedirectionPolicies.HasRedirectionPolicy(candidateAssemblyName))
                    {
                        AssemblyLoadHelper.WriteLine(
                            "Policies apply to assembly '{0}'. We cannot " +
                            "load an assembly whose reference should be redirected.",
                            candidateAssemblyName);
                        return null;
                    }

                    AssemblyLoadHelper.WriteLine( "Selecting file '{0}'.", file );
                    return file;
                }

                AssemblyLoadHelper.WriteLine("File '{0}' does not match the reference.", file);
            }

           
            return null;
        }
    }

    /// <summary>
    /// Arguments of the <see cref="AssemblyLocator.LocatingAssembly"/> event.
    /// </summary>
    public sealed class AssemblyLocateEventArgs : EventArgs
    {
        private readonly IAssemblyName assemblyName;

        internal AssemblyLocateEventArgs(IAssemblyName name)
        {
            this.assemblyName = name;
        }

        /// <summary>
        /// Gets the name of the assembly being resolved.
        /// </summary>
        public IAssemblyName AssemblyName
        {
            get { return assemblyName; }
        }

        /// <summary>
        /// Gets or sets the assembly location on the filesystem.
        /// </summary>
        public string AssemblyLocation { get; set; }
    }
}