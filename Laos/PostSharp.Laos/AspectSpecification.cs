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
    /// Completely specifies an aspect. Contains either the aspect instance itself (<see cref="Aspect"/> property),
    /// either information allowing to construct the aspect (<see cref="AspectConstruction"/>) and configure
    /// the weaver (<see cref="AspectConfiguration"/>).
    /// </summary>
    public sealed class AspectSpecification
    {
        private readonly ILaosAspect aspect;
        private readonly IObjectConstruction aspectConstruction;
        private readonly ILaosAspectConfiguration aspectConfiguration;

        /// <summary>
        /// Initializes a new <see cref="AspectSpecification"/> from an aspect instance.
        /// </summary>
        /// <param name="aspect">Aspect instance.</param>
        /// <param name="aspectConstruction">Construction of the aspect (or <b>null</b> if the construction
        /// is not available - in this case the aspect is required to have a serializer).</param>
        public AspectSpecification( IObjectConstruction aspectConstruction, ILaosAspect aspect )
        {
            #region Preconditions

            if ( aspect == null ) throw new ArgumentNullException( "aspect" );

            #endregion

            this.aspectConstruction = aspectConstruction;
            this.aspect = aspect;
        }

        /// <summary>
        /// Initializes a new <see cref="AspectSpecification"/> when one cannot provide an aspect instance,
        /// i.e. from an <see cref="IObjectConstruction"/> and a <see cref="ILaosAspectConfiguration"/>.
        /// </summary>
        /// <param name="aspectConstruction">Aspect construction.</param>
        /// <param name="aspectConfiguration">Aspect configuration.</param>
        public AspectSpecification( IObjectConstruction aspectConstruction, ILaosAspectConfiguration aspectConfiguration )
        {
            #region Preconditions

            if ( aspectConstruction == null ) throw new ArgumentNullException( "aspectConstruction" );

            #endregion

            this.aspectConfiguration = aspectConfiguration;
            this.aspectConstruction = aspectConstruction;
        }


        /// <summary>
        /// Gets the aspect configuration.
        /// </summary>
        /// <value>
        /// The aspect configuration, or <b>null</b> if none was provided.
        /// </value>
        public ILaosAspectConfiguration AspectConfiguration
        {
            get { return aspectConfiguration; }
        }

        /// <summary>
        /// Gets the aspect construction.
        /// </summary>
        /// <value>
        /// The aspect construction, or <b>null</b> if the aspect instance was provided instead.
        /// </value>
        public IObjectConstruction AspectConstruction
        {
            get { return aspectConstruction; }
        }

        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        /// <value>
        /// The aspect instance, or <b>null</b> if the <see cref="AspectConfiguration"/> was provided instead.
        /// </value>
        public ILaosAspect Aspect
        {
            get { return aspect; }
        }

        /// <summary>
        /// Gets the assembly-qualified type name of the aspect.
        /// </summary>
        public string AspectAssemblyQualifiedTypeName
        {
            get
            {
                return this.aspect != null
                           ? this.aspect.GetType().AssemblyQualifiedName
                           : this.aspectConstruction.TypeName;
            }
        }

        /// <summary>
        /// Gets the type name of the aspect.
        /// </summary>
        public string AspectTypeName
        {
            get
            {
                if ( this.aspect != null )
                    return this.aspect.GetType().FullName;
                else
                {
                    int index = this.aspectConstruction.TypeName.IndexOf( ',' );
                    if ( index > 0 )
                        return this.aspectConstruction.TypeName.Substring( index );
                    else
                        return this.aspectConstruction.TypeName;
                }
            }
        }
    }
}

#endif