using System.ComponentModel;

namespace PostSharp.CodeModel
{
    /// <summary>Represents an element of a tree structure.</summary>
    /// <remarks>
    /// <para>
    /// The CLI module is basically represented as a tree of objects what are called
    /// <see cref="Declaration"/> in PostSharp. The root of the module tree is the
    /// <see cref="ModuleDeclaration"/>. Modules are contained in assemblies (<see cref="AssemblyEnvelope"/>)
    /// and assemblies are bounded in the scope of a specified <see cref="Domain"/>.
    /// </para>
    /// <para>
    /// All elements that are a part of a <see cref="Domain"/> are represented by the <see cref="Element"/> type. 
    /// This class is basically a tree node.
    /// <see cref="Element"/> instances have two properties: <see cref="Element.Parent"/>
    /// gives the direct parent, and <see cref="Element.Domain"/> gives the root
    /// <see cref="Domain"/>.
    /// </para>
    /// <para>
    /// The child-parent relationship is managed automatically when the child is related to the parent either adding it 
    /// into a collection of children, either setting it as the value of a property of the parent.
    /// </para>
    /// </remarks>
    public abstract class Element
    {
        /// <summary>
        /// Parent <see cref="Element"/>.
        /// </summary>
        /// <remarks>
        /// An <see cref="Element"/>, or <b>null</b> if the declaration is
        /// either a <see cref="ModuleDeclaration"/> either a stand-alone declaration.
        /// </remarks>
        private Element parent;

        private string role;

        internal bool IsImported { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Declaration"/>.
        /// </summary>
        internal Element()
        {
        }

        /// <summary>
        /// Gets the parent <see cref="Element"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Element"/>, or <b>null</b> if the current element
        /// is a <see cref="Domain"/> or is a stand-alone (detached) declaration.
        /// </value>
        [Browsable( false )]
        public Element Parent
        {
            get { return this.parent; }
            // Elements that belong to the tree should not use this property setter.
            internal set { this.parent = value; }
        }

        /// <summary>
        /// Gets the role of the current <see cref="Element"/> in its parent.
        /// </summary>
        /// <remarks>
        /// This property allows the parent to determine to which collection a child belong,
        /// and take appropriate actions in case of notification (as <see cref="NotifyChildPropertyChanged"/>).
        /// </remarks>
        [Browsable( false )]
        public string Role
        {
            get { return this.role; }
        }

        /// <summary>
        /// Gets the <see cref="Domain"/> to which the current element belong, or <b>null</b>
        /// if the current element is a stand-alone (detached) declaration.
        /// </summary>
        /// <remarks>
        /// This property is implemented recursively on the <see cref="Parent"/> property.
        /// Since the cost may be non-trivial for some declaration, consider caching its
        /// result where appropriate.
        /// </remarks>
        [Browsable( false )]
        public Domain Domain
        {
            get
            {
                Domain thisDomain = this as Domain;
                if ( thisDomain != null )
                {
                    return thisDomain;
                }
                if ( this.parent == null )
                {
                    return null;
                }
                return this.parent.Domain;
            }
        }

        internal virtual void OnAddingToParent( Element parent, string role )
        {
            this.parent = parent;
            this.role = role;
        }

        internal virtual void OnRemovingFromParent()
        {
            this.parent = null;
        }

        /// <summary>
        /// Method invoked when a property of the current instance has been changed.
        /// </summary>
        /// <param name="property">Name of the changed property.</param>
        /// <param name="oldValue">Old value of the property.</param>
        /// <param name="newValue">New value of the property.</param>
        protected void OnPropertyChanged( string property, object oldValue, object newValue )
        {
            if ( this.parent != null )
                this.parent.NotifyChildPropertyChanged( this, property, oldValue, newValue );
        }

        /// <summary>
        /// Method invoked when a property of a child has been changed.
        /// </summary>
        /// <param name="child">Child object that has been changed.</param>
        /// <param name="property">Name of the changed property.</param>
        /// <param name="oldValue">Old value of the property.</param>
        /// <param name="newValue">New value of the property.</param>
        /// <returns><b>true</b> if the notification was processed by the current method, otherwise <b>false</b>.</returns>
        /// <remarks>Implementations of this method should first call the base implementation; if the base implementation
        /// returns <b>true</b>, the notification has been processed, so the current implementation can return 
        /// immediately <b>true</b>.</remarks>
        protected virtual bool NotifyChildPropertyChanged( Element child, string property, object oldValue,
                                                           object newValue )
        {
            return false;
        }

        /// <summary>
        /// Clear the cache (typically mapping to <b>System.Reflection</b> or,
        /// if the current element is a reference, to the related definition) 
        /// of the current <see cref="Element"/> and all its children.
        /// </summary>
        public virtual void ClearCache()
        {
            
        }
    }
}