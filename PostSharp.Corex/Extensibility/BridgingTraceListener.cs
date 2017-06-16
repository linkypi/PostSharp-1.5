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

using System.Diagnostics;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Allows to propagate <see cref="System.Diagnostics.Trace"/> messages
    /// from one application domain to another.
    /// </summary>
    /// <remarks>
    /// The current class should be instantiated in the target application
    /// domain (where messages should be received) and added as a listener
    /// in the source application (the one where messages are emitted).
    /// </remarks>
    internal sealed class BridgingTraceListener : TraceListener
    {
        /// <inheritdoc />
        public override void Write( string message )
        {
            System.Diagnostics.Trace.Write( message );
        }

        /// <inheritdoc />
        public override void WriteLine( string message )
        {
            System.Diagnostics.Trace.WriteLine( message );
        }
    }
}