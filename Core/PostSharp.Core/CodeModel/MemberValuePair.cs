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
using System.Diagnostics;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents the fact that a member is assigned a value.
    /// </summary>
    public sealed class MemberValuePair : IPositioned, INamed
    {
        #region Fields

        /// <summary>
        /// Ordinal of this pair in the parent collection.
        /// </summary>
        private readonly int ordinal;

        /// <summary>
        /// The kind of member refered to by <see cref="memberName"/>.
        /// </summary>
        private readonly MemberKind memberKind;

        /// <summary>
        /// The member name.
        /// </summary>
        private readonly string memberName;

        /// <summary>
        /// The value.
        /// </summary>
        private SerializedValue value;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MemberValuePair"/>.
        /// </summary>
        /// <param name="memberKind">Member kind (<see cref="PostSharp.CodeModel.MemberKind.Field"/>,
        /// <see cref="PostSharp.CodeModel.MemberKind.Parameter"/> or <see cref="PostSharp.CodeModel.MemberKind.Property"/>).</param>
        /// <param name="memberName">Member name.</param>
        /// <param name="value">Value.</param>
        /// <param name="ordinal">Ordinal of this instance in the parent collection.</param>
        public MemberValuePair( MemberKind memberKind, int ordinal, string memberName, SerializedValue value )
        {
            #region Preconditions

            if ( string.IsNullOrEmpty( memberName ) )
            {
                throw new ArgumentNullException( "memberName" );
            }

            if ( memberKind != MemberKind.Parameter &&
                 memberKind != MemberKind.Property &&
                 memberKind != MemberKind.Field )
            {
                throw new ArgumentOutOfRangeException( "memberKind" );
            }

            #endregion

            this.memberKind = memberKind;
            this.memberName = memberName;
            this.value = value;
            this.ordinal = ordinal;
        }

        #region Properties

        /// <summary>
        /// Kind of member (<see cref="PostSharp.CodeModel.MemberKind.Field"/>,
        /// <see cref="PostSharp.CodeModel.MemberKind.Parameter"/> or 
        /// <see cref="PostSharp.CodeModel.MemberKind.Property"/>).
        /// </summary>
        /// <value>
        /// A <see cref="PostSharp.CodeModel.MemberKind"/>.
        /// </value>
        public MemberKind MemberKind
        {
            get { return memberKind; }
        }

        /// <summary>
        /// Member name.
        /// </summary>
        /// <value>
        /// A non-empty string.
        /// </value>
        public string MemberName
        {
            get { return memberName; }
        }

        string INamed.Name
        {
            get { return this.memberName; }
        }

        /// <summary>
        /// Gets the ordinal of this instance in the parent collection.
        /// </summary>
        public int Ordinal
        {
            get { return this.ordinal; }
        }

        int IPositioned.Ordinal
        {
            get { return this.ordinal; }
        }

        /// <summary>
        /// Member value.
        /// </summary>
        /// <value>
        /// A <see cref="SerializedValue"/>.
        /// </value>
        public SerializedValue Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #endregion

        #region IWriteILDefinition Members

        /// <inheritdoc />
        internal void WriteILDefinition( ModuleDeclaration module, ILWriter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( writer, "writer" );

            #endregion

            switch ( this.MemberKind )
            {
                case MemberKind.Field:
                    writer.WriteKeyword( "field" );
                    break;

                case MemberKind.Property:
                    writer.WriteKeyword( "property" );
                    break;

                case MemberKind.Parameter:
                    break;

                default:
                    throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidEnumerationValue",
                                                                               typeof(MemberKind).Name, this.MemberKind );
            }


            if ( this.memberKind != MemberKind.Parameter )
            {
                this.value.WriteILType( writer );

                writer.WriteQuotedString( this.MemberName );
                writer.WriteSymbol( '=' );
            }
            this.value.WriteILValue( writer, WriteSerializedValueMode.AttributeParameterValue );
        }

        #endregion

        /// <summary>
        /// Clone the current instance, but sets another ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal of the new instance.</param>
        /// <returns>A clone of the current instance, but with the specified ordinal.</returns>
        public MemberValuePair Clone( int ordinal )
        {
            return new MemberValuePair( this.memberKind, ordinal, this.memberName, this.value );
        }

        /// <summary>
        /// Translates the current <see cref="MemberValuePair"/> so that it can be used in another module.
        /// </summary>
        /// <param name="module">The module into which the translated instance should be expressed.</param>
        /// <returns>A new <see cref="MemberValuePair"/> equivalent to the current one, but expressed
        /// for the other <paramref name="module"/>.</returns>
        public MemberValuePair Translate( ModuleDeclaration module )
        {
            return new MemberValuePair( this.memberKind, ordinal, this.memberName, this.value.Translate( module ) );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format( "{0}: {1}={2}", this.ordinal, this.memberName, this.value.ToString() );
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of member-value pairs (<see cref="MemberValuePair"/>).
        /// </summary>
        [DebuggerTypeProxy(typeof(CollectionDebugViewer))]
        [DebuggerDisplay("{GetType().Name}, Count={Count}")]
        public sealed class MemberValuePairCollection : IndexedCollection<MemberValuePair>,
                                                        ICloneable
        {
            private static readonly ICollectionFactory<MemberValuePair>[] indexFactories =
                new ICollectionFactory<MemberValuePair>[]
                    {
                        new OrdinalIndexFactory<MemberValuePair>(),
                        new NameIndexFactory<MemberValuePair>( true )
                    };


            /// <summary>
            /// Initializes a new <see cref="MemberValuePairCollection"/>
            /// and specifies the initial capacity.
            /// </summary>
            /// <param name="capacity">Initial capacity.</param>
            public MemberValuePairCollection( int capacity ) : base( indexFactories, capacity )
            {
            }

            /// <summary>
            /// Initializes a new <see cref="MemberValuePairCollection"/>
            /// with default initial capacity.
            /// </summary>
            public MemberValuePairCollection() : this( 4 )
            {
            }


            /// <summary>
            /// Gets a member given its name.
            /// </summary>
            /// <param name="name">Member name.</param>
            /// <returns>he <see cref="MemberValuePair"/> whose name is <paramref name="name"/>,
            /// or <b>null</b> if it could not be found.</returns>
            public MemberValuePair this[ string name ]
            {
                get
                {
                    MemberValuePair item;
                    this.TryGetFirstValueByKey( 1, name, out item );
                    return item;
                }
            }

            /// <summary>
            /// Gets a member given its ordinal.
            /// </summary>
            /// <param name="ordinal">Member ordinal.</param>
            /// <returns>he <see cref="MemberValuePair"/> whose ordinal is <paramref name="ordinal"/>,
            /// or <b>null</b> if it could not be found.</returns>
            public MemberValuePair this[ int ordinal ]
            {
                get
                {
                    MemberValuePair item;
                    this.TryGetFirstValueByKey( 0, ordinal, out item );
                    return item;
                }
            }

            #region IDisposable Members

            [Conditional( "ASSERT" )]
            private void AssertNotDisposed()
            {
                if ( this.IsDisposed )
                {
                    throw new ObjectDisposedException( "MemberValuePairCollection" );
                }
            }

            #endregion

            /// <summary>
            /// Deeply clones the current collection.
            /// </summary>
            /// <returns>A new <see cref="MemberValuePairCollection"/> containing
            /// clones of all members of the current collection.</returns>
            public MemberValuePairCollection Clone()
            {
                MemberValuePairCollection clone = new MemberValuePairCollection( this.Count );
                foreach ( MemberValuePair pair in this )
                {
                    clone.Add( pair.Clone( pair.Ordinal ) );
                }

                return clone;
            }

            #region ICloneable Members

            /// <inheritdoc />
            object ICloneable.Clone()
            {
                return this.Clone();
            }

            #endregion
        }
    }
}