using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests.TestObjects
{
    [Table("reviews")]
    public class Review : Entity<int>
    {
        [Column("book_id")]
        [ForeignKey("books")]
        public int BookId { get; set; }

        [Column("source")]
        public string Source { get; set; }

        [Column("score")]
        public byte Score { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }
    }
}
