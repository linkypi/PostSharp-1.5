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
using PostSharp.PlatformAbstraction;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Base class for weavers of method-level aspects (<see cref="ILaosMethodLevelAspect"/>).
    /// </summary>
    public abstract class TypeLevelAspectWeaver : LaosAspectWeaver
    {
        private IMethod baseTypeAspect_RuntimeInitialize;
        private Type targetReflectionType;

        /// <summary>
        /// Initializes a new <see cref="TypeLevelAspectWeaver"/>.
        /// </summary>
        /// <param name="defaultConfiguration">Default configuration.</param>
        protected TypeLevelAspectWeaver( LaosAspectConfigurationAttribute defaultConfiguration )
            : base( defaultConfiguration )
        {
        }

        /// <summary>
        /// Gets the compile-time instance of the aspect, casted as a <see cref="ILaosTypeLevelAspect"/>.
        /// </summary>
        public ILaosTypeLevelAspect TypeLevelAspect
        {
            get { return (ILaosTypeLevelAspect) this.Aspect; }
        }


        /// <summary>
        /// Gets the Reflection representation of the type to which the current
        /// aspect is applied.
        /// </summary>
        public Type TargetReflectionType
        {
            get { return this.targetReflectionType; }
        }

        /// <summary>
        /// Gets the type to which this aspect is applied.
        /// </summary>
        public IType TargetType
        {
            get { return (IType) this.TargetElement; }
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            ModuleDeclaration module = this.Task.Project.Module;

            // Initialize the weaver.
            this.baseTypeAspect_RuntimeInitialize =
                module.Cache.GetItem( () => module.FindMethod(
                                                module.GetTypeForFrameworkVariant( typeof(ILaosTypeLevelAspect) ),
                                                "RuntimeInitialize" ) );

            if ( !(this.Task.Project.Module.Domain.ReflectionDisabled || this.RequiresReflectionWrapper) )
            {
                try
                {
                    this.targetReflectionType = this.TargetType.GetSystemType( null, null );
                }
                catch(TypeLoadException)
                {
                    
                }
            }

            if (this.targetReflectionType == null)
            {
                this.targetReflectionType = this.TargetType.GetReflectionWrapper( null, null );
            }
        }

        /// <inheritdoc />
        public override void InitializeAspect()
        {
            base.InitializeAspect();

            // Initialize the aspect.
            ILaosTypeLevelAspectBuildSemantics aspectBuildSemantics = this.Aspect as ILaosTypeLevelAspectBuildSemantics;
            if (aspectBuildSemantics != null)
            {
                aspectBuildSemantics.CompileTimeInitialize(this.targetReflectionType);
            }
        }

        /// <inheritdoc />
        public override bool ValidateSelf()
        {
            if ( !this.ValidateDeclaringType( this.TargetType ) )
            {
                return false;
            }

            if ( this.Aspect == null )
                return true;

            ILaosAspectBuildSemantics aspectBuildSemantics = this.Aspect as ILaosAspectBuildSemantics;
            return aspectBuildSemantics == null || aspectBuildSemantics.CompileTimeValidate(this.targetReflectionType);
        }

        /// <inheritdoc />
        public override void EmitRuntimeInitialization( InstructionEmitter writer )
        {
            base.EmitRuntimeInitialization( writer );

            // Create and initialize a field that will contain the MethodBase of the target method.
            FieldDefDeclaration targetTypeRuntimeInstance = new FieldDefDeclaration
                                                                {
                                                                    Name =
                                                                        Platform.Current.NormalizeCilIdentifier(
                                                                        "~targetType~" + this.UniqueName ),
                                                                    Attributes =
                                                                        ( FieldAttributes.Assembly |
                                                                          FieldAttributes.Static ),
                                                                    FieldType =
                                                                        this.Task.Project.Module.Cache.GetType(
                                                                        "System.Type, mscorlib" )
                                                                };
            this.Task.ImplementationDetailsType.Fields.Add( targetTypeRuntimeInstance );

            this.Task.WeavingHelper.GetRuntimeType( this.TargetType, writer );
            writer.EmitInstructionField( OpCodeNumber.Stsfld, targetTypeRuntimeInstance );

            // Construct the static constructor.
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, this.AspectRuntimeInstanceField );
            writer.EmitInstructionField( OpCodeNumber.Ldsfld, targetTypeRuntimeInstance );
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, baseTypeAspect_RuntimeInitialize );
        }
    }
}