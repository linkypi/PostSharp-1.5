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

using System.Collections.Generic;
using PostSharp.Samples.Librarian.Entities;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.BusinessProcesses
{
    public interface ICustomerProcesses : IStatefulService
    {
        Customer CreateCustomer( Customer customer );
        void DeleteCustomer( EntityRef<Customer> customerRef );
        void UpdateCustomer( Customer customer );
        void AddCustomerAccountLine( CustomerAccountLine accountLine );
        IEnumerable<Customer> FindCustomers( string customerId, string firstName, string lastName, int max );
        CustomerInfo GetCustomerInfo( EntityRef<Customer> customer, bool getAllRentals );
        void AcceptCustomerPayment( EntityRef<Customer> customer, decimal amount );
    }
}
