#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion


#if !SMALL

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Receives messages.
    /// </summary>
    /// <remarks>
    /// Use this interface instead of events for cross-domain communication.
    /// </remarks>
    public interface IMessageSink
    {
        /// <summary>
        /// Writes a message to the sink.
        /// </summary>
        /// <param name="message">A message.</param>
        void Write( Message message );
    }
}

#endif