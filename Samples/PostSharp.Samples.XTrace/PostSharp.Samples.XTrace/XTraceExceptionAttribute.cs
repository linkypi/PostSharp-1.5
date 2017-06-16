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
    /// Custom attribute that, when applied (or multicasted) to a method, 
    /// writes an error record to the <see cref="Trace"/> class every time
    /// this method exits when an exception.
    /// </summary>
    [Serializable]
    public sealed class XTraceExceptionAttribute : OnExceptionAspect
    {
        /* Note that all these fields are initialized at compile time
         * and are deserialized at runtime.                             */
        private string prefix;
        private MethodFormatStrings formatStrings;

        // This field is not serialized. It is used only at compile time.
        [NonSerialized] private readonly Type exceptionType;

        /// <summary>
        /// Declares a <see cref="XTraceExceptionAttribute"/> custom attribute
        /// that logs every exception flowing out of the methods to which
        /// the custom attribute is applied.
        /// </summary>
        public XTraceExceptionAttribute()
        {
        }

        /// <summary>
        /// Declares a <see cref="XTraceExceptionAttribute"/> custom attribute
        /// that logs every exception derived from a given <see cref="Type"/>
        /// flowing out of the methods to which
        /// the custom attribute is applied.
        /// </summary>
        /// <param name="exceptionType"></param>
        public XTraceExceptionAttribute( Type exceptionType )
        {
            this.exceptionType = exceptionType;
        }

        /// <summary>
        /// Gets or sets the prefix string, printed before every trace message.
        /// </summary>
        /// <value>
        /// For instance <c>[Exception]</c>.
        /// </value>
        public string Prefix { get { return this.prefix; } set { this.prefix = value; } }

        /// <summary>
        /// Initializes the current object. Called at compile time by PostSharp.
        /// </summary>
        /// <param name="method">Method to which the current instance is
        /// associated.</param>
        public override void CompileTimeInitialize( MethodBase method )
        {
            // We just initialize our fields. They will be serialized at compile-time
            // and deserialized at runtime.
            this.formatStrings = Formatter.GetMethodFormatStrings( method );
            this.prefix = Formatter.NormalizePrefix( this.prefix );
        }

        public override Type GetExceptionType( MethodBase method )
        {
            return this.exceptionType;
        }

        /// <summary>
        /// Method executed when an exception occurs in the methods to which the current
        /// custom attribute has been applied. We just write a record to the tracing
        /// subsystem.
        /// </summary>
        /// <param name="context">Event arguments specifying which method
        /// is being called and with which parameters.</param>
        public override void OnException( MethodExecutionEventArgs context )
        {
            Trace.TraceError( "{0}Exception {1} {{{2}}} in {{{3}}}.",
                              this.prefix,
                              context.Exception.GetType().Name,
                              context.Exception.Message,
                              this.formatStrings.Format( context.Instance, context.Method, context.GetReadOnlyArgumentArray() ) );
        }
    }
}