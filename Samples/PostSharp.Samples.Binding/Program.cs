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
using System.ComponentModel;

namespace PostSharp.Samples.Binding
{
    internal class Program
    {
        private static void Main()
        {
            Customer customer = new Customer();

            // Get the INotifyPropertyChanged interface for the new customer.
            // Note the post-compilation cast.
            INotifyPropertyChanged observable = Post.Cast<Customer, INotifyPropertyChanged>( customer );

            // Now that we have INotifyPropertyChanged, we can register our PropertyChanged event handler.
            Console.WriteLine( "We register an event handler for the PropertyChanged event," +
                               " so we will be notified when a property will change." );
            observable.PropertyChanged += Program_PropertyChanged;

            Console.WriteLine(
                "We initialize the customer name to 'Sylvestre' when the object is not in editable mode. " +
                " Changes are 'committed' automatically." );

            // We update the object outside a transaction. The change is accepted automatically.
            customer.FirstName = "Sylvestre";

            Console.WriteLine( "Now we enter edit mode and we change the name to 'John' " +
                               "and the SegmentId to 5." );

            // Start editing. Note the post-compilation cast again.
            Post.Cast<Customer, IEditableObject>( customer ).BeginEdit();

            // This should cause the PropertyChanged to be raised.
            customer.FirstName = "John";
            customer.SegmentId = 5;

            // Cancel changes.
            Console.WriteLine("We cancel.");
         
            Post.Cast<Customer, IEditableObject>( customer ).CancelEdit();

            Console.WriteLine( "After cancel, customer.FirstName = {{{0}}}, customer.SegmentId = {{{1}}}.",
                               customer.FirstName, customer.SegmentId );
        }

        // Called when the PropertyChanged is invoked on the Customer entity.
        private static void Program_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Console.WriteLine( "Property changed: {0}.", e.PropertyName ?? "ALL" );
        }
    }
}