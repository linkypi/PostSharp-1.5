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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Xml;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Checks whether newer versions of PostSharp or other related are available and
    /// write a warning if so.
    /// </summary>
    public static class AutoUpdate
    {
        private static Thread autoUpdateThread;

        /// <summary>
        /// Executes <see cref="GetAutoUpdateMessages"/> in a new thread. Gives the
        /// opportunity to the caller to kill the thread if it does not finish in time.
        /// </summary>
        /// <returns>The thread in which the <see cref="GetAutoUpdateMessages"/>
        /// method runs.</returns>
        public static void BeginRetrieveMessages()
        {
            autoUpdateThread = new Thread( GetAutoUpdateMessages ) {Priority = ThreadPriority.Lowest};
            autoUpdateThread.Start();
        }

        /// <summary>
        /// Interrupts the process initiated by the <see cref="BeginRetrieveMessages"/>
        /// method.
        /// </summary>
        public static void StopRetrieveMessages()
        {
            if ( autoUpdateThread != null )
            {
                if ( autoUpdateThread.IsAlive )
                {
                    Trace.AutoUpdate.WriteLine( "Aborting auto-update..." );
                    autoUpdateThread.Abort();
                    autoUpdateThread.Join();
                }
                autoUpdateThread = null;
            }
        }


        /// <summary>
        /// Checks whether newer versions of PostSharp or other related are available and
        /// write a warning if so.
        /// </summary>
        public static void GetAutoUpdateMessages()
        {
            Trace.AutoUpdate.WriteLine( "Starting." );

            string autoUpdateDirectory =
                Path.Combine(
                    Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                    "PostSharp 1.5" );

            string autoUpdateXmlFile =
                Path.Combine( autoUpdateDirectory, "AutoUpdate.xml" );

            // The presence of this file waive the auto-update process the
            // number of days specified in the file.
            string autoUpdateWaiveFile =
                Path.Combine( autoUpdateDirectory, "AutoUpdate.waive" );

            string autoUpdateNoUpdateFile =
                Path.Combine( autoUpdateDirectory, "NoUpdates" );

            try
            {
                string updatesUrl = "http://www.sharpcrafters.com/updates.xml";


                // Create the directory in which the update file shall be downloaded.
                if ( !Directory.Exists( autoUpdateDirectory ) )
                {
                    Trace.AutoUpdate.WriteLine( "Creating the directory {{{0}}}.", autoUpdateDirectory );
                    Directory.CreateDirectory( autoUpdateDirectory );
                }

                // Determines when the last AutoUpdate check has been performed.

                DateTime lastCheckDate = File.Exists( autoUpdateXmlFile )
                                             ? File.GetLastWriteTime( autoUpdateXmlFile )
                                             : DateTime.MinValue;

                DateTime nextCheckDate = lastCheckDate.AddDays( 7 );

                if ( File.Exists( autoUpdateWaiveFile ) )
                {
                    DateTime waiveDate = File.GetLastWriteTime( autoUpdateWaiveFile );
                    int waiveDays;

                    if ( int.TryParse( File.ReadAllText( autoUpdateWaiveFile ).Trim(
                                           ' ', '\t', '\n', '\r' ), out waiveDays ) )
                    {
                        nextCheckDate = waiveDate.AddDays( waiveDays );

                        if ( nextCheckDate <= DateTime.Now )
                            File.Delete( autoUpdateWaiveFile );
                    }
                    else
                    {
                        File.Delete( autoUpdateWaiveFile );
                    }
                }

                Trace.AutoUpdate.WriteLine( "The next check date is {0}.", nextCheckDate );


                if ( nextCheckDate <= DateTime.Now )
                {
                    // We should read the updates file

                    // Check if the computer is connected to the network.
                    // MONO: This will return always TRUE
                    if ( !NetworkInterface.GetIsNetworkAvailable() )
                    {
                        Trace.AutoUpdate.WriteLine(
                            "Skipping auto-update because the computer is not connected to the network." );
                        return;
                    }

                    // Delete the "NoUpdate" file, since there may be new ones.
                    if ( File.Exists( autoUpdateNoUpdateFile ) )
                        File.Delete( autoUpdateNoUpdateFile );


                    updatesUrl += "?current=" + ApplicationInfo.Version.ToString();

                    Trace.AutoUpdate.WriteLine( "Retrieving the update file from {{{0}}} into {{{1}}}.",
                                                updatesUrl, autoUpdateXmlFile );

                    // Retrieve the update file
                    using ( WebClient webClient = new WebClient() )
                    {
                        bool success = false;

                        try
                        {
                            ManualResetEvent doneEvent = new ManualResetEvent( false );
                            webClient.DownloadFileCompleted +=
                                delegate( Object sender, AsyncCompletedEventArgs e )
                                    {
                                        if ( e.Cancelled )
                                        {
                                            Trace.AutoUpdate.WriteLine( "Automatic update was cancelled." );
                                        }
                                        else if ( e.Error != null )
                                        {
                                            CoreMessageSource.Instance.Write( SeverityType.Info,
                                                                              "PS0092", new object[] {e.Error.Message} );
                                        }
                                        else
                                        {
                                            success = true;
                                        }
                                        doneEvent.Set();
                                    };

                            webClient.DownloadFileAsync( new Uri( updatesUrl ), autoUpdateXmlFile );

                            // This call is likely to get an "abort" signal.
                            doneEvent.WaitOne();
                        }
                        catch ( ThreadAbortException )
                        {
                            // MONO: Mono implementation may be buggy.
                            webClient.CancelAsync();
                            success = true;
                            return;
                        }
                        catch ( Exception e )
                        {
                            // Emit a message.
                            CoreMessageSource.Instance.Write(
                                SeverityType.ImportantInfo, "PS0063", new object[] {e.Message} );

                            return;
                        }
                        finally
                        {
                            if ( !success )
                            {
                                // Set that we will try tomorrow.
                                File.WriteAllText( autoUpdateWaiveFile, "1" );
                                Trace.AutoUpdate.WriteLine( "Failure: we won't make any other attempt during one day." );

                                if ( File.Exists( autoUpdateXmlFile ) )
                                {
                                    Trace.AutoUpdate.WriteLine( "Deleting {0}.", autoUpdateXmlFile );
                                    File.Delete( autoUpdateXmlFile );
                                }
                            }
                        }
                    }
                }


                // At this point, if the update file exists, we should check versions.
                if ( File.Exists( autoUpdateXmlFile ) &&
                     !File.Exists( autoUpdateNoUpdateFile ) )
                {
                    bool updateAvailable = false;

                    Trace.AutoUpdate.WriteLine( "The file {{{0}}} was found. Checking versions.",
                                                autoUpdateXmlFile );

                    // Load the document in an XML DOM
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load( autoUpdateXmlFile );
                    XmlNamespaceManager nsManager = new XmlNamespaceManager( xmlDocument.NameTable );
                    nsManager.AddNamespace( "u", "http://schemas.postsharp.org/1.0/autoupdate" );

                    // Test the version of each application and write available updates
                    // into the user registry hive.
                    foreach (
                        XmlElement applicationElement in
                            xmlDocument.SelectNodes( "/u:Updates/u:Application", nsManager ) )
                    {
                        string applicationId = applicationElement.GetAttribute( "id" );

                        Trace.AutoUpdate.WriteLine( "Inspecting the application {{{0}}}.", applicationId );

                        string versionFile = Path.Combine(
                            ApplicationInfo.BaseDirectory, applicationId + ".version" );

                        // Check whether this application is installed.
                        if ( File.Exists( versionFile ) )
                        {
                            string installedVersionString = File.ReadAllLines( versionFile )[0];
                            string availableVersionString = applicationElement.GetAttribute( "Version" );


                            Version installedVersion = new Version( installedVersionString );
                            Version availableVersion = new Version( availableVersionString );

                            Trace.AutoUpdate.WriteLine( "Version {{{0}}} is installed, version {{{1}}} is available.",
                                                        installedVersionString, availableVersionString );

                            if ( availableVersion > installedVersion )
                            {
                                string downloadUrl = applicationElement.GetAttribute( "Url" );

                                // A new version of {0} is available. You have currently {1}, 
                                // and you can download the version {2} on {3}.
                                CoreMessageSource.Instance.Write(
                                    SeverityType.Warning,
                                    "PS0064",
                                    new object[]
                                        {
                                            applicationElement.GetAttribute( "Name" ),
                                            installedVersionString, availableVersionString,
                                            downloadUrl
                                        }, (string) null );

                                updateAvailable = true;
                            }
                        }
                    }

                    // If no update was found, we can delete the file.
                    if ( !updateAvailable )
                    {
                        Trace.AutoUpdate.WriteLine( "No update available." );
                        File.WriteAllText( autoUpdateNoUpdateFile, "No update available." );
                    }
                }
            }
            catch ( ThreadAbortException )
            {
                try
                {
                    if ( File.Exists( autoUpdateXmlFile ) )
                        File.Delete( autoUpdateXmlFile );
                }
// ReSharper disable EmptyGeneralCatchClause
                catch
// ReSharper restore EmptyGeneralCatchClause
                {
                }

                Trace.AutoUpdate.WriteLine( "Auto-update aborted." );
            }
            catch ( Exception e )
            {
                try
                {
                    if ( File.Exists( autoUpdateXmlFile ) )
                        File.Delete( autoUpdateXmlFile );
                }
                catch
                {
                }


                CoreMessageSource.Instance.Write( SeverityType.Info,
                                                  "PS0092", new object[] {e.Message} );
            }
            finally
            {
                Trace.AutoUpdate.WriteLine( "Finished." );
            }
        }
    }
}