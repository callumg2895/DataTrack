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
		public void TestEntityBeanQuery_ShouldReturnCorrectNumberOfItems()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];
			new EntityQuery<Author>().Create(author).Execute();

			// Act
			List<BookInfo> bookInfoCollection = new EntityBeanQuery<BookInfo>().Execute();
			new EntityQuery<Author>().Delete().Execute();


			// Assert
			Assert.IsTrue(bookInfoCollection.Count == 0);
		}

	}
}
