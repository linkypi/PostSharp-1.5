using System.Collections.Generic;

namespace PostSharp.Collections
{
    /// <summary>
    /// Exposes a <see cref="Name"/> property.
    /// </summary>
    public interface INamed
    {
        /// <summary>
        /// Gets the name of the current object.
        /// </summary>
        string Name { get; }
    }

    internal class NameIndex<T> : Index<string, T> where T : class, INamed
    {
        public NameIndex( ICollectionFactory<KeyValuePair<string, T>> collectionFactory )
            : base( collectionFactory )
        {
        }


        protected override string GetItemKey( T value )
        {
            return value.Name;
        }
    }

    internal class NameIndexFactory<T> : IndexFactory<string, T> where T : class, INamed
    {
        private readonly ICollectionFactory<KeyValuePair<string, T>> collectionFactory;

        public NameIndexFactory( bool unique )
            : this( unique, FastStringComparer.Instance )
        {
        }

        public NameIndexFactory( bool unique, IEqualityComparer<string> comparer )
            : this(
                unique
                    ? (ICollectionFactory<KeyValuePair<string, T>>) DictionaryFactory<string, T>.Default
                    :
                        new MultiDictionaryFactory<string, T>( comparer ) )
        {
        }

        public NameIndexFactory( ICollectionFactory<KeyValuePair<string, T>> collectionFactory )
        {
            this.collectionFactory = collectionFactory;
        }

        public override ICollection<T> CreateCollection()
        {
            return new NameIndex<T>( collectionFactory );
        }
    }
}