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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using PostSharp.Utilities;

namespace PostSharp.Extensibility
{
    internal static class ToolInvocationHelper
    {
        public static int InvokeTool( string fileName, string commandLine, string workingDirectory, TextWriter outputWriter  )
        {
            string toolName = Path.GetFileNameWithoutExtension( fileName ).ToUpperInvariant();

            Process process = new Process
                                  {
                                      StartInfo =
                                          {
                                              FileName = fileName,
                                              Arguments = commandLine,
                                              WorkingDirectory = workingDirectory,
                                              ErrorDialog = false,
                                              UseShellExecute = false,
                                              CreateNoWindow = true,
                                              RedirectStandardError = true,
                                              RedirectStandardOutput = true
                                          }
                                  };

            process.ErrorDataReceived +=
                (( sender, e ) => outputWriter.WriteLine( e.Data ));

            process.OutputDataReceived +=
                ((sender, e) => outputWriter.WriteLine(e.Data));

            CoreMessageSource.Instance.Write( SeverityType.Info, "PS0035",
                                              new object[] {process.StartInfo.FileName, process.StartInfo.Arguments} );


            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
            {
                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                if ( !process.HasExited )
                {
                    process.WaitForExit();
                }

                Trace.Timings.WriteLine( "{0} executed in {1} ms.", toolName, timer.CurrentTime );
            }

            return process.ExitCode;
        }

        private static void ReceiveOutput( string toolName, SeverityType severity, string data, Regex parseRegex,
                                           Regex ignoreRegex )
        {
            if ( data == null )
            {
                return;
            }

            if ( ignoreRegex != null && ignoreRegex.IsMatch( data ) )
            {
                return;
            }

            if ( parseRegex != null )
            {
                Match match = parseRegex.Match( data );
                if ( match.Success )
                {
                    SeverityType parsedSeverity;
                    if ( match.Groups["severity"] != null )
                    {
                        switch ( match.Groups["severity"].Value )
                        {
                            case "error":
                                parsedSeverity = SeverityType.Error;
                                break;

                            case "warning":
                                parsedSeverity = SeverityType.Warning;
                                break;

                            default:
                                parsedSeverity = severity;
                                break;
                        }
                    }
                    else
                    {
                        parsedSeverity = severity;
                    }

                    int line = 0;
                    if ( match.Groups["line"] != null )
                    {
                        if ( !int.TryParse( match.Groups["line"].Value, out line ) )
                        {
                            line = 0;
                        }
                    }


                    CoreMessageSource.Instance.Write(
                        parsedSeverity, "PS0033",
                        new object[] {toolName, match.Groups["message"].Value},
                        match.Groups["filename"] == null ? null : match.Groups["filename"].Value,
                        line, Message.NotAvailable );
                }
            }
            else
            {
                CoreMessageSource.Instance.Write( severity, "PS0033",
                                                  new object[] {toolName, data} );
            }
        }
    }
}
