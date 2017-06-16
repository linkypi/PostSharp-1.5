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
using PostSharp.CodeModel.Collections;

namespace PostSharp.Extensibility.Tasks
{

    /// <summary>
    /// Tasks that detects <see cref="DisablePostSharpMessageAttribute"/> custom attributes and
    /// disable messages accordingly.
    /// </summary>
    public sealed class DisableMessagesTask : Task
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            ITypeSignature disableAttributeType = this.Project.Module.GetTypeForFrameworkVariant( typeof(DisablePostSharpMessageAttribute) );
            ITypeSignature escalateAttributeType = this.Project.Module.GetTypeForFrameworkVariant(typeof(EscalatePostSharpMessageAttribute));
            InspectAttributes( this.Project.Module.CustomAttributes, disableAttributeType, escalateAttributeType );
            if ( this.Project.Module.AssemblyManifest != null )
            {
                InspectAttributes( this.Project.Module.AssemblyManifest.CustomAttributes, disableAttributeType, escalateAttributeType );
            }
            return true;
        }

        private static void InspectAttributes( CustomAttributeDeclarationCollection attributes,
                                               ITypeSignature disableAttributeType,
                                               ITypeSignature escalateAttributeType)
        {
            foreach ( CustomAttributeDeclaration attribute in attributes )
            {
                if ( attribute.Constructor.DeclaringType == disableAttributeType )
                {
                    Messenger.Current.DisableMessage( (string) attribute.ConstructorArguments[0].Value.Value );
                }
                else if (attribute.Constructor.DeclaringType == escalateAttributeType)
                {
                    Messenger.Current.EscalateMessage((string)attribute.ConstructorArguments[0].Value.Value);
                }
            }
        }
    }
}