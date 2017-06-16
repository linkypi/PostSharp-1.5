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
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Base interface for all aspects that can be woven, that is, that
    /// really change the code.
    /// </summary>
    public interface ILaosWeavableAspect : ILaosAspect
    {
    }

    /// <summary>
    /// Configuration of <see cref="ILaosWeavableAspect"/> aspects.
    /// </summary>
    /// <seealso cref="LaosWeavableAspectConfigurationAttribute"/>
    public interface ILaosWeavableAspectConfiguration : ILaosAspectConfiguration
    {
        /// <summary>
        /// Gets the weaving priority of the aspect.
        /// </summary>
        /// <value>The aspect priority, or <b>null</b> if the aspect priority is not specified.</value>
        /// <remarks>
        /// Handlers with lower priority are executed before
        /// in case of 'entry' semantics (entering or invoking a method, setting a field value),
        /// but this order is inverted for handlers of 'exit' semantics (leaving a method, getting a field value).
        /// </remarks>
        [CompileTimeSemantic]
        int? AspectPriority { get; }

        /// <summary>
        /// Gets the type of the serializer that will be used
        /// to configure the current aspect. 
        /// </summary>
        /// <remarks>
        /// This type should have derive from <see cref="LaosSerializer"/>
        /// and have a default constructor. Use <b>null</b> to specify that the aspect will not
        /// be serialized, but will be constructed using MSIL instructions.
        /// </remarks>
        Type SerializerType { get; }
    }


    /// <summary>
    /// Base class of a custom attribute that, 
    /// when applied on a class implementing <see cref="ILaosWeavableAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This custom attribute is typically used with the Compact Framework or Silverlight.
    /// </para>
    /// <para>
    /// If the class to which this custom attribute is applied already implements 
    /// <see cref="ILaosWeavableAspectConfiguration"/> (which is the case with the full .NET Framework), 
    /// the <see cref="LaosWeavableAspectConfigurationAttribute"/> is ignored.
    /// </para>
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class )]
    public abstract class LaosWeavableAspectConfigurationAttribute : LaosAspectConfigurationAttribute,
                                                                     ILaosWeavableAspectConfiguration
    {
        private bool aspectProritySpecified;
        private int aspectPriority;

        /// <summary>
        /// Gets or sets the weaving priority of the aspect.
        /// </summary>
        /// <value>The aspect priority, or <b>null</b> if the aspect priority is not specified.</value>
        /// <remarks>
        /// <para>Handlers with lower priority are executed before
        /// in case of 'entry' semantics (entering or invoking a method, setting a field value),
        /// but this order is inverted for handlers of 'exit' semantics (leaving a method, getting a field value).
        /// </para>
        /// <para>This property must not be confused with <see cref="MulticastAttribute.AttributePriority"/>,
        /// which solely influences the multicasting process.</para>
        /// </remarks>
        public int AspectPriority
        {
            get { return this.aspectPriority; }
            set
            {
                this.aspectPriority = value;
                this.aspectProritySpecified = true;
            }
        }

        /// <summary>
        /// Gets or sets the type of the serializer that will be used
        /// to configure the current aspect. 
        /// </summary>
        /// <remarks>
        /// This type should have derive from <see cref="LaosSerializer"/>
        /// and have a default constructor. Use <b>null</b> to specify that the aspect will not
        /// be serialized, but will be constructed using MSIL instructions.
        /// </remarks>
        public Type SerializerType { get; set; }

        /// <inheritdoc />
        int? ILaosWeavableAspectConfiguration.AspectPriority
        {
            get { return this.aspectProritySpecified ? this.aspectPriority : (int?) null; }
        }
    }
}