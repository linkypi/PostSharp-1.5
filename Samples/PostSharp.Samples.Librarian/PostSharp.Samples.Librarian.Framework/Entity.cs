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

namespace PostSharp.Samples.Librarian.Framework
{
    /// <summary>
    /// Base for all entities.
    /// </summary>
    /// <remarks>
    /// An entity is the root of an object tree (not graph!) that can
    /// be stored atomically in database.
    /// </remarks>
    /// <note>Modifying an entity member does not directly modify the
    /// entity in database. All changes should go through the
    /// process layer.</note>
    [Serializable]
    public abstract class Entity : Aspectable
    {
        private EntityKey entityKey;

        /// <summary>
        /// Initializes a new <see cref="Entity"/>.
        /// </summary>
        protected Entity()
        {
        }

        /// <summary>
        /// Gets or sets the entity primary key.
        /// </summary>
        public EntityKey EntityKey { get { return this.entityKey; } set { this.entityKey = value; } }

        /// <summary>
        /// Clones the current object.
        /// </summary>
        /// <returns>A deep clone of the current object.</returns>
        public new Entity Clone()
        {
            return (Entity) base.Clone();
        }

        /// <summary>
        /// Gets a 'fresh' or 'vanilla' copy of the current object directly
        /// from the database.
        /// </summary>
        /// <returns>A 'fresh' or 'vanilla' copy of the current object directly
        /// from the database (with the modifications done by the current
        /// transaction, but without the changes that would have been applied
        /// to the current instance and not submitted to the database).</returns>
        public Entity GetVanilla()
        {
            return EntityResolver.GetEntity( this.entityKey );
        }

        /// <summary>
        /// Gets a string representing the current object.
        /// </summary>
        /// <returns>A string, usually useful during debugging or tracing.</returns>
        public override string ToString()
        {
            return this.GetType().Name + " " + this.entityKey.ToString();
        }

        #region Resolving

        private static IEntityResolver entityResolver;

        public static IEntityResolver EntityResolver { get { return entityResolver; } set { entityResolver = value; } }

        #endregion
    }
}