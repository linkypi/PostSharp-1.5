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
using System.Diagnostics;
using System.Reflection;
using PostSharp.Laos;

namespace PostSharp.Samples.XTrace
{
    /// <summary>
    /// Custom attribute that, when applied (or multicasted) to a method, writes
    /// an informational record into the <see cref="Trace"/> class before and
    /// after its execution.
    /// </summary>
    [Serializable]
    public sealed class XTraceMethodInvocationAttribute : OnMethodInvocationAspect
    {
        /* Note that the following fields are initialized at compile time
         * and are deserialized at runtime.                             */
        private string prefix;
        private MethodFormatStrings formatStrings;
        private bool isVoid;

        // This is the method to which the attribute was applied. We want to compare
        // it with the delegate target given at call time in order to detect
        // virtual calls.
        [NonSerialized] private MethodBase method;

        /// <summary>
        /// Gets or sets the prefix string, printed before every trace message.
        /// </summary>
        /// <value>
        /// For instance <c>[Trace]</c>.
        /// </value>
        public string Prefix { get { return this.prefix; } set { this.prefix = value; } }

        /// <summary>
        /// Initializes the current object. Called at compile time by PostSharp.
        /// </summary>
        /// <param name="method">Method to which the current instance is
        /// associated.</param>
        public override void CompileTimeInitialize( MethodBase method )
        {
            this.prefix = Formatter.NormalizePrefix( this.prefix );
            this.formatStrings = Formatter.GetMethodFormatStrings( method );
            MethodInfo methodInfo = method as MethodInfo;
            if ( methodInfo != null )
            {
                this.isVoid = methodInfo.ReturnType == typeof(void);
            }
            else
            {
                this.isVoid = true;
            }
        }


        /// <summary>
        /// Initializes the current object. Called at runtime by PostSharp.
        /// </summary>
        /// <param name="method">Method to which the current instance is
        /// associated.</param>
        public override void RuntimeInitialize( MethodBase method )
        {
            // Here we have the runtime MethodBase object. We can save it in a transient field.
            this.method = method;
        }

        /// <summary>
        /// Method called instead of the intercepted method.
        /// </summary>
        /// <param name="context">Event arguments specifying which method
        /// is being executed and which are its arguments. The implementation
        /// should set the return value and ouput arguments.</param>
        public override void OnInvocation( MethodInvocationEventArgs context )
        {
            string methodName = this.formatStrings.Format( context.Delegate.Target,
                                                           context.Delegate.Method,
                                                           context.GetArgumentArray() ) +
                                ( context.Delegate.Method == this.method
                                      ? ""
                                      :
                                          " [overriden by class " + context.Delegate.Method.DeclaringType.FullName + "]" );

            Trace.TraceInformation( this.prefix + "Entering " + methodName );
#if !MONO
            Trace.Indent();
#endif

            try
            {
                context.Proceed();
            }
            catch ( Exception e )
            {
                Trace.Unindent();

                Trace.TraceWarning(
                    Formatter.FormatString( "{0}Leaving {{{1}}} with exception {2}: {{{3}}}.",
                                            this.prefix, methodName, e.GetType().Name, e.Message ) );
                throw;
            }

#if !MONO
            Trace.Unindent();
#endif
            
            Trace.TraceInformation(
                this.prefix + "Leaving " +
                methodName + ( this.isVoid ? "" : Formatter.FormatString( " : {{{0}}}.", context.ReturnValue ) ) );
        }
    }
}
