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

#region Using directives

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using PostSharp.Collections;

#endregion

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Return codes of <see cref="CommandLineProgram"/>.
    /// </summary>
    public enum CommandLineReturnCode
    {
        /// <summary>
        /// Success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Invalid command line.
        /// </summary>
        InvalidCommandLine = 10,

        /// <summary>
        /// Error (emitted as a <see cref="Message"/>).
        /// </summary>
        Error = 11,

        /// <summary>
        /// Unhandled exception.
        /// </summary>
        Exception = 12,

        /// <summary>
        /// An expected message was not emitted.
        /// </summary>
        DisappointingMessages = 13
    }

    /// <summary>
    /// Implementation of the command-line utility.
    /// </summary>
    public static class CommandLineProgram
    {
        private static readonly Set<string> expectedMessages =
            new Set<string>(4, StringComparer.InvariantCultureIgnoreCase);

        private static readonly Set<string> receivedMessages =
            new Set<string>(4, StringComparer.InvariantCultureIgnoreCase);

        private static bool failOnUnexpectedMessage;
        private static bool hasFailedBecauseOfUnexpectedMessage;
        private static bool verbose = true;

        private static void Usage(string[] args)
        {
            Console.WriteLine(
                @"
Usage: postsharp [<options>] <project> <input> [<options>]

Options:
/?              Display the current help. 
/P:name=value   Sets a property.
/E:messageId    Expects a message to be emitted (the program will fail if not emitted).
/D:messageId    Disables a message.
/U              Fails on unexpected messages, even on information or warning messages.
/Pause          Wait for user input after completion.
/Attach         Gives the opportunity to attach a debugger to the process.
/SkipAutoUpdate Does not display a warning when a new version is available.
/V              Verbose (enable tracing - you should enable tracing in PostSharp-Library.config).

");

            if (args != null)
            {
                Console.WriteLine("PostSharp.exe received the following arguments:");
                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine("[{0}] = {1}", i, args[i]);
                }
                Console.WriteLine("Total: {0} argument(s)", args.Length);
            }
        }

        /// <summary>
        /// Invokes the command-line utility.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Return code.</returns>
#pragma warning disable 28
        public static CommandLineReturnCode Main(string[] args)
#pragma warning restore 28
        {
            CommandLineReturnCode returnCode;
            try
            {
                returnCode = InternalMain(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return CommandLineReturnCode.Exception;
            }

            if (returnCode == CommandLineReturnCode.Success)
            {
                foreach (string messageId in expectedMessages)
                {
                    if (!receivedMessages.Contains(messageId))
                    {
                        WriteColoredLine(ConsoleColor.Red, Console.Error,
                                         "Message {0} was expected but not received.", messageId);
                        returnCode = CommandLineReturnCode.DisappointingMessages;
                    }
                }

                if (hasFailedBecauseOfUnexpectedMessage)
                {
                    WriteColoredLine(ConsoleColor.Red, Console.Error,
                                     "Unexpected messages (informations, warnings or errors) were received.");
                    returnCode = CommandLineReturnCode.DisappointingMessages;
                }
            }
            

            Environment.ExitCode = (int) returnCode;
            return returnCode;
        }


        private static void WriteColoredLine(ConsoleColor color, TextWriter output, string text, params object[] args)
        {
            System.Diagnostics.Trace.Flush();
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (args == null || args.Length == 0)
                output.WriteLine(text);
            else
                output.WriteLine(text, args);
            Console.ForegroundColor = oldColor;
        }

        private static CommandLineReturnCode InternalMain(string[] args)
        {
            bool attachDebugger = false;
            bool pause = false;

            if (args.Length == 1 && args[0].ToLowerInvariant() == "autoupdate")
            {
                AutoUpdate.GetAutoUpdateMessages();
                return 0;
            }

            try
            {
                PostSharpObjectSettings settings = new PostSharpObjectSettings {CreatePrivateAppDomain = false};

                // Disable tracing in both application domains by default.
                settings.Settings["Trace"] = "false";
                ApplicationInfo.SetSetting("Trace", "false");

                PropertyCollection parameters = new PropertyCollection();

                Console.WriteLine("");
                Console.WriteLine("PostSharp 1.5 [{0}] - Copyright (c) 2004-2010 by SharpCrafters s.r.o..",
                                  ApplicationInfo.Version);
                Console.WriteLine("");

                // Reading the command line
                if (args.Length < 2)
                {
                    WriteColoredLine(ConsoleColor.Red, Console.Error, "Error: not enough arguments.");
                    Usage(args);
                    return CommandLineReturnCode.InvalidCommandLine;
                }


                string projectFileName = null;
                string input = null;
                bool skipAutoUpdate = false;

                int argIterator = 0;

                while (argIterator < args.Length)
                {
                    string currentArg = args[argIterator].Trim();

                    switch (currentArg.ToLowerInvariant())
                    {
                        case "/?":
                            Usage(null);
                            return CommandLineReturnCode.Success;


                        case "/pause":
                            pause = true;
                            argIterator++;
                            break;

                        case "/skipautoupdate":
                            skipAutoUpdate = true;
                            argIterator++;
                            break;

                        case "/u":
                            failOnUnexpectedMessage = true;
                            argIterator++;
                            break;

                        case "/attach":
                            attachDebugger = true;
                            argIterator++;
                            break;

                        case "/v":
                            verbose = true;
                            settings.Settings["Trace"] = "true";
                            ApplicationInfo.SetSetting("Trace", "true");
                            argIterator++;
                            break;

                        case "/noreflection":
                            settings.DisableReflection = true;
                            argIterator++;
                            break;

                        default:
                            if (currentArg.StartsWith("/P:") || currentArg.StartsWith("/p:"))
                            {
                                // This is a constant definition.
                                Match match = Regex.Match(currentArg, @"/[P|p]\:(?<name>[^=]+)=(?<value>.*)$");
                                if (!match.Success)
                                {
                                    WriteColoredLine(ConsoleColor.Red, Console.Error,
                                                     "Cannot parse the argument {{{0}}}.", currentArg);
                                    Usage(args);
                                    return CommandLineReturnCode.InvalidCommandLine;
                                }

                                parameters[match.Groups["name"].Value] = match.Groups["value"].Value;
                                argIterator++;
                            }
                            else if (currentArg.StartsWith("/e:") || currentArg.StartsWith("/E:"))
                            {
                                Match match = Regex.Match(currentArg, @"/[E|e]\:(?<messageId>\w+)$");
                                if (!match.Success)
                                {
                                    WriteColoredLine(ConsoleColor.Red, Console.Error,
                                                     "Cannot parse the argument {{{0}}}", currentArg);
                                    Usage(args);
                                    return CommandLineReturnCode.InvalidCommandLine;
                                }

                                expectedMessages.AddIfAbsent(match.Groups["messageId"].Value);
                                argIterator++;
                            }
                            else if (currentArg.StartsWith("/d:") || currentArg.StartsWith("/D:"))
                            {
                                Match match = Regex.Match(currentArg, @"/[D|d]\:(?<messageId>\w+)$");
                                if (!match.Success)
                                {
                                    WriteColoredLine(ConsoleColor.Red, Console.Error,
                                                     "Cannot parse the argument {{{0}}}", currentArg);
                                    Usage(args);
                                    return CommandLineReturnCode.InvalidCommandLine;
                                }

                                settings.DisabledMessages.AddIfAbsent(match.Groups["messageId"].Value);
                                argIterator++;
                            }
                            else if (!currentArg.StartsWith("/"))
                            {
                                argIterator++;

                                if (projectFileName == null)
                                {
                                    projectFileName = Path.GetFullPath(currentArg);
                                }
                                else if (input == null)
                                {
                                    input = Path.GetFullPath(currentArg);
                                }
                                else
                                {
                                    WriteColoredLine(ConsoleColor.Red, Console.Error,
                                                     "Invalid argument: {{{0}}}. A project and an input assembly have already been specified.",
                                                     currentArg);
                                    Usage(args);
                                    return CommandLineReturnCode.InvalidCommandLine;
                                }
                            }
                            else
                            {
                                WriteColoredLine(ConsoleColor.Red, Console.Error,
                                                 "Invalid argument: {{{0}}}. Unrecognized switch.",
                                                 currentArg);
                                Usage(args);
                                return CommandLineReturnCode.InvalidCommandLine;
                            }
                            break;
                    }
                }

                if (projectFileName == null)
                {
                    Console.Error.WriteLine("No project specified.");
                    Usage(args);
                    return CommandLineReturnCode.InvalidCommandLine;
                }

                if (input == null)
                {
                    Console.Error.WriteLine("No input specified.");
                    Usage(args);
                    return CommandLineReturnCode.InvalidCommandLine;
                }

                // Initialize the messenger.
                Messenger.Current.Message += OnMessage;

                if (attachDebugger)
                    Debugger.Launch();

                // Start automatic updates in a second thread.

                if (!skipAutoUpdate)
                {
                    AutoUpdate.BeginRetrieveMessages();
                }


                ProjectInvocationParameters invocationParameters = new ProjectInvocationParameters(projectFileName);
                invocationParameters.Properties.Merge(parameters);
                ModuleLoadStrategy moduleLoadStrategy = settings.DisableReflection ? (ModuleLoadStrategy)new ModuleLoadDirectFromFileStrategy(input) : new ModuleLoadReflectionFromFileStrategy(input);

                using (IPostSharpObject postSharpObject = PostSharpObject.CreateInstance(settings, null))
                {
                    postSharpObject.InvokeProject(new ProjectInvocation( invocationParameters, moduleLoadStrategy) );

                    // We pause before disposing everything. It helps memory profiling.
                    if (pause)
                    {
                        pause = false;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Console.WriteLine("Press any key.");
                        Console.Read();
                    }
                }


                return CommandLineReturnCode.Success;
            }
            catch (Exception e)
            {
                if (!(e is MessageException))
                {
                    WriteColoredLine(ConsoleColor.Red, Console.Error,
                                     string.Format("error: Unhandled exception: {0} See below for details.", e.Message));
                    WriteColoredLine(ConsoleColor.Red, Console.Error, e.ToString());
                    return CommandLineReturnCode.Exception;
                }
                else
                {
                    return CommandLineReturnCode.Error;
                }
            }
            finally
            {
                AutoUpdate.StopRetrieveMessages();

                if (pause)
                {
                    Console.WriteLine("Press any key.");
                    Console.Read();
                }
            }
        }

        private static void OnMessage(object sender, MessageEventArgs e)
        {
            string format;

            SeverityType severity = e.Message.Severity;
            string messageText = e.Message.MessageText;

            if (e.Message.Severity != SeverityType.Debug)
                receivedMessages.AddIfAbsent(e.Message.MessageId);

            if ((e.Message.Severity == SeverityType.Debug ||
                 e.Message.Severity == SeverityType.Verbose) &&
                !verbose)
                return;

            if (expectedMessages.Contains(e.Message.MessageId))
            {
                severity = SeverityType.ImportantInfo;
                messageText = string.Format("[Expected {0}] {1}", e.Message.Severity, messageText);
            }
            else if (failOnUnexpectedMessage && e.Message.Severity != SeverityType.Debug)
            {
                hasFailedBecauseOfUnexpectedMessage = true;
            }

            if ((failOnUnexpectedMessage && severity == SeverityType.Warning) ||
                severity == SeverityType.Fatal)
            {
                severity = SeverityType.Error;
            }

            if (e.Message.LocationColumn != Message.NotAvailable)
            {
                format = "{0}({1},{2}): {3} {4}: {5}";
            }
            else if (e.Message.LocationLine != Message.NotAvailable)
            {
                format = "{0}({1}): {3} {4}: {5}";
            }
            else if (e.Message.LocationFile != null)
            {
                format = "{0}: {3} {4}: {5}";
            }
            else
            {
                format = "{3} {4}: {5}";
            }

            string message =
                string.Format(format, e.Message.LocationFile, e.Message.LocationLine, e.Message.LocationColumn,
                              severity.ToString().ToLower(), e.Message.MessageId, messageText);

            ConsoleColor color;
            TextWriter output;

            switch (e.Message.Severity)
            {
                case SeverityType.Error:
                case SeverityType.Fatal:
                    color = ConsoleColor.Red;
                    output = Console.Error;
                    break;

                case SeverityType.Warning:
                    color = ConsoleColor.DarkYellow;
                    output = Console.Out;
                    break;

                case SeverityType.Debug:
                case SeverityType.Verbose:
                    color = ConsoleColor.DarkGray;
                    output = Console.Out;
                    break;

                default:
                    color = Console.ForegroundColor;
                    output = Console.Out;
                    break;
            }

            WriteColoredLine(color, output, message);
        }
    }
}