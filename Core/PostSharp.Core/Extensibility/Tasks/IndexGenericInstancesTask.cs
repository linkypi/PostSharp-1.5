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
    /// Task that references generic instances as tags in generic definitions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tasks creates the following backlinks:
    /// </para>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Parent</term>
    ///         <description>Children</description>
    ///     </listheader>
    ///     <item>
    ///         <term><see cref="TypeDefDeclaration"/></term>
    ///         <description><see cref="TypeSpecDeclaration"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="FieldDefDeclaration"/></term>
    ///         <description><see cref="FieldRefDeclaration"/> whose declaring type is a <see cref="TypeSpecDeclaration"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="MethodDefDeclaration"/></term>
    ///         <description>
    ///             <see cref="MethodRefDeclaration"/> whose declaring type is a <see cref="TypeSpecDeclaration"/>;
    ///             <see cref="MethodSpecDeclaration"/>.
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    public sealed class IndexGenericInstancesTask : Task
    {
        /// <summary>
        /// Name of the current task type.
        /// </summary>
        private const string taskTypeName = "IndexGenericInstancesTask";

        /// <summary>
        /// GUID of the tag that stores references to generic instances. The type of this tag is <see cref="ICollection{T}"/>.
        /// </summary>
        private static readonly Guid genericInstancesTag = new Guid( "{480C0296-95C9-49c5-82B1-C8DE0D719620}" );

        private static void AddReference( IMetadataDeclaration source, MetadataDeclaration target )
        {
            Trace.IndexGenericInstanceTask.WriteLine("{0} {1} --> {2} {3}",
                    source.GetTokenType(), source, target.GetTokenType(), target);

            ICollection<MetadataDeclaration> refCollection = (ICollection<MetadataDeclaration>)
                                                             source.GetTag( genericInstancesTag );
            if ( refCollection == null )
            {
                refCollection = new List<MetadataDeclaration>();
                source.SetTag( genericInstancesTag, refCollection );
            }

            refCollection.Add( target );

        }

        /// <summary>
        /// Get the collection of generic instances of a given generic declaration.
        /// </summary>
        /// <param name="item">A generic declaration.</param>
        /// <returns>The collection of generic instances of <paramref name="item"/>, or 
        /// <b>null</b> if <paramref name="item"/> has no generic instance in
        /// the given module.</returns>
        public static ICollection<MetadataDeclaration> GetGenericInstances( MetadataDeclaration item )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( item, "item" );

            #endregion

            ICollection<MetadataDeclaration> collection = (ICollection<MetadataDeclaration>)
                                                          item.GetTag( genericInstancesTag );

            if ( collection != null )
            {
                return collection;
        }
            else
            {
                 return EmptyCollection<MetadataDeclaration>.GetInstance();
            }

        }

        /// <summary>
        /// Finds the current task in a <see cref="Project"/>.
        /// </summary>
        /// <param name="project">A <see cref="Project"/>.</param>
        /// <returns>The <see cref="IndexGenericInstancesTask"/> contained in the project,
        /// or <b>null</b> if this project does not contai the current task.</returns>
        public static IndexGenericInstancesTask GetTask( Project project )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( project, "project" );

            #endregion

            return (IndexGenericInstancesTask) project.Tasks[taskTypeName];
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            // Process type specifications (and referenced fields and methods).
            using (
                IEnumerator<MetadataDeclaration> typeSpecEnumerator =
                    this.Project.Module.GetDeclarationEnumerator( TokenType.TypeSpec ) )
            {
                while ( typeSpecEnumerator.MoveNext() )
                {
                    TypeSpecDeclaration typeSpec = (TypeSpecDeclaration) typeSpecEnumerator.Current;

                    if ( typeSpec.IsGenericInstance )
                    {
                        TypeDefDeclaration typeDef = typeSpec.GetTypeDefinition();

                        // Reference the type specification.
                        AddReference( typeDef, typeSpec );

                        // Reference fields and methods.
                        foreach ( FieldRefDeclaration fieldRef in typeSpec.FieldRefs )
                        {
                            AddReference( fieldRef.GetFieldDefinition(), fieldRef );
                        }

                        foreach ( MethodRefDeclaration methodRef in typeSpec.MethodRefs )
                        {
                            AddReference( methodRef.GetMethodDefinition(), methodRef );
                        }
                    }
                }
            }

            // Process method specifications.
            using (
                IEnumerator<MetadataDeclaration> methodSpecEnumerator =
                    this.Project.Module.GetDeclarationEnumerator( TokenType.MethodSpec ) )
            {
                while ( methodSpecEnumerator.MoveNext() )
                {
                    MethodSpecDeclaration methodSpec = (MethodSpecDeclaration) methodSpecEnumerator.Current;
                    IMethod genericDeclaration = methodSpec.GenericMethod;

                    AddReference( (MetadataDeclaration) genericDeclaration, methodSpec );
                }
            }

            return true;
        }
    }
}
