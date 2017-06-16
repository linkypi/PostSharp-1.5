using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PostSharp.Extensibility;

namespace PostSharp.MSBuild
{
    internal class PostSharpRemoteTask : MarshalByRefObject
    {
        private static readonly char[] trimmableChars = new[] {' ', '\t', '\f', '\n', '\r'};
        private PostSharpTaskParameters parameters;
        private RemotingAccessor<TaskLoggingHelper> log;

        public bool Execute( PostSharpTaskParameters parameters, TaskLoggingHelper log )
        {
            this.parameters = parameters;
            this.log = new RemotingAccessor<TaskLoggingHelper>( log, false );

            string inputFullPath = !string.IsNullOrEmpty( this.parameters.InputReferenceDirectory )
                                       ?
                                           Path.Combine( this.parameters.InputReferenceDirectory, this.parameters.Input )
                                       :
                                           this.parameters.Input;

            if ( !this.parameters.NoLogo )
            {
                this.log.Value.LogMessage( MessageImportance.High,
                                     "PostSharp 1.5 [{0}] - Copyright (c) 2004-2010 by SharpCrafters s.r.o..",
                                     ApplicationInfo.Version );
            }


            this.log.Value.LogMessage(MessageImportance.Low,
                                 "Executing project {{{0}}} with input assembly {{{1}}}.",
                                 this.parameters.Project,
                                 inputFullPath );

            this.log.Value.LogMessage(MessageImportance.Low,
                                 "PostSharp implementation is in {{{0}}}.",
                                 typeof(IPostSharpObject).Assembly.Location );

            if ( this.parameters.AttachDebugger )
                Debugger.Launch();

            string verbose = parameters.Verbose.ToString().ToLowerInvariant();
            ApplicationInfo.SetSetting( "Trace", verbose );

            // Perform static configuration.
            Messenger.Current.Message += OnMessage;


            ProjectInvocationParameters invocationParameters = new ProjectInvocationParameters(
                this.parameters.Project );

            try
            {
                // Parse properties
                if ( !string.IsNullOrEmpty( this.parameters.Parameters  ) )
                {
                    foreach ( string pair in this.parameters.Parameters.Split( ';' ) )
                    {
                        string trimmedPair = pair.Trim( trimmableChars );

                        if ( trimmedPair.Length != 0 )
                        {
                            this.log.Value.LogMessage(MessageImportance.Low,
                                                 "Parsing parameter: {{{0}}}.", trimmedPair );

                            string[] thisPair = trimmedPair.Split( '=' );
                            if ( thisPair.Length != 2 )
                            {
                                this.log.Value.LogWarning(
                                    "PostSharp: ignoring this property because it is malformed: \"{0}\".",
                                    pair );
                                continue;
                            }
                            invocationParameters.Properties[thisPair[0].Trim( trimmableChars )] =
                                thisPair[1].Trim( trimmableChars );
                        }
                    }
                }

                // Start automatic updates in a second thread.
                if ( !this.parameters.AutoUpdateDisabled )
                {
                    AutoUpdate.BeginRetrieveMessages();
                }


                // Execute the project.
                try
                {
                    PostSharpObjectSettings postSharpObjectSettings = new PostSharpObjectSettings
                                                                          {CreatePrivateAppDomain = false};
                    postSharpObjectSettings.Settings["Trace"] = verbose;
                    postSharpObjectSettings.DisableReflection = this.parameters.DisableReflection;

                    ModuleLoadStrategy moduleLoadStrategy = postSharpObjectSettings.DisableReflection
                                                                ? new ModuleLoadDirectFromFileStrategy( inputFullPath )
                                                                : (ModuleLoadStrategy)
                                                                  new ModuleLoadReflectionFromFileStrategy(
                                                                      inputFullPath );


                    using (
                        IPostSharpObject postSharpObject =
                            PostSharpObject.CreateInstance( postSharpObjectSettings, null ) )
                    {
                        ProjectInvocation projectInvocation = new ProjectInvocation(
                            invocationParameters, moduleLoadStrategy );
                        postSharpObject.InvokeProject( projectInvocation );
                    }
                }
                catch ( MessageException e )
                {
                    this.log.Value.LogMessage(MessageImportance.Low, "The task failed because of a MessageException: " +
                                                                e.ToString() );
                    return false;
                }
                catch ( Exception e )
                {
                    // Do not pass the exception to the MSBuild domain. It may not have the right assembly.
                    this.log.Value.LogError("Unhandled exception: " + e.ToString());
                    return false;
                }
            }
            finally
            {
                if ( !this.parameters.AutoUpdateDisabled )
                {
                    AutoUpdate.StopRetrieveMessages();
                }

                // Remove static configuration.
                Messenger.Current.Message -= OnMessage;
            }

            this.log.Value.LogMessage( MessageImportance.Low, "The PostSharp task terminated normally." );
            return true;
        }

        private void OnMessage( object sender, MessageEventArgs e )
        {
            const string prefix = "PostSharp: ";
            string fileName = e.Message.LocationFile ?? "unknown_location";

            switch ( e.Message.Severity )
            {
                case SeverityType.Error:
                case SeverityType.Fatal:
                    this.log.Value.LogError("postsharp", e.Message.MessageId,
                                       e.Message.HelpLink, fileName, e.Message.LocationLine,
                                       e.Message.LocationColumn, 0, 0, prefix + e.Message.MessageText );
                    break;

                case SeverityType.Warning:
                    this.log.Value.LogWarning("postsharp", e.Message.MessageId,
                                         e.Message.HelpLink, fileName, e.Message.LocationLine,
                                         e.Message.LocationColumn, 0, 0, prefix + e.Message.MessageText );
                    break;


                default:
                    {
                        string format;

                        if ( e.Message.LocationColumn != Message.NotAvailable )
                        {
                            format = "{0}({1},{2}): {3} {4}: {5}";
                        }
                        else if ( e.Message.LocationLine != Message.NotAvailable )
                        {
                            format = "{0}({1}): {3} {4}: {5}";
                        }
                        else if ( e.Message.LocationFile != null )
                        {
                            format = "{0}: {3} {4}: {5}";
                        }
                        else
                        {
                            format = "{3} {4}: {5}";
                        }

                        string message =
                            string.Format( format, fileName, e.Message.LocationLine, e.Message.LocationColumn,
                                           e.Message.Severity.ToString().ToLower(), e.Message.MessageId,
                                           prefix + e.Message.MessageText );

                        switch ( e.Message.Severity )
                        {
                            case SeverityType.ImportantInfo:
                                this.log.Value.LogMessage(MessageImportance.High, message);
                                break;

                            case SeverityType.Info:
                                this.log.Value.LogMessage(MessageImportance.Normal, message);
                                break;

                            case SeverityType.Verbose:
                                this.log.Value.LogMessage(MessageImportance.Low, message);
                                break;

                            case SeverityType.Debug:
                                this.log.Value.LogMessage(MessageImportance.Low, message);
                                break;

                            case SeverityType.CommandLine:
                                this.log.Value.LogCommandLine(MessageImportance.High, message);
                                break;
                        }
                    }
                    break;
            }
        }
    }
}