using DataTrack.Core.Attributes;
using System;
using DataTrack.Core.Components.Mapping;

namespace DataTrack.Core.Tests.TestClasses.TestEntities
{
	[Table("reviews")]
	public class Review : Entity<Guid>
	{
		[Column("identifier")]
		[PrimaryKey]
		public override Guid ID { get; set; }

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
