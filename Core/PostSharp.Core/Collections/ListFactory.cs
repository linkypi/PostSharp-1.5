using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Factory of <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of list items.</typeparam>
    public sealed class ListFactory<T> : IListFactory<T>, ICollectionFactory<T>
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public static readonly ListFactory<T> Default = new ListFactory<T>();

        private ListFactory()
        {
        }

        #region IListFactory<T> Members

        /// <inheritdoc />
        public IList<T> CreateList()
        {
            return new List<T>();
        }

        /// <inheritdoc />
        public IList<T> CreateList( int capacity )
        {
            return new List<T>( capacity );
        }

        /// <inheritdoc />
        public void EnsureCapacity( IList<T> list, int capacity )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( list, "list" );

            #endregion

            ( (List<T>) list ).Capacity = capacity;
        }

        #endregion

        #region ICollectionFactory<T> Members

        /// <inheritdoc />
        public ICollection<T> CreateCollection()
        {
            return this.CreateList();
        }

        /// <inheritdoc />
        public ICollection<T> CreateCollection( int capacity )
        {
            return this.CreateList( capacity );
        }

        /// <inheritdoc />
        public void EnsureCapacity( ICollection<T> collection, int capacity )
        {
            this.EnsureCapacity( (IList<T>) collection, capacity );
        }

        #endregion
    }
}