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

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using PostSharp.Extensibility;

namespace PostSharp
{
    /// <summary>
    /// Provides tracing functionality. An instance of the <see cref="Trace"/> type
    /// implements a trace sink.
    /// </summary>
    public sealed class Trace
    {
        internal static readonly Trace CodeModel;
        internal static readonly Trace AssemblyBinder;
        internal static readonly Trace ProjectLoader;
        internal static readonly Trace MulticastAttributeTask;
        internal static readonly Trace ModuleReader;
        internal static readonly Trace CodeWeaver;
        internal static readonly Trace InstructionReader;
        internal static readonly Trace InstructionEmitter;
        internal static readonly Trace Messenger;
        internal static readonly Trace Remoting;
        internal static readonly Trace ReflectionBinding;
        internal static readonly Trace ModuleWriter;
        internal static readonly Trace CustomAttributeDictionaryTask;
        internal static readonly Trace Domain;
        internal static readonly Trace Timings;
        internal static readonly Trace CompileTask;
        internal static readonly Trace AutoDetectTask;
        internal static readonly Trace PostSharpObject;
        internal static readonly Trace IndexUsagesTask;
        internal static readonly Trace AutoUpdate;
        internal static readonly Trace TaskResolving;
        internal static readonly Trace ILWriter;
        internal static readonly Trace ImageReader;
        internal static readonly Trace IndexGenericInstanceTask;
        internal static readonly Trace AssemblyRedirectionPolicies;


        private static bool globalEnabled;

        static Trace()
        {
            Extensibility.Messenger.Initialize();
            globalEnabled = ApplicationInfo.GetSettingBoolean( "Trace", false );
#if TRACE
            ApplicationInfo.SettingChanged += OnGlobalSettingChanged;
#endif
            CodeModel = new Trace( "CodeModel" );
            AssemblyBinder = new Trace( "AssemblyBinder" );
            ProjectLoader = new Trace( "ProjectLoader" );
            MulticastAttributeTask = new Trace( "MulticastAttributeTask" );
            ModuleReader = new Trace( "ModuleReader" );
            CodeWeaver = new Trace( "CodeWeaver" );
            InstructionReader = new Trace( "InstructionReader" );
            InstructionEmitter = new Trace( "InstructionEmitter" );
            Messenger = new Trace( "Messenger" );
            Remoting = new Trace( "Remoting" );
            ReflectionBinding = new Trace( "ReflectionBinding" );
            ModuleWriter = new Trace( "ModuleWriter" );
            CustomAttributeDictionaryTask = new Trace( "CustomAttributeDictionaryTask" );
            Domain = new Trace( "Domain" );
            Timings = new Trace( "Timings" );
            CompileTask = new Trace( "CompileTask" );
            AutoDetectTask = new Trace( "AutoDetectTask" );
            PostSharpObject = new Trace( "PostSharpObject" );
            IndexUsagesTask = new Trace( "IndexUsagesTask" );
            AutoUpdate = new Trace( "AutoUpdate" );
            TaskResolving = new Trace( "TaskResolving" );
            ILWriter = new Trace( "ILWriter" );
            ImageReader = new Trace( "ImageReader" );
            IndexGenericInstanceTask = new Trace( "IndexGenericInstanceTask" );
            AssemblyRedirectionPolicies = new Trace( "AssemblyRedirectionPolicies" );
        }

        private static void OnGlobalSettingChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "Trace" )
            {
                globalEnabled = ApplicationInfo.GetSettingBoolean( "Trace", false );
            }
        }

        #region Instance members

        private bool enabled;
        private readonly string category;
        private readonly StringBuilder lineBuffer = new StringBuilder();

        /// <summary>
        /// Forces initialization of the tracing facility.
        /// </summary>
        public static void Initialize()
        {
        }


        /// <summary>
        /// Initialize a new <see cref="Trace"/> sink.
        /// </summary>
        /// <param name="category">Category of messages.</param>
        [SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
        public Trace( string category )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( category, "category" );

            #endregion

            this.category = category;
#if TRACE
            this.enabled = globalEnabled && ApplicationInfo.GetSettingBoolean( "Trace." + category, false );
            ApplicationInfo.SettingChanged += OnInstanceSettingChanged;
#else
            this.enabled = false;
#endif
        }

#if TRACE
        private void OnInstanceSettingChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "Trace." + this.category || e.PropertyName == "Trace" )
            {
                this.enabled = globalEnabled && ApplicationInfo.GetSettingBoolean( "Trace." + category, false );
            }
        }
#endif

        /// <summary>
        /// Determines whether tracing is globally enabled.
        /// </summary>
        public static bool IsGloballyEnabled
        {
            get { return globalEnabled; }
        }

        private void InternalWrite( string message )
        {
            this.lineBuffer.Append( message );
        }

        private void InternalWriteLine( string message )
        {
            string completeMessage;
            if ( this.lineBuffer.Length > 0 )
            {
                this.lineBuffer.Append( message );
                completeMessage = this.lineBuffer.ToString();
                this.lineBuffer.Length = 0;
            }
            else
            {
                completeMessage = message;
            }

            Extensibility.Messenger.Current.Write(
                new Message( SeverityType.Debug, this.category, completeMessage, null, "Trace",
                             null, Message.NotAvailable, Message.NotAvailable, null ) );

            if ( Debugger.IsAttached )
            {
                Debugger.Log( 0, this.category, completeMessage + "\n" );
            }
        }

        /// <summary>
        /// Writes a message to the current sink with many formatting parameters.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="parameters">Formatting parameters.</param>
        [Conditional( "TRACE" )]
        public void Write( string format, params object[] parameters )
        {
            if ( this.enabled )
            {
                ExceptionHelper.AssertArgumentNotNull( parameters, "parameters" );
                InternalWrite(
                    string.Format( CultureInfo.InvariantCulture, format, parameters ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink with a single formatting parameter.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="arg0">The first formatting parameter.</param>
        [Conditional( "TRACE" )]
        public void Write( string format, object arg0 )
        {
            if ( this.enabled )
            {
                InternalWrite(
                    string.Format( CultureInfo.InvariantCulture, format, arg0 ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink with two formatting parameters.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="arg0">The first formatting parameter.</param>
        /// <param name="arg1">The second formatting parameter.</param>
        [Conditional( "TRACE" )]
        public void Write( string format, object arg0, object arg1 )
        {
            if ( this.enabled )
            {
                InternalWrite(
                    string.Format( CultureInfo.InvariantCulture, format, arg0, arg1 ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink with three formatting parameters.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="arg0">The first formatting parameter.</param>
        /// <param name="arg1">The second formatting parameter.</param>
        /// <param name="arg2">The thirs formatting parameter.</param>
        [Conditional( "TRACE" )]
        public void Write( string format, object arg0, object arg1, object arg2 )
        {
            if ( this.enabled )
            {
                InternalWrite(
                    string.Format( CultureInfo.InvariantCulture, format, arg0, arg1, arg2 ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink without formatting parameter.
        /// </summary>
        /// <param name="message">Message.</param>
        [Conditional( "TRACE" )]
        public void Write( string message )
        {
            if ( this.enabled )
            {
                InternalWrite( message );
            }
        }


        /// <summary>
        /// Writes a message to the current sink with many formatting parameters, and issues a line break.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="parameters">Formatting parameters.</param>
        [Conditional( "TRACE" )]
        public void WriteLine( string format, params object[] parameters )
        {
            if ( this.enabled )
            {
                ExceptionHelper.AssertArgumentNotNull( parameters, "parameters" );
                InternalWriteLine(
                    string.Format( CultureInfo.InvariantCulture, format, parameters ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink with a single formatting parameter, 
        /// and issues a line break.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="arg0">The first formatting parameter.</param>
        [Conditional( "TRACE" )]
        public void WriteLine( string format, object arg0 )
        {
            if ( this.enabled )
            {
                InternalWriteLine(
                    string.Format( CultureInfo.InvariantCulture, format, arg0 ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink with two formatting parameters, 
        /// and issues a line break.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="arg0">The first formatting parameter.</param>
        /// <param name="arg1">The second formatting parameter.</param>
        [Conditional( "TRACE" )]
        public void WriteLine( string format, object arg0, object arg1 )
        {
            if ( this.enabled )
            {
                InternalWriteLine(
                    string.Format( CultureInfo.InvariantCulture, format, arg0, arg1 ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink with three formatting parameters, 
        /// and issues a line break.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        /// <param name="arg0">The first formatting parameter.</param>
        /// <param name="arg1">The second formatting parameter.</param>
        /// <param name="arg2">The thirs formatting parameter.</param>
        [Conditional( "TRACE" )]
        public void WriteLine( string format, object arg0, object arg1, object arg2 )
        {
            if ( this.enabled )
            {
                InternalWriteLine(
                    string.Format( CultureInfo.InvariantCulture, format, arg0, arg1, arg2 ) );
            }
        }

        /// <summary>
        /// Writes a message to the current sink without parameter, 
        /// and issues a line break.
        /// </summary>
        /// <param name="format">Message formatting string.</param>
        [Conditional( "TRACE" )]
        public void WriteLine( string format )
        {
            if ( this.enabled )
            {
                InternalWriteLine( format );
            }
        }


        /// <summary>
        /// Determines whether the current trace sink is enabled.
        /// </summary>
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
        public bool Enabled
        {
            get
            {
#if TRACE
                return this.enabled;
#else
                return false;
#endif
            }
        }

        #endregion
    }
}