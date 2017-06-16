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
using PostSharp.Samples.Librarian.Data;
using PostSharp.Samples.Librarian.Entities;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.BusinessProcesses
{
    [Trace]
    internal class BookProcesses : SessionBoundService, IBookProcesses
    {
        public BookProcesses( ServerSession session )
            : base( session )
        {
        }

        [Transaction]
        public Book CreateBook( Book book )
        {
            BusinessRulesManager.Assert( "CreateBook", book );
            StorageContext.Current.Insert( book );
            return book;
        }

        public IEnumerable<Book> FindBooks( string bookId, string authors, string title, string isbn, int max )
        {
            return StorageContext.Current.Find<Book>(
                delegate( Book book )
                    {
                        return !book.Deleted &&
                               ( string.IsNullOrEmpty( bookId ) || string.Compare( book.BookId, bookId, true ) == 0 ) &&
                               ( string.IsNullOrEmpty( isbn ) || string.Compare( book.Isbn, isbn, true ) == 0 ) &&
                               ( string.IsNullOrEmpty( title ) || book.Title.ToLower().Contains( title.ToLower() ) ) &&
                               ( string.IsNullOrEmpty( authors ) || book.Authors.ToLower().Contains( authors.ToLower() ) );
                    }, max );
        }
    }
}