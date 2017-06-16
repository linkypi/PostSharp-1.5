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

#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PostSharp.Extensibility;

#endregion

namespace PostSharp.Samples.Host
{
    internal enum ReturnCode
    {
        Success = 0,
        InvalidCommandLine = 10,
        Error = 11,
        Exception = 12,
    }

    /// <summary>
    /// This class just parses the command line, then instantiate the <see cref="Host"/>.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Prints command line usage information.
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine(
                @"
Usage: PostSharp.Samples.Host.exe [<options>] <project> [<options>] <program>  [<program parameters>]

Options:
/?             Display the current help. 
/D:name=value  Sets a property.
/Pause         Wait for user input after completion.
/License       Prints the license and terminates.

" );
        }

        public static int Main( string[] args )
        {
            Environment.ExitCode = (int) InternalMain( args );
            return Environment.ExitCode;
        }

        private static ReturnCode InternalMain( string[] args )
        {
            bool pause = false;

            try
            {
                // A first check.
                if ( args.Length < 2 )
                {
                    Usage();
                    return ReturnCode.InvalidCommandLine;
                }

                // We have to fill the following variables.
                string programFileName = null;
                string projectFileName = null;
                PropertyCollection projectParameters = new PropertyCollection();
                List<string> programArguments = new List<string>();

                #region Iteration through arguments

                int argIterator = 0;
                while ( argIterator < args.Length )
                {
                    string currentArg = args[argIterator].ToLowerInvariant();

                    switch ( currentArg )
                    {
                        case "/?":
                            Usage();
                            return ReturnCode.Success;

                        case "/pause":
                            pause = true;
                            argIterator++;
                            break;

                        default:
                            if ( currentArg.StartsWith( "/P:" ) || currentArg.StartsWith( "/p:" ) )
                            {
                                // This is a constant definition.
                                Match match = Regex.Match( currentArg, @"/[P|p]\:(?<name>[^=]+)=(?<value>.*)$" );
                                if ( !match.Success )
                                {
                                    Usage();
                                    return ReturnCode.InvalidCommandLine;
                                }

                                projectParameters[match.Groups["name"].Value] = match.Groups["value"].Value;
                                argIterator++;
                            }
                            else if ( !currentArg.StartsWith( "/" ) )
                            {
                                argIterator++;
                                if ( projectFileName == null )
                                {
                                    projectFileName = Path.GetFullPath( currentArg );
                                }
                                else if ( programFileName == null )
                                {
                                    programFileName = Path.GetFullPath( currentArg );
                                    for ( ; argIterator < args.Length ; argIterator++ )
                                    {
                                        programArguments.Add( args[argIterator] );
                                    }
                                }
                                else
                                {
                                    Usage();
                                    return ReturnCode.InvalidCommandLine;
                                }
                            }
                            else
                            {
                                Usage();
                                return ReturnCode.InvalidCommandLine;
                            }
                            break;
                    }
                }

                #endregion

                // Some checks.
                if ( projectFileName == null )
                {
                    Console.Error.WriteLine( "No project specified." );
                    Usage();
                    return ReturnCode.InvalidCommandLine;
                }

                if ( !File.Exists( projectFileName ) )
                {
                    Console.Error.WriteLine( "The file {{{0}}} does not exist.", projectFileName );
                    return ReturnCode.Error;
                }

                if ( programFileName == null )
                {
                    Console.Error.WriteLine( "No input specified." );
                    Usage();
                    return ReturnCode.InvalidCommandLine;
                }

                if ( !File.Exists( programFileName ) )
                {
                    Console.Error.WriteLine( "The file {{{0}}} does not exist.", programFileName );
                    return ReturnCode.Error;
                }

                // All seems OK. Create the host and start execution.
                Host host = new Host( programFileName, programArguments.ToArray(), projectFileName, projectParameters );
                host.Execute();

                return ReturnCode.Success;
            }
            catch ( Exception e )
            {
                // If we have a MessageException, the message has already been written.
                if ( !( e is MessageException ) )
                {
                    Console.Error.WriteLine( e.ToString() );
                    return ReturnCode.Exception;
                }
                else
                {
                    return ReturnCode.Error;
                }
            }
            finally
            {
                if ( pause )
                {
                    Console.WriteLine( "Press any key." );
                    Console.Read();
                }
            }
        }
    }
}