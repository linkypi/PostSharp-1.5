using System;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a method implementation (<see cref="TokenType.MethodImpl"/>), i.e. an indication of
    /// the interface method being implemented by the current <see cref="MethodDefDeclaration"/>.
    /// </summary>
    /// <remarks>
    /// Method implementations are owned by method definitions (<see cref="MethodDefDeclaration"/>).
    /// </remarks>
    public sealed class MethodImplementationDeclaration : MetadataDeclaration
    {
        /// <summary>
        /// Gets or sets the implemented method (an interface method).
        /// </summary>
        public IMethod ImplementedMethod { get; set; }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.MethodImpl;
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of <see cref="MethodImplementationDeclaration"/>.
        /// </summary>
        public sealed class MethodImplementationDeclarationCollection :
            SimpleElementCollection<MethodImplementationDeclaration>
        {
            internal MethodImplementationDeclarationCollection( Declaration parent, string role )
                : base( parent, role )
            {
            }

            /// <inheritdoc />
            protected override bool IsLazyLoadingSupported
            {
                get { return true; }
            }

            /// <inheritdoc />
            protected override void DoLazyLoading()
            {
                MethodDefDeclaration owner = (MethodDefDeclaration) this.Owner;
                owner.Module.ModuleReader.ImportMethodImplementations( owner );
            }

            /// <summary>
            /// Adds a new method implementation to the current collection by specifying only the method.
            /// </summary>
            /// <param name="method">The implemented method.</param>
            /// <returns>The <see cref="MethodImplementationDeclaration"/> that has been created to
            /// encapsulate <paramref name="method"/> and added to the current collection.</returns>
            public MethodImplementationDeclaration Add( IMethod method )
            {
                MethodImplementationDeclaration o = new MethodImplementationDeclaration {ImplementedMethod = method};
                this.Add( o );
                return o;
            }
        }
    }
}