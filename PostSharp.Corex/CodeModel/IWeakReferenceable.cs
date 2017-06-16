namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of declarations (<see cref="TypeRefDeclaration"/>
    /// and <see cref="AssemblyRefDeclaration"/>) that can be "weakly references".
    /// </summary>
    /// <remarks>
    /// <para>
    /// Serialized constructions (custom attributes and permission set) may reference a
    /// type using its fully qualified name instead of an entry in a metadata table.
    /// In PostSharp, these "weak references" are not implemented by strings
    /// but by references to pseudo-metadata declarations. They are metadata declarations
    /// that are marked to have no "strong" reference. 
    /// </para>
    /// <para>
    /// Metadata declarations with "strong" references are called <i>linked</i>
    /// and other non-linked.
    /// </para>
    /// </remarks>
    public interface IWeakReferenceable
    {
        /// <summary>
        /// Determines whether the current declaration is weakly or strongly
        /// referenced in the current module.
        /// </summary>
        bool IsWeaklyReferenced { get; set; }
    }
}