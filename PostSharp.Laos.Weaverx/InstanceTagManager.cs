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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.ModuleWriter;
using PostSharp.PlatformAbstraction;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// Encapsulates the work with instance tags. Given an <see cref="InstanceTagRequest"/>,
    /// returns a <see cref="FieldDefDeclaration"/>.
    /// </summary>
    public class InstanceTagManager
    {
        private readonly IMethod setInstanceTagMethod;
        private readonly IMethod getInstanceTagMethod;
        private readonly LaosTask task;

        private readonly Dictionary<Pair<TypeDefDeclaration, Guid>, FieldDefDeclaration> fields =
            new Dictionary<Pair<TypeDefDeclaration, Guid>, FieldDefDeclaration>();

        internal InstanceTagManager( LaosTask task )
        {
            this.task = task;
            ModuleDeclaration module = task.Project.Module;

            this.getInstanceTagMethod = module.Cache
                .GetItem( () => module.FindMethod(
                                    module.GetTypeForFrameworkVariant(
                                        typeof(InstanceBoundLaosEventArgs) ),
                                    "get_InstanceTag" ) );

            this.setInstanceTagMethod = module.Cache
                .GetItem( () => module.FindMethod(
                                    module.GetTypeForFrameworkVariant(
                                        typeof(InstanceBoundLaosEventArgs) ),
                                    "set_InstanceTag" ) );
        }


        /// <summary>
        /// Given an <see cref="InstanceTagRequest"/>,
        /// returns a <see cref="FieldDefDeclaration"/>.
        /// </summary>
        /// <param name="typeDef">Type in which the instance tag is requested.</param>
        /// <param name="request">An <see cref="InstanceTagRequest"/>.</param>
        /// <returns>A <see cref="FieldDefDeclaration"/> aimed to contained the instance
        /// tag, fullfilling the <paramref name="request"/>.</returns>
        /// <param name="forceStatic">Force the tag to be stored in a static field.</param>
        public FieldDefDeclaration GetInstanceTagField( TypeDefDeclaration typeDef, InstanceTagRequest request,
                                                        bool forceStatic )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( request, "request" );
            ExceptionHelper.AssertArgumentNotNull( typeDef, "typeDef" );

            #endregion

            bool isStatic = forceStatic || request.ForceStatic;

            Pair<TypeDefDeclaration, Guid> key = new Pair<TypeDefDeclaration, Guid>( typeDef, request.Guid );

            if ( this.fields.ContainsKey( key ) )
            {
                FieldDefDeclaration field = this.fields[key];
                if ( isStatic != ( ( field.Attributes & FieldAttributes.Static ) != 0 ) )
                {
                    LaosMessageSource.Instance.Write(
                        SeverityType.Error, "LA0008",
                        new object[] {request.Guid, request.PreferredName, typeDef.Name} );
                    return null;
                }
                return field;
            }
            else
            {
                FieldDefDeclaration field = new FieldDefDeclaration();
                string fieldName = request.PreferredName;
                int i = 0;
                while ( typeDef.Fields.GetByName( fieldName ) != null )
                {
                    i++;
                    fieldName = request.PreferredName + "~" + i.ToString( CultureInfo.InvariantCulture );
                }
                field.Name = Platform.Current.NormalizeCilIdentifier( fieldName );
                field.FieldType = typeDef.Module.Cache.GetIntrinsic( IntrinsicType.Object );
                field.Attributes = FieldAttributes.Private;
                if ( isStatic )
                {
                    field.Attributes |= FieldAttributes.Static;
                }
                typeDef.Fields.Add( field );
                this.task.WeavingHelper.AddCompilerGeneratedAttribute( field.CustomAttributes );

                this.fields.Add( key, field );

                return field;
            }
        }

        /// <summary>
        /// Emits instructions that load the instance tag from the field into the event argument property.
        /// </summary>
        /// <param name="eventArgsLocal">Local variable where the event argument is stored.</param>
        /// <param name="instanceTagField">Field where the instance tag is stored.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        public void EmitLoadInstanceTag( LocalVariableSymbol eventArgsLocal, FieldDefDeclaration instanceTagField,
                                         InstructionEmitter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( eventArgsLocal, "eventArgsLocal" );

            #endregion

            if ( instanceTagField != null )
            {
                bool isStatic = ( instanceTagField.Attributes & FieldAttributes.Static ) != 0;

                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );

                if ( isStatic )
                {
                    writer.EmitInstructionField( OpCodeNumber.Ldsfld, GenericHelper.GetFieldCanonicalGenericInstance( instanceTagField ) );
                }
                else
                {
                    writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                    writer.EmitInstructionField( OpCodeNumber.Ldfld, GenericHelper.GetFieldCanonicalGenericInstance(  instanceTagField ) );
                }
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.setInstanceTagMethod );
            }
        }

        /// <summary>
        /// Emits instructions that store the instance tag from the event argument property into the field .
        /// </summary>
        /// <param name="eventArgsLocal">Local variable where the event argument is stored.</param>
        /// <param name="instanceTagField">Field where the instance tag is stored.</param>
        /// <param name="writer">An <see cref="InstructionEmitter"/>.</param>
        public void EmitStoreInstanceTag( LocalVariableSymbol eventArgsLocal, FieldDefDeclaration instanceTagField,
                                          InstructionEmitter writer )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( eventArgsLocal, "eventArgsLocal" );

            #endregion

            if ( instanceTagField != null )
            {
                bool isStatic = ( instanceTagField.Attributes & FieldAttributes.Static ) != 0;

                if ( !isStatic )
                {
                    writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
                }
                writer.EmitInstructionLocalVariable( OpCodeNumber.Ldloc, eventArgsLocal );
                writer.EmitInstructionMethod( OpCodeNumber.Call, this.getInstanceTagMethod );

                if ( isStatic )
                {
                    writer.EmitInstructionField( OpCodeNumber.Stsfld, GenericHelper.GetFieldCanonicalGenericInstance( instanceTagField ) );
                }
                else
                {
                    writer.EmitInstructionField( OpCodeNumber.Stfld, GenericHelper.GetFieldCanonicalGenericInstance( instanceTagField ) );
                }
            }
        }
    }
}