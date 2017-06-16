#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;

namespace PostSharp.Samples.Trace
{
    /// <summary>
    /// Implementation of <see cref="TraceAttribute"/>. Weaves tracing code into procedures.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     This task implements the <see cref="IAdviceProvider"/> interface. The <see cref="WeaverTask"/>
    ///     class calls our <see cref="ProvideAdvices"/> method, which emits an advice for each traced method. 
    ///     Then, the <see cref="CodeWeaver"/> class calls our advice class.
    /// </para>
    /// <para>
    ///     Each instance of the <see cref="TraceAttribute"/> custom attribute is associated to an advice. The join points
    ///     are the entry and the exit of the body of all methods having the <see cref="TraceAttribute"/>.
    /// </para>
    /// </remarks>
    public class TraceTask : Task, IAdviceProvider
    {
        internal IMethod TraceWriteLineMethod;
        internal IMethod TraceIndentMethod;
        internal IMethod TraceUnindentMethod;

        /// <summary>
        /// Initializes the task.
        /// </summary>
        /// <remarks>
        /// This method is called once per module. It caches what it can. Concretely, this method looks 
        /// for the methods <b>System.Diagnostics.Trace.*</b>.
        /// </remarks>
        protected override void Initialize()
        {
            ModuleDeclaration module = this.Project.Module;

            this.TraceWriteLineMethod = module.FindMethod( typeof(System.Diagnostics.Trace).GetMethod( "WriteLine",
                                                                                                       new Type[]
                                                                                                           {
                                                                                                               typeof(
                                                                                                                   string
                                                                                                                   ),
                                                                                                               typeof(
                                                                                                                   string
                                                                                                                   )
                                                                                                           } ),
                                                           BindingOptions.Default );

            this.TraceIndentMethod = module.FindMethod( typeof(System.Diagnostics.Trace).GetMethod( "Indent",
                                                                                                    new Type[] {} ),
                                                        BindingOptions.Default );

            this.TraceUnindentMethod = module.FindMethod( typeof(System.Diagnostics.Trace).GetMethod( "Unindent",
                                                                                                      new Type[] {} ),
                                                          BindingOptions.Default );
        }

        public void ProvideAdvices( Weaver codeWeaver )
        {
            // Gets the dictionary of custom attributes.
            AnnotationRepositoryTask annotationRepository =
                AnnotationRepositoryTask.GetTask(this.Project);

            // Requests an enumerator of all instances of our TraceAttribute.
            IEnumerator<IAnnotationInstance> customAttributeEnumerator =
                annotationRepository.GetAnnotationsOfType( typeof(TraceAttribute), false );

            // For each instance of our TraceAttribute.
            while ( customAttributeEnumerator.MoveNext() )
            {
                // Gets the method to which it applies.
                MethodDefDeclaration methodDef = customAttributeEnumerator.Current.TargetElement
                                                 as MethodDefDeclaration;

                if ( methodDef != null )
                {
                    // Constructs a custom attribute instance.
                    TraceAttribute attribute =
                        (TraceAttribute)
                        CustomAttributeHelper.ConstructRuntimeObject( customAttributeEnumerator.Current.Value,
                                                                      this.Project.Module );

                    // Build an advice based on this custom attribute.
                    TraceAdvice advice = new TraceAdvice( this, attribute );

                    codeWeaver.AddMethodLevelAdvice( advice,
                                                     new Singleton<MethodDefDeclaration>( methodDef ),
                                                     JoinPointKinds.BeforeMethodBody |
                                                     JoinPointKinds.AfterMethodBodyAlways,
                                                     null );
                }
            }
        }
    }
}