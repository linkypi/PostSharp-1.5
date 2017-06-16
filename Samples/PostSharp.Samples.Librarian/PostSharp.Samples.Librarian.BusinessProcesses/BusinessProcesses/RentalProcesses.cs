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
using PostSharp.Samples.Librarian.Data;
using PostSharp.Samples.Librarian.Entities;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.BusinessProcesses
{
    [Trace]
    internal class RentalProcesses : SessionBoundService, IRentalProcesses
    {
        private readonly CustomerProcesses customerProcesses;

        public RentalProcesses( ServerSession session )
            : base( session )
        {
            this.customerProcesses = new CustomerProcesses( session );
        }

        [Transaction]
        public Rental OpenRental( Rental rental )
        {
            if ( rental == null )
                throw new ArgumentNullException( "rental" );

            // Check business rules.
            BusinessRulesManager.Assert( "OpenRental", rental );

            StorageContext.Current.Insert( rental );

            return rental;
        }

        [Transaction]
        public void ReportLostBook( EntityRef<Rental> rentalRef )
        {
            if ( rentalRef.IsNull )
                throw new ArgumentNullException( "rentalRef" );

            Rental rental = rentalRef.GetVanillaEntity();
            Book book = rental.Book.GetVanillaEntity();

            // Check business rules.
            BusinessRulesManager.Assert( "ReportLostBook", rental );

            // Apply the penalty.
            CustomerAccountLine penalty = new CustomerAccountLine();
            penalty.Customer = rental.Customer;
            penalty.Employee = this.Session.Employee;
            penalty.Rental = rentalRef;
            penalty.Date = DateTime.Today;
            penalty.Amount = -book.LostPenalty;
            penalty.Description = string.Format(
                "Lost of the book [{0}; {1}]",
                book.Authors,
                book.Title );
            this.customerProcesses.AddCustomerAccountLine( penalty );

            // Create a note for the penalty.
            Note note = new Note();
            note.Owner = rental;
            note.Employee = this.Session.Employee;
            note.Date = DateTime.Now;
            note.Title = "Penalty Applied";
            note.Text = string.Format(
                "A penalty of {2} EUR was applied for the lost of the book [{0}; {1}]",
                book.Authors,
                book.Title,
                -penalty.Amount );
            StorageContext.Current.Insert( note );

            // Update the rental.
            rental.ReturnDate = DateTime.Now;
            rental.Closed = true;
            StorageContext.Current.Update( rental );

            // Delete the book.
            book.Deleted = true;
            StorageContext.Current.Update( book );
        }

        [Transaction]
        public void ReturnBook( EntityRef<Rental> rentalRef )
        {
            if ( rentalRef.IsNull )
                throw new ArgumentNullException( "rentalRef" );

            Rental rental = rentalRef.GetVanillaEntity();

            // Check business rules.
            BusinessRulesManager.Assert( "ReturnBook", rental );

            // Check if the book has been returned in time and apply penalty.
            if ( DateTime.Today > rental.ScheduledReturnDate )
            {
                int delay = (int) Math.Ceiling( ( DateTime.Today - rental.ScheduledReturnDate ).TotalDays );

                CustomerAccountLine penalty = new CustomerAccountLine();
                penalty.Customer = rental.Customer;
                penalty.Employee = this.Session.Employee;
                penalty.Rental = rentalRef;
                penalty.Date = DateTime.Today;
                penalty.Amount = -0.1M*delay;
                penalty.Description = string.Format(
                    "Delay of {0} day(s) while returning the book [{1}; {2}]",
                    delay,
                    rental.Book.Entity.Authors,
                    rental.Book.Entity.Title );

                this.customerProcesses.AddCustomerAccountLine( penalty );


                Note note = new Note();
                note.Owner = rental;
                note.Title = "Penalty Applied";
                note.Employee = this.Session.Employee;
                note.Date = DateTime.Now;
                note.Text = string.Format(
                    "A penalty of {3} EUR was applied for a {0}-day(s) delay while returning the book [{1}; {2}]",
                    delay,
                    rental.Book.Entity.Authors,
                    rental.Book.Entity.Title,
                    -penalty.Amount );
                StorageContext.Current.Insert( note );
            }

            rental.ReturnDate = DateTime.Now;
            rental.Closed = true;
            StorageContext.Current.Update( rental );
        }
    }
}
