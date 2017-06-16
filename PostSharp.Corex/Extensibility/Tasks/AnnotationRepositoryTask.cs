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
    /// Repository of custom attributes applying on a module. Custom attributes
    /// may come from different sources (sources whould implement the
    /// <see cref="IAnnotationProvider"/> interface) and are indexed
    /// by type.
    /// </summary>
    public sealed class AnnotationRepositoryTask : Task
    {
        /// <summary>
        /// Name of the current task type.
        /// </summary>
        private const string taskTypeName = "AnnotationRepository";

        #region Fields

        private readonly MultiDictionary<TypeDefDeclaration, IAnnotationInstance> instancesByType =
            new MultiDictionary<TypeDefDeclaration, IAnnotationInstance>( 256 );

        private readonly MultiDictionary<MetadataDeclaration, IAnnotationInstance> instancesByTarget =
            new MultiDictionary<MetadataDeclaration, IAnnotationInstance>( 256 );

        private TypeHierarchyTask typeHierarchy;

        private readonly Set<ITypeSignature> indexedAnnotationTypes = new Set<ITypeSignature>();
        private readonly Set<TypeDefDeclaration> indexedAnnotationTypeDefs = new Set<TypeDefDeclaration>();

        #endregion

        /// <summary>
        /// Finds the current task in a <see cref="Project"/>.
        /// </summary>
        /// <param name="project">A <see cref="Project"/>.</param>
        /// <returns>The <see cref="IndexGenericInstancesTask"/> contained in the project,
        /// or <b>null</b> if this project does not contai the current task.</returns>
        public static AnnotationRepositoryTask GetTask( Project project )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( project, "project" );

            #endregion

            return (AnnotationRepositoryTask) project.Tasks[taskTypeName];
        }

        private TypeHierarchyTask TypeHierarchy
        {
            get
            {
                if ( typeHierarchy == null )
                {
                    typeHierarchy = TypeHierarchyTask.GetTask( this.Project );

                    if ( typeHierarchy == null )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0040",
                                                          new object[] {"TypeHierarchyTask", taskTypeName} );
                    }
                }

                return typeHierarchy;
            }
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            IEnumerator<IAnnotationProvider> providerEnumerator =
                this.Project.Tasks.GetInterfaces<IAnnotationProvider>();


            while ( providerEnumerator.MoveNext() )
            {
                IEnumerator<IAnnotationInstance> attributeEnumerator =
                    providerEnumerator.Current.GetAnnotations();

                while ( attributeEnumerator.MoveNext() )
                {
                    this.InternalAddAnnotation( attributeEnumerator.Current );
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a custom attribute instance to the current repository.
        /// </summary>
        /// <param name="instance">A new custom attribute instance.</param>
        public void AddAnnotation( IAnnotationInstance instance )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( instance, "instance" );
            ExceptionHelper.AssertArgumentNotNull( instance.TargetElement, "instance.TargetElement" );

            #endregion

            this.InternalAddAnnotation( instance );
        }


        /// <summary>
        /// Called internally to index a custom attribute instance.
        /// </summary>
        /// <param name="attribute">A custom attribute instance.</param>
        private void InternalAddAnnotation( IAnnotationInstance attribute )
        {
            if ( Trace.CustomAttributeDictionaryTask.Enabled )
            {
                Trace.CustomAttributeDictionaryTask.WriteLine(
                    "Adding custom attribute {0} to {1} {2}.",
                    attribute.Value.Constructor.DeclaringType,
                    attribute.TargetElement.GetType().Name,
                    attribute.TargetElement );
            }

            TypeDefDeclaration typeDef = attribute.Value.Constructor.DeclaringType.GetTypeDefinition();
            this.instancesByTarget.Add( attribute.TargetElement, attribute );
            this.instancesByType.Add( typeDef, attribute );
            this.TypeHierarchy.IndexType( typeDef );

            // We should also index the attributes on the attribute and on its parents.
            ITypeSignature attributeType = attribute.Value.Constructor.DeclaringType;
            if ( indexedAnnotationTypes.AddIfAbsent( attributeType ) )
            {
                TypeDefDeclaration attributeTypeDef = attributeType.GetTypeDefinition();

                while ( true )
                {
                    if ( indexedAnnotationTypeDefs.AddIfAbsent( attributeTypeDef ) )
                    {
                        // We do not index the attributes on custom attributes of the current module,
                        // because they are already provided by ModuleAnnotationProvider.
                        if ( attributeTypeDef.Module != this.Project.Module )
                        {
                            foreach (
                                CustomAttributeDeclaration secondLevelAttribute in attributeTypeDef.CustomAttributes
                                )
                            {
                                this.InternalAddAnnotation( secondLevelAttribute );
                            }
                        }

                        if ( attributeTypeDef.BaseType == null )
                            break;
                        else
                            attributeTypeDef = attributeTypeDef.BaseType.GetTypeDefinition();
                    }
                    else break;
                }
            }
        }

        /// <summary>
        /// Removes a custom attribute instance from the current repository.
        /// </summary>
        /// <param name="instance">The custom attribute instance to remove.</param>
        public void RemoveAnnotation( IAnnotationInstance instance )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( instance, "instance" );

            #endregion

            if ( Trace.CustomAttributeDictionaryTask.Enabled )
            {
                Trace.CustomAttributeDictionaryTask.WriteLine(
                    "Removing custom attribute {0} from {1} {2}.",
                    instance.Value.Constructor.DeclaringType,
                    instance.TargetElement.GetType().Name,
                    instance.TargetElement );
            }

            this.instancesByTarget.Remove( instance.TargetElement, instance );
            this.instancesByType.Remove( instance.Value.Constructor.DeclaringType.GetTypeDefinition(), instance );
        }

        /// <summary>
        /// Gets an enumerator of all annotations defined on a target (i.e. on a <see cref="MetadataDeclaration"/>).
        /// </summary>
        /// <param name="target">The declaration whose annotations have to be returned.</param>
        /// <returns>An enumerator of all annotations defined on <paramref name="target"/>.</returns>
        public IEnumerator<IAnnotationInstance> GetAnnotationsOnTarget( MetadataDeclaration target )
        {
            IEnumerable<IAnnotationInstance> enumerable = this.instancesByTarget[target];

            if ( enumerable == null )
                return EmptyEnumerator<IAnnotationInstance>.GetInstance();
            else
                return enumerable.GetEnumerator();
        }


        /// <summary>
        /// Gets all custom attribute instances of a given custom attribute class,
        /// excluding types derived from the requested type.
        /// </summary>
        /// <param name="type">The exact type of requested custom attribute instances.</param>
        /// <returns>An enumerator of all custom attribute instances of the exact
        /// requested type in the module associated by the current 
        /// <see cref="AnnotationRepositoryTask"/>.</returns>
        private IEnumerator<IAnnotationInstance> GetAnnotationsOfPreciseType( TypeDefDeclaration type )
        {
            return this.instancesByType[type].GetEnumerator();
        }

        /// <summary>
        /// Gets all custom attribute instances of a given custom attribute class,
        /// including types derived from the requested type.
        /// </summary>
        /// <param name="type">The base type of requested custom attribute instances.</param>
        /// <returns>An enumerator of all custom attribute instances derived from the
        /// requested type.</returns>
        private IEnumerator<IAnnotationInstance> GetAnnotationsOfDerivedTypes( TypeDefDeclaration type )
        {
            if ( type != null )
            {
                foreach ( IAnnotationInstance attribute in this.instancesByType[type] )
                {
                    yield return attribute;
                }
            }

            IEnumerator<TypeDefDeclaration> typeEnumerator = GetTypeEnumerator( type );
            while ( typeEnumerator.MoveNext() )
            {
                foreach ( IAnnotationInstance attribute in this.instancesByType[typeEnumerator.Current] )
                {
                    yield return attribute;
                }
            }
        }

        /// <summary>
        /// Enumerates all types derived from a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The base type.</param>
        /// <returns>A <see cref="Type"/> enumerator.</returns>
        private static IEnumerator<TypeDefDeclaration> GetTypeEnumerator( TypeDefDeclaration type )
        {
            return TypeHierarchyTask.GetDerivedTypesEnumerator( type, true, null );
        }

        /// <summary>
        /// Gets all custom attribute instances of a given custom attribute class,
        /// given a base <see cref="Type"/>.
        /// </summary>
        /// <param name="type">Base type of requested custom attribute instances.</param>
        /// <param name="inherited">Whether instances of types inherited from <paramref name="type"/>
        /// are requested.</param>
        /// <returns>An enumerator of the requested custom attribute instances.</returns>
        public IEnumerator<IAnnotationInstance> GetAnnotationsOfType( Type type, bool inherited )
        {
            return
                this.GetAnnotationsOfType(
                    this.Project.Module.Domain.FindTypeDefinition( type ),
                    inherited );
        }


        /// <summary>
        /// Gets all custom attribute instances of a given custom attribute class,
        /// given a base <see cref="TypeDefDeclaration"/>.
        /// </summary>
        /// <param name="type">Base type of requested custom attribute instances.</param>
        /// <param name="inherited">Whether instances of types inherited from <paramref name="type"/>
        /// are requested.</param>
        /// <returns>An enumerator of the requested custom attribute instances.</returns>
        public IEnumerator<IAnnotationInstance> GetAnnotationsOfType( TypeDefDeclaration type, bool inherited )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( type, "type" );

            #endregion

            if ( !inherited )
            {
                return this.GetAnnotationsOfPreciseType( type );
            }
            else
            {
                return this.GetAnnotationsOfDerivedTypes( type );
            }
        }
    }
}