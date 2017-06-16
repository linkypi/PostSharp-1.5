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

using PostSharp.Samples.Librarian.Entities;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.BusinessRules
{
    [BusinessRuleApplies( "OpenRental" )]
    internal class LowBalanceWhenOpenRental : BusinessRule
    {
        public override bool Evaluate( object item )
        {
            return ( (Rental) item ).Customer.Entity.CurrentBalance > 5;
        }
    }
}
