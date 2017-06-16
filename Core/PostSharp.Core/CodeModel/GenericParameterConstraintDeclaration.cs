using System;
using System.Collections.Generic;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Represents a constraint of a generic parameter (<see cref="TokenType.GenericParamConstraint"/>).
    /// </summary>
    /// <remarks>
    /// Generic paraeter constraints are owner by generic parameters (<see cref="GenericParameterDeclaration"/>).
    /// </remarks>
    public sealed class GenericParameterConstraintDeclaration : MetadataDeclaration
    {
        /// <inheritdoc />
        public override TokenType GetTokenType()
        {
            return TokenType.GenericParamConstraint;
        }

        internal override object GetReflectionWrapperImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }

        internal override object GetReflectionObjectImpl( Type[] genericTypeArguments, Type[] genericMethodArguments )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the constraint type.
        /// </summary>
        public ITypeSignature ConstraintType { get; set; }
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of <see cref="GenericParameterConstraintDeclaration"/>.
        /// </summary>
        public sealed class GenericParameterConstraintDeclarationCollection :
            SimpleElementCollection<GenericParameterConstraintDeclaration>
        {
            internal GenericParameterConstraintDeclarationCollection( GenericParameterDeclaration parent, string role )
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
                GenericParameterDeclaration owner = (GenericParameterDeclaration) this.Owner;
                owner.Module.ModuleReader.ImportGenericParameterConstraints( owner );
            }

            /// <summary>
            /// Adds a constraint to the current collection by specifying its type (<see cref="ITypeSignature"/>).
            /// </summary>
            /// <param name="type">Constraint type.</param>
            /// <returns>The <see cref="GenericParameterConstraintDeclaration"/> representing the
            /// new constraint.</returns>
            public GenericParameterConstraintDeclaration Add( ITypeSignature type )
            {
                ExceptionHelper.AssertArgumentNotNull( type, "type" );

                GenericParameterConstraintDeclaration o = new GenericParameterConstraintDeclaration
                                                              {ConstraintType = type};
                this.Add( o );
                return o;
            }

            /// <summary>
            /// Clone a set of constraints and add them to the current collection after having mapped their
            /// generic arguments.
            /// </summary>
            /// <param name="constraints">The set of constraints to be cloned and added to the current collection.</param>
            /// <param name="genericMap">Generic map with which generic arguments in contraint types have to be mapped.</param>
            public void AddRangeClone( IEnumerable<GenericParameterConstraintDeclaration> constraints, GenericMap genericMap )
            {
                ModuleDeclaration targetModule = ( (GenericParameterDeclaration) this.Owner ).Module;

                foreach ( GenericParameterConstraintDeclaration constraint in constraints )
                {
                    this.Add( new GenericParameterConstraintDeclaration
                                  {
                                      ConstraintType =
                                          constraint.ConstraintType.Translate( targetModule ).MapGenericArguments(
                                          genericMap )
                                  } );
                }
            }
        }
    }
}