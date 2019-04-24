using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using System.Collections.Generic;

namespace DataTrack.Core.Tests.TestObjects
{
    [Table("books")]
    public class Book : Entity<int>
    {
        private static int totalBooks = 0;

        public static List<Book> GetBooks(int n)
        {
            List<Book> books = new List<Book>();

            for (int i = totalBooks; i < totalBooks + n; i++)
            {
                books.Add(new Book() { Title = $"Title{i}" });
            }

            totalBooks += n;

            return books;
        }

        [Column("author_id")]
        [ForeignKey("authors")]
        public virtual int AuthorId { get; set; }

        [Column("title")]
        public virtual string Title { get; set; }
    }
}
