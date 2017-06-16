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
using System.Globalization;
using System.IO;
using System.Resources;
using PostSharp.CodeModel;

namespace PostSharp
{
    /// <summary>
    /// Provides methods that retrieve exception messages from an embedded 
    /// resource and throws an exception.
    /// </summary>
    [DebuggerNonUserCode]
    public class ExceptionHelper
    {
        /// <summary>
        /// Manages the access to exception messages.
        /// </summary>
        private readonly ResourceManager resourceManager;

        /// <summary>
        /// Initializes a new <see cref="ExceptionHelper"/>.
        /// </summary>
        /// <param name="resourceManager">Resources containing message definitions.</param>
        protected ExceptionHelper( ResourceManager resourceManager )
        {
            #region Preconditions

            AssertArgumentNotNull( resourceManager, "resourceManager" );

            #endregion

            this.resourceManager = resourceManager;
        }

        internal static readonly ExceptionHelper Core = new ExceptionHelper(
            new ResourceManager( "PostSharp.Resources.Exceptions",
                                 typeof(ExceptionHelper).Assembly ) );

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if a reference is null.
        /// </summary>
        /// <param name="value">An object reference.</param>
        /// <param name="parameterName">Parameter name.</param>
        [Conditional( "ASSERT" )]
        public static void AssertArgumentNotNull( object value, string parameterName )
        {
            if ( value == null )
            {
                throw new ArgumentNullException( parameterName );
            }
        }

        /// <summary>
        /// Tells the static analyser that the given reference is trusted to be non-null.
        /// </summary>
        /// <param name="value">An object reference.</param>
        [Conditional( "ASSERT" )]
        public static void AssumeNotNull( object value )
        {
        }


        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if a pointer is null.
        /// </summary>
        /// <param name="value">A pointer.</param>
        /// <param name="parameterName">Parameter name.</param>
        [Conditional( "ASSERT" )]
        public static unsafe void AssertArgumentNotNull( void* value, string parameterName )
        {
            if ( value == null )
            {
                throw new ArgumentNullException( parameterName );
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if a string reference is null
        /// or if the referenced string is emptu.
        /// </summary>
        /// <param name="value">A string reference.</param>
        /// <param name="parameterName">Parameter name.</param>
        [Conditional( "ASSERT" )]
        public static void AssertArgumentNotEmptyOrNull( string value, string parameterName )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                throw new ArgumentNullException( parameterName );
            }
        }

        /// <summary>
        /// Conditionally throws an <see cref="InvalidOperationException"/> (message
        /// withparameters).
        /// </summary>
        /// <param name="condition"><b>false</b> to throw the exception, otherwise <b>true</b>.</param>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        [Conditional( "ASSERT" )]
        public void AssertValidOperation( bool condition, string messageKey, params object[] arguments )
        {
            if ( !condition )
            {
                throw this.CreateInvalidOperationException( messageKey, arguments );
            }
        }

        /// <summary>
        /// Creates an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        public Exception CreateInvalidOperationException( string messageKey, params object[] arguments )
        {
            return new InvalidOperationException( GetMessage( messageKey, arguments ) );
        }

        /// <summary>
        /// Creates an <see cref="FileNotFoundException"/>.
        /// </summary>
        /// <param name="fileName">Path of the file that was not found.</param>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        public Exception CreateFileNotFoundException( string fileName, string messageKey, params object[] arguments )
        {
            return new FileNotFoundException( GetMessage( messageKey, arguments ), fileName );
        }

        /// <summary>
        /// Creates an <see cref="BindingException"/>.
        /// </summary>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        public Exception CreateBindingException( string messageKey, params object[] arguments )
        {
            return new BindingException( GetMessage( messageKey, arguments ) );
        }

        /// <summary>
        /// Creates an <see cref="BindingException"/> and includes an inner exception.
        /// </summary>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="innerException">The exception that is wrapped by the new exception.</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        public Exception CreateBindingException( string messageKey, Exception innerException, params object[] arguments )
        {
            return new BindingException( GetMessage( messageKey, arguments ), innerException );
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="parameterName">Name of the incorrect parameter.</param>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        public Exception CreateArgumentException( string parameterName, string messageKey, params object[] arguments )
        {
            return new ArgumentException(
                GetMessage( messageKey, arguments ),
                parameterName );
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if a condition is not fulfilled.
        /// </summary>
        /// <param name="condition">Condition. Should be true, otherwise an exception is thrown.</param>
        /// <param name="parameterName">Name of the tested parameter.</param>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        [Conditional( "ASSERT" )]
        public void AssertValidArgument( bool condition, string parameterName, string messageKey,
                                         params object[] arguments )
        {
            if ( !condition )
            {
                throw CreateArgumentException( parameterName, messageKey, arguments );
            }
        }


        /// <summary>
        /// Gets an exception message.
        /// </summary>
        /// <param name="messageKey">Key of the resource string containing the message.</param>
        /// <returns>The message corresponding to <paramref name="messageKey"/>. The text
        /// may be a formatting string containing parameters like {0}, {1}, ...</returns>
        protected string GetMessage( string messageKey )
        {
            string message = resourceManager.GetString( messageKey );
            if ( message == null )
            {
                throw new ArgumentException( "Invalid messageKey: " + messageKey + ".", "messageKey" );
            }

            return message;
        }

        /// <summary>
        /// Gets an exception message from resources and format it using arguments.
        /// </summary>
        /// <param name="messageKey">Key of the resource string containing the message.</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        /// <returns>The message corresponding to <paramref name="messageKey"/> formatted using <paramref name="arguments"/>.</returns>
        protected string GetMessage( string messageKey, params object[] arguments )
        {
            string messageFormat = GetMessage( messageKey );
            if ( arguments != null )
            {
                return string.Format( CultureInfo.InvariantCulture, messageFormat,
                                      arguments );
            }
            else
            {
                return messageFormat;
            }
        }

        /// <summary>
        /// Creates an <see cref="AssertionFailedException"/> with a message telling
        /// that an enumeration contained an unexpected value.
        /// </summary>
        /// <param name="value">Enumeration value.</param>
        /// <param name="location">Name of the variable or field containing the enumeration value.</param>
        /// <returns>An <see cref="AssertionFailedException"/>.</returns>
        public static Exception CreateInvalidEnumerationValueException( object value, string location )
        {
            AssertArgumentNotNull( value, "value" );

            return Core.CreateAssertionFailedException( "InvalidEnumerationValue",
                                                        value, value.GetType().Name, location );
        }

        /// <summary>
        /// Creates an <see cref="AssertionFailedException"/> and reads the message from
        /// the resource manager.
        /// </summary>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        /// <returns>An <see cref="AssertionFailedException"/>.</returns>
        public Exception CreateAssertionFailedException( string messageKey, params object[] arguments )
        {
            return new AssertionFailedException(
                GetMessage( messageKey, arguments ) );
        }

        /// <summary>
        /// Throws an <see cref="AssertionFailedException"/> if a condition is not fulfilled.
        /// </summary>
        /// <param name="condition">Condition. Should be true, otherwise an exception is thrown.</param>
        /// <param name="messageKey">Key of the resource string containing the message.
        /// The message may contain arguments like {0}, {1}, ...</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        [Conditional( "ASSERT" )]
        public void Assert( bool condition, string messageKey, params object[] arguments )
        {
            if ( !condition )
            {
                throw CreateAssertionFailedException( messageKey, arguments );
            }
        }
    }
}