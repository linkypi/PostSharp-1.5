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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using PostSharp.Extensibility;

namespace PostSharp
{
    /// <summary>
    /// Provides information about PostSharp.
    /// </summary>
    public static class ApplicationInfo
    {
        private static readonly string baseDirectory;
        private static readonly NameValueCollection settings = new NameValueCollection();
        private static readonly Version version;

        [SuppressMessage( "Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline" )]
        static ApplicationInfo()
        {
            // Determines the base location.
            Assembly assembly = typeof(ApplicationInfo).Assembly;

            if ( assembly.GlobalAssemblyCache )
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey( LocalMachineRegistryKeyName, false );

                if ( key == null )
                {
                    CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0053", null );
                    throw new UnreachableException();
                }

                try
                {
                    baseDirectory = (string) key.GetValue( "Location" );
                }
                finally
                {
                    key.Close();
                }
            }
            else
            {
                baseDirectory = Path.GetDirectoryName( assembly.Location );
            }

            // Gets the assembly version.
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo( assembly.Location );
            version = new Version( fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart,
                                   fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart );

            // Load application settings.
            string settingsFileName = Path.Combine( baseDirectory, "PostSharp-Library.config" );
            if ( File.Exists( settingsFileName ) )
            {
                using ( StreamReader reader = new StreamReader( settingsFileName ) )
                {
                    string line;
                    int lineNo = 0;
                    char[] trimmableChars = new[] {' ', '\t'};

                    while ( ( line = reader.ReadLine() ) != null )
                    {
                        lineNo++;

                        line = line.TrimStart( trimmableChars );
                        if ( line.StartsWith( "#" ) || line.Length == 0 )
                        {
                            continue;
                        }

                        int pos = line.IndexOf( '#' );
                        if ( pos >= 0 )
                        {
                            line = line.Remove( pos );
                        }

                        line = line.TrimEnd( trimmableChars );

                        pos = line.IndexOf( '=' );
                        if ( pos < 0 || pos == line.Length - 1 )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Warning,
                                                              "PS0056", new object[] {lineNo} );
                            continue;
                        }


                        settings.Add( line.Substring( 0, pos ).TrimEnd( trimmableChars ),
                                      line.Substring( pos + 1 ).TrimStart( trimmableChars ) );
                    }
                }
            }
        }


        /// <summary>
        /// Gets a setting whose value is <see cref="bool"/>.
        /// </summary>
        /// <param name="key">Setting name.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>The setting value, or <paramref name="defaultValue"/> if the setting is not defined.</returns>
        public static bool GetSettingBoolean( string key, bool defaultValue )
        {
            string valueString = settings[key];
            if ( valueString == null )
            {
                return defaultValue;
            }
            return String.Compare( valueString, "true", true,
                                   CultureInfo.InvariantCulture ) == 0;
        }


        /// <summary>
        /// Gets the application base installation directory.
        /// </summary>
        public static string BaseDirectory
        {
            get { return baseDirectory; }
        }

        /// <summary>
        /// Gets the version of the <b>PostSharp.Core</b> assembly.
        /// </summary>
        public static Version Version
        {
            get { return version; }
        }


        /// <summary>
        /// Gets the registry key under PostSharp configuration is registered,
        /// under the LOCAL_MACHINE registry hive.
        /// </summary>
        public static string LocalMachineRegistryKeyName
        {
            get
            {
                if ( IntPtr.Size == 4 )
                {
                    return @"SOFTWARE\postsharp.org\PostSharp 1.5";
                }
                else
                {
                    return @"SOFTWARE\Wow6432Node\postsharp.org\PostSharp 1.5";
                }
            }
        }

        /// <summary>
        /// Gets the registry key under PostSharp configuration is registered,
        /// under the USER registry hive.
        /// </summary>
        public static string UserRegistryKeyName
        {
            get { return @"SOFTWARE\postsharp.org\PostSharp 1.5"; }
        }

        /// <summary>
        /// Event raised when a setting is changed after it is initially read from the configuration file.
        /// </summary>
        public static event PropertyChangedEventHandler SettingChanged;

        /// <summary>
        /// Change an application setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        public static void SetSetting( string name, string value )
        {
            if ( value != settings[name] )
            {
                settings[name] = value;
                if ( SettingChanged != null )
                {
                    SettingChanged( null, new PropertyChangedEventArgs( name ) );
                }
            }
        }
    }
}