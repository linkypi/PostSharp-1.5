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

using System.ComponentModel;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a reference to a field or method (<see cref="TokenType.MemberRef"/>). This type
    /// is the base of <see cref="FieldRefDeclaration"/> and <see cref="MethodRefDeclaration"/>.
    /// </summary>
    public abstract class MemberRefDeclaration : NamedDeclaration
    {
        /// <summary>
        /// Initializes a new <see cref="MemberRefDeclaration"/>.
        /// </summary>
        internal MemberRefDeclaration()
        {
        }

        /// <summary>
        /// Gets scope in which the current reference should be resolved.
        /// </summary>
        [Browsable( false )]
        public IMemberRefResolutionScope ResolutionScope
        {
            get { return (IMemberRefResolutionScope) this.Parent; }
        }

        /// <summary>
        /// Gets the declaring <see cref="IType"/>.
        /// </summary>
        /// <value>
        /// The declaring type (<see cref="TypeRefDeclaration"/> or 
        /// <see cref="TypeSpecDeclaration"/>) or <b>null</b>
        /// if <see cref="ResolutionScope"/> is not a type.
        /// </value>
        public IType DeclaringType
        {
            get { return this.Parent as IType; }
        }

        /// <summary>
        /// Gets the assembly in which the member is defined.
        /// </summary>
        /// <value>
        /// An <see cref="IAssembly"/>.
        /// </value>
        [Browsable( false )]
        public override IAssembly DeclaringAssembly
        {
            get
            {
                ITypeSignature declaringType = this.DeclaringType;
                if ( declaringType != null )
                {
                    return declaringType.DeclaringAssembly;
                }
                else
                {
                    IModule declaringModule = this.Parent as IModule;
                    if ( declaringModule != null )
                    {
                        return declaringModule.Assembly;
                    }
                    else
                    {
                        throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidType",
                                                                                   this.Parent.GetType(), "this.Parent",
                                                                                   "IModule, ITypeSignature" );
                    }
                }
            }
        }

        /// <inheritdoc />
        public override sealed TokenType GetTokenType()
        {
            return TokenType.MemberRef;
        }
    }
}