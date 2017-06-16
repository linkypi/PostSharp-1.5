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
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Laos
{
    /// <summary>
    /// Custom attribute that can be extended to develop combined aspects,
    /// i.e. aspects composed of many 'sub-aspects'. These are provided
    /// by the <see cref="ProvideAspects"/> method, which should be
    /// overwritten.
    /// </summary>
    /// <seealso cref="ICompoundAspect"/>
#if !SMALL
    [Serializable]
    [AttributeUsage( AttributeTargets.All, AllowMultiple = true, Inherited = false )]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class CompoundAspect : LaosAspect, ICompoundAspect
    {
        private object targetElement;

        /// <summary>
        /// Provides new aspects.
        /// </summary>
        /// <param name="element">Element to which the current custom attribute is applied.</param>
        /// <param name="collection">Collection into which
        /// new aspects have to be added.</param>
        public abstract void ProvideAspects( object element, LaosReflectionAspectCollection collection );


        /// <inheritdoc />
        public virtual void CompileTimeInitialize( object element )
        {
            this.targetElement = element;
        }

        /// <summary>
        /// Gets the options of the current <see cref="CompoundAspect"/>.
        /// </summary>
        /// <returns>The options of the current <see cref="CompoundAspect"/>.</returns>
        public virtual CompositionAspectOptions GetOptions()
        {
            return CompositionAspectOptions.None;
        }

        /// <inheritdoc />
        void ILaosReflectionAspectProvider.ProvideAspects( LaosReflectionAspectCollection collection )
        {
            this.ProvideAspects( this.targetElement, collection );
        }
    }

    /// <summary>
    /// Options for the aspect <see cref="CompoundAspect"/>.
    /// </summary>
    [Flags]
    public enum CompoundAspectOptions
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0
    }

    /// <summary>
    /// Custom attribute configuring the aspect <see cref="CompoundAspect"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class CompoundAspectConfigurationAttribute : LaosAspectConfigurationAttribute
    {
    }
#endif
}