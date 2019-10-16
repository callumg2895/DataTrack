using DataTrack.Core.Attributes;
using System.Collections.Generic;
using DataTrack.Core.Components.Mapping;

namespace DataTrack.Core.Tests.TestClasses.TestEntities
{
	[Table("books")]
	public class Book : Entity<int>
	{
		[Column("author_id")]
		[ForeignKey("authors")]
		public virtual int AuthorId { get; set; }

		[Column("title")]
		public virtual string Title { get; set; }

		[Formula("avg_score", @"(select coalesce(avg(r.score), 0) from reviews as r where r.book_id = id)")]
		public int AverageScore { get; private set; }

		[Child("reviews")]
		public List<Review> Reviews { get; set; }

		[Parent("authors")]
		public Author Author { get; set; }
	}
}
