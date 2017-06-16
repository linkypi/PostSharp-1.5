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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using PostSharp.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a method semantic (<see cref="TokenType.MethodSemantic"/>), i.e. the association of a method and a role in 
    /// a collection (<see cref="PropertyDeclaration"/> or <see cref="EventDeclaration"/>).
    /// </summary>
    /// <remarks>
    /// Method semantics
    /// are owned by <see cref="MethodGroupDeclaration"/> (<see cref="PropertyDeclaration"/>
    /// or <see cref="EventDeclaration"/>).
    /// </remarks>
    public sealed class MethodSemanticDeclaration : MetadataDeclaration, IRemoveable
    {
        #region Fields

        /// <summary>
        /// Method.
        /// </summary>
        private MethodDefDeclaration method;

        /// <summary>
        /// Semantic of the method.
        /// </summary>
        private MethodSemantics semantic;

        #endregion

        /// <summary>
        /// Initializes a new empty <see cref="MethodSemanticDeclaration"/>.
        /// </summary>
        public MethodSemanticDeclaration()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MethodSemanticDeclaration"/> and sets its content.
        /// </summary>
        /// <param name="semantic">Semantic.</param>
        /// <param name="method">Method implementing the semantic.</param>
        public MethodSemanticDeclaration(MethodSemantics semantic, MethodDefDeclaration method)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(method, "method");

            #endregion

            this.semantic = semantic;
            this.method = method;
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.MethodSemantic;
        }

        /// <summary>
        /// Gets or sets the method linked to the semantic.
        /// </summary>
        public MethodDefDeclaration Method
        {
            get
            {
                return method;
            }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull(value, "value");

                #endregion

                this.method = value;
            }
        }

        /// <summary>
        /// Gets or sets the semantic of the method in the containing collection.
        /// </summary>
        public MethodSemantics Semantic
        {
            get { return semantic; }
            set { semantic = value; }
        }

        /// <summary>
        /// Gets the parent <see cref="MethodGroupDeclaration"/>.
        /// </summary>
        [Browsable(false)]
        public new MethodGroupDeclaration Parent
        {
            get { return (MethodGroupDeclaration) base.Parent; }
        }

        #region IRemoveable Members

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation(this.Parent != null, "CannotRemoveBecauseNoParent");

            #endregion

            this.Parent.Members.Remove(this);
        }

        #endregion

        /// <summary>
        /// Implementation of <see cref="IComparer{MethodSemanticDeclaration}"/> comparing instances
        /// by their <see cref="MethodSemanticDeclaration.Semantic"/> property.
        /// </summary>
        public sealed class BySemanticComparer : IComparer<MethodSemanticDeclaration>
        {
            private static readonly BySemanticComparer instance = new BySemanticComparer();

            /// <summary>
            /// Gets an instance of <see cref="BySemanticComparer"/>.
            /// </summary>
            /// <returns></returns>
            public static BySemanticComparer GetInstance()
            {
                return instance;
            }

            #region IComparer<MethodSemanticDeclaration> Members
            
            /// <inheritdoc />
            public int Compare(MethodSemanticDeclaration x, MethodSemanticDeclaration y)
            {
                return x.semantic - y.semantic;
            }

            #endregion
        }

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return new NotSupportedException();
        }

        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of method semantics (<see cref="MethodSemanticDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy(typeof(CollectionDebugViewer))]
        [DebuggerDisplay("{GetType().Name}, Count={Count}")]
        public sealed class MethodSemanticDeclarationCollection :
            OrderedEmitDeclarationCollection<MethodSemanticDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MethodSemanticDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal MethodSemanticDeclarationCollection(Declaration parent, string role) :
                base(parent, role)
            {
            }

            /// <inheritdoc />
            protected override bool RequiresEmitOrdering
            {
                get
                {
#if ORDERED_EMIT
                    return false;
#else
                    return true;
#endif
                }
            }


            /// <summary>
            /// Gets the <see cref="MethodSemanticDeclaration"/> given its semantic (<see cref="MethodSemantics"/>).
            /// </summary>
            /// <param name="semantic">Method semantic.</param>
            /// <returns>The <see cref="MethodSemanticDeclaration"/> whose semantic equals
            /// <paramref name="semantic"/>, or <b>null</b> if the current collection does not
            /// contain a method with this semantic.</returns>
            /// <remarks>
            /// If the collection contains more than one method with this semantic, the current
            /// method returns any arbitrary of them.
            /// </remarks>
            public MethodSemanticDeclaration GetBySemantic(MethodSemantics semantic)
            {
                foreach (MethodSemanticDeclaration declaration in this)
                {
                    if (declaration.Semantic == semantic)
                    {
                        return declaration;
                    }
                }

                return null;
            }

            /// <summary>
            /// Determines whether the current collection contains a method with given semantic.
            /// </summary>
            /// <param name="semantic">Method semantic.</param>
            /// <returns><b>true</b> if the current collection contains at least one method
            /// with given semantic, otherwise <b>false</b>.</returns>
            public bool Contains(MethodSemantics semantic)
            {
                return GetBySemantic(semantic) != null;
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get
                {
                    return true;
                }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                MethodGroupDeclaration mg = (MethodGroupDeclaration) this.Owner;
                mg.Module.ModuleReader.ImportMethodSemantics( mg );
            }
        }
    }


    /// <summary>
    /// Enumerates the method semantics.
    /// </summary>
    [Flags]
    public enum MethodSemantics
    {
        /// <summary>
        /// No semantic.
        /// </summary>
        None = 0,
        /// <summary>
        /// Setter for property.
        /// </summary>
        Setter = 0x0001,
        /// <summary>
        /// Getter for property
        /// </summary>
        Getter = 0x0002, //	
        /// <summary>
        /// Other method for property or event 
        /// </summary>
        Other = 0x0004, //	  
        /// <summary>
        /// AddOn method for event
        /// </summary>
        AddOn = 0x0008, //	
        /// <summary>
        /// 	RemoveOn method for event 
        /// </summary>
        RemoveOn = 0x0010, //  
        /// <summary>
        /// Fire method for event
        /// </summary>
        Fire = 0x0020, //	
    }
}