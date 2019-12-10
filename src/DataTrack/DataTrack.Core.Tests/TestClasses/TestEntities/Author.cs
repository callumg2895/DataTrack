using DataTrack.Core.Attributes;
using System.Collections.Generic;
using DataTrack.Core.Components.Mapping;

namespace DataTrack.Core.Tests.TestClasses.TestEntities
{
	[Table("authors")]
	public class Author : Entity<int>
	{
		[Column("first_name")]
		public virtual string FirstName { get; set; }

		[Column("last_name")]
		public virtual string LastName { get; set; }

		[Child("books")]
		public virtual List<Book> Books { get; set; }

		[Unmapped] // This property is just a getter, and so does not have a 1:1 database mapping
		public virtual string FullName
		{
			get
			{
				if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
				{
					return string.Empty;
				}
				else
				{
					return $"{FirstName} {LastName}";
				}
			}
		}
	}
}
