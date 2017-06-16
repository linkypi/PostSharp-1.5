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
using System.Reflection;
using PostSharp.CodeModel.ReflectionWrapper;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents an event (<see cref="TokenType.Event"/>).
    /// </summary>
    /// <remarks>
    /// Events are owned by types (<see cref="TypeDefDeclaration"/>) and
    /// are exposed on the <see cref="TypeDefDeclaration.Events"/> property.
    /// </remarks>
    public sealed class EventDeclaration : MethodGroupDeclaration, IWriteILDefinition, IRemoveable
    {
        #region Fields

        /// <summary>
        /// Event attributes.
        /// </summary>
        private EventAttributes attributes;

        /// <summary>
        /// Reference to the type of the event delegate.
        /// </summary>
        private ITypeSignature eventType;

        private EventInfo cachedReflectionEvent;

        #endregion

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.Event;
        }

        /// <summary>
        /// Gets the system runtime event corresponding to the current event.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments valid in the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments valid in the current context.</param>
        /// <returns>The system runtime <see cref="System.Reflection.EventInfo"/>, or <b>null</b> if
        /// the current event could not be bound.</returns>
        public EventInfo GetSystemEvent( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            if ( this.cachedReflectionEvent == null )
            {
                Type declaringReflectionType =
                    this.DeclaringType.GetSystemType( genericTypeArguments, genericMethodArguments );
                if ( declaringReflectionType == null )
                {
                    return null;
                }

                this.cachedReflectionEvent =
                    declaringReflectionType.GetEvent( this.Name,
                                                      BindingFlags.Instance | BindingFlags.Public |
                                                      BindingFlags.NonPublic | BindingFlags.Static |
                                                      BindingFlags.DeclaredOnly );
            }

            return this.cachedReflectionEvent;
        }

        /// <summary>
        /// Gets a reflection <see cref="EventInfo"/> that wraps the current event.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>A <see cref="EventInfo"/> wrapping current event in the
        /// given generic context.</returns>
        /// <remarks>
        /// This method returns a <see cref="EventInfo"/> that is different from the system
        /// runtime event that is retrieved by <see cref="GetSystemEvent"/>. This allows
        /// a have a <b>System.Reflection</b> representation of the current event even
        /// when the declaring type it cannot be loaded in the Virtual Runtime Engine.
        /// </remarks>
        public EventInfo GetReflectionWrapper( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return new EventWrapper( this, genericTypeArguments, genericMethodArguments );
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionWrapper( genericTypeArguments, genericMethodArguments );
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            return this.GetReflectionObjectImpl( genericTypeArguments, genericMethodArguments );
        }


        /// <summary>
        /// Gets or sets the event attributes.
        /// </summary>
        /// <value>
        /// A combination of <see cref="EventAttributes"/>.
        /// </value>
        [ReadOnly( true )]
        public EventAttributes Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        /// <summary>
        /// Gets or sets the type of the event handler.
        /// </summary>
        /// <value>
        /// A delegate type.
        /// </value>
        [ReadOnly( true )]
        public ITypeSignature EventType
        {
            get { return eventType; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                eventType = value;
            }
        }

        #region writer IL

        /// <inheritdoc />
        public void WriteILDefinition( ILWriter writer )
        {
            this.WriteILDefinition( writer, this.DeclaringType.GetGenericContext( GenericContextOptions.None ) );
        }

        /// <overloads>
        /// Writes the IL definition of the current event.
        /// </overloads>
        /// <summary>
        /// Writes the IL definition of the current event, given the <see cref="GenericMap"/>
        /// of the declaring type.
        /// </summary>
        /// <param name="writer">An <see cref="ILWriter"/>.</param>
        /// <param name="genericMap">The <see cref="GenericMap"/> of
        /// the containing type.</param>
        internal void WriteILDefinition( ILWriter writer, GenericMap genericMap )
        {
            writer.WriteKeyword( ".event" );

            if ( ( this.attributes & EventAttributes.SpecialName ) == EventAttributes.SpecialName )
            {
                writer.WriteKeyword( "specialname" );
            }
            if ( ( this.attributes & EventAttributes.RTSpecialName ) == EventAttributes.RTSpecialName )
            {
                writer.WriteKeyword( "rtspecialname" );
            }

            ( (ITypeSignatureInternal) this.eventType ).WriteILReference( writer, genericMap,
                                                                          WriteTypeReferenceOptions.None );

            writer.WriteIdentifier( this.Name );
            writer.WriteLineBreak();
            writer.BeginBlock();

            this.CustomAttributes.WriteILDefinition( writer );

            this.WriteMethodsILDefinition( writer, genericMap );


            writer.EndBlock();
        }

        #endregion

        #region IRemoveable Members

        /// <inheritdoc />
        public void Remove()
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.Parent != null, "CannotRemoveBecauseNoParent" );

            #endregion

            this.DeclaringType.Events.Remove( this );
        }


        #endregion

        /// <inheritdoc />
        public override void ClearCache()
        {
            base.ClearCache();
            this.cachedReflectionEvent = null;
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of events.
        /// </summary>
        [DebuggerTypeProxy( typeof(CollectionDebugViewer) )]
        [DebuggerDisplay( "{GetType().Name}, Count={Count}" )]
        public sealed class EventDeclarationCollection :
            OrderedEmitAndByUniqueNameDeclarationCollection<EventDeclaration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EventDeclarationCollection"/>
            /// type with zero initial capacity.
            /// </summary>
            /// <param name="parent">Declaration to which the new collection will belong.</param>
            /// <param name="role">Role of the new collection in its parent.</param>
            internal EventDeclarationCollection( Declaration parent, string role ) : base( parent, role )
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

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                TypeDefDeclaration owner = (TypeDefDeclaration) this.Owner;
                owner.Module.ModuleReader.ImportEvents( owner );
            }
        }

        
    }
}