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
using PostSharp.CodeModel;
using PostSharp.ModuleWriter;

namespace PostSharp.Laos.Weaver
{
    internal class CompoundAspectWeaver : LaosAspectWeaver
    {
        private object targetReflectionElement;

        private static readonly CompositionAspectConfigurationAttribute defaultConfiguration =
            new CompositionAspectConfigurationAttribute();

        public CompoundAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            if ( this.RequiresReflectionWrapper )
            {
                this.targetReflectionElement =
                    ( (IMetadataDeclaration) this.TargetElement ).GetReflectionWrapperObject( null, null );
            }
            else
            {
                this.targetReflectionElement =
                    ( (IMetadataDeclaration) this.TargetElement ).GetReflectionSystemObject( null, null );
            }
        }

        public override void InitializeAspect()
        {
            if ( this.Aspect != null )
            {
                ( (ICompoundAspect) this.Aspect ).CompileTimeInitialize( this.targetReflectionElement );
            }
        }

        public override bool ValidateSelf()
        {
            if ( this.Aspect == null ) return true;

            return ( (ICompoundAspect) this.Aspect ).CompileTimeValidate( this.targetReflectionElement );
        }

        public override void EmitRuntimeInitialization( InstructionEmitter writer )
        {
            throw new NotSupportedException();
        }

        public override void Implement()
        {
        }

        public override Type GetSerializerType()
        {
            return null;
        }

        public override bool RequiresImplementation
        {
            get { return false; }
        }
    }
}