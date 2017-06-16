#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.Data
{

    /// <summary>
    /// A dictionary of entities exposing discovery methods (<b>Find</b>, <b>Exists</b>).
    /// </summary>
    public abstract class EntityRepository
    {
        readonly Dictionary<EntityKey, Entity> entities = new Dictionary<EntityKey, Entity>();


        #region Dictionary update
        protected void InternalSet(Entity entity)
        {
            this.entities[entity.EntityKey] = entity;
        }

        protected void InternalRemove(EntityKey entityKey)
        {
            this.entities.Remove(entityKey);
        }

        protected Entity InternalGet(EntityKey entityKey)
        {
            Entity entity;
            this.entities.TryGetValue(entityKey, out entity);
            return entity;
        }

        protected void InternalClear()
        {
            this.entities.Clear();
        }
        #endregion

        #region Discovery methods

        /// <summary>
        /// Gets all entities contained in this repository.
        /// </summary>
        /// <returns>An enumerator.</returns>
        /// <remarks>
        /// This method can be overridden if 'fall-back' mechanisms should be used 
        /// (when not in this repository, look elsewhere).
        /// </remarks>
        protected virtual IEnumerator<Entity> GetAllEntitiesEnumerator()
        {
            return this.entities.Values.GetEnumerator();
        }


        internal IEnumerator<T> InternalFind<T>(Predicate<Entity> predicate, int max)
           where T : Entity
        {
            IEnumerator<Entity> enumerator = this.GetAllEntitiesEnumerator();
            int i = 0;
            while (enumerator.MoveNext() && (max < 0 || i < max))
            {
                Entity entity = enumerator.Current;

                if (predicate(entity))
                {
                    i++;
                    yield return (T)entity.Clone();
                }
            }
        }

        public IEnumerable<Entity> Find(Predicate<Entity> predicate)
        {
            return this.Find(predicate, -1);
        }

        public IEnumerable<Entity> Find(Predicate<Entity> predicate, int max)
        {
            return new Enumerable<Entity>( InternalFind<Entity>(predicate, max));
        }

        public IEnumerable<T> Find<T>(Predicate<T> predicate)
            where T : Entity
        {
            return this.Find<T>(predicate, -1);
        }

        public IEnumerable<T> Find<T>(Predicate<T> predicate, int max)
            where T : Entity
        {
            return new List<T>(new Enumerable<T>(InternalFind<T>(
               
                delegate(Entity entity)
                {
                    T typedEntity = entity as T;
                    return typedEntity != null && ( predicate == null || predicate(typedEntity) );
                }, max)));
        }

        public bool Exists<T>(Predicate<T> predicate)
            where T : Entity
        {
            IEnumerator<Entity> enumerator = this.GetAllEntitiesEnumerator();
            while (enumerator.MoveNext())
            {
                Entity entity = enumerator.Current;
                T typedEntity = entity as T;
                if (typedEntity != null && (predicate == null || predicate(typedEntity)))
                    return true;
            }

            return false;
        }
        #endregion
    }
}
