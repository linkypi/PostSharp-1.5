using System.Collections.Generic;

namespace PostSharp.Collections
{
    /// <summary>
    /// Exposes the <see cref="Ordinal"/> property.
    /// </summary>
    public interface IPositioned
    {
        /// <summary>
        /// Position of the current item in its parent collection.
        /// </summary>
        int Ordinal { get; }
    }

    internal class OrdinalIndex<T> : Index<int, T>
        where T : class, IPositioned
    {
        public OrdinalIndex( ICollectionFactory<KeyValuePair<int, T>> dictionaryFactory )
            : base( dictionaryFactory )
        {
        }


        protected override int GetItemKey( T value )
        {
            return value.Ordinal;
        }
    }

    internal class OrdinalIndexFactory<T> : IndexFactory<int, T>
        where T : class, IPositioned
    {
        private readonly ICollectionFactory<KeyValuePair<int, T>> dictionaryFactory;

        public OrdinalIndexFactory( ICollectionFactory<KeyValuePair<int, T>> dictionaryFactory )
        {
            this.dictionaryFactory = dictionaryFactory;
        }

        public OrdinalIndexFactory()
            : this( AppendingSortedListFactory<int, T>.Default )
        {
        }

        public override ICollection<T> CreateCollection()
        {
            return new OrdinalIndex<T>( this.dictionaryFactory );
        }
    }
}