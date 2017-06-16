namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of a domain element, which is basically a domain-rooted tree node.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Gets the <see cref="Domain"/> to which the current element belongs.
        /// </summary>
        /// <value>
        /// A <see cref="Domain"/>, or <b>null</b> if the element is detached.
        /// </value>
        Domain Domain { get; }

        /// <summary>
        /// Gets the direct parent of the current element.
        /// </summary>
        /// <value>
        /// An <see cref="Element"/>, or <b>null</b> if the element is detached.
        /// </value>
        Element Parent { get; }
    }
}