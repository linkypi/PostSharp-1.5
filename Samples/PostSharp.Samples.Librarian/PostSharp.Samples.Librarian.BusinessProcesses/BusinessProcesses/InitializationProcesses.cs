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

using PostSharp.Samples.Librarian.Data;
using PostSharp.Samples.Librarian.Entities;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.BusinessProcesses
{
    [Trace]
    public class InitializationProcesses
    {
        [Transaction]
        public void CheckDatabaseInitialized()
        {
            if ( !StorageContext.Current.Exists<Employee>( null ) )
            {
                Employee employee = new Employee();
                employee.FirstName = "Initial";
                employee.LastName = "Employee";
                employee.Login = "init";
                employee.SetPassword( "init" );
                StorageContext.Current.Insert( employee );
            }

            if ( !StorageContext.Current.Exists<Cashbox>( null ) )
            {
                Cashbox cashbox = new Cashbox();
                cashbox.CashboxId = "1";
                cashbox.Name = "My Cashbox";
                cashbox.Balance = 0;
                StorageContext.Current.Insert( cashbox );
            }
        }
    }
}