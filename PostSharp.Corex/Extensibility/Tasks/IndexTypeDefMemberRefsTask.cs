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
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Indexes all <b>MemberRefs</b> (<see cref="MemberRefDeclaration"/>) owned by a <b>TypeDef</b> (<see cref="Type"/>),
    /// and relate them to the corresponding <see cref="FieldDefDeclaration"/> or <see cref="MethodDefDeclaration"/>.
    /// </summary>
    /// <remarks>
    /// It is stupid, but some compilers use a <see cref="FieldRefDeclaration"/> or a <see cref="MethodRefDeclaration"/>
    /// inside the current module, although they could use 
    ///  a <see cref="FieldDefDeclaration"/> or a <see cref="MethodDefDeclaration"/>
    /// </remarks>
    public sealed class IndexTypeDefMemberRefsTask : Task
    {
        private static readonly Guid tag = new Guid("{99833FBF-A96E-4cec-A9BD-B2D5E28103B1}");

        /// <inheritdoc />
        public override bool Execute()
        {
            IEnumerator<MetadataDeclaration> enumerator =
                this.Project.Module.GetDeclarationEnumerator(TokenType.MemberRef);

            while (enumerator.MoveNext())
            {
                MemberRefDeclaration memberRef = (MemberRefDeclaration) enumerator.Current;
                TypeDefDeclaration typeDef = memberRef.ResolutionScope as TypeDefDeclaration;
                IMetadataDeclaration def;

                if (typeDef != null)
                {
                    FieldRefDeclaration fieldRef;

                    if ((fieldRef = memberRef as FieldRefDeclaration) != null)
                    {
                        def = typeDef.Fields.GetField(fieldRef.Name, fieldRef.FieldType, BindingOptions.OnlyExisting);
                    }
                    else
                    {
                        MethodRefDeclaration methodRef = (MethodRefDeclaration) memberRef;
                        def = typeDef.Methods.GetMethod(methodRef.Name, methodRef, BindingOptions.OnlyExisting);
                    }

                    Set<MemberRefDeclaration> references = (Set<MemberRefDeclaration>)
                                                           def.GetTag(tag);
                    if (references == null)
                    {
                        references = new Set<MemberRefDeclaration>();
                        def.SetTag(tag, references);
                    }

                    references.Add(memberRef);
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the collection of <b>MemberRefs</b> (<see cref="MemberRefDeclaration"/>) that
        /// are used to access a given <see cref="FieldDefDeclaration"/> or <see cref="MethodDefDeclaration"/>.
        /// </summary>
        /// <param name="member">A <see cref="FieldDefDeclaration"/> or a <see cref="MethodDefDeclaration"/>.</param>
        /// <returns>The collection of <see cref="MemberRefDeclaration"/> referencing <paramref name="member"/>.</returns>
        public static ICollection<MemberRefDeclaration> GetReferences(IMember member)
        {
            Set<MemberRefDeclaration> references = (Set<MemberRefDeclaration>)
                                                   member.GetTag(tag);

            return references ?? EmptyCollection<MemberRefDeclaration>.GetInstance();
        }
    }
}