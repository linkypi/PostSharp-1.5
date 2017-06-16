#region Released to Public Domain by Gael Fraiteur
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
using PostSharp.Extensibility;

namespace PostSharp.Samples.Host
{
    /// <summary>
    /// Sample local host. This class stays between the <see cref="PostSharpObject"/> and
    /// the host, but resides in the PostSharp AppDomain.
    /// </summary>
    internal class LocalHost : PostSharpLocalHost
    {
        /// <summary>
        /// Initialize the current instance. 
        /// </summary>
        public override void Initialize()
        {
            // Register to events.
            this.PostSharpObject.PhaseExecuted += new EventHandler<PhaseExecutedEventArgs>( OnPhaseExecuted );
            this.PostSharpObject.PhaseExecuting += new EventHandler<PhaseExecutedEventArgs>( OnPhaseExecuting );
        }

        private void OnPhaseExecuting( object sender, PhaseExecutedEventArgs e )
        {
            Console.WriteLine( "This is the event PhaseExecuting for phase {0}.", e.Phase );
        }

        private void OnPhaseExecuted( object sender, PhaseExecutedEventArgs e )
        {
            Console.WriteLine( "This is the event PhaseExecuted for phase {0}.", e.Phase );
        }
    }
}