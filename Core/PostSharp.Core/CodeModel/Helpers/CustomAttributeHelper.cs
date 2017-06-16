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
using System.Reflection;
using System.Text;

namespace PostSharp.CodeModel.Helpers
{
    /// <summary>
    /// Provides utility methods for working with custom attributes, for instance constructing
    /// a runtime object or rendering it to a string.
    /// </summary>
    public static class CustomAttributeHelper
    {
        /// <summary>
        /// Construct the instance (typically an object derived from <see cref="System.Attribute"/>)
        /// represented by the current <see cref="CustomAttributeDeclaration"/>.
        /// </summary>
        /// <returns>A new instance (typically derived from <see cref="System.Attribute"/>) constructed 
        /// on by the constructor
        /// represented by the <see cref="IAnnotationValue.Constructor"/> property and based
        /// on the values given in <see cref="IAnnotationValue.ConstructorArguments"/>
        /// and <see cref="IAnnotationValue.NamedArguments"/>.</returns>
        /// <exception cref="CustomAttributeConstructorException">The constructor or a property setter
        /// threw an exception.</exception>
        public static object ConstructRuntimeObject( IAnnotationValue attribute, ModuleDeclaration module )
        {
            ConstructorInfo constructorInfo =
                (ConstructorInfo)
                attribute.Constructor.GetSystemMethod( null, null, BindingOptions.RequireGenericInstance );
            Type instanceType = constructorInfo.DeclaringType;
            ParameterInfo[] constructorParametersInfo = constructorInfo.GetParameters();

            // Some basic validation
            ExceptionHelper.Core.AssertValidOperation(
                constructorParametersInfo.Length == attribute.ConstructorArguments.Count,
                "InvalidNumberOfConstructorArguments" );

            // Build the constructor arguments
            object[] constructorArguments = new object[constructorParametersInfo.Length];
            for ( int i = 0; i < constructorArguments.Length; i++ )
            {
                constructorArguments[i] = attribute.ConstructorArguments[i].Value.GetRuntimeValue();
            }

            // Build the instance
            object instance;
            try
            {
                instance = constructorInfo.Invoke( constructorArguments );
            }
            catch ( TargetInvocationException e )
            {
                throw new CustomAttributeConstructorException(
                    string.Format( "The custom attribute '{0}' constructor threw the exception {1}: {2}",
                                   constructorInfo.DeclaringType.FullName,
                                   e.InnerException.GetType().Name,
                                   e.InnerException.Message ), e.InnerException );
            }

            if ( instance == null )
            {
                throw ExceptionHelper.Core.CreateAssertionFailedException( "ConstructorReturnsNull", constructorInfo );
            }

            // Set the named parameters
            foreach ( MemberValuePair pair in attribute.NamedArguments )
            {
                switch ( pair.MemberKind )
                {
                    case MemberKind.Field:
                        instanceType.GetField( pair.MemberName ).SetValue( instance, pair.Value.GetRuntimeValue() );
                        break;

                    case MemberKind.Property:
                        try
                        {
                            instanceType.GetProperty( pair.MemberName ).SetValue( instance,
                                                                                  pair.Value.GetRuntimeValue(),
                                                                                  null );
                        }
                        catch ( TargetInvocationException e )
                        {
                            throw new CustomAttributeConstructorException(
                                string.Format(
                                    "The custom attribute '{0}' property setter '{1}' threw the exception {2}: {3}",
                                    constructorInfo.DeclaringType.FullName,
                                    pair.MemberName,
                                    e.InnerException.GetType().Name,
                                    e.InnerException.Message ), e.InnerException );
                        }
                        break;

                    default:
                        throw ExceptionHelper.Core.CreateAssertionFailedException( "Unexpected MemberKind." );
                }
            }

            return instance;
        }

        /// <summary>
        /// Renders a custom attribute value to an existing <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="attribute">A custom attribute.</param>
        /// <param name="target">A <see cref="StringBuilder"/> where to writer the attribute.</param>
        public static void Render( IAnnotationValue attribute, StringBuilder target )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( attribute, "attribute" );
            ExceptionHelper.AssertArgumentNotNull( target, "target" );

            #endregion

            target.Append( ( (INamedType) attribute.Constructor.DeclaringType ).Name );
            if ( attribute.ConstructorArguments.Count > 0 ||
                 attribute.NamedArguments.Count > 0 )
            {
                target.Append( "( " );

                bool first = true;
                foreach ( MemberValuePair pair in attribute.ConstructorArguments )
                {
                    if ( first )
                    {
                        first = false;
                    }
                    else
                    {
                        target.Append( ", " );
                    }

                    target.Append( pair.Value.ToString() );
                }

                foreach ( MemberValuePair pair in attribute.NamedArguments )
                {
                    if ( first )
                    {
                        first = false;
                    }
                    else
                    {
                        target.Append( ", " );
                    }

                    target.Append( pair.MemberName );
                    target.Append( " = " );
                    target.Append( pair.Value.ToString() );
                }

                target.Append( " )" );
            }
        }

        /// <summary>
        /// Renders a custom attribute value to a string.
        /// </summary>
        /// <param name="attribute">A custom attribute value.</param>
        /// <returns>A string representing <paramref name="attribute"/>.</returns>
        public static string Render( IAnnotationValue attribute )
        {
            StringBuilder stringBuilder = new StringBuilder( 500 );
            Render( attribute, stringBuilder );
            return stringBuilder.ToString();
        }
    }
}