using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PostSharp.CodeModel.Collections;
using PostSharp.CodeModel.ReflectionWrapper;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Defines the semantics of a metadata declaration, i.e. a declaration with
    /// a token.
    /// </summary>
    public interface IMetadataDeclaration : IDeclarationWithCustomAttributes, ITaggable
    {
        /// <summary>
        /// Gets the metadata token type of the current declaration.
        /// </summary>
        /// <returns>A <see cref="TokenType"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
        TokenType GetTokenType();

        /// <summary>
        /// Gets the metadata token of the current declaration.
        /// </summary>
        MetadataToken MetadataToken { get; }


        /// <summary>
        /// Gets a reflection object (<see cref="Type"/>, <see cref="FieldInfo"/>, <see cref="MethodInfo"/>,
        /// <see cref="ParameterInfo"/>, ...) that wraps the current declaration.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>A <b>System.Reflection</b> object wrapping current declaration. If the
        /// current declaration is an assembly (<see cref="IAssembly"/>), an <see cref="IAssemblyWrapper"/>
        /// object is returned.
        /// </returns>
        /// <remarks>
        /// This method returns an object that is different from the system
        /// runtime type that is retrieved by <see cref="GetReflectionSystemObject"/>. This allows
        /// a have a <b>System.Reflection</b> representation of the current declaration even
        /// when it cannot be loaded in the Virtual Runtime Engine.
        /// </remarks>
        object GetReflectionWrapperObject( Type[] genericTypeArguments, Type[] genericMethodArguments );

        /// <summary>
        /// Gets the system, runtime object (<see cref="Type"/>, <see cref="FieldInfo"/>, <see cref="MethodInfo"/>,
        /// <see cref="ParameterInfo"/>, ...) corresponding to the current declaration.
        /// </summary>
        /// <param name="genericTypeArguments">Array of generic type arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <param name="genericMethodArguments">Array of generic method arguments in the
        /// current context, or <b>null</b> if there is no generic type arguments in
        /// the current context.</param>
        /// <returns>The system object associated the current type in the
        /// given generic context.</returns>
        object GetReflectionSystemObject( Type[] genericTypeArguments, Type[] genericMethodArguments );
    }

    /// <summary>
    /// Defines the semantics of a declaration with custom attributes.
    /// </summary>
    public interface IDeclarationWithCustomAttributes : IDeclaration
    {
        /// <summary>
        /// Gets the collection of custom attributes.
        /// </summary>
        CustomAttributeDeclarationCollection CustomAttributes { get; }
    }
}