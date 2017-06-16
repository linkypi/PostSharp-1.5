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

using System;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Custom attribute that, when applied on an assembly, means that a given message
    /// should be disabled during the current PostSharp session.
    /// </summary>
    /// <remarks>
    /// Errors and fatal errors cannot be disabled.
    /// </remarks>
    [MulticastAttributeUsage(MulticastTargets.Assembly, AllowMultiple = true)]
    public sealed class DisablePostSharpMessageAttribute : Attribute
    {
        private readonly string messageId;

        /// <summary>
        /// Initializes a new <see cref="DisablePostSharpMessageAttribute"/>.
        /// </summary>
        /// <param name="messageId">Identifier of the message to be disabled.</param>
        public DisablePostSharpMessageAttribute(string messageId)
        {
            this.messageId = messageId;
        }

        /// <summary>
        /// Gets the identifier of the message to be disabled.
        /// </summary>
        public string MessageId { get { return this.messageId; } }
    }
}
