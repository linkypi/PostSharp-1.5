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

#region Using directives

using System.Collections.Generic;
using System.ComponentModel;
using PostSharp.CodeModel.Collections;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Implements the functionalities that are common to <see cref="PropertyDeclaration"/>
    /// and <see cref="EventDeclaration"/>, which are both lexical collections of
    /// method semantics (<see cref="MethodSemanticDeclaration"/>).
    /// </summary>
    public abstract class MethodGroupDeclaration : NamedDeclaration
    {
        /// <summary>
        /// Collection of method semantics.
        /// </summary>
        private readonly MethodSemanticDeclarationCollection methods;

        /// <summary>
        /// Initializes a new <see cref="MethodGroupDeclaration"/>.
        /// </summary>
        internal MethodGroupDeclaration()
        {
            this.methods = new MethodSemanticDeclarationCollection( this, "methods" );
        }

        /// <summary>
        /// Gets the parent <see cref="TypeDefDeclaration"/>.
        /// </summary>
        public TypeDefDeclaration DeclaringType
        {
            get { return (TypeDefDeclaration) this.Parent; }
        }


        /// <summary>
        /// Gets the collection of method semantics, which map a method to a role
        /// in the current event or property.
        /// </summary>
        [Browsable( false )]
        public MethodSemanticDeclarationCollection Members
        {
            get
            {
                this.AssertNotDisposed();
                return this.methods;
            }
        }

        /// <summary>
        /// Gets an accessor method.
        /// </summary>
        /// <param name="semantic">Semantic of the requested accessor method.</param>
        /// <returns>The method of requested semantic, or <b>null</b> if the current <see cref="MethodGroupDeclaration"/>
        /// does not contain the requested semantic.</returns>
        public MethodDefDeclaration GetAccessor( MethodSemantics semantic )
        {
            MethodSemanticDeclaration methodSemantic = this.Members.GetBySemantic( semantic );

            return methodSemantic == null ? null : methodSemantic.Method;
        }

        /// <summary>
        /// Gets the visibility of the current element.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                int visibility = (int) Visibility.Private;
                foreach ( MethodSemanticDeclaration method in this.methods )
                {
                    if ( (int) method.Method.Visibility < visibility )
                    {
                        visibility = (int) method.Method.Visibility;
                    }
                }
                return (Visibility) visibility;
            }
        }

        /// <summary>
        /// Writes the IL definition of the method semantics contained in the current collection.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of the
        /// declaring type.</param>
        protected void WriteMethodsILDefinition( ILWriter writer, GenericMap genericMap )
        {
            IEnumerable<MethodSemanticDeclaration> enumerable;

#if ORDERED_EMIT
            enumerable = this.methods.GetByEmitOrder();
#else
            enumerable = this.methods;
#endif

            foreach ( MethodSemanticDeclaration methodSemantic in enumerable )
            {
                string role;

                switch ( methodSemantic.Semantic )
                {
                    case MethodSemantics.AddOn:
                        role = ".addon";
                        break;

                    case MethodSemantics.Fire:
                        role = ".fire";
                        break;

                    case MethodSemantics.Getter:
                        role = ".get";
                        break;

                    case MethodSemantics.Other:
                        role = ".other";
                        break;

                    case MethodSemantics.RemoveOn:
                        role = ".removeon";
                        break;

                    case MethodSemantics.Setter:
                        role = ".set";
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( methodSemantic.Semantic,
                                                                                      "this.Members[?].Semantic" );
                }

                writer.WriteKeyword( role );
                ( (IMethodInternal) methodSemantic.Method ).WriteILReference( writer, genericMap,
                                                                              WriteMethodReferenceOptions.None );
                writer.WriteLineBreak();
            }
        }

        /// <inheritdoc />
        internal override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            if ( disposing )
            {
                this.methods.Dispose();
            }
        }
    }
}