using System;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents an interface implementation (<see cref="TokenType.InterfaceImpl"/>) of a <see cref="TypeDefDeclaration"/>.
    /// </summary>
    /// <remarks>
    /// Interface implementations are owned by type definitions (<see cref="TypeDefDeclaration"/>).
    /// </remarks>
    public sealed class InterfaceImplementationDeclaration : MetadataDeclaration, IRemoveable
    {
        /// <summary>
        /// Gets or sets the implemented interface.
        /// </summary>
        public ITypeSignature ImplementedInterface { get; set; }

        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.InterfaceImpl;
        }

        /// <inheritdoc />
        public void Remove()
        {
            ((TypeDefDeclaration) this.Parent).InterfaceImplementations.Remove( this );
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
        /// Collection of <see cref="InterfaceImplementationDeclaration"/>.
        /// </summary>
        public sealed class InterfaceImplementationDeclarationCollection :
            SimpleElementCollection<InterfaceImplementationDeclaration>
        {
            internal InterfaceImplementationDeclarationCollection( TypeDefDeclaration parent, string role )
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
                TypeDefDeclaration owner = (TypeDefDeclaration) this.Owner;
                owner.Module.ModuleReader.ImportInterfaceImplementations( owner );
            }

            /// <summary>
            /// Adds a new interface implementation to the current collection by only specifying the
            /// interfafce type.
            /// </summary>
            /// <param name="type">Interface type.</param>
            /// <returns>The <see cref="InterfaceImplementationDeclaration"/> that has been created
            /// to encapsulate <paramref name="type"/>.</returns>
            public InterfaceImplementationDeclaration Add( ITypeSignature type )
            {
                InterfaceImplementationDeclaration o = new InterfaceImplementationDeclaration
                                                           {ImplementedInterface = type};
                this.Add( o );
                return o;
            }
        }
    }
}