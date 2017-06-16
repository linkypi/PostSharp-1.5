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
using System;
using PostSharp.Reflection;

namespace PostSharp.Laos
{
    /// <summary>
    /// Defines the semantics of an aspect that, when applied to a target,
    /// adds a custom attribute to this target.
    /// </summary>
    public interface ICustomAttributeInjectorAspect : ILaosAspect
    {
        /// <summary>
        /// Gets the custom attribute to be applied to the
        /// target of this aspect.
        /// </summary>
        ObjectConstruction CustomAttribute { get; }
    }


#pragma warning disable 3015
    /// <summary>
    /// Aspect that, when applied to a target, adds a custom attribute to this target.
    /// </summary>
    public sealed class CustomAttributeInjectorAspect : LaosAspect, ICustomAttributeInjectorAspect
    {
        private readonly ObjectConstruction customAttribute;

        /// <summary>
        /// Initializes a new <see cref="CustomAttributeInjectorAspect"/>.
        /// </summary>
        /// <param name="attribute">Custom attribute to be added to the target.</param>
        public CustomAttributeInjectorAspect( ObjectConstruction attribute )
        {
            #region Preconditions

            if ( attribute == null ) throw new ArgumentNullException( "attribute" );

            #endregion

            this.customAttribute = attribute;
        }

        /// <inheritdoc />
        public ObjectConstruction CustomAttribute
        {
            get { return this.customAttribute; }
        }
    }

#pragma warning restore 3015

    /// <summary>
    /// Custom attribute configuring the aspect <see cref="ICustomAttributeInjectorAspect"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class CustomAttributeInjectorAspectConfigurationAttribute : LaosAspectConfigurationAttribute
    {
    }
}

#endif