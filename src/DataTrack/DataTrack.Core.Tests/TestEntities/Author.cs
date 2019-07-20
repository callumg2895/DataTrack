using DataTrack.Core.Attributes;
using System.Collections.Generic;
using DataTrack.Core.Components.Mapping;

namespace DataTrack.Core.Tests.TestEntities
{
	[Table("authors")]
	public class Author : Entity<int>
	{
		[Column("first_name")]
		public virtual string FirstName { get; set; }

		[Column("last_name")]
		public virtual string LastName { get; set; }

		[Table("books")]
		public virtual List<Book> Books { get; set; }
	}
}
