using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Tests.TestClasses.TestEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests.TestClasses.TestBeans
{
	public class BookInfo : EntityBean
	{
		[Entity(typeof(Book), "Title")]
		public string Title { get; set; }

		[Entity(typeof(Author), "FirstName")]
		public string Author { get; set; }
	}
}
