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
    internal class EmployeeProcesses : SessionBoundService, IEmployeeProcesses
    {
        public EmployeeProcesses( ServerSession session )
            : base( session )
        {
        }

        public Employee FindEmployeeByLogin( string login )
        {
            foreach (
                Employee employee in
                    StorageContext.Current.Find<Employee>(
                        delegate( Employee candidate )
                            {
                                return
                                    string.Compare( candidate.Login, login, StringComparison.InvariantCultureIgnoreCase ) ==
                                    0;
                            } ) )
            {
                return employee;
            }

            return null;
        }
    }
}