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


namespace PostSharp.CodeModel.TypeSignatures
{
    /// <summary>
    /// Represents a custom modifier, which are tags emitted by compilers that
    /// add some meaning to the modified type.
    /// </summary>
    /// <remarks>
    /// <para>
    ///	   A custom modifier modifies a type and gives a a different meaning. For instance,
    ///    the <see cref="System.Runtime.CompilerServices.IsConst"/> means that the argument
    ///    should not be changed. Custom modifiers are emitted by compilers.
    /// </para>
    /// </remarks>
    public struct CustomModifier 
    {
        #region Fields

        /// <summary>
        /// Determines wether the custom modifier is required to be understood.
        /// </summary>
        private bool required;

        /// <summary>
        /// References to the type representing the custom attribute.
        /// </summary>
        private ITypeSignature type;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="CustomModifier"/>.
        /// </summary>
        /// <param name="required">Whether the custom attribute is required.</param>
        /// <param name="type">Type of the custom attribute.</param>
        public CustomModifier( bool required, ITypeSignature type )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            this.required = required;
            this.type = type;
        }

        /// <summary>
        /// Specifies whether the custom modifier is required.
        /// </summary>
        public bool Required { get { return required; } set { required = value; } }

        /// <summary>
        /// Gets or sets the type of the custom modifier.
        /// </summary>
        public ITypeSignature Type
        {
            get { return type; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                type = value;
            }
        }

        /// <inheritdoc />
        public CustomModifier Translate( ModuleDeclaration targetModule )
        {
            #region Preconditions

            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetModule, "targetModule" );
            ExceptionHelper.Core.AssertValidArgument( targetModule.Domain == this.type.Module.Domain,
                                                      "targetModule", "ModuleInSameDomain" );

            #endregion

            if ( targetModule == this.type.Module )
            {
                return this;
            }
            else
            {
                return new CustomModifier( this.required, this.type.Translate( targetModule ) );
            }

            #endregion
        }

        
        
    }
}