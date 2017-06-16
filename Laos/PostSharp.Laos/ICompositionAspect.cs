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

namespace PostSharp.Laos
{
    /// <summary>
    /// Defines the semantics of an aspect that, when applied on a type, adds
    /// an interface to this type and delegates the implementation of this interface
    /// to a third object.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="Post"/>.<see cref="Post.Cast{SourceType,TargetType}"/>
    /// method to cast the enhanced type to the newly implemented interface. This
    /// cast is verified during post-compilation.
    /// </remarks>
    public interface ICompositionAspect : ILaosTypeLevelAspect
    {
        /// <summary>
        /// Method called at runtime, in the constructor of the container object,
        /// to create the composed object.
        /// </summary>
        /// <returns>The composed object. This interface should implement the interface returned
        /// by the <see cref="ICompositionAspectConfiguration.GetPublicInterface"/> method.</returns>
        object CreateImplementationObject( InstanceBoundLaosEventArgs eventArgs );
    }

    /// <summary>
    /// Configuration of the <see cref="ICompositionAspect"/>.
    /// </summary>
    /// <seealso cref="CompositionAspectConfigurationAttribute"/>
    public interface ICompositionAspectConfiguration : IInstanceBoundLaosAspectConfiguration
    {
        /// <summary>
        /// Gets the type name of the public interface. Called at compile time.
        /// </summary>
        /// <param name="containerType">Type of the container object.</param>
        /// <returns>The name of the interface that should be exposed on the container object and that
        /// will be implemented by the implementation object (<see cref="ICompositionAspect.CreateImplementationObject"/>).</returns>
        TypeIdentity GetPublicInterface( Type containerType );

        /// <summary>
        /// Gets the name of all interfaces exposed with 'protected' visibility. Called at compile time.
        /// </summary>
        /// <param name="containerType">Type of the container object.</param>
        /// <returns>The interface that should be exposed on the container object and that
        /// will be implemented by the implementation object (<see cref="ICompositionAspect.CreateImplementationObject"/>).</returns>
        /// <seealso cref="InstanceCredentials"/>
        TypeIdentity[] GetProtectedInterfaces( Type containerType );

        /// <summary>
        /// Gets the weaving options.
        /// </summary>
        /// <returns>Weaving options, or <b>null</b> if no weaving option is specified.</returns>
        CompositionAspectOptions? GetOptions();
    }

    /// <summary>
    /// Custom attribute that, when applied on a class implementing <see cref="ICompositionAspect"/>,
    /// defines the configuration of that aspect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Important.</b>
    /// See the comment on the <see cref="LaosWeavableAspectConfigurationAttribute"/> class.
    /// </para>
    /// </remarks>
    public sealed class CompositionAspectConfigurationAttribute : InstanceBoundAspectConfigurationAttribute,
                                                                  ICompositionAspectConfiguration
    {
        private string publicInterface;
        private string[] protectedInterfaces;
        private CompositionAspectOptions? options;

        /// <summary>
        /// Gets or sets the type of the public interface to be injected in the target type.
        /// </summary>
        public string PublicInterface
        {
            get { return this.publicInterface; }
            set { this.publicInterface = value; }
        }

        /// <summary>
        /// Gets or sets the set of 'protected' interfaces to be injected to the target type.
        /// </summary>
        public string[] ProtectedInterfaces
        {
            get { return this.protectedInterfaces; }
            set { this.protectedInterfaces = value; }
        }

        /// <summary>
        /// Gets or sets the aspect options.
        /// </summary>
        public CompositionAspectOptions Options
        {
            get { return this.options ?? CompositionAspectOptions.None; }
            set { this.options = value; }
        }

        /// <inheritdoc />
        TypeIdentity ICompositionAspectConfiguration.GetPublicInterface( Type containerType )
        {
            return TypeIdentity.Wrap( this.publicInterface );
        }

        /// <inheritdoc />
        TypeIdentity[] ICompositionAspectConfiguration.GetProtectedInterfaces( Type containerType )
        {
            return TypeIdentity.Wrap( this.protectedInterfaces );
        }

        /// <inheritdoc />
        CompositionAspectOptions? ICompositionAspectConfiguration.GetOptions()
        {
            return this.options;
        }
    }

    /// <summary>
    /// Weaving options of <see cref="ICompositionAspect"/>.
    /// </summary>
    [Flags]
    public enum CompositionAspectOptions
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Ignore this aspect (i.e. does not implement it) if the interface
        /// is already implemented (even indirectly) in a parent type.
        /// </summary>
        IgnoreIfAlreadyImplemented = 1,

        /// <summary>
        /// Implements the <see cref="IComposed{T}"/> interface on the 
        /// target type.
        /// </summary>
        GenerateImplementationAccessor = 2,

        /// <summary>
        /// Marks the field storing the interface implementation as
        /// non serialized (see <see cref="NonSerializedAttribute"/>).
        /// </summary>
        NonSerializedImplementationField = 4
    }
}