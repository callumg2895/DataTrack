using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using System;

namespace DataTrack.Core.Tests.TestObjects
{
	[Table("reviews")]
	public class Review : Entity<Guid>
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
