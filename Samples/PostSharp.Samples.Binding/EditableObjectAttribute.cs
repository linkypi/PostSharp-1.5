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
using System.ComponentModel;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;

namespace PostSharp.Samples.Binding
{
    /// <summary>
    /// Custom attribute that, when applied on a class designated as the <i>target class</i>, 
    /// implements the <see cref="IEditableObject"/> on this target class. This interface
    /// enables working on a working object of instances of the target class, and use
    /// the <see cref="IEditableObject.EndEdit"/> and <see cref="IEditableObject.CancelEdit"/>"
    /// methods to commit or rollback changes, respectively.
    /// </summary>
    /// <remarks>
    /// This compound aspect is made of a type-level composition aspect that implements the
    /// <see cref="IEditableObject"/> interface and, for each instance field, of an on-field-access
    /// aspect. Note that the original field is removed: instance storage is replaced by
    /// a dictionary.
    /// </remarks>
    [MulticastAttributeUsage(MulticastTargets.Class)]
    public sealed class EditableObjectAttribute : CompoundAspect
    {
        /// <summary>
        /// Method called at compile time to get individual aspects required by the current compound
        /// aspect.
        /// </summary>
        /// <param name="element">Metadata element (<see cref="Type"/> in our case) to which
        /// the current custom attribute instance is applied.</param>
        /// <param name="collection">Collection of aspects to which individual aspects should be
        /// added.</param>
        public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
        {
            // Get the target type.
            Type targetType = (Type)element;

            // On the type, add a Composition aspect to implement the INotifyPropertyChanged interface.
            collection.AddAspect(targetType, new AddEditableObjectInterfaceSubAspect());

            // Add a OnFieldAccess on each writable non-static field.
            foreach (
                FieldInfo field in
                    targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.DeclaringType == targetType && (field.Attributes & FieldAttributes.InitOnly) == 0)
                {
                    collection.AddAspect(field, new OnFieldAccessSubAspect());
                }
            }
        }

        /// <summary>
        /// Implementation of <see cref="CompositionAspect"/> that adds the <see cref="IEditableObject"/>
        /// interface to the type to which it is applied.
        /// </summary>
        [Serializable]
        internal class AddEditableObjectInterfaceSubAspect : CompositionAspect
        {
            /// <summary>
            /// Called at runtime, creates the implementation of the <see cref="IEditableObject"/> interface.
            /// </summary>
            /// <param name="eventArgs">Execution context.</param>
            /// <returns>A new instance of <see cref="EditableObjectImplementation"/>, which implements
            /// <see cref="IEditableObject"/>.</returns>
            public override object CreateImplementationObject(InstanceBoundLaosEventArgs eventArgs)
            {
                return new EditableObjectImplementation(eventArgs.Instance as IProtectedInterface<IFirePropertyChanged>, eventArgs.InstanceCredentials);
            }

            /// <summary>
            /// Called at compile-time, gets the interface that should be publicly exposed.
            /// </summary>
            /// <param name="containerType">Type on which the interface will be implemented.</param>
            /// <returns></returns>
            public override Type GetPublicInterface(Type containerType)
            {
                return typeof(IEditableObject);
            }

            /// <summary>
            /// Gets weaving options.
            /// </summary>
            /// <returns>Weaving options specifying that the implementation accessor interface (<see cref="IComposed{T}"/>)
            /// should be exposed, and that the implementation of interfaces should be silently ignored if they are
            /// already implemented in the parent types.</returns>
            public override CompositionAspectOptions GetOptions()
            {
                return
                    CompositionAspectOptions.GenerateImplementationAccessor |
                    CompositionAspectOptions.IgnoreIfAlreadyImplemented;
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IEditableObject"/> interface.
        /// </summary>
        /// <remarks>
        /// This implementation object also contains two dictionaries for the working copy and the backup copy
        /// of field values.
        /// </remarks>
        internal class EditableObjectImplementation : IEditableObject
        {
            private readonly InstanceCredentials credentials;
            private readonly IProtectedInterface<IFirePropertyChanged> parentObject;
            private bool editable;
            private readonly Dictionary<string, object> backupCopy = new Dictionary<string, object>();
            private readonly Dictionary<string, object> workingCopy = new Dictionary<string, object>();

            public EditableObjectImplementation( IProtectedInterface<IFirePropertyChanged> parentObject,
                InstanceCredentials credentials )
            {
                this.credentials = credentials;
                this.parentObject = parentObject;
            }


            /// <summary>
            /// Sets the value of an instance field.
            /// </summary>
            /// <param name="fieldName">Field name.</param>
            /// <param name="value">Field value.</param>
            public void SetValue(string fieldName, object value)
            {
                // Set the field value.
                this.workingCopy[fieldName] = value;
            }

            /// <summary>
            /// Gets the value of an instance field.
            /// </summary>
            /// <param name="fieldName">Field name.</param>
            /// <see cref="SetValue"/> method).</param>
            /// <returns>The value of the field <paramref name="fieldName"/>, or <b>null</b> if this field
            /// has not been set before.</returns>
            public object GetValue( string fieldName )
            {
                object value;
                this.workingCopy.TryGetValue( fieldName, out value );
                return value;
            }

            #region IEditableObject Members

            /// <summary>
            /// Copies one dictionary into another.
            /// </summary>
            /// <param name="source">Source dictionary.</param>
            /// <param name="target">Target dictionary.</param>
            private static void Copy(Dictionary<string, object> source, Dictionary<string, object> target)
            {
                target.Clear();
                foreach (KeyValuePair<string, object> pair in source)
                {
                    target.Add(pair.Key, pair.Value);
                }
            }

            /// <summary>
            /// Begins editing and backup current field values.
            /// </summary>
            public void BeginEdit()
            {
                if (this.editable)
                    throw new InvalidOperationException();

                Copy(workingCopy, backupCopy);
                this.editable = true;
            }

            /// <summary>
            /// Cancels editing and rolls back changes.
            /// </summary>
            public void CancelEdit()
            {
                if (!this.editable)
                    throw new InvalidOperationException();

                Copy(backupCopy, workingCopy);
                this.editable = false;

                if (this.parentObject!= null)
                {
                    this.parentObject.GetInterface(this.credentials).OnPropertyChanged(null);
                }
            }

            /// <summary>
            /// Ends editing and accept new field values.
            /// </summary>
            public void EndEdit()
            {
                if (!this.editable)
                    throw new InvalidOperationException();

                this.editable = false;
            }

            #endregion
        }

        /// <summary>
        /// Aspect that replaces field get/set operations to calls to <see cref="EditableObjectImplementation"/>.
        /// </summary>
        [Serializable]
        internal class OnFieldAccessSubAspect : OnFieldAccessAspect
        {
            private string fieldName;

            public OnFieldAccessSubAspect()
            {
                this.AspectPriority = int.MinValue;
            }

            /// <summary>
            /// Called at compile-time, initializes the current instance with
            /// values that depends on the fields to which the current sub-aspect is applied.
            /// We basically remember the field name and its default value.
            /// </summary>
            /// <param name="field">Field to which the current sub-aspect is applied.</param>
            public override void CompileTimeInitialize(FieldInfo field)
            {
                base.CompileTimeInitialize(field);
                this.fieldName = field.DeclaringType.FullName + "::" + field.Name;
            }

            /// <summary>
            /// Method called at runtime instead of the <b>get field</b> operation.
            /// </summary>
            /// <param name="eventArgs">Information about the current execution context.</param>
            public override void OnGetValue(FieldAccessEventArgs eventArgs)
            {
                // Get the EditableObjectImplementation object for the object instance.
                EditableObjectImplementation impl =
                    (EditableObjectImplementation)
                    ((IComposed<IEditableObject>)eventArgs.Instance).GetImplementation(eventArgs.InstanceCredentials);

                // Let EditableObjectImplementation returns the value.
                eventArgs.ExposedFieldValue = impl.GetValue(this.fieldName);
            }

            /// <summary>
            /// Method called at runtime instead of the <b>set field</b> operation.
            /// </summary>
            /// <param name="eventArgs">Information about the current execution context.</param>
            public override void OnSetValue(FieldAccessEventArgs eventArgs)
            {
                // Get the EditableObjectImplementation object for the object instance.
                EditableObjectImplementation impl =
                    (EditableObjectImplementation)
                    ((IComposed<IEditableObject>)eventArgs.Instance).GetImplementation(eventArgs.InstanceCredentials);

                // Set the value inside EditableObjectImplementation.
                impl.SetValue(this.fieldName, eventArgs.ExposedFieldValue);
            }

            /// <summary>
            /// Gets weaving options.
            /// </summary>
            /// <returns>An option meaning that we want the physical field to be removed (since we replace all fields by a dictionary).</returns>
            public override OnFieldAccessAspectOptions GetOptions()
            {
                return OnFieldAccessAspectOptions.RemoveFieldStorage;
            }
        }
    }
}