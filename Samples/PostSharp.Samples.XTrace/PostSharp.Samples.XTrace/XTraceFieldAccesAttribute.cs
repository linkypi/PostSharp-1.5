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
    /// Custom attribute that, when applied (or multicasted) to a field, 
    /// writes an informative record
    /// to the <see cref="Trace"/> class every time a this field is read or written.
    /// </summary>
    [Serializable]
    public sealed class XTraceFieldAccessAttribute : OnFieldAccessAspect
    {
        /* Note that all these fields are initialized at compile time
         * and are deserialized at runtime.                             */
        private string prefix;
        private string typeFormatString;
        private string getFormatString;
        private string setFormatString;

        /// <summary>
        /// Gets or sets the prefix string, printed before every trace message.
        /// </summary>
        /// <value>
        /// For instance <c>[FieldAccess]</c>.
        /// </value>
        public string Prefix { get { return this.prefix; } set { this.prefix = value; } }

        /// <summary>
        /// Initializes the current object. Called at compile time by PostSharp.
        /// </summary>
        /// <param name="field">Field to which the current instance is
        /// associated.</param>
        public override void CompileTimeInitialize( FieldInfo field )
        {
            // We just initialize our fields. They will be serialized at compile-time
            // and deserialized at runtime.
            this.typeFormatString = Formatter.GettypeFormatString( field.DeclaringType );

            if ( field.IsStatic )
            {
                this.getFormatString = string.Format(
                    "::{0}.get() : {{{{{{1}}}}}}.", field.Name );
                this.setFormatString = string.Format(
                    "::{0}.set( {{{{{{1}}}}}} ).", field.Name );
            }
            else
            {
                this.getFormatString = string.Format(
                    "{{{{{{0}}}}}}::{0}.get() : {{{{{{1}}}}}}.", field.DeclaringType.FullName, field.Name );
                this.setFormatString = string.Format(
                    "{{{{{{0}}}}}}::{0}.set( {{{{{{1}}}}}} )", field.DeclaringType.FullName, field.Name );
            }

            this.prefix = Formatter.NormalizePrefix( this.prefix );
        }

        /// <summary>
        /// Method called instead of the <i>get</i> operation on the modified field. We
        /// just write a record to the trace subsystem and return the field value.
        /// </summary>
        /// <param name="context">Event arguments specifying which field is being
        /// accessed and which is its current value.</param>
        public override void OnGetValue( FieldAccessEventArgs context )
        {
            base.OnGetValue( context );

            Trace.TraceInformation( this.prefix +
                                    Formatter.FormatString( this.typeFormatString,
                                                            context.DeclaringType.GetGenericArguments() ) +
                                    Formatter.FormatString( this.getFormatString, context.Instance,
                                                            context.StoredFieldValue ) );
        }


        /// <summary>
        /// Method called instead of the <i>set</i> operation on the modified field. 
        /// We just write a record to the trace subsystem and set the field value.
        /// </summary>
        /// <param name="context">Event arguments specifying which field is being
        /// accessed and which is its current value, and allowing to change its value.
        /// </param>
        public override void OnSetValue( FieldAccessEventArgs context )
        {
            base.OnSetValue( context );

            Trace.TraceInformation( this.prefix +
                                    Formatter.FormatString( this.typeFormatString,
                                                            context.DeclaringType.GetGenericArguments() ) +
                                    Formatter.FormatString( this.setFormatString, context.Instance,
                                                            context.StoredFieldValue ) );
        }
    }
}