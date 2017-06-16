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
using PostSharp.Samples.Librarian.Data;
using PostSharp.Samples.Librarian.Entities;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.BusinessProcesses
{
    [Trace]
    internal class NoteProcesses : SessionBoundService, INoteProcesses
    {
        public NoteProcesses( ServerSession session )
            : base( session )
        {
        }

        public IEnumerable<Note> GetNotes( EntityRef<Entity> owner, int max )
        {
            if ( owner.IsNull )
                throw new ArgumentNullException( "owner" );

            return StorageContext.Current.Find<Note>(
                delegate( Note note ) { return note.Owner == owner; }, max );
        }

        [Transaction]
        public void CreateNote( Note note )
        {
            if ( note == null )
                throw new ArgumentNullException( "note" );
            note.Employee = this.Session.Employee;
            BusinessRulesManager.Assert( "CreateNote", note );
            StorageContext.Current.Insert( note );
        }
    }
}