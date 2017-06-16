#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

#if !CF
using System;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Custom attribute that, when applied to a method, intercepts all calls to this method.
    /// The aspect implementation receives a delegate to the called method and the array of
    /// arguments.
    /// </summary>
    /// <remarks>
    /// <para>This aspect creates a stub for each method to which it is applied. The stub
    /// instantiates a delegate to the original method, then it calls the <see cref="OnInvocation"/> 
    /// method of the aspect implementation and passes the delegate as a part of the event
    /// arguments (<see cref="MethodInvocationEventArgs"/>).
    /// </para>
    /// <para>This aspect can be woven at two location:</para>
    /// <list type="table">
    ///     <item>
    ///         <term>Call Site</term>
    ///         <description>Each <c>call</c> instruction to the aspected method
    ///         is replaced by a call to our stub. This allows to intercept calls
    ///         to methods defined in another assembly than the current one.
    ///         However, since it does not modify the target method, calls from
    ///         other assemblies will not be intercepted. This weaving site
    ///         is generally not useful when applied to methods defined in the
    ///         current assembly.</description>
    ///     </item>
    ///     <item>
    ///         <term>Target Site</term>
    ///         <description>The method itself is modified, so the aspect will
    ///         be applied even when the method will be called from a different
    ///         assembly. Actually, the body of the original method is moved
    ///         into a new method, and the original method is replaced by
    ///         the stub. That is, the original method becomes the stub and the
    ///         new method contain the original method body, 
    ///         By default, all custom attributes defined on the original
    ///         method are moved to the new method. The <see cref="ImplementationBoundAttributeAttribute"/>
    ///         custom attribute controls which custom attribute should stay on
    ///         the original method.</description>
    ///     </item>
    /// </list>
    /// <para>
    /// The default behavior is to use the <b>Call Site</b> strategy for methods defined
    /// in an external assembly, and <b>Target Site</b> for methods defined in the
    /// current assembly. This behavior can be changed by overwriting the <see cref="GetOptions"/>
    /// method.
    /// </para>
    /// </remarks>
#if !SMALL
    [Serializable]
#endif
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class |
                     AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property |
                     AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false )]
    [MulticastAttributeUsage( MulticastTargets.Method, TargetTypeAttributes = MulticastAttributes.UserGenerated,
        AllowExternalAssemblies = true, AllowMultiple = true )]
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    public abstract class OnMethodInvocationAspect : LaosMethodLevelAspect, IOnMethodInvocationAspect
#if !SMALL
, IOnMethodInvocationAspectConfiguration
#endif
    {
        /// <inheritdoc />
        public virtual void OnInvocation( MethodInvocationEventArgs eventArgs )
        {
            eventArgs.Proceed();
        }

#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual OnMethodInvocationAspectOptions GetOptions()
        {
            return OnMethodInvocationAspectOptions.None;
        }

        /// <inheritdoc />
        [CompileTimeSemantic]
        public virtual InstanceTagRequest GetInstanceTagRequest()
        {
            return null;
        }
#endif
    }
}

#endif