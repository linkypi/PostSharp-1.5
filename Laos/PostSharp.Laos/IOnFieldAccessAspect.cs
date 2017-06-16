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
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Defines the semantics of an aspect that, when applied on a field, intercepts
    /// all <b>get</b> and <b>set</b> operations to this field.
    /// </summary>
    public interface IOnFieldAccessAspect : ILaosFieldLevelAspect
    {
        /// <summary>
        /// Method called instead of the <i>get</i> operation on the modified field.
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which field is being
        /// accessed and which is its current value.</param>
        void OnGetValue( FieldAccessEventArgs eventArgs );

        /// <summary>
        /// Method called instead of the <i>set</i> operation on the modified field.
        /// </summary>
        /// <param name="eventArgs">Event arguments specifying which field is being
        /// accessed and which is its current value, and allowing to change its value.
        /// </param>
        void OnSetValue( FieldAccessEventArgs eventArgs );
    }

    /// <summary>
    /// Configuration of the <see cref="IOnFieldAccessAspect"/> aspect.
    /// </summary>
    /// <seealso cref="OnFieldAccessAspectConfigurationAttribute"/>
    public interface IOnFieldAccessAspectConfiguration : IInstanceBoundLaosAspectConfiguration
    {
        /// <summary>
        /// Gets the weaving options.
        /// </summary>
        /// <returns>Weaving options.</returns>
        [CompileTimeSemantic]
        OnFieldAccessAspectOptions? GetOptions();
    }

    /// <summary>
    /// Weaving options for the <see cref="IOnFieldAccessAspect"/> aspect.
    /// </summary>
    [Flags]
    public enum OnFieldAccessAspectOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the field should be removed from the declaring type.
        /// </summary>
        RemoveFieldStorage = 1,


        /// <summary>
        /// Mask isolating <see cref="GeneratePropertyAuto"/>, 
        /// <see cref="GeneratePropertyAlways"/> and <see cref="GeneratePropertyNever"/>.
        /// </summary>
        GeneratePropertyMask = 6,

        /// <summary>
        /// Specifies that a property should be generated around the field
        /// if and only if the field is exposed outside the assembly.
        /// </summary>
        /// <remarks>
        /// If this property is set and a public or protected field is found,
        /// the field is made private and renamed, and a property
        /// with the original name and visibility of the field is created.
        /// </remarks>
        GeneratePropertyAuto = 0,

        /// <summary>
        /// Obsolete.
        /// </summary>
        [Obsolete( "Correct spelling is GeneratePropertyNever :-)" )] GenerarePropertyNever = GeneratePropertyNever,

        /// <summary>
        /// Specifies the field should <b>never</b> be encapsulated into
        /// a property.
        /// </summary>
        GeneratePropertyNever = 2,

        /// <summary>
        /// Specifies that a property should be generated around the field,
        /// even if the field is not visible outside its assembly.
        /// </summary>
        /// <remarks>
        /// If this property is set, the field is made private and renamed, and a property
        /// with the original name and visibility of the field is created.
        /// </remarks>
        GeneratePropertyAlways = 4
    }

    /// <summary>
    /// Custom attribute that, when applied on a class implementing <see cref="IOnFieldAccessAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public sealed class OnFieldAccessAspectConfigurationAttribute : InstanceBoundAspectConfigurationAttribute,
                                                                    IOnFieldAccessAspectConfiguration
    {
        private OnFieldAccessAspectOptions? options;

        /// <summary>
        /// Gets or sets the aspect options.
        /// </summary>
        public OnFieldAccessAspectOptions Options
        {
            get { return this.options ?? OnFieldAccessAspectOptions.None; }
            set { this.options = value; }
        }

        /// <inheritdoc />
        OnFieldAccessAspectOptions? IOnFieldAccessAspectConfiguration.GetOptions()
        {
            return this.options;
        }
    }
}