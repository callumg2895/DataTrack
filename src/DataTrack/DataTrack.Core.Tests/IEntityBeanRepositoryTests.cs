using DataTrack.Core.Components.Query;
using DataTrack.Core.Interface;
using DataTrack.Core.Repository;
using DataTrack.Core.Tests.TestBeans;
using DataTrack.Core.Tests.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests
{
	[TestClass]
	public class IEntityBeanRepositoryTests : BaseTest
	{
		[TestMethod]
		public void TestEntityBeanQuery_ShouldReturnCorrectItems()
		{
			// Arrange
			IEntityRepository<Author> authorRepository = new EntityRepository<Author>();
			IEntityBeanRepository<BookInfo> bookInfoRepository = new EntityBeanRepository<BookInfo>();

			Author author = GetAuthors(1, 5)[0];
			authorRepository.Create(author);

			// Act
			List<BookInfo> bookInfoCollection = bookInfoRepository.GetAll();
			authorRepository.DeleteAll();

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
