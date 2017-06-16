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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Collections;

#endregion

namespace PostSharp.CodeModel.Collections
{
    /// <internal />
    /// <summary>
    /// Collection of elements (<see cref="Element"/>).
    /// </summary>
    /// <typeparam name="ItemType">A type derived from <see cref="Element"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// This collection should be owned by an <see cref="Element"/>. The objective
    /// of the current class is to notify the owner element that a child was added or removed.
    /// In order for the owner element to know to which collection the child belongs,
    /// each collection has a <i>role</i> in the owner collection.
    /// </para>
    /// <para>
    /// This class maintains the parent-child relationship.
    /// </para>
    /// <para>
    /// In order to save memory, the underlying collection is created only on demand.
    /// Empty collections should not have underlying collection.
    /// </para>
    /// </remarks>
    public abstract class ElementCollection<ItemType> : ICollection<ItemType>, ICollection, IDisposable
        where ItemType : Element
    {
        #region Fields

        /// <summary>
        /// Collection implementation.
        /// </summary>
        private ICollection<ItemType> inner;

        /// <summary>
        /// <see cref="Element"/> owning the collection (null if disposed).
        /// </summary>
        private Element parent;

        /// <summary>
        /// Role of the collection in the parent element.
        /// </summary>
        private readonly string role;

        private LazyLoadingState lazyLoadingState;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementCollection{ItemType}"/>
        /// type.
        /// </summary>
        /// <param name="parent">Element to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal ElementCollection( Element parent, string role )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( parent, "parent" );

            #endregion

            this.parent = parent;
            this.role = role;
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            if ( this.IsLazyLoadingSupported )
// ReSharper restore DoNotCallOverridableMethodsInConstructor
            {
                this.lazyLoadingState = LazyLoadingState.Dirty;
            }
        }

        /// <summary>
        /// Gets the underlying collection implementation.
        /// </summary>
        internal ICollection<ItemType> Implementation
        {
            get
            {
                this.CheckState();
                return this.inner;
            }
        }

        /// <summary>
        /// Gets the underlying collection factory.
        /// </summary>
        /// <returns></returns>
        internal abstract ICollectionFactory<ItemType> GetCollectionFactory();

        internal void PrepareImport( int count )
        {
            this.EnsureCapacity( count );
            this.lazyLoadingState = LazyLoadingState.Loaded;
        }

        /// <summary>
        /// Allocates additional capacity in the collection.
        /// </summary>
        /// <param name="capacity">Additional capacity.</param>
        public void EnsureCapacity( int capacity )
        {
            #region Preconditions

#if ASSERT
            if ( capacity < 0 )
            {
                throw new ArgumentOutOfRangeException( "capacity" );
            }
#endif

            #endregion

            if ( capacity == 0 )
                return;

            if ( this.inner == null )
            {
                this.inner = this.GetCollectionFactory().CreateCollection( capacity );
            }
            else
            {
                this.GetCollectionFactory().EnsureCapacity( this.inner, capacity );
            }
        }

        #endregion

        #region Feedback

        /// <summary>
        /// Called before adding an item to the collection.
        /// </summary>
        /// <param name="item"></param>
        private void OnItemAdding( Element item )
        {
            if ( this.parent != null )
            {
#if ASSERT
                if ( item.Parent != null )
                {
                    throw new ArgumentOutOfRangeException( "item" );
                }
#endif

                item.OnAddingToParent( this.parent, this.role );
            }
        }

        /// <summary>
        /// Called before removing an item from the collection.
        /// </summary>
        /// <param name="item"></param>
        private void OnItemRemoved( Element item )
        {
#if ASSERT
            if ( item.Parent != this.parent )
            {
                throw new ArgumentOutOfRangeException();
            }
#endif

            item.OnRemovingFromParent();
        }

        #endregion

        #region ICollection<ItemType> Members

        /// <inheritdoc />
        public void Add( ItemType item )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            ExceptionHelper.Core.AssertValidOperation( item.Parent == null, "ElementHasAlreadyParent" );

            #endregion

            this.CheckState();

            if ( this.inner == null )
            {
                this.EnsureCapacity( 4 );
            }

            this.OnItemAdding( item );
            this.inner.Add( item );
        }

        /// <summary>
        /// Adds a collection of items to the current collection.
        /// </summary>
        /// <param name="items">The collection of items to be added.</param>
        public void AddRange( ICollection<ItemType> items )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( items, "items" );

            #endregion

            this.CheckState();

            if ( items.Count > 0 )
            {
                if ( this.inner == null )
                {
                    this.EnsureCapacity( items.Count );
                }

                foreach ( ItemType item in items )
                {
                    this.Add( item );
                }
            }
        }

        /// <summary>
        /// Adds <i>clones</i> of items of a given collection to the current collection.
        /// </summary>
        /// <param name="items">The collection of items to be added.</param>
        /// <remarks>
        /// This method requires <typeparamref name="ItemType"/> to implement the
        /// <see cref="ICloneable"/> interface. Clones should be themselves
        /// of the <typeparamref name="ItemType"/> type.
        /// </remarks>
        public void AddCloneRange( ICollection<ItemType> items )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( items, "items" );

            #endregion

            this.CheckState();


            if ( items.Count > 0 )
            {
                if ( this.inner == null )
                {
                    this.EnsureCapacity( items.Count );
                }

                foreach ( ICloneable item in items )
                {
                    this.Add( (ItemType) item.Clone() );
                }
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.CheckState();

            if ( this.inner != null )
            {
                // Copy the collection to a temporary list.
                List<ItemType> unrootedElements = new List<ItemType>( this.inner.Count );
                unrootedElements.AddRange( this.inner );

                // Clear the collection. 
                this.inner.Clear();

                // Invoke callback methods.
                foreach ( ItemType item in unrootedElements )
                {
                    this.OnItemRemoved( item );
                }
            }
        }

        /// <inheritdoc />
        public bool Contains( ItemType item )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( item, "item" );

            #endregion

            this.CheckState();


            if ( this.inner == null )
            {
                return false;
            }
            return this.inner.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( ItemType[] array, int arrayIndex )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( array, "array" );

#if ASSERT
            if ( arrayIndex < 0 )
            {
                throw new ArgumentOutOfRangeException( "arrayIndex",
                                                       "The parameter 'arrayIndex' should be zero or positive." );
            }
#endif
            this.CheckState();

            #endregion

            if ( this.inner == null )
            {
                return;
            }

            this.inner.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                this.CheckState();
                return this.inner == null ? 0 : this.inner.Count;
            }
        }

        /// <inheritdoc />
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool Remove( ItemType item )
        {
            this.CheckState();

            if ( this.inner == null )
            {
                return false;
            }

            if ( this.inner.Contains( item ) )
            {
                this.inner.Remove( item );
                this.OnItemRemoved( item );
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable<ItemType> Members

        /// <inheritdoc />
        public IEnumerator<ItemType> GetEnumerator()
        {
            this.CheckState();

            if ( this.inner == null )
            {
                return EmptyEnumerator<ItemType>.GetInstance();
            }
            else
            {
                return this.inner.GetEnumerator();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IElementCollection Members

        /// <summary>
        /// Gets the owner <see cref="Element"/>.
        /// </summary>
        public Element Owner
        {
            get { return this.parent; }
        }

        /// <summary>
        /// Gets the role of the current collection in the owner element.
        /// </summary>
        public string Role
        {
            get { return this.role; }
        }

        #endregion

        #region ICollection Members

        /// <inheritdoc />
        void ICollection.CopyTo( Array array, int index )
        {
            this.CopyTo( (ItemType[]) array, index );
        }

        [SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        [SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
        object ICollection.SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Determines whether the current collection has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.parent == null; }
        }

        private void CheckState()
        {
            if ( this.lazyLoadingState != LazyLoadingState.Loaded )
            {
                // If the declaration is new (i.e. it is not imported from the module), 
                // we don't do lazy loading.
                if ( !this.Owner.IsImported )
                {
                    this.lazyLoadingState = LazyLoadingState.Loaded;
                }
                else
                {
#if ASSERT
                    if ( this.lazyLoadingState == LazyLoadingState.Loading )
                        throw new AssertionFailedException( "Recursion in lazy loading." );
                    this.lazyLoadingState = LazyLoadingState.Loading;
#endif

                    this.DoLazyLoading();
#if ASSERT
                    if ( this.lazyLoadingState != LazyLoadingState.Loaded )
                        throw new AssertionFailedException(
                            string.Format( "LazyLoadingState is expected to be Loaded (collection {0}).", this.GetType() ) );
#endif
                }
            }

#if ASSERT
            if ( this.parent == null )
            {
                throw new ObjectDisposedException( this.GetType().FullName );
            }
#endif
        }

        /// <summary>
        /// Determines if the current collection supports lazy loading.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a collection supports lazy loading, the method <see cref="DoLazyLoading"/>
        /// should be implemented; it will be called before any access to the collection.
        /// </para>
        /// <para>
        /// This property is evaluated <i>before</i> the collection constructor is invoked.
        /// Therefore, it is considered as static for the collection class.
        /// </para>
        /// </remarks>
        protected abstract bool IsLazyLoadingSupported { get; }

        /// <summary>
        /// Loads the content of the collection.
        /// </summary>
        protected abstract void DoLazyLoading();

        /// <summary>
        /// Adds an item to the current collection and does not
        /// cause lazy loading to be triggered.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        internal void Import( ItemType item )
        {
            if ( this.inner == null )
            {
                this.EnsureCapacity( 4 );
            }

            this.OnItemAdding( item );
            this.inner.Add( item );
        }


        /// <summary>
        /// Disposes the resources held by the current instance.
        /// </summary>
        [SuppressMessage( "Microsoft.Design", "CA1063:ImplementIDisposableCorrectly" )]
        public void Dispose()
        {
            if ( this.parent != null )
            {
                IDisposable innerDisposable = this.inner as IDisposable;
                if ( innerDisposable != null )
                {
                    innerDisposable.Dispose();
                }
                this.inner = null;
                this.parent = null;
            }
        }

        #endregion

        /// <summary>
        /// Converts the current collection into a typed array.
        /// </summary>
        /// <returns>An array containing all elements of the current collection.</returns>
        public ItemType[] ToArray()
        {
            this.CheckState();

            int n = this.Count;
            if ( n > 0 )
            {
                ItemType[] array = new ItemType[n];
                int i = 0;
                foreach ( ItemType item in this.inner )
                {
                    array[i] = item;
                    i++;
                }
                return array;
            }
            else
            {
                return EmptyArray<ItemType>.GetInstance();
            }
        }

        /// <summary>
        /// Clear the cache (typically mapping to <b>System.Reflection</b> or,
        /// if the current element is a reference, to the related definition) 
        /// of all items of this collection.
        /// </summary>
        public void ClearCache()
        {
            foreach ( ItemType item in this )
            {
                item.ClearCache();
            }
        }
    }

    internal enum LazyLoadingState
    {
        /// <summary>
        /// Fully loaded.
        /// </summary>
        Loaded,


        Loading,

        /// <summary>
        /// Something needs to be loaded and it is possible.
        /// </summary>
        Dirty,
    }
    /// <internal />
    /// <summary>
    /// Simple collection of elements without any indexing or ordering.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public abstract class SimpleElementCollection<ItemType> : ElementCollection<ItemType>
        where ItemType : Element
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleElementCollection{ItemType}"/>
        /// type with zero initial capacity.
        /// </summary>
        /// <param name="parent">Element to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal SimpleElementCollection( Declaration parent, string role )
            :
                base( parent, role )
        {
        }

        internal override ICollectionFactory<ItemType> GetCollectionFactory()
        {
            return ListFactory<ItemType>.Default;
        }

        /// <summary>
        /// Gets the item at a given index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>The item at position <paramref name="index"/>.</returns>
        public ItemType this[ int index ]
        {
            get { return ( (List<ItemType>) this.Implementation )[index]; }
        }
    }


    /// <summary>
    /// Base class for collections of elements having a name, with an index on the <see cref="INamed.Name"/> property.
    /// </summary>
    /// <typeparam name="ItemType">Type of collection items.</typeparam>
    public abstract class NamedElementCollection<ItemType> : ElementCollection<ItemType>
        where ItemType : Element, INamed
    {
        internal NamedElementCollection( Element parent, string role ) : base( parent, role )
        {
        }

        /// <summary>
        /// Gets the underlying implementation (an <see cref="Index{K,V}"/>).
        /// </summary>
        protected internal new Index<string, ItemType> Implementation
        {
            get { return (Index<string, ItemType>) base.Implementation; }
        }

        internal void OnItemNameChanged( ItemType item, string oldName )
        {
            this.Implementation.Remove( item, oldName );
            this.Implementation.Add( item );
        }
    }

    /// <internal />
    /// <summary>
    /// Collection of elements indexed by name, supporting many elements
    /// of the same name.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public abstract class NonUniquelyNamedElementCollection<ItemType> : NamedElementCollection<ItemType>
        where ItemType : Element, INamed
    {
        internal static readonly ICollectionFactory<ItemType> collectionFactory = new NameIndexFactory<ItemType>( false );


        /// <summary>
        /// Initializes a new instance of the <see cref="NonUniquelyNamedElementCollection{ItemType}"/>
        /// type with zero initial capacity.
        /// </summary>
        /// <param name="parent">Element to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal NonUniquelyNamedElementCollection( Element parent, string role ) :
            base( parent, role )
        {
        }

        internal override ICollectionFactory<ItemType> GetCollectionFactory()
        {
            return collectionFactory;
        }

        /// <summary>
        /// Gets the collection of elements given a name.
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <returns>A collection of element named <paramref name="name"/>.
        /// If no element is found, an empty collection is returned.</returns>
        public IEnumerable<ItemType> GetByName( string name )
        {
            if ( this.Implementation == null )
            {
                return EmptyCollection<ItemType>.GetInstance();
            }
            else
            {
                return this.Implementation.GetValuesByKey( name );
            }
        }


        /// <summary>
        /// Gets arbitrarly one declaration among the ones having a given name.
        /// </summary>
        /// <param name="name">Name of the declaration.</param>
        /// <returns>A declaration named <paramref name="name"/> (arbitrarly any of all named
        /// <paramref name="name"/> if there are many), or <b>null</b> if no declaration
        /// is named <paramref name="name"/>.</returns>
        public ItemType GetOneByName( string name )
        {
            Index<string, ItemType> implementation = this.Implementation;

            if ( implementation == null )
            {
                return null;
            }
            else
            {
                ItemType item;
                implementation.TryGetFirstValueByKey( name, out item );
                return item;
            }
        }
    }

    /// <internal />
    /// <summary>
    /// Collection of declarations indexed by name, supporting only single declaration
    /// per name.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public abstract class UniquelyNamedElementCollection<ItemType> : NamedElementCollection<ItemType>
        where ItemType : Element, INamed
    {
        internal static readonly ICollectionFactory<ItemType> nameIndexFactory =
            new NameIndexFactory<ItemType>( true );


        /// <summary>
        /// Initializes a new instance of the <see cref="UniquelyNamedElementCollection{ItemType}"/>
        /// type with zero initial capacity.
        /// </summary>
        /// <param name="parent">Element to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal UniquelyNamedElementCollection( Element parent, string role )
            : base( parent, role )
        {
        }


        internal override ICollectionFactory<ItemType> GetCollectionFactory()
        {
            return nameIndexFactory;
        }


        /// <summary>
        /// Gets the element given its name.
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <returns>The elementy named <paramref name="name"/>, or
        /// <b>null</b> if no element matches the current name.</returns>
        public ItemType GetByName( string name )
        {
            Index<string, ItemType> implementation = this.Implementation;
            if ( implementation == null )
            {
                return null;
            }
            else
            {
                return implementation.GetFirstValueByKey( name );
            }
        }
    }


    internal class MetadataTokenIndex<ItemType> : Index<MetadataToken, ItemType>
        where ItemType : MetadataDeclaration
    {
        public MetadataTokenIndex()
            : base( AppendingSortedListFactory<MetadataToken, ItemType>.Default )
        {
        }

        protected override MetadataToken GetItemKey( ItemType value )
        {
            return value.MetadataToken;
        }
    }

    internal class MetadataTokenIndexFactory<ItemType> : IndexFactory<MetadataToken, ItemType>
        where ItemType : MetadataDeclaration
    {
        public override ICollection<ItemType> CreateCollection()
        {
            return new MetadataTokenIndex<ItemType>();
        }
    }

    /// <internal />
    /// <summary>
    /// Collection of elements order by emit order (i.e. by metadata token).
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public abstract class OrderedEmitDeclarationCollection<ItemType> :
        ElementCollection<ItemType>
        where ItemType : MetadataDeclaration
    {
        private static readonly OrderedEmitDeclarationCollectionImplFactory factory =
            new OrderedEmitDeclarationCollectionImplFactory();


        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedEmitDeclarationCollection{T}"/>
        /// type with zero initial capacity.
        /// </summary>
        /// <param name="parent">Declaration to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal OrderedEmitDeclarationCollection( Declaration parent, string role )
            : base( parent, role )
        {
        }

        internal override ICollectionFactory<ItemType> GetCollectionFactory()
        {
            if (this.RequiresEmitOrdering)
            {
                return factory;
            }
            else
            {
                return ListFactory<ItemType>.Default;
            }
        }

        /// <summary>
        /// Determines whether the current collection requires to be sorted by emission order, so
        /// the collection can be written in the original order.
        /// </summary>
        /// <remarks>
        /// Order of declaration emission is not important for most collections (expect <see cref="FieldDefDeclarationCollection"/>
        /// and <see cref="MethodDefDeclaration"/>) in P-Invoke scenarios). However, in debug builds, most collections
        /// respect the original emission order so that it is easier to compare the enhanced assembly with the
        /// original assembly. Debug and release builds typically returns a different value. For a given class,
        /// this property returns always the same value.
        /// </remarks>
        protected abstract bool RequiresEmitOrdering { get;  }

        /// <summary>
        /// Gets all declarations ordered by emit order (i.e. metadata token).
        /// </summary>
        /// <returns>All declarations ordered by emit order (i.e. metadata token).</returns>
        [SuppressMessage( "Microsoft.Design",
            "CA1024", Justification = "This is conceptually not a property." )]
        public IEnumerable<ItemType> GetByEmitOrder()
        {
            if (this.RequiresEmitOrdering)
            {
                OrderedEmitDeclarationCollectionImpl implementation =
                    (OrderedEmitDeclarationCollectionImpl) this.Implementation;

                if (implementation == null)
                {
                    return EmptyCollection<ItemType>.GetInstance();
                }
                else
                {
                    return implementation.GetByEmitOrder();
                }
            }
            else
            {
                return this.Implementation;
            }
        }


        internal class OrderedEmitDeclarationCollectionImpl : IndexedCollection<ItemType>
        {
            protected static readonly MetadataTokenIndexFactory<ItemType> MetadataTokenIndexFactory =
                new MetadataTokenIndexFactory<ItemType>();

            public OrderedEmitDeclarationCollectionImpl( int capacity )
                : base( new ICollectionFactory<ItemType>[] {MetadataTokenIndexFactory}, capacity )
            {
            }

            protected OrderedEmitDeclarationCollectionImpl( ICollectionFactory<ItemType>[] indexFactories, int capacity )
                : base( indexFactories, capacity )
            {
            }

            public IEnumerable<ItemType> GetByEmitOrder()
            {
                return this.GetOrderedValues( 0 );
            }
        }

        private class OrderedEmitDeclarationCollectionImplFactory : IndexedCollectionFactory<ItemType>
        {
            protected override IndexedCollection<ItemType> CreateCollection( int capacity )
            {
                return new OrderedEmitDeclarationCollectionImpl( capacity );
            }
        }
    }


    /// <internal />
    /// <summary>
    /// Collections of declarations with an index per emit order and a non-unique index
    /// on name.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public abstract class OrderedEmitAndByNonUniqueNameDeclarationCollection<ItemType> :
        OrderedEmitDeclarationCollection<ItemType>
        where ItemType : NamedDeclaration
    {
        private static readonly CollectionImplFactory orderedEmitFactory = new CollectionImplFactory(true);
        private static readonly CollectionImplFactory unorderedEmitFactory = new CollectionImplFactory(false);


        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedEmitAndByNonUniqueNameDeclarationCollection{T}"/>
        /// type with zero initial capacity.
        /// </summary>
        /// <param name="parent">Declaration to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal OrderedEmitAndByNonUniqueNameDeclarationCollection( Declaration parent, string role )
            : base( parent, role )
        {
        }

        internal override ICollectionFactory<ItemType> GetCollectionFactory()
        {
            return this.RequiresEmitOrdering ? orderedEmitFactory : unorderedEmitFactory;
        }


        /// <summary>
        /// Gets the collection of declarations given a name.
        /// </summary>
        /// <param name="name">Name of the declaration.</param>
        /// <returns>A collection of declaration named <paramref name="name"/>.
        /// If no declaration is found, an empty collection is returned.</returns>
        public IEnumerable<ItemType> GetByName( string name )
        {
            CollectionImpl implementation = (CollectionImpl) this.Implementation;

            if ( implementation == null )
            {
                return EmptyCollection<ItemType>.GetInstance();
            }
            else
            {
                return implementation.GetByName( name );
            }
        }

        /// <summary>
        /// Gets arbitrarly one declaration among the ones having a given name.
        /// </summary>
        /// <param name="name">Name of the declaration.</param>
        /// <returns>A declaration named <paramref name="name"/> (arbitrarly any of all named
        /// <paramref name="name"/> if there are many), or <b>null</b> if no declaration
        /// is named <paramref name="name"/>.</returns>
        public ItemType GetOneByName( string name )
        {
            CollectionImpl implementation = (CollectionImpl) this.Implementation;

            if ( implementation == null )
            {
                return null;
            }
            else
            {
                return implementation.GetOneByName( name );
            }
        }

        internal void OnItemNameChanged( ItemType item, string oldName )
        {
            ( (CollectionImpl) this.Implementation ).OnItemNameChanged( item, oldName );
        }

        private class CollectionImpl : OrderedEmitDeclarationCollectionImpl
        {
            private static readonly ICollectionFactory<ItemType>[] orderedEmitIndexFactories =
                new ICollectionFactory<ItemType>[] {MetadataTokenIndexFactory, new NameIndexFactory<ItemType>( false )};

            private static readonly ICollectionFactory<ItemType>[] unorderedEmitIndexFactories =
                new ICollectionFactory<ItemType>[] { ListFactory<ItemType>.Default, new NameIndexFactory<ItemType>(false) };


            public CollectionImpl( int capacity, bool orderedEmit ) :
                base( orderedEmit ? orderedEmitIndexFactories : unorderedEmitIndexFactories, capacity )
            {
            }

            public ItemType GetOneByName( string name )
            {
                ItemType item;
                this.TryGetFirstValueByKey( 1, name, out item );
                return item;
            }

            public IEnumerable<ItemType> GetByName( string name )
            {
                return this.GetValuesByKey( 1, name );
            }

            internal void OnItemNameChanged( ItemType item, string oldName )
            {
                this.OnItemPropertyChanged( 1, item, oldName );
            }
        }

        private class CollectionImplFactory : IndexedCollectionFactory<ItemType>
        {
            private readonly bool orderedEmit;

            public CollectionImplFactory(bool orderedEmit)
            {
                this.orderedEmit = orderedEmit;
            }

            protected override IndexedCollection<ItemType> CreateCollection( int capacity )
            {
                return new CollectionImpl( capacity, this.orderedEmit );
            }
        }
    }

    /// <internal />
    /// <summary>
    /// Collections of declarations with an index per emit order and a non-unique index
    /// on name.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public abstract class OrderedEmitAndByUniqueNameDeclarationCollection<ItemType> :
        OrderedEmitDeclarationCollection<ItemType>
        where ItemType : NamedDeclaration
    {
        private static readonly CollectionImplFactory orderedEmitFactory = new CollectionImplFactory(true);
        private static readonly CollectionImplFactory unorderedEmitFactory = new CollectionImplFactory(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedEmitAndByUniqueNameDeclarationCollection{T}"/>
        /// type with zero initial capacity.
        /// </summary>
        /// <param name="parent">Declaration to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal OrderedEmitAndByUniqueNameDeclarationCollection( Declaration parent, string role )
            : base( parent, role )
        {
        }

        internal override ICollectionFactory<ItemType> GetCollectionFactory()
        {
            return this.RequiresEmitOrdering ? orderedEmitFactory : unorderedEmitFactory;
        }


        /// <summary>
        /// Gets the declaration given its name.
        /// </summary>
        /// <param name="name">Name of the declaration.</param>
        /// <returns>The declaration named <paramref name="name"/>, or
        /// <b>null</b> if no declaration matches the current name.</returns>
        public ItemType GetByName( string name )
        {
            CollectionImpl implementation = (CollectionImpl) this.Implementation;

            if ( implementation == null )
            {
                return null;
            }
            else
            {
                return implementation.GetByName( name );
            }
        }

        internal void OnItemNameChanged( ItemType item, string oldName )
        {
            ( (CollectionImpl) this.Implementation ).OnItemNameChanged( item, oldName );
        }


        private class CollectionImpl : OrderedEmitDeclarationCollectionImpl
        {
            private static readonly ICollectionFactory<ItemType>[] orderedEmitIndexFactories =
                new ICollectionFactory<ItemType>[] {MetadataTokenIndexFactory, new NameIndexFactory<ItemType>( true )};

            private static readonly ICollectionFactory<ItemType>[] unorderedEmitIndexFactories =
                new ICollectionFactory<ItemType>[] { ListFactory<ItemType>.Default, new NameIndexFactory<ItemType>(true) };

            public CollectionImpl( int capacity, bool orderedEmit ) :
                base(orderedEmit ? orderedEmitIndexFactories : unorderedEmitIndexFactories, capacity)
            {
            }


            public ItemType GetByName( string name )
            {
                ItemType value;
                this.TryGetFirstValueByKey( 1, name, out value );
                return value;
            }


            internal void OnItemNameChanged( ItemType item, string oldName )
            {
                this.OnItemPropertyChanged( 1, item, oldName );
            }
        }

        private class CollectionImplFactory : IndexedCollectionFactory<ItemType>
        {
            private readonly bool orderedEmit;

            public CollectionImplFactory(bool orderedEmit)
            {
                this.orderedEmit = orderedEmit;
            }

            protected override IndexedCollection<ItemType> CreateCollection( int capacity )
            {
                return new CollectionImpl( capacity, this.orderedEmit );
            }
        }
    }

    /// <internal />
    /// <summary>
    /// Collection of declarations indexed by ordinal, implementing <see cref="IList{ItemType}"/>.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public abstract class OrdinalDeclarationCollection<ItemType> :
        ElementCollection<ItemType>, IList<ItemType>
        where ItemType : Declaration, IPositioned
    {
        private static readonly IndexFactory<int, ItemType> ordinalIndexFactory = new OrdinalIndexFactory<ItemType>();


        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalDeclarationCollection{T}"/>
        /// type with zero initial capacity.
        /// </summary>
        /// <param name="parent">Declaration to which the new collection will belong.</param>
        /// <param name="role">Role of the new collection in its parent.</param>
        internal OrdinalDeclarationCollection( Declaration parent, string role )
            : base( parent, role )
        {
        }

        /// <summary>
        /// Gets the underlying implementation (an <see cref="Index{K,V}"/>).
        /// </summary>
        protected internal new Index<int, ItemType> Implementation
        {
            get { return (Index<int, ItemType>) base.Implementation; }
        }

        internal override sealed ICollectionFactory<ItemType> GetCollectionFactory()
        {
            return ordinalIndexFactory;
        }

        /// <summary>
        /// Gets the declaration given its ordinal.
        /// </summary>
        /// <param name="index">Declaration ordinal.</param>
        /// <returns>The declaration whose ordinal is <paramref name="index"/>,
        /// or <b>null</b> the collection does not contain such declaration.</returns>
        public ItemType this[ int index ]
        {
            [SuppressMessage( "Microsoft.Naming", "CA172:ParameterNamesShouldMatchBaseDeclaration" )]
            get
            {
                Index<int, ItemType> implementation = this.Implementation;

                if ( implementation == null )
                {
                    return null;
                }
                else
                {
                    return implementation.GetFirstValueByKey( index );
                }
            }

            [SuppressMessage( "Microsoft.Naming", "CA172:ParameterNamesShouldMatchBaseDeclaration" )]
            set { throw new NotSupportedException(); }
        }

        internal void OnItemOrdinalChanged( ItemType item, int oldOrdinal )
        {
            this.Implementation.Remove( item, oldOrdinal );
            this.Implementation.Add( item );
        }

        #region IList<ItemType> Members

        /// <notSupported />
        int IList<ItemType>.IndexOf( ItemType item )
        {
            throw new NotSupportedException();
        }

        /// <notSupported />
        void IList<ItemType>.Insert( int index, ItemType item )
        {
            throw new NotSupportedException();
        }

        /// <notSupported />
        void IList<ItemType>.RemoveAt( int index )
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}