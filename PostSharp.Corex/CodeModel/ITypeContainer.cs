using PostSharp.CodeModel.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Exposes the semantics of a declaration that can contain type definitions
    /// (<see cref="ModuleDeclaration"/> or <see cref="TypeDefDeclaration"/>).
    /// </summary>
    public interface ITypeContainer : IMetadataDeclaration
    {
        /// <summary>
        /// Gets the collection of types.
        /// </summary>
        TypeDefDeclarationCollection Types { get; }
    }
}