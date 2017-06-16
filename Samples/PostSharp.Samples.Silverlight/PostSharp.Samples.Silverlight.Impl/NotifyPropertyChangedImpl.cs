#region Released to Public Domain by Gael Fraiteur

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
using System.ComponentModel;
using System.Reflection;
using PostSharp.Laos;
using PostSharp.Reflection;

namespace PostSharp.Samples.Silverlight.Impl
{
    /// <summary>
    /// Custom attribute that, when applied on a type (designated <i>target type</i>), implements the interface
    /// <see cref="INotifyPropertyChanged"/> and raises the <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event when any property of the target type is modified.
    /// </summary>
    /// <remarks>
    /// Event raising is implemented by appending logic to the <b>set</b> accessor of properties. The 
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> is raised only when accessors successfully complete.
    /// </remarks>
    public sealed class NotifyPropertyChangedImpl : IExternalAspectImplementation
    {
        public void ImplementAspect(object target, IObjectConstruction aspectData,
                                    LaosReflectionAspectCollection collection)
        {
            // Get the target type.
            Type targetType = (Type) target;

            // On the type, add a Composition aspect to implement the INotifyPropertyChanged interface.

            collection.AddAspectConstruction(
                targetType,
                new ObjectConstruction(
                    "PostSharp.Samples.Silverlight.Aspects.Impl.NotifyPropertyChangedCompositionAdvice, PostSharp.Samples.Silverlight.Aspects"),
                null);

            // Add a OnMethodBoundaryAspect on each writable non-static property.
            foreach (PropertyInfo property in targetType.GetProperties())
            {
                if (property.DeclaringType.Equals(targetType) && property.CanWrite)
                {
                    MethodInfo method = property.GetSetMethod(true);

                    if (!method.IsStatic)
                    {
                        collection.AddAspectConstruction(
                            method,
                            new ObjectConstruction(
                                "PostSharp.Samples.Silverlight.Aspects.Impl.NotifyPropertyChangedSetterAdvice, PostSharp.Samples.Silverlight.Aspects",
                                property.Name),
                            null);
                    }
                }
            }
        }
    }
}