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
    /// Sample task that does nothing but demonstrates the 'Callback' mechanism through
    /// project-level tags.
    /// </summary>
    public sealed class SampleTask : Task
    {
        /// <summary>
        /// GUID of the project-level tag.
        /// </summary>
        internal static readonly Guid ProjectTag = new Guid( "{C85EC853-EED7-4d7e-8D57-2D1B30219DE2}" );

        /// <inheritdoc />
        public override bool Execute()
        {
            Host host = (Host) this.Project.GetTag( ProjectTag );
            host.SampleCallback();

            return true;
        }
    }
}