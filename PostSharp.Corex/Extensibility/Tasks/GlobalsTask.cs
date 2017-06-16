#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Performs the transformations required by the <see cref="Post"/> class.
    /// </summary>
    public sealed class GlobalsTask : Task, IAdviceProvider
    {
        private bool active;
        private TypeRefDeclaration postTypeRef;

        /// <inheritdoc />
        public override bool Execute()
        {
            this.postTypeRef = (TypeRefDeclaration) this.Project.Module.GetTypeForFrameworkVariant(typeof (Post),
                BindingOptions.OnlyExisting | BindingOptions.DontThrowException);

            // Look if the module references the type "PostSharp.Post".
            // If yes, we need to include the CodeWeaver task in the project,
            // and we will supply advices.
            this.active = this.postTypeRef != null;
            if ( this.active )
            {
                this.Project.Tasks.Add( "CodeWeaver" );
            }

            return true;
        }

        #region IAdviceProvider Members

        /// <inheritdoc />
        public void ProvideAdvices( Weaver codeWeaver )
        {
            if ( !this.active )
                return;

            IMethod postMethod =
                this.Project.Module.FindMethod( this.postTypeRef, "Cast");

                codeWeaver.AddMethodLevelAdvice( new CastAdvice(),
                                                 null, JoinPointKinds.InsteadOfCall,
                                                 new Singleton<MetadataDeclaration>( (MetadataDeclaration) postMethod ) );

            IMethod isTransformedMethod =
                this.Project.Module.FindMethod( this.postTypeRef, "get_IsTransformed" );
                codeWeaver.AddMethodLevelAdvice( new IsTransformedAdvice(),
                                                 null, JoinPointKinds.InsteadOfCall,
                                                 new Singleton<MetadataDeclaration>(
                                                     (MetadataDeclaration) isTransformedMethod ) );
        }

        #endregion

        private class CastAdvice : IAdvice
        {
            #region IAdvice Members

            public int Priority { get { return 0; } }

            public bool RequiresWeave( WeavingContext context )
            {
                return true;
            }

            public void Weave( WeavingContext context, InstructionBlock block )
            {
                MethodSpecDeclaration methodSpec = (MethodSpecDeclaration) context.JoinPoint.Instruction.MethodOperand;
                ITypeSignature sourceType = methodSpec.GenericArguments[0];
                ITypeSignature targetType = methodSpec.GenericArguments[1];


                if ( !sourceType.IsAssignableTo( targetType,
                    context.Method.GetGenericContext(GenericContextOptions.None ) ) )
                {
                    CoreMessageSource.Instance.Write( SeverityType.Error, "PS0087",
                                                      new object[] {sourceType, targetType},
                                                      context.JoinPoint.Instruction.LastSymbolSequencePoint );
                }
            }

            #endregion
        }

        private class IsTransformedAdvice : IAdvice
        {
            public int Priority { get { return 0; } }

            public bool RequiresWeave( WeavingContext context )
            {
                return true;
            }

            public void Weave( WeavingContext context, InstructionBlock block )
            {
                InstructionSequence sequence = block.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence( sequence, NodePosition.Before, null );
                context.InstructionWriter.AttachInstructionSequence( sequence );

                context.InstructionWriter.EmitInstruction( OpCodeNumber.Ldc_I4_1 );
                context.InstructionWriter.DetachInstructionSequence();
            }
        }
    }
}
