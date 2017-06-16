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

using System.Collections.Generic;
using PostSharp.CodeModel;

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// Tasks that detects automatically other tasks required by the module on the base
    /// of custom attributes annotated with the <see cref="RequirePostSharpAttribute"/>
    /// custom attribute.
    /// </summary>
    public sealed class AutoDetectTask : Task
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            int taskCount = 0, pluginCount = 0;

            #region Process attributes defined in linked assemblies

            /*
            AssemblyName[] referencedAssemblies =
                this.Project.Module.Assembly.GetSystemAssembly().GetReferencedAssemblies();

            for ( int i = 0 ; i < referencedAssemblies.Length ; i++ )
            {
                Assembly referencedAssembly =
                    AssemblyLoadHelper.LoadAssemblyFromName( this.Project.Module.Domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName( referencedAssemblies[i].FullName ) );

                foreach (
                    ReferencingAssembliesRequirePostSharpAttribute attribute in
                        referencedAssembly.GetCustomAttributes( typeof(ReferencingAssembliesRequirePostSharpAttribute),
                                                                false ) )
                {
                    if ( !this.Project.PlugIns.Contains( attribute.PlugIn ) )
                    {
                        if ( !this.Project.AddPlugIn( attribute.PlugIn ) )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Fatal,
                                                              "PS0058",
                                                              new object[]
                                                                  {attribute.PlugIn, referencedAssembly.FullName} );
                        }

                        pluginCount++;
                    }

                    if ( this.Project.Tasks[attribute.Task] == null )
                    {
                        Trace.AutoDetectTask.WriteLine(
                            "Task {{{0}}} detected because of the attribute ReferencingAssembliesRequirePostSharpAttribute in assembly {{{1}}}.",
                            attribute.Task, referencedAssembly.FullName );
                        this.Project.Tasks.Add( attribute.Task );

                        taskCount++;
                    }
                }
            }
            */

            #endregion

            #region Process attributes defined in the current assembly

            AnnotationRepositoryTask annotations = AnnotationRepositoryTask.GetTask( this.Project );

            if ( annotations == null || annotations.State != TaskState.Executed )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0040",
                                                  new object[] {"AnnotationRepositoryTask", "AutoDetect"} );
            }


            ModuleDeclaration module = this.Project.Module;

            TypeDefDeclaration requirePostSharpAttributeType =
                module.GetTypeForFrameworkVariant( typeof(RequirePostSharpAttribute) ).GetTypeDefinition();


            IEnumerator<IAnnotationInstance> requirePostSharpAnnotationsEnumerator =
                annotations.GetAnnotationsOfType( requirePostSharpAttributeType, false );

            while ( requirePostSharpAnnotationsEnumerator.MoveNext() )
            {
                IAnnotationInstance requirePostSharpAnnotation = requirePostSharpAnnotationsEnumerator.Current;
                TypeDefDeclaration annotationTypeRequiringPostSharp =
                    (TypeDefDeclaration) requirePostSharpAnnotation.TargetElement;

                IEnumerator<IAnnotationInstance> annotationsRequiringPostSharpEnumerator =
                    annotations.GetAnnotationsOfType( annotationTypeRequiringPostSharp, true );

                while ( annotationsRequiringPostSharpEnumerator.MoveNext() )
                {
                    string plugIn = (string) requirePostSharpAnnotation.Value.ConstructorArguments[0].Value.Value;
                    string task = (string) requirePostSharpAnnotation.Value.ConstructorArguments[1].Value.Value;

                    Trace.AutoDetectTask.WriteLine(
                        "The custom attribute {2} requires plug-in {0} and task {1}.",
                        plugIn, task, annotationTypeRequiringPostSharp.Name );

                    if ( !this.Project.AddPlugIn( plugIn ) )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Fatal,
                                                          "PS0052",
                                                          new object[]
                                                              {plugIn, annotationTypeRequiringPostSharp.Name} );
                    }

                    if ( this.Project.Tasks[task] == null )
                    {
                        this.Project.Tasks.Add( task );
                    }


                    taskCount++;

                    pluginCount++;

                    break; // Break, because we do not to have a relevant type any more.
                }
            }

            #endregion

            Trace.AutoDetectTask.WriteLine( "Detected {0} plugin(s) and {1} task(s).", pluginCount, taskCount );

            return true;
        }
    }
}