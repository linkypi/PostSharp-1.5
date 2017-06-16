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
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Helper class to load assemblies. Provides a "binding log" for our custom
    /// binder and appends this log in exception text.
    /// </summary>
    public static class AssemblyLoadHelper
    {
        private static readonly StringBuilder stringBuilder = new StringBuilder();
        private static int nestingLevel;

        private static void Indent()
        {
            nestingLevel++;
        }

        private static void Unindent()
        {
            nestingLevel--;
            if ( nestingLevel == 0 )
                stringBuilder.Length = 0;
        }

        internal static void WriteLine( string message )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( message, "message" );

            #endregion

            stringBuilder.Append( '\t', nestingLevel );

            stringBuilder.Append( message );
            stringBuilder.Append( Environment.NewLine );

            Trace.AssemblyBinder.WriteLine( message );
        }

        internal static void WriteLine( string formatString, params object[] args )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( formatString, "formatString" );

            #endregion

            stringBuilder.Append( '\t', nestingLevel );

            if ( args != null )
            {
                stringBuilder.AppendFormat( formatString, args );
            }
            else
            {
                stringBuilder.Append( formatString );
            }

            stringBuilder.Append( Environment.NewLine );

            Trace.AssemblyBinder.WriteLine( formatString, args );
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        public static void ClearLog()
        {
            stringBuilder.Length = 0;
        }

        /// <summary>
        /// Gets the current log content.
        /// </summary>
        /// <returns>The current log content.</returns>
        public static string GetLog()
        {
            return stringBuilder.ToString();
        }

        private static void ManageAssemblyLoadException( string assemblyName, Exception e )
        {
            throw ExceptionHelper.Core.CreateBindingException( "CannotLoadAssembly",
                                                               assemblyName, e.Message, GetLog() );
        }

        /// <summary>
        /// Loads an assembly given its name.
        /// </summary>
        /// <param name="assemblyName">The full assembly name.</param>
        /// <returns>The assembly whose name is <paramref name="assemblyName"/>.</returns>
        public static Assembly LoadAssemblyFromName( string assemblyName )
        {
            return LoadAssemblyFromName( assemblyName, null );
        }

        /// <summary>
        /// Loads an assembly given its name, and passes an <see cref="Evidence"/>.
        /// </summary>
        /// <param name="assemblyName">The full assembly name.</param>
        /// <param name="assemblySecurity">Evidence with which the assembly should be loaded.</param>
        /// <returns>The assembly whose name is <paramref name="assemblyName"/>.</returns>
        public static Assembly LoadAssemblyFromName( string assemblyName, Evidence assemblySecurity )
        {
            Indent();
            WriteLine( "Loading assembly {{{0}}}.", assemblyName );
            try
            {
                Assembly assembly = Assembly.Load( assemblyName, assemblySecurity );
                WriteLine( "Assembly {{{0}}} loaded successfully.", assemblyName );
                return assembly;
            }
            catch ( Exception e )
            {
                WriteLine( e.ToString() );
                ManageAssemblyLoadException( assemblyName, e );
                throw; // unreacheable
            }
            finally
            {
                Unindent();
            }
        }

        /// <summary>
        /// Loads an assembly given its location full path.
        /// </summary>
        /// <param name="fileName">Full path of the assembly file.</param>
        /// <returns>The <see cref="Assembly"/> located at <paramref name="fileName"/>.</returns>
        public static Assembly LoadAssemblyFromFile( string fileName )
        {
            return LoadAssemblyFromFile( fileName, null );
        }

        /// <summary>
        /// Loads an assembly given its location full path and passes an <see cref="Evidence"/>.
        /// </summary>
        /// <param name="fileName">Full path of the assembly file.</param>
        /// <param name="assemblySecurity">Evidence with which the assembly should be loaded.</param>
        /// <returns>The <see cref="Assembly"/> located at <paramref name="fileName"/>.</returns>
        public static Assembly LoadAssemblyFromFile( string fileName, Evidence assemblySecurity )
        {
            fileName = Path.GetFullPath(fileName);

            WriteLine( "Loading assembly from file {{{0}}}.", fileName );
            Indent();
            try
            {
                Assembly assembly = Assembly.LoadFile( fileName );
                WriteLine( "Assembly {{{0}}} loaded successfully.", fileName );
                return assembly;
            }
            catch ( Exception e )
            {
                WriteLine( e.ToString() );
                ManageAssemblyLoadException( fileName, e );
                throw; // unreacheable
            }
            finally
            {
                Unindent();
            }
        }

        /// <summary>
        /// Splits a assembly-qualified type name into a type name and an assembly name.
        /// </summary>
        /// <param name="qualifiedTypeName">Assembly-qualified type name.</param>
        /// <param name="typeName">Type name.</param>
        /// <param name="assemblyName">Assembly name.</param>
        public static void SplitTypeName( string qualifiedTypeName, out string typeName, out string assemblyName )
        {
            int index = qualifiedTypeName.IndexOf(',');
            if (index < 0) throw new ArgumentOutOfRangeException("qualifiedTypeName");
            typeName = qualifiedTypeName.Substring(0, index).Trim();
            assemblyName = qualifiedTypeName.Substring(index + 1).Trim();
        }

        /// <summary>
        /// Loads a type given its fully qualified name.
        /// </summary>
        /// <param name="typeName">Fully qualified type name.</param>
        /// <returns>The type named <paramref name="typeName"/>.</returns>
        public static Type LoadType( string typeName )
        {
            Indent();
            WriteLine( "Loading type {{{0}}}.", typeName );
            try
            {
                Type type = Type.GetType( typeName, true );
                WriteLine( "Type {{{0}}} loaded successfully.", typeName );
                return type;
            }
            catch ( Exception e )
            {
                WriteLine( e.ToString() );
                throw ExceptionHelper.Core.CreateBindingException( "CannotLoadType",
                                                                   typeName, e.Message, GetLog() );
            }
            finally
            {
                Unindent();
            }
        }
    }
}