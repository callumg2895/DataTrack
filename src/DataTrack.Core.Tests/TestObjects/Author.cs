using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.DataStructures;
using System.Collections.Generic;

namespace DataTrack.Core.Tests.TestObjects
{
    [TableMapping("authors")]
    public class Author : Entity<int>
    {
        private static int totalAuthors = 0;

        public static List<Author> GetAuthors(int n, int m = 0)
        {
            List<Author> authors = new List<Author>();

            for (int i = totalAuthors; i < totalAuthors + n; i++)
            {
                authors.Add(new Author() { FirstName = $"FirstName{i}", LastName = $"LastName{i}", Books = Book.GetBooks(m)});
            }

            totalAuthors += n;

            return authors;
        }

        [ColumnMapping("authors", "first_name")]
        public virtual string FirstName { get; set; }

        [ColumnMapping("authors", "last_name")]
        public virtual string LastName { get; set; }

        [TableMapping("books")]
        public virtual List<Book> Books { get; set; }
    }
}
