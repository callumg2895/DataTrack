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

		[Table("reviews")]
		public List<Review> Reviews { get; set; }
	}
}
