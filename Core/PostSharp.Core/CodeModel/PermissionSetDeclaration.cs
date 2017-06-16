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
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using PostSharp.CodeModel.Collections;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a permission set (<see cref="TokenType.Permission"/>). 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Permissions sets are
    /// owned by types implementing <see cref="ISecurable"/>, i.e.
    /// types (<see cref="TypeDefDeclaration"/>) and methods (<see cref="MethodDefDeclaration"/>).
    /// </para>
    /// <para>
    /// A permission set can be represented in the PE image either as a serialized custom
    /// attributes (in such case the <see cref="PermissionSetDeclaration.Attributes"/> property is set),
    /// either as XML (in such case the <see cref="PermissionSetDeclaration.Xml"/> property is set).
    /// </para>
    /// </remarks>
    public sealed class PermissionSetDeclaration : MetadataDeclaration, IWriteILDefinition
    {
        #region Fields

        /// <summary>
        /// Security action.
        /// </summary>
        private SecurityAction action;

        /// <summary>
        /// Collection of permission attributes.
        /// </summary>
        private readonly PermissionDeclarationCollection permissions;

        /// <summary>
        /// XML representation, or <b>null</b> if the permission set is
        /// given in binarily serialized form.
        /// </summary>
        private string xml;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="PermissionSetDeclaration"/>.
        /// </summary>
        public PermissionSetDeclaration()
        {
            this.permissions = new PermissionDeclarationCollection(this, "permission");
        }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.Permission;
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityAction"/> of the current permission set.
        /// </summary>
        [ReadOnly(true)]
        public SecurityAction SecurityAction
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Gets the collection of permission attributes (<see cref="PermissionDeclaration"/>).
        /// </summary>
        /// <remarks>
        /// This collection is not relevant if the <see cref="PermissionSetDeclaration.Xml"/>
        /// property is not null.
        /// </remarks>
        [Browsable(false)]
        public PermissionDeclarationCollection Attributes
        {
            get
            {
                this.AssertNotDisposed();
                return permissions;
            }
        }

        /// <summary>
        /// Gets the XML representation.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> containing the XML representation, or <b>null</b> if the
        /// permission set is binarily serialized, in which case individual attributes
        /// are available on the <see cref="Attributes"/> property.
        /// </value>
        [ReadOnly(true)]
        public string Xml
        {
            get { return this.xml; }
            set { this.xml = value; }
        }

        #region IWriteILDefinition Members

        /// <inheritdoc />
        public void WriteILDefinition(ILWriter writer)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(writer, "writer");

            #endregion

            writer.WriteKeyword(".permissionset");
            switch (this.action)
            {
                case SecurityAction.Assert:
                    writer.WriteKeyword("assert");
                    break;

                case SecurityAction.Demand:
                    writer.WriteKeyword("demand");
                    break;

                case SecurityAction.Deny:
                    writer.WriteKeyword("deny");
                    break;

                case SecurityAction.InheritanceDemand:
                    writer.WriteKeyword("inheritcheck");
                    break;

                case SecurityAction.LinkDemand:
                    writer.WriteKeyword("linkcheck");
                    break;

                case SecurityAction.PermitOnly:
                    writer.WriteKeyword("permitonly");
                    break;

                case SecurityAction.RequestMinimum:
                    writer.WriteKeyword("reqmin");
                    break;

                case SecurityAction.RequestOptional:
                    writer.WriteKeyword("reqopt");
                    break;

                case SecurityAction.RequestRefuse:
                    writer.WriteKeyword("reqrefuse");
                    break;

                    // Mono specific
                case (SecurityAction) 14:
                    writer.WriteKeyword("noncaslinkdemand");
                    break;

                case (SecurityAction) 15:
                    writer.WriteKeyword("noncasinheritance");
                    break;

                default:
                    writer.WriteRaw(" ??" + this.action + "?? ");
                    break;
                    // throw ExceptionHelper.CreateInvalidEnumerationValueException( this.action, "this.Action" );
            }
            writer.WriteLineBreak();
            writer.Indent++;

            if (!string.IsNullOrEmpty(xml))
            {
                writer.WriteQuotedString(this.xml, WriteStringOptions.DoubleQuoted);
            }
            else
            {
                writer.WriteSymbol('=');
                writer.WriteSymbol('{');
                writer.MarkAutoIndentLocation();


                bool first = true;
                foreach (PermissionDeclaration permission in this.permissions)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteSymbol(',');
                        writer.WriteLineBreak();
                    }
                    permission.WriteILDefinition(writer);
                }

                writer.ResetIndentLocation();
                writer.WriteSymbol('}');
                writer.Indent--;
            }
            writer.WriteLineBreak();
        }

        #endregion

        /// <inheritdoc />
        internal override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.permissions.Dispose();
            }
        }

        internal override object GetReflectionWrapperImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }

        internal override object GetReflectionObjectImpl(Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotSupportedException();
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of permission sets (<see cref="PermissionSetDeclaration"/>).
        /// </summary>
        [DebuggerTypeProxy(typeof (CollectionDebugViewer))]
        [DebuggerDisplay("{GetType().Name}, Count={Count}")]
        public sealed class PermissionSetDeclarationCollection :
            OrderedEmitDeclarationCollection<PermissionSetDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PermissionSetDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal PermissionSetDeclarationCollection(Declaration parent, string role) :
                base(parent, role)
            {
            }

            /// <inheritdoc />
            protected override bool RequiresEmitOrdering
            {
                get
                {
#if ORDERED_EMIT
                    return true;
#else
                    return false;
#endif
                }
            }
   

            /// <summary>
            /// Writes the IL definition of all permission sets in the current collection.
            /// </summary>
            /// <param name="writer">An <see cref="ILWriter"/>.</param>
            internal void WriteILDefinition(ILWriter writer)
            {
                foreach (PermissionSetDeclaration decl in this)
                {
                    decl.WriteILDefinition(writer);
                }
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
                ISecurable owner = (ISecurable) this.Owner;
                owner.Module.ModuleReader.ImportPermissionSets( owner);
            }
        }
    }
}