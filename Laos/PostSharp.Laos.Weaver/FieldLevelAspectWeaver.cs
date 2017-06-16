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
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.ModuleWriter;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Base class for weavers of field-level aspects (<see cref="ILaosFieldLevelAspect"/>).
    /// </summary>
    public abstract class FieldLevelAspectWeaver : LaosAspectWeaver
    {
        private IMethod baseFieldAspect_RuntimeInitialize;
        private FieldInfo targetReflectionField;
        private FieldDefDeclaration targetFieldRuntimeInstanceField;

        /// <summary>
        /// Initializes a new <see cref="FieldLevelAspectWeaver"/>.
        /// </summary>
        /// <param name="defaultConfiguration">Default aspect configuration.</param>
        protected FieldLevelAspectWeaver( LaosAspectConfigurationAttribute defaultConfiguration )
            : base( defaultConfiguration )
        {
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();


            // Initialize the weaver.
            ModuleDeclaration module = this.Task.Project.Module;

            this.baseFieldAspect_RuntimeInitialize = module.Cache.GetItem(
                () => module.FindMethod(
                          module.GetTypeForFrameworkVariant( typeof(LaosFieldLevelAspect) ),
                          "RuntimeInitialize" ) );


            if ( !(this.Task.Project.Module.Domain.ReflectionDisabled || this.RequiresReflectionWrapper) )
            {
                try
                {
                    this.targetReflectionField = this.TargetField.GetSystemField( null, null );
                } 
                catch(TypeLoadException)
                {
                    
                }

                
            }
            
            if ( this.targetReflectionField == null )
            {
                this.targetReflectionField = this.TargetField.GetReflectionWrapper(null, null);
            }
        }

        /// <inheritdoc />
        public override void InitializeAspect()
        {
            base.InitializeAspect();


            // Initialize the aspect.
            ILaosFieldLevelAspectBuildSemantics aspectBuildSemantics = this.Aspect as ILaosFieldLevelAspectBuildSemantics;
            if ( aspectBuildSemantics != null )
            {
                aspectBuildSemantics.CompileTimeInitialize(this.targetReflectionField);
            }
        }


        /// <inheritdoc />
        public override bool ValidateSelf()
        {
            if ( !this.ValidateDeclaringType( this.TargetField.DeclaringType ) )
            {
                return false;
            }

            if ( this.Aspect == null )
                return true;

            ILaosAspectBuildSemantics aspectBuildSemantics = this.Aspect as ILaosAspectBuildSemantics;
            return aspectBuildSemantics == null || aspectBuildSemantics.CompileTimeValidate( this.targetReflectionField );
        }


        /// <inheritdoc />
        public override void EmitRuntimeInitialization( InstructionEmitter writer )
        {
            this.EmitRuntimeInitialization( writer, false );
        }

        /// <summary>
        /// Field containing the target <see cref="FieldInfo"/> at runtime.
        /// </summary>
        public FieldDefDeclaration TargetFieldRuntimeInstanceField
        {
            get { return targetFieldRuntimeInstanceField; }
        }

        /// <summary>
        /// Emits instructions that initialize the aspect at runtime. These instructions
        /// will be injected in the static constructor of the PostSharp Laos implementation
        /// details object, <i>after</i> the field containing the runtime instance
        /// of the instance (<see cref="LaosAspectWeaver.AspectRuntimeInstanceField"/>) has been 
        /// initialized.
        /// </summary>
        /// <param name="writer">The <see cref="InstructionWriter"/> into which
        /// instructions have to be written.</param>
        /// <param name="suppressField">Whether the field declaration will be removed.</param>
        /// <remarks>
        /// It is expected that implementations generate only simple streams of instructions,
        /// without branching instructions. If more complexity is required, they should
        /// generate auxiliary methods.
        /// </remarks>
        protected virtual void EmitRuntimeInitialization( InstructionEmitter writer, bool suppressField )
        {
            this.CreateTargetFieldRuntimeInstanceField();

            if ( !suppressField )
            {
                // Initialize the field storing the runtime instance of the target field.
                this.Task.WeavingHelper.GetRuntimeField( this.TargetField, writer );
                writer.EmitInstructionField( OpCodeNumber.Stsfld, targetFieldRuntimeInstanceField );
            }
            else
            {
                targetFieldRuntimeInstanceField = null;
            }

            // Construct the static constructor.
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );

            if ( !suppressField )
            {
                writer.EmitInstructionField( OpCodeNumber.Ldsfld, targetFieldRuntimeInstanceField );
            }
            else
            {
                writer.EmitInstruction( OpCodeNumber.Ldnull );
            }

            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, baseFieldAspect_RuntimeInitialize );
        }

//        /// <inheritdoc />
//        public override void Implement()
//        {
//            this.CreateTargetFieldRuntimeInstanceField();
//        }


        private void CreateTargetFieldRuntimeInstanceField()
        {
            if ( this.targetFieldRuntimeInstanceField != null ) return;

            // Create and initialize a field that will contain the FieldInfo of the target field.
            this.targetFieldRuntimeInstanceField = new FieldDefDeclaration
                                                  {
                                                      Name = ( "~targetField~" + this.UniqueName ),
                                                      Attributes =
                                                          ( FieldAttributes.Assembly | FieldAttributes.Static ),
                                                      FieldType =
                                                          this.Task.Project.Module.FindType( "System.Reflection.FieldInfo, mscorlib", BindingOptions.Default )
                                                  };
            this.Task.ImplementationDetailsType.Fields.Add( targetFieldRuntimeInstanceField );
        }

        /// <summary>
        /// Gets the Reflection representation of the type to which the current
        /// aspect is applied.
        /// </summary>
        public FieldInfo TargetReflectionField
        {
            get { return this.targetReflectionField; }
        }

        /// <summary>
        /// Gets the target field.
        /// </summary>
        public IField TargetField
        {
            get { return (IField) this.TargetElement; }
        }

        /// <summary>
        /// Gets the aspect.
        /// </summary>
        public ILaosFieldLevelAspect FieldLevelAspect
        {
            get { return (ILaosFieldLevelAspect) this.Aspect; }
        }
    }
}