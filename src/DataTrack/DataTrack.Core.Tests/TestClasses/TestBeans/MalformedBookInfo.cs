using DataTrack.Core.Components.Data;
using DataTrack.Core.Attributes;
using DataTrack.Core.Tests.TestClasses.TestEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests.TestClasses.TestBeans
{
	public class MalformedBookInfo : EntityBean
	{
		[Entity(typeof(Book), "Title")]
		public string Title { get; set; }

		[Entity(typeof(Author), "FirstName")]
		public string Author { get; set; }

		[Formula("avg_score", "(select coalesce(avg(r.score), 0) from reviews as r where r.book_id = id)")]
		public int AverageScore { get; private set; }
	}
}
