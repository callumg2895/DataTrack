using DataTrack.Core.Components.Query;
using DataTrack.Core.Tests.TestBeans;
using DataTrack.Core.Tests.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests
{
	[TestClass]
	public class EntityBeanQueryTest : BaseTest
	{
		[TestMethod]
		public void TestEntityBeanQuery_ShouldReturnCorrectItems()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];
			new EntityQuery<Author>().Create(author).Execute();

			// Act
			List<BookInfo> bookInfoCollection = new EntityBeanQuery<BookInfo>().Execute();
			new EntityQuery<Author>().Delete().Execute();

			// Assert
			Assert.IsTrue(bookInfoCollection.Count == 5);

			Assert.AreEqual(bookInfoCollection[0].Author, author.FirstName);
			Assert.AreEqual(bookInfoCollection[1].Author, author.FirstName);
			Assert.AreEqual(bookInfoCollection[2].Author, author.FirstName);
			Assert.AreEqual(bookInfoCollection[3].Author, author.FirstName);
			Assert.AreEqual(bookInfoCollection[4].Author, author.FirstName);

			Assert.AreEqual(bookInfoCollection[0].Title, author.Books[0].Title);
			Assert.AreEqual(bookInfoCollection[1].Title, author.Books[1].Title);
			Assert.AreEqual(bookInfoCollection[2].Title, author.Books[2].Title);
			Assert.AreEqual(bookInfoCollection[3].Title, author.Books[3].Title);
			Assert.AreEqual(bookInfoCollection[4].Title, author.Books[4].Title);
		}

	}
}
