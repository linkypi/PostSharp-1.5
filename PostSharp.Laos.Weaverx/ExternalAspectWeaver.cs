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
using PostSharp.Extensibility;

namespace PostSharp.Laos.Weaver
{
    internal class ExternalAspectWeaver : LaosAspectWeaver
    {
        private static readonly ExternalAspectConfigurationAttribute defaultConfiguration =
            new ExternalAspectConfigurationAttribute( null );

        public ExternalAspectWeaver() : base( defaultConfiguration )
        {
        }

        public override bool ValidateSelf()
        {
            return true;
        }

        public override void Implement()
        {
        }

        public override void ProvideAspects( LaosReflectionAspectCollection collection )
        {
            ModuleDeclaration module = this.TargetElement.Module;

            TypeDefDeclaration aspectType = module.Cache.GetType( this.GetAspectTypeName() ).GetTypeDefinition();

            CustomAttributeDeclaration attribute =
                aspectType.CustomAttributes.GetOneByType( (IType)
                                                          module.FindType(
                                                              "PostSharp.Laos.ExternalAspectConfigurationAttribute, " +
                                                              module.GetAssemblyNameForFrameworkVariant(
                                                                  "PostSharp.Laos" ),
                                                              BindingOptions.Default ) );

            IExternalAspectImplementation implementation;

            if ( attribute == null )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error,
                                                  "LA0025", new object[] {this.GetAspectTypeName()} );
                return;
            }

            string implementationTypeName = (string) attribute.ConstructorArguments[0].Value.Value;

            if ( string.IsNullOrEmpty( implementationTypeName ) )
            {
                LaosMessageSource.Instance.Write( SeverityType.Error,
                                                  "LA0026", new object[] {this.GetAspectTypeName()} );
                return;
            }

            try
            {
                Type type = Type.GetType( implementationTypeName, true );

                if ( type == null )
                {
                    LaosMessageSource.Instance.Write( SeverityType.Error, "LA0029",
                                                      new object[] {implementationTypeName, this.GetAspectTypeName()} );
                    return;
                }

                implementation = (IExternalAspectImplementation) Activator.CreateInstance( type );
            }
            catch ( Exception e )
            {
                // Cannot create an instance of the type "{0}" implementing the aspect "{1}": {2}
                LaosMessageSource.Instance.Write( SeverityType.Error, "LA0024",
                                                  new object[]
                                                      {
                                                          implementationTypeName, this.GetAspectTypeName(),
                                                          e.Message,
                                                      } );
                return;
            }

            implementation.ImplementAspect(
                ( (IMetadataDeclaration) this.TargetElement ).GetReflectionWrapperObject( null, null ),
                this.AspectSpecification.AspectConstruction, collection );
        }

        public override bool RequiresImplementation
        {
            get { return false; }
        }

        public override Type GetSerializerType()
        {
            return null;
        }
    }
}