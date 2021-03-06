#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace PostSharp.Samples.Host
{
    /// <summary>
    /// This class serves as the entry point from the parent AppDomain. It starts the
    /// client program in the proper thread apartment and resolves assembly references.
    /// </summary>
    internal class ClientDomainManager : MarshalByRefObject
    {
        /// <summary>
        /// Maps an assembly name (generated by <see cref="MakeAssemblyKey"/>)
        /// to the location of the woven implementation. This dictionary is maintained
        /// by the <see cref="SetAssemblyLocation"/> and <see cref="RenameAssembly"/>
        /// methods and consumed by the <see cref="OnAssemblyResolve"/> method.
        /// </summary>
        private readonly Dictionary<string, string> assemblyLocations =
            new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );


        /// <summary>
        /// Initializes a new <see cref="ClientDomainManager"/>.
        /// </summary>
        public ClientDomainManager()
        {
            // Registers for the AssemblyResolve event.
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        #region Program Starting Up

        /// <summary>
        /// Executes a given program assembly with given arguments. Called by
        /// the system AppDomain.
        /// </summary>
        /// <param name="program">Location of the assembly to be executed.</param>
        /// <param name="arguments">Arguments to be passed.</param>
        public void Execute( string program, string[] arguments )
        {
            // Load the program assembly.
            Assembly programAssembly = Assembly.LoadFrom( program );

            // Choose the kind of thread model.
            ApartmentState apartmentState;
            if ( programAssembly.EntryPoint.GetCustomAttributes( typeof(STAThreadAttribute), false ).Length > 0 )
            {
                apartmentState = ApartmentState.STA;
            }
            else
            {
                apartmentState = ApartmentState.MTA;
            }


            // Choose the kind of method signature.
            object[] methodArguments;
            switch ( programAssembly.EntryPoint.GetParameters().Length )
            {
                case 0:
                    methodArguments = null;
                    break;

                case 1:
                    methodArguments = new object[] {arguments};
                    break;

                default:
                    throw new InvalidProgramException();
            }

            // Initialize a ProgramStarter instance and execute it in a new thread with proper apartment
            // thread.
            ProgramStarter programStarter = new ProgramStarter( programAssembly.EntryPoint, methodArguments );
            if ( apartmentState == ApartmentState.STA )
            {
                // Create the new thread, set the apartment state and start it.
                Thread thread = new Thread( programStarter.Invoke );
                thread.SetApartmentState( apartmentState );
                thread.Start();

                // Wait until this thread (i.e. the client program) ends.
                thread.Join();
            }
            else
            {
                // If we do not need STA, we do not need a new thread neither.
                programStarter.Invoke();
            }
        }

        /// <summary>
        /// Encapsulate a call to a method through reflection. The <see cref="Invoke"/>
        /// method can be used as a <see cref="Thread"/> main method.
        /// </summary>
        private class ProgramStarter
        {
            private readonly MethodInfo method;
            private readonly object[] arguments;

            /// <summary>
            /// Initializes a new <see cref="ProgramStarter"/>.
            /// </summary>
            /// <param name="method">The method to be executed.</param>
            /// <param name="arguments">The array of arguments passed to the method.</param>
            public ProgramStarter( MethodInfo method, object[] arguments )
            {
                this.method = method;
                this.arguments = arguments;
            }

            /// <summary>
            /// Executes the invocation represented by the current instance.
            /// </summary>
            public void Invoke()
            {
                this.method.Invoke( null, arguments );
            }
        }

        #endregion

        #region Assembly Resolution

        /// <summary>
        /// Make a canonized <i>assembly key</i> for indexing  in the <see cref="assemblyLocations"/>
        /// dictionary.
        /// </summary>
        /// <param name="assemblyName">Complete assembly name.</param>
        /// <returns>A canonized string corresponding to <paramref name="assemblyName"/>.</returns>
        public string MakeAssemblyKey( AssemblyName assemblyName )
        {
            return string.Format( "{0}, Version={1}", assemblyName.Name, assemblyName.Version );
        }

        /// <summary>
        /// Handles the <see cref="AppDomain.AssemblyResolve"/> event raised by the VRE.
        /// When the requested assembly has been processed, loads the woven file. 
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Event arguments.</param>
        /// <returns></returns>
        private Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            // Get the canonized key.
            string assemblyKey = MakeAssemblyKey( new AssemblyName( args.Name ) );

            // Gets the location of this file.
            string file;
            if ( this.assemblyLocations.TryGetValue( assemblyKey, out file ) )
            {
                // We have a location. It means that the assembly has been woven.
                // Load it and return it.
                Assembly assembly = Assembly.LoadFrom( file );
                return assembly;
            }
            else
            {
                // We do not know this assembly.
                return null;
            }
        }


        /// <summary>
        /// Called by the host to maintain the repository of woven assemblies.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="file">Location of the assembly.</param>
        public void SetAssemblyLocation( AssemblyName assemblyName, string file )
        {
            this.assemblyLocations.Add( MakeAssemblyKey( assemblyName ), file );
        }

        /// <summary>
        /// Called by the host (and originally by the PostSharp Object) when
        /// an assembly has been renamed. We should update our records in
        /// the repository of woven assemblies.
        /// </summary>
        /// <param name="oldAssemblyName">Original name.</param>
        /// <param name="newAssemblyName">New name.</param>
        public void RenameAssembly( AssemblyName oldAssemblyName, AssemblyName newAssemblyName )
        {
            // We just rename the key.
            string oldAssemblyKey = MakeAssemblyKey( oldAssemblyName );
            string newAssemblyKey = MakeAssemblyKey( newAssemblyName );

            string file = this.assemblyLocations[oldAssemblyKey];
            this.assemblyLocations[newAssemblyKey] = file;
            this.assemblyLocations.Remove( oldAssemblyKey );
        }

        #endregion
    }
}