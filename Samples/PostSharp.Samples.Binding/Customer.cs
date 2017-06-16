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

namespace PostSharp.Samples.Binding
{
    [EditableObject]
    [NotifyPropertyChanged]
    public class Customer
    {
        private Guid guid;
        private string firstName;
        private string lastName;
        private int segmentId;
        public const int defaultSegmentId = 1;

        public Customer()
        {
            this.guid = System.Guid.NewGuid();
            this.segmentId = defaultSegmentId;
        }


        public string FirstName { get { return firstName; } set { firstName = value; } }

        public string LastName { get { return lastName; } set { lastName = value; } }

        public int SegmentId { get { return segmentId; } set { segmentId = value; } }

        public string Guid { get { return guid.ToString(); } }
    }
}