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
using PostSharp.CodeModel;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Stores an <see cref="AspectSpecification"/> and its target <see cref="MetadataDeclaration"/>.
    /// </summary>
    public sealed class AspectTargetPair
    {
        private readonly MetadataDeclaration target;
        private readonly AspectSpecification aspectSpecification;

        internal AspectTargetPair( MetadataDeclaration target, AspectSpecification aspect )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( target, "target" );
            ExceptionHelper.AssertArgumentNotNull( aspect, "aspect" );

            #endregion

            this.target = target;
            this.aspectSpecification = aspect;
        }

        /// <summary>
        /// Gets the target <see cref="MetadataDeclaration"/> of the aspect.
        /// </summary>
        public MetadataDeclaration Target
        {
            get { return this.target; }
        }

        /// <summary>
        /// Gets the aspect.
        /// </summary>
        public AspectSpecification AspectSpecification
        {
            get { return this.aspectSpecification; }
        }

        /// <summary>
        /// Determines whether the current aspect is derived from a <see cref="Type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns><b>true</b> if the current aspect is derived from <paramref name="type"/>,
        /// otherwise <b>false</b>.</returns>
        public bool IsDerivedFrom( Type type )
        {
            if ( this.aspectSpecification.Aspect != null )
            {
                // We have the aspect instance.
                return type.IsAssignableFrom( this.aspectSpecification.Aspect.GetType() );
            }
            else if ( this.aspectSpecification.AspectConstruction != null )
            {
                // We have the aspect construction.
                ModuleDeclaration module = this.target.Module;

                ITypeSignature typeSignature = module.GetTypeForFrameworkVariant( type );

                return
                    module.FindType( this.aspectSpecification.AspectConstruction.TypeName, BindingOptions.Default )
                        .IsAssignableTo( typeSignature, GenericMap.Empty );
            }
            else
            {
                // We have only the aspect configuration.
                return type.IsAssignableFrom( this.aspectSpecification.AspectConfiguration.GetType() );
            }
        }
    }
}