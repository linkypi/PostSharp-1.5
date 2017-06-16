using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using PostSharp.CodeModel.Collections;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Post-compilation phase, which divides sequentially the post-compilation process
    /// and to which tasks belong.
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class PhaseConfiguration : ConfigurationElement, INamed, IPositioned
    {
        /// <summary>
        /// Gets or sets the phase ordinal.
        /// </summary>
        [XmlAttribute( "Ordinal" )]
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the phase name.
        /// </summary>
        [XmlAttribute( "Name" )]
        public string Name { get; set; }
    }

    /// <summary>
    /// Collection of phases (<see cref="PhaseConfiguration"/>).
    /// </summary>
    [Serializable]
    [XmlType( AnonymousType = true )]
    public sealed class PhaseConfigurationCollection : MarshalByRefList<PhaseConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="PhaseConfigurationCollection"/>.
        /// </summary>
        public PhaseConfigurationCollection()
        {
        }
    }

    /// <summary>
    /// Collection of phases ordered by ordinal and accessible by name.
    /// </summary>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    [SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
    public sealed class PhaseConfigurationDictionary : IndexedCollection<PhaseConfiguration>
    {
        private static readonly ICollectionFactory<PhaseConfiguration>[] indexFactories =
            new ICollectionFactory<PhaseConfiguration>[]
                {
                    new NameIndexFactory<PhaseConfiguration>(true, StringComparer.InvariantCultureIgnoreCase),
                    new OrdinalIndexFactory<PhaseConfiguration>(SortedDictionaryFactory<int, PhaseConfiguration>.Default )
                };



        /// <summary>
        /// Initializes a new <see cref="PhaseConfigurationDictionary"/>.
        /// </summary>
        internal PhaseConfigurationDictionary() : base(indexFactories, 8)
        {
        }



        /// <summary>
        /// Gets a phase given its name.
        /// </summary>
        /// <param name="name">Phase name.</param>
        /// <returns>The phase named <paramref name="name"/>, or <b>null</b> if no
        /// phase was found.</returns>
        public PhaseConfiguration this[ string name ]
        {
            get
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotEmptyOrNull( name, "name" );

                #endregion

                PhaseConfiguration phase;
                this.TryGetFirstValueByKey( 0, name, out phase );
                return phase;
            }
        }

        /// <summary>
        /// Determines whether there exists a phase of a given name.
        /// </summary>
        /// <param name="name">Phase name.</param>
        /// <returns><b>true</b> if there exists a phase named <paramref name="name"/>,
        /// <value>false</value> otherwise.</returns>
        public bool Contains( string name )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( name, "name" );

            #endregion

            return this[name] != null;
        }

    }
}
