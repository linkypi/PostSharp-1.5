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
using PostSharp.CodeModel.Binding;
using PostSharp.Collections;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Analysis that builds an inheritance tree of all types in the module.
    /// </summary>
    public sealed class TypeHierarchyTask : Task
    {
        /// <summary>
        /// Name of the current task type.
        /// </summary>
        private const string taskTypeName = "TypeHierarchyAnalysis";

        /// <summary>
        /// Determines whether the <see cref="IndexTypeDefDeclarations"/> method
        /// has already been called.
        /// </summary>
        private bool typeDefIndexed;

        private static readonly Guid childrenTag = new Guid("{7BCCA13C-5078-4621-9531-8E8B9F65B3A6}");
        private static readonly Guid processedTag = new Guid("{FAA7960B-8E6B-40c8-9354-2F188B934F49}");
        private bool allTypeDefinitionsIndexed;

        /// <summary>
        /// Finds the current task in a <see cref="Project"/>.
        /// </summary>
        /// <param name="project">A <see cref="Project"/>.</param>
        /// <returns>The <see cref="IndexGenericInstancesTask"/> contained in the project,
        /// or <b>null</b> if this project does not contai the current task.</returns>
        public static TypeHierarchyTask GetTask(Project project)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(project, "project");

            #endregion

            return (TypeHierarchyTask) project.Tasks[taskTypeName];
        }

        #region Read Access


        /// <summary>
        /// Gets an enumerator of derived types, given a <see cref="TypeDefDeclaration"/>,
        ///  with the possibility to include types
        /// defined outside the module.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        /// <param name="deep">Whether derived types of second and higher level should be
        /// returned. If <b>false</b>, only direct descendant of <paramref name="baseType"/>
        /// shall be returned.</param>
        /// <param name="module">If non-null, returns only types defined in this module.
        /// If null, returns all types.</param>
        /// <returns>An enumerator of types derived from <paramref name="baseType"/>.</returns>
        public static IEnumerator<TypeDefDeclaration> GetDerivedTypesEnumerator(TypeDefDeclaration baseType, bool deep, ModuleDeclaration module)
        {
            TypeNode children = GetNode(baseType, false);

            if (children != null)
            {
                if (!deep)
                {
                    if (module == null)
                    {
                        return children.Keys.GetEnumerator();
                    }
                    else
                    {
                        return FilterEnumeratorByModule(children.Keys.GetEnumerator(), module);

                    }
                }
                else
                {
                    return InternalGetDerivedTypesEnumerator(children, module);
                }
            }
            else
            {
                return EmptyEnumerator<TypeDefDeclaration>.GetInstance();
            }
        }

        public static IEnumerator<DerivedTypeInfo> GetDerivedTypeInfosEnumerator(TypeDefDeclaration baseType)
        {
            TypeNode children = GetNode(baseType, false);

            if (children != null)
            {
                return InternalGetDerivedTypeInfosEnumerator(new KeyValuePair<TypeNode, GenericMap>(children, baseType.GetGenericContext( GenericContextOptions.None )));
            }
            else
            {
                return EmptyEnumerator<DerivedTypeInfo>.GetInstance();
            }
        }

        private static IEnumerator<TypeDefDeclaration> FilterEnumeratorByModule(IEnumerator<TypeDefDeclaration> enumerator, ModuleDeclaration module)
        {
            while ( enumerator.MoveNext() )
            {
                if ( module == null || enumerator.Current.Module == module )
                {
                    yield return enumerator.Current;
                }
            }
        }

        private static IEnumerator<TypeDefDeclaration> InternalGetDerivedTypesEnumerator(
            TypeNode children, ModuleDeclaration module)
        {
            Queue<TypeNode> queue = new Queue<TypeNode>();
            Set<TypeDefDeclaration> result = new Set<TypeDefDeclaration>(16);

            queue.Enqueue(children);
            while (queue.Count > 0)
            {
                children = queue.Dequeue();

                foreach (TypeDefDeclaration child in children.Keys)
                {
                    if ( module == null || child.Module == module)
                        result.AddIfAbsent(child);

                    TypeNode childrenOfChidren = GetNode(child, false);
                    if (childrenOfChidren != null)
                    {
                        queue.Enqueue(childrenOfChidren);
                    }
                }
            }

            return result.GetEnumerator();
        }

        private static IEnumerator<DerivedTypeInfo> InternalGetDerivedTypeInfosEnumerator(
           KeyValuePair<TypeNode, GenericMap> typeInfo)
        {
            Queue<KeyValuePair<TypeNode, GenericMap>> queue = new Queue<KeyValuePair<TypeNode, GenericMap>>();
            Set<DerivedTypeInfo> result = new Set<DerivedTypeInfo>(16);

            queue.Enqueue(typeInfo);
            while (queue.Count > 0)
            {
                typeInfo = queue.Dequeue();

                foreach (KeyValuePair<TypeDefDeclaration, GenericMap> child in typeInfo.Key)
                {
                    GenericMap genericMap = typeInfo.Value.Apply( child.Value );
                    
                    result.AddIfAbsent(new DerivedTypeInfo( child.Key, genericMap ));

                    TypeNode childrenOfChidren = GetNode(child.Key, false);
                    if (childrenOfChidren != null)
                    {
                        queue.Enqueue(new KeyValuePair<TypeNode, GenericMap>(childrenOfChidren, genericMap));
                    }
                }
            }

            return result.GetEnumerator();
        }

        private static TypeNode GetNode(TypeDefDeclaration typeDef, bool create)
        {
            TypeNode children = (TypeNode)typeDef.GetTag(childrenTag);
            if (children == null && create)
            {
                children = new TypeNode();
                typeDef.SetTag(childrenTag, children);
            }

            return children;
        }

        #endregion

        /// <summary>
        /// Inserts all type declarations (<see cref="TypeDefDeclaration"/>) in the type hierarchy.
        /// </summary>
        public void IndexTypeDefDeclarations()
        {
            if (!this.typeDefIndexed)
            {
                IEnumerator<MetadataDeclaration> enumerator =
                    this.Project.Module.Tables.GetEnumerator(TokenType.TypeDef);
                while (enumerator.MoveNext())
                {
                    TypeDefDeclaration typeDef = (TypeDefDeclaration) enumerator.Current;
                    this.IndexType(typeDef);
                }
                this.typeDefIndexed = true;
            }
        }

        /// <summary>
        /// Indexes all <see cref="TypeDefDeclaration"/> in the current module.
        /// </summary>
        public void IndexAllTypeDefinitions()
        {
            if (allTypeDefinitionsIndexed)
                return;
            IEnumerator<MetadataDeclaration> enumerator = this.Project.Module.GetDeclarationEnumerator(TokenType.TypeDef);

            while ( enumerator.MoveNext() )
            {
                this.IndexType((TypeDefDeclaration) enumerator.Current);
            }

            allTypeDefinitionsIndexed = true;
        }

        /// <summary>
        /// Inserts a given <see cref="TypeDefDeclaration"/> in the type hierarchy.
        /// </summary>
        /// <param name="type"></param>
        public void IndexType(TypeDefDeclaration type)
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull(type, "type");

            #endregion

            if (type.GetTag(processedTag) != null)
                return;

            type.SetTag(processedTag, "processed");

            if (type.BaseType == null)
                return;

            TypeDefDeclaration baseType = type.BaseType.GetTypeDefinition();
            GetNode( baseType, true ).Add( type, type.BaseType.GetGenericContext( GenericContextOptions.None ) );

            this.IndexType(baseType);

            foreach (InterfaceImplementationDeclaration interfaceImpl in type.InterfaceImplementations)
            {
                TypeDefDeclaration interfaceTypeDef = interfaceImpl.ImplementedInterface.GetTypeDefinition();

                if (interfaceTypeDef == null)
                    throw new AssertionFailedException();


                GetNode( interfaceTypeDef, true ).Add( type, interfaceImpl.ImplementedInterface.GetGenericContext( GenericContextOptions.None ) );

                this.IndexType(interfaceTypeDef);
            }
        }

        class TypeNode : MultiDictionary<TypeDefDeclaration,GenericMap>
        {
            
        }

        public sealed class DerivedTypeInfo : IEquatable<DerivedTypeInfo>
        {
            private readonly int hashCode;

            public DerivedTypeInfo(TypeDefDeclaration type, GenericMap genericMap)
            {
                this.ChildType = type;
                this.GenericMapToParent = genericMap;
                this.hashCode = HashCodeHelper.CombineHashCodes(type.GetHashCode(), GenericMapComparer.GetInstance().GetHashCode(genericMap));
            }

            public TypeDefDeclaration ChildType { get; private set; }
            public GenericMap GenericMapToParent { get; private set; }

            public bool Equals(DerivedTypeInfo other)
            {
                if (other.hashCode != this.hashCode)
                    return false;

                if (other.ChildType != this.ChildType)
                    return false;

                return GenericMapComparer.GetInstance().Equals(other.GenericMapToParent, this.GenericMapToParent);

            }

            public KeyValuePair<TypeDefDeclaration, GenericMap> ToKeyValuePair()
            {
                return new KeyValuePair<TypeDefDeclaration, GenericMap>(this.ChildType, this.GenericMapToParent);
            }
        }

       
    }
}