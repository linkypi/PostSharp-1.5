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
using System.Collections;
using System.Collections.Generic;
using PostSharp.Collections;
using PostSharp.ModuleReader;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Exposes the metadata tables. 
    /// </summary>
    /// <remarks>
    /// These provide a by-token access to all 
    /// metadata declarations of the assembly. This class is accessible
    /// on the <see cref="ModuleDeclaration.Tables"/> property.
    /// </remarks>
    /// <devDoc>
    /// This class is typically used to resolve tokens found in binary IL.
    /// </devDoc>
    public sealed class MetadataDeclarationTables
    {
        #region Fields

        /// <summary>
        /// Number of metadata tables.
        /// </summary>
        public const int TableCount = ( (int) TokenType.TableCount ) >> 24;

        /// <summary>
        /// List of tables.
        /// </summary>
        private readonly List<Table> tables;

        /// <summary>
        /// A PE image reader.
        /// </summary>
        private readonly ImageReader imageReader;

        private readonly ModuleReader.ModuleReader moduleReader;

        /// <summary>
        /// Indexes <see cref="CustomStringDeclaration"/> by their value.
        /// </summary>
        private readonly Dictionary<string, CustomStringDeclaration> customStrings =
            new Dictionary<string, CustomStringDeclaration>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataDeclarationTables"/>
        /// type.
        /// </summary>
        internal MetadataDeclarationTables( ModuleReader.ModuleReader moduleReader )
        {
            tables = new List<Table>( TableCount );
            for ( int i = 0; i < TableCount; i++ )
            {
                tables.Add( null );
            }

            this.SetTable( TokenType.CustomString, null );
            this.SetTable( TokenType.WeaklyReferencedDeclaration, null );
            this.moduleReader = moduleReader;
            this.imageReader = moduleReader.ImageReader;
        }

        #region Internal methods

        /// <summary>
        /// Gets the index of the table containing the declarations
        /// of a given <see cref="TokenType"/>.
        /// </summary>
        /// <param name="tokenType">A <see cref="TokenType"/>.</param>
        /// <returns>An index valid in the <see cref="tables"/> list.</returns>
        private static int GetTableIndex( TokenType tokenType )
        {
            int tableIndex = ( (int) tokenType ) >> 24;
            if ( tableIndex >= TableCount )
            {
                throw new ArgumentOutOfRangeException( "tokenType" );
            }
            return tableIndex;
        }

        /// <summary>
        /// Gets the table containing the declarations of a given <see cref="TokenType"/>.
        /// </summary>
        /// <param name="tokenType">A <see cref="TokenType"/>.</param>
        /// <param name="createIfAbsent">Whether the table should be created if it does not exist yet.</param>
        /// <returns>The table containing the declarations of a type <paramref name="tokenType"/>.
        /// </returns>
        private Table GetTable( TokenType tokenType, bool createIfAbsent )
        {
            int tableIndex = GetTableIndex( tokenType );
            Table table = this.tables[tableIndex];
            if ( table == null && createIfAbsent )
            {
                table = new Table( this, null, tokenType );
                this.tables[tableIndex] = table;
            }
            return table;
        }


        internal int GetTableSize( TokenType tokenType )
        {
            Table table = this.GetTable( tokenType, false );
            return table == null ? 0 : table.Size;
        }

        /// <summary>
        /// Sets the table of declarations.
        /// </summary>
        /// <param name="tokenType">A <see cref="TokenType"/>.</param>
        /// <param name="declarations">The initial array of declarations, whose
        /// type should be <paramref name="tokenType"/>.</param>
        /// <remarks>
        /// This method is typically called by the <see cref="ModuleReader"/> after
        /// metadata tables have been created.
        /// </remarks>
        internal void SetTable( TokenType tokenType, MetadataDeclaration[] declarations )
        {
#if DEBUG
            // Check that all entries of the table have a token.
            if (declarations != null)
            {
                for ( int i = 0; i < declarations.Length; i++ )
                {
                    if (declarations[i] != null && declarations[i].MetadataToken.IsNull)
                        throw new ArgumentException( "Declarations cannot have a null token." );
                }
            }
#endif
            this.tables[GetTableIndex( tokenType )] = new Table( this, declarations, tokenType );
        }

        #endregion

        #region Direct access

        /// <summary>
        /// Gets the <see cref="MetadataDeclaration"/> given its <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>The <see cref="MetadataDeclaration"/> whose token is <paramref name="token"/>,
        /// or <b>null</b> if <paramref name="token"/> is null.</returns>
        public MetadataDeclaration GetDeclaration( MetadataToken token )
        {
            if ( token.IsNull )
            {
                return null;
            }
            Table table = GetTable( token.TokenType, false );
            MetadataDeclaration declaration = table[token];

            if ( declaration == null )
            {
                // Load the declaration lazily.
                declaration = this.moduleReader.ResolveToken( token );

#if ASSERT
                if ( declaration == null )
                {
                    throw new AssertionFailedException( "The token should have been resolved." );
                }
#endif
                table[token] = declaration;
            }

            return declaration;
        }

        /// <summary>
        /// Adds a <see cref="MetadataDeclaration"/> to the proper table.
        /// </summary>
        /// <param name="declaration">A <see cref="MetadataDeclaration"/> with null
        /// token.</param>
        /// <returns>The new <see cref="MetadataToken"/> of <paramref name="declaration"/>.</returns>
        public MetadataToken AddStronglyReferencedDeclaration( MetadataDeclaration declaration )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( declaration, "declaration" );
            if ( !declaration.MetadataToken.IsNull )
            {
                throw new ArgumentException( "The declaration is already attached to a module." );
            }

            #endregion

            return GetTable( declaration.GetTokenType(), true ).Add( declaration );
        }

        /// <summary>
        /// Registers a new user string (called custom string) and assigns
        /// it a metadata token.
        /// </summary>
        /// <param name="value">A <see cref="LiteralString"/>.</param>
        /// <returns>The <see cref="MetadataToken"/> assigned to
        /// <paramref name="value"/>.</returns>
        /// <remarks>
        /// It is not possible to define new user strings, because these
        /// strings are not stored in an extensible type. However, we need
        /// to be able to address a string by a token. That is why
        /// we created a table of custom strings.
        /// </remarks>
        internal MetadataToken AddCustomString( LiteralString value )
        {
            CustomStringDeclaration customString;
            string strValue = value.ToString();

            if ( !this.customStrings.TryGetValue( strValue, out customString ) )
            {
                customString = new CustomStringDeclaration( value );
                this.AddStronglyReferencedDeclaration( customString );
                this.customStrings.Add( strValue, customString );
            }

            return customString.MetadataToken;
        }

        /// <summary>
        /// Removes a <see cref="MetadataDeclaration"/> from its table.
        /// </summary>
        /// <param name="declaration">A <see cref="MetadataDeclaration"/> with 
        /// non null token.</param>
        public void RemoveDeclaration( MetadataDeclaration declaration )
        {
            #region Precondition

            if ( declaration == null )
            {
                throw new ArgumentNullException( "declaration" );
            }
            if ( declaration.MetadataToken.IsNull )
            {
                throw new ArgumentException( "This declaration is not a part of the current module." );
            }

            #endregion

            Table table = GetTable( declaration.GetTokenType(), false );
            if ( table != null ) table.Remove( declaration );
        }

        /// <summary>
        /// Adds a <see cref="MetadataDeclaration"/> to the table of non-linked
        /// declarations. Non-linked declarations are stored in the current class
        /// for convenience, but shall not be in the metadata tables of the
        /// final PE file.
        /// </summary>
        /// <param name="declaration">A <see cref="MetadataDeclaration"/>.</param>
        /// <remarks>
        /// Serialized values (custom attributes, security declarations) often reference
        /// types by full name. These types are referenced by the module but are not
        /// present in the <b>TypeRef</b> metadata tables. Non-linked declarations allow
        /// to represent these kind of type references in an uniform way, while keeping
        /// the possibility not to include it in the metadata tables.
        /// </remarks>
        internal void AddWeaklyReferencedDeclaration( MetadataDeclaration declaration )
        {
            GetTable( TokenType.WeaklyReferencedDeclaration, true ).Add( declaration );
        }

        /// <summary>
        /// Changes a previously weakly referenced declaration into a strongly
        /// referenced declaration.
        /// </summary>
        /// <param name="declaration">A non-linked <see cref="MetadataDeclaration"/>.</param>
        /// <seealso cref="AddWeaklyReferencedDeclaration"/>.
        internal void SetStrongReference( MetadataDeclaration declaration )
        {
            Table.Move( declaration,
                        GetTable( TokenType.WeaklyReferencedDeclaration, true ),
                        GetTable( declaration.GetTokenType(), false ) );
        }

        #endregion

        #region Typed Get methods

        /// <summary>
        /// Gets the <see cref="IType"/> corresponding to a given <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>An <see cref="IType"/>, or <b>null</b> if the current token was not 
        /// found in the tables.</returns>
        /// <exception cref="ArgumentException">The token type is invalid.</exception>
        public ITypeSignature GetType( MetadataToken token )
        {
            MetadataDeclaration declaration = this.GetDeclaration( token );
            if ( declaration == null )
            {
                return null;
            }
            ITypeSignature type = declaration as ITypeSignature;
            ExceptionHelper.Core.AssertValidArgument( type != null, "token", "InvalidTokenType" );
            return type;
        }

        /// <summary>
        /// Gets the <see cref="IField"/> corresponding to a given <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>An <see cref="IField"/>, or <b>null</b> if the current token was not 
        /// found in the tables.</returns>
        /// <exception cref="ArgumentException">The token type is invalid.</exception>
        public IField GetField( MetadataToken token )
        {
            MetadataDeclaration declaration = this.GetDeclaration( token );
            if ( declaration == null )
            {
                return null;
            }
            IField field = declaration as IField;
            ExceptionHelper.Core.AssertValidArgument( field != null, "token", "InvalidTokenType" );
            return field;
        }

        /// <summary>
        /// Gets the <see cref="IMethod"/> corresponding to a given <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>An <see cref="IMethod"/>, or <b>null</b> if the current token was not 
        /// found in the tables.</returns>
        /// <exception cref="ArgumentException">The token type is invalid.</exception>
        public IMethod GetMethod( MetadataToken token )
        {
            MetadataDeclaration declaration = this.GetDeclaration( token );
            if ( declaration == null )
            {
                return null;
            }
            IMethod method = declaration as IMethod;
            ExceptionHelper.Core.AssertValidArgument( method != null, "token", "InvalidTokenType" );
            return method;
        }

        /// <summary>
        /// Gets the <see cref="MemberRefDeclaration"/> corresponding to a given <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>An <see cref="MemberRefDeclaration"/>, or <b>null</b> if the current token was not 
        /// found in the tables.</returns>
        /// <exception cref="ArgumentException">The token type is invalid.</exception>
        public MemberRefDeclaration GetMemberRef( MetadataToken token )
        {
            MetadataDeclaration declaration = this.GetDeclaration( token );
            if ( declaration == null )
            {
                return null;
            }
            MemberRefDeclaration memberRef = declaration as MemberRefDeclaration;
            ExceptionHelper.Core.AssertValidArgument( memberRef != null, "token", "InvalidTokenType" );
            return memberRef;
        }

        /// <summary>
        /// Gets the <see cref="LiteralString"/> corresponding to a given <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>An <see cref="LiteralString"/>, or a null <see cref="LiteralString"/> if the current token was not 
        /// found in the tables.</returns>
        /// <exception cref="ArgumentException">The token type is invalid.</exception>
        public LiteralString GetUserString( MetadataToken token )
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidArgument( token.TokenType == TokenType.String, "token", "InvalidTokenType" );

            #endregion

            return new LiteralString( this.imageReader.GetUserString( token ) );
        }

        /// <summary>
        /// Gets the <see cref="StandaloneSignatureDeclaration"/> corresponding to a given <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>An <see cref="StandaloneSignatureDeclaration"/>, or <b>null</b> if the current token was not 
        /// found in the tables.</returns>
        /// <exception cref="ArgumentException">The token type is invalid.</exception>
        public StandaloneSignatureDeclaration GetStandaloneSignature( MetadataToken token )
        {
            MetadataDeclaration declaration = this.GetDeclaration( token );
            if ( declaration == null )
            {
                return null;
            }
            StandaloneSignatureDeclaration signature = declaration as StandaloneSignatureDeclaration;
            ExceptionHelper.Core.AssertValidArgument( signature != null, "token", "InvalidTokenType" );
            return signature;
        }

        /// <summary>
        /// Gets the <see cref="LiteralString"/> corresponding to a given <see cref="MetadataToken"/>.
        /// </summary>
        /// <param name="token">A <see cref="MetadataToken"/>.</param>
        /// <returns>An <see cref="LiteralString"/>, or a null <see cref="LiteralString"/> if the current token was not 
        /// found in the tables.</returns>
        /// <exception cref="ArgumentException">The token type is invalid.</exception>
        internal LiteralString GetCustomString( MetadataToken token )
        {
            MetadataDeclaration declaration = this.GetDeclaration( token );
            if ( declaration == null )
            {
                return LiteralString.Null;
            }
            CustomStringDeclaration customString = declaration as CustomStringDeclaration;
            ExceptionHelper.Core.AssertValidArgument( customString != null, "token", "InvalidTokenType" );

            return customString.Value;
        }

        /// <summary>
        /// Gets the physical address corresponding to a given RVA.
        /// </summary>
        /// <param name="rva">A RVA (offset relative to the first byte of the PE image).</param>
        /// <returns>A pointer to the location corresponding to <paramref name="rva"/>.</returns>
        public IntPtr RvaToPointer( uint rva )
        {
            return this.imageReader.RvaToIntPtr( rva );
        }

        #endregion

        /// <summary>
        /// Gets an enumerator of all declarations of a
        /// given <see cref="TokenType"/>.
        /// </summary>
        /// <param name="tokenType">A <see cref="TokenType"/>.</param>
        /// <returns>An enumerator.</returns>
        public IEnumerator<MetadataDeclaration> GetEnumerator( TokenType tokenType )
        {
            Table table = GetTable( tokenType, false );
            if ( table == null )
            {
                return EmptyEnumerator<MetadataDeclaration>.GetInstance();
            }
            else
            {
                return table.GetEnumerator();
            }
        }

        #region Table class

        /// <summary>
        /// Stores declarations of a single type.
        /// </summary>
        private sealed class Table : IEnumerable<MetadataDeclaration>
        {
            private readonly MetadataDeclarationTables parent;

            /// <summary>
            /// List of declarations.
            /// </summary>
            private readonly List<MetadataDeclaration> list;

            /// <summary>
            /// Type of tokens stored in this table.
            /// </summary>
            private readonly TokenType tokenType;


            /// <summary>
            /// Initializes a new <see cref="PostSharp.CodeModel.MetadataDeclarationTables.Table"/>.
            /// </summary>
            /// <param name="parent">Parent.</param>
            /// <param name="list">Initial tabel content, or <b>null</b>
            /// if the table is initially empty.</param>
            /// <param name="tokenType"><see cref="TokenType"/> or declarations
            /// stored in the current table.</param>
            public Table( MetadataDeclarationTables parent, IEnumerable<MetadataDeclaration> list, TokenType tokenType )
            {
                this.parent = parent;

                if ( list != null )
                {
                    this.list = new List<MetadataDeclaration>( list );
                }
                else
                {
                    this.list = new List<MetadataDeclaration>( 32 );
                }

                this.tokenType = tokenType;
            }

            /// <summary>
            /// Gets or sets a <see cref="MetadataDeclaration"/> given its <see cref="MetadataToken"/>.
            /// </summary>
            /// <param name="token">A <see cref="MetadataToken"/> whose
            /// <see cref="TokenType"/> is equal to the token type of the current table.</param>
            /// <returns>The <see cref="MetadataDeclaration"/> whose
            /// token is <paramref name="token"/>.</returns>
            public MetadataDeclaration this[ MetadataToken token ]
            {
                get
                {
                    MetadataDeclaration obj = this.list[token.Index];

                    ExceptionHelper.Core.AssertValidArgument( obj != DeletedMetadataDeclaration.Instance, "token",
                                                              "DeclarationRemoved", token );

                    return obj;
                }

                set { this.list[token.Index] = value; }
            }

            /// <summary>
            /// Adds a declaration to the current table.
            /// </summary>
            /// <param name="declaration">A <see cref="MetadataToken"/> whose
            /// <see cref="TokenType"/> is equal to the token type of the current table.</param>
            /// <returns>The <see cref="MetadataToken"/> assigned to <paramref name="declaration"/>.</returns>
            public MetadataToken Add( MetadataDeclaration declaration )
            {
                declaration.SetMetadataToken( new MetadataToken( this.tokenType, this.list.Count ), false );
                this.list.Add( declaration );
                return declaration.MetadataToken;
            }

            /// <summary>
            /// Removes a declaration from the table.
            /// </summary>
            /// <param name="declaration">A <see cref="MetadataDeclaration"/> belonging
            /// to the current table.</param>
            public void Remove( MetadataDeclaration declaration )
            {
                this.list[declaration.MetadataToken.Index] = DeletedMetadataDeclaration.Instance;
                declaration.MetadataToken = MetadataToken.Null;
            }

            /// <summary>
            /// Gets the size of the table.
            /// </summary>
            public int Size
            {
                get { return this.list.Count; }
            }

            /// <summary>
            /// Moves a <see cref="MetadataDeclaration"/> from a table to another
            /// and reassigns its <see cref="MetadataToken"/>.
            /// </summary>
            /// <param name="declaration">The <see cref="MetadataDeclaration"/> to be
            /// moved.</param>
            /// <param name="oldTable">Original table.</param>
            /// <param name="newTable">New table.</param>
            /// <returns>The new <see cref="MetadataToken"/> of <paramref name="declaration"/>.</returns>
            internal static void Move( MetadataDeclaration declaration,
                                       Table oldTable, Table newTable )
            {
                MetadataToken newToken = new MetadataToken( newTable.tokenType, newTable.list.Count );


                oldTable.list[declaration.MetadataToken.Index] = null;
                declaration.SetMetadataToken( newToken, false );
                newTable.list.Add( declaration );

                return;
            }

            #region IEnumerable<MetadataDeclaration> Members

            /// <inheritdoc />
            public IEnumerator<MetadataDeclaration> GetEnumerator()
            {
                for ( int i = 0; i < this.list.Count; i++ )
                {
                    MetadataDeclaration declaration = this.list[i];
                    if ( declaration != DeletedMetadataDeclaration.Instance )
                    {
                        if ( declaration == null )
                        {
                            declaration = this.parent.moduleReader.ResolveToken( new MetadataToken( this.tokenType, i ) );

#if ASSERT
                            if ( declaration == null )
                                throw new AssertionFailedException(
                                    string.Format( "Could not resolve the token {0} {1}.",
                                                   this.tokenType, i ) );
#endif

                            this.list[i] = declaration;
                        }

                        yield return declaration;
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion

            public override string ToString()
            {
                return "Table " + this.tokenType.ToString();
            }
        }

        #endregion

        private class DeletedMetadataDeclaration : MetadataDeclaration
        {
            public static readonly DeletedMetadataDeclaration Instance = new DeletedMetadataDeclaration();

            public override TokenType GetTokenType()
            {
                throw new NotSupportedException();
            }

            internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments,
                                                               Type[] genericMethodArguments )
            {
                throw new NotSupportedException();
            }

            internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
            {
                throw new NotSupportedException();
            }

            public override string ToString()
            {
                return "<Deleted>";
            }
        }
    }
}