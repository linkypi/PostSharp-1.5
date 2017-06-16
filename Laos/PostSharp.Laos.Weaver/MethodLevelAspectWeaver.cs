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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.ModuleWriter;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Base class for weavers of method-level aspects (<see cref="ILaosMethodLevelAspect"/>).
    /// </summary>
    [SuppressMessage( "Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable" )]
    public abstract class MethodLevelAspectWeaver : LaosAspectWeaver
    {
        private IMethod baseMethodAspect_RuntimeInitialize;
        private MethodBase targetReflectionMethod;
        private FieldDefDeclaration targetMethodRuntimeInstanceField;

        /// <summary>
        /// Initializes a new <see cref="MethodLevelAspectWeaver"/>.
        /// </summary>
        /// <param name="defaultConfiguration">Default aspect configuration.</param>
        protected MethodLevelAspectWeaver( LaosAspectConfigurationAttribute defaultConfiguration )
            : base( defaultConfiguration )
        {
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            // Initialize the weaver.
            ModuleDeclaration module = this.Task.Project.Module;
            this.baseMethodAspect_RuntimeInitialize = module.Cache.GetItem(
                () => module.FindMethod(
                          module.GetTypeForFrameworkVariant( typeof(ILaosMethodLevelAspect) ),
                          "RuntimeInitialize" ) );

            // Load the system method if we can.
            if ( !this.Task.Project.Module.Domain.ReflectionDisabled && !this.RequiresReflectionWrapper )
            {
                try
                {
                    this.targetReflectionMethod = this.TargetMethod.GetSystemMethod(null, null,
                                                                                BindingOptions.RequireGenericDefinition);
                    
                }
                catch(TypeLoadException)
                {
                    
                }
            }

            // Load the wrapper if we could not load the reflection method.
            if ( this.targetReflectionMethod == null )
            {
                this.targetReflectionMethod = this.TargetMethod.GetReflectionWrapper(null, null);
            }
            
        }

        /// <inheritdoc />
        public override void InitializeAspect()
        {
            base.InitializeAspect();

            // Initialize the aspect.
            ILaosMethodLevelAspectBuildSemantics aspectBuildSemantics = this.Aspect as ILaosMethodLevelAspectBuildSemantics;
            if ( aspectBuildSemantics != null )
            {
                aspectBuildSemantics.CompileTimeInitialize( this.targetReflectionMethod );
            }
        }

        /// <summary>
        /// Gets the Reflection representation of the method to which the current aspect
        /// is applied.
        /// </summary>
        public MethodBase TargetReflectionMethod
        {
            get { return this.targetReflectionMethod; }
        }

        /// <summary>
        /// Gets the method to which the aspect is applied.
        /// </summary>
        public IMethod TargetMethod
        {
            get { return (IMethod) this.TargetElement; }
        }

        /// <summary>
        /// Gets the compile-time instance of the aspect, casted as a <see cref="ILaosMethodLevelAspect"/>.
        /// </summary>
        public ILaosMethodLevelAspect MethodLevelAspect
        {
            get { return (ILaosMethodLevelAspect) this.Aspect; }
        }

        /// <summary>
        /// Field containing the target <see cref="MethodBase"/> at runtime.
        /// </summary>
        public FieldDefDeclaration TargetMethodRuntimeInstanceField
        {
            get { return targetMethodRuntimeInstanceField; }
        }

        /// <inheritdoc />
        public override bool ValidateSelf()
        {
            if ( !this.ValidateDeclaringType( this.TargetMethod.DeclaringType ) )
            {
                return false;
            }

            if ( this.Aspect == null )
                return true;

            ILaosAspectBuildSemantics aspectBuildSemantics = this.Aspect as ILaosAspectBuildSemantics;
            return aspectBuildSemantics == null || aspectBuildSemantics.CompileTimeValidate(this.targetReflectionMethod);
        }

        /// <inheritdoc />
        public override void Implement()
        {
            this.CreateTargetMethodRuntimeInstanceField();
        }

        private void CreateTargetMethodRuntimeInstanceField()
        {
            if ( this.targetMethodRuntimeInstanceField == null )
            {
                this.targetMethodRuntimeInstanceField = new FieldDefDeclaration();
                targetMethodRuntimeInstanceField.Name = "~targetMethod~" + this.UniqueName;
                targetMethodRuntimeInstanceField.Attributes = FieldAttributes.Assembly | FieldAttributes.Static;
                targetMethodRuntimeInstanceField.FieldType =
                    this.Task.ImplementationDetailsType.Module.Domain.FindTypeDefinition("System.Reflection.MethodBase, mscorlib").Translate(this.Task.ImplementationDetailsType.Module);
                this.Task.ImplementationDetailsType.Fields.Add( targetMethodRuntimeInstanceField );
            }
        }

        /// <inheritdoc />
        public override void EmitRuntimeInitialization( InstructionEmitter writer )
        {
            this.CreateTargetMethodRuntimeInstanceField();

            this.Task.WeavingHelper.GetRuntimeMethod( (IMethod) this.TargetMethod.Translate( this.Task.Project.Module ),
                                                      writer );


            writer.EmitInstructionField( OpCodeNumber.Stsfld, targetMethodRuntimeInstanceField );

            // Construct the static constructor.
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, targetMethodRuntimeInstanceField );
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, baseMethodAspect_RuntimeInitialize );
        }
    }
}