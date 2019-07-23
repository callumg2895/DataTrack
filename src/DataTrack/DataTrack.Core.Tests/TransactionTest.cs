using DataTrack.Core.Tests.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataTrack.Core.Components.Query;
using System.Collections.Generic;

namespace DataTrack.Core.Tests
{
	[TestClass]
	public class TransactionTest : BaseTest
	{

		[TestMethod]
		public void TestTransaction_ShouldReturnCorrectObjectForReadWithRestriction()
		{
			// Arrange
			Book book = new Book() { Title = "The Great Gatsby" };

			Author author = new Author()
			{
				FirstName = "John",
				LastName = "Smith",
				Books = new List<Book>() { book }
			};

			List<Author> results = null;

			//Act
			using (Transaction t1 = new Transaction())
			{
				t1.Execute(new EntityQuery<Author>().Create(author));
				results = t1.Execute(new EntityQuery<Author>().Read().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName));
				t1.Commit();
			}

			new EntityQuery<Author>().Delete().Execute();

			// Assert
			Assert.IsTrue(AuthorsAreEqual(results[0], author));
		}

		[TestMethod]
		public void TestTransaction_ShouldReturnCorrectObjectBeforeAndAfterUpdate()
		{
			// Arrange
			Author authorBefore = new Author() { FirstName = "John", LastName = "Smith", Books = new List<Book>() };
			Author authorAfter = new Author() { FirstName = "James", LastName = "Smith", Books = new List<Book>() };
			List<Author> results1 = null;
			List<Author> results2 = null;

			// Act

			using (Transaction t1 = new Transaction())
			{
				t1.Execute(new EntityQuery<Author>().Create(authorBefore));
				results1 = t1.Execute(new EntityQuery<Author>().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, authorBefore.FirstName));
				t1.Commit();
			}

			Author beforeUpdate = results1[0];
			authorAfter.ID = beforeUpdate.ID;

			using (Transaction t2 = new Transaction())
			{
				t2.Execute(new EntityQuery<Author>().Update(authorAfter));
				results2 = t2.Execute(new EntityQuery<Author>().Read(authorAfter.ID).AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, authorAfter.FirstName));
				t2.Commit();
			}


			Author afterUpdate = results2[0];

			new EntityQuery<Author>().Delete(beforeUpdate).Execute();
			new EntityQuery<Author>().Delete(afterUpdate).Execute();

			// Assert
			Assert.IsTrue(AuthorsAreEqual(beforeUpdate, authorBefore));
			Assert.IsTrue(AuthorsAreEqual(afterUpdate, authorAfter));
		}

		[TestMethod]
		public void TestTransaction_ShouldNotDeleteIfTransactionRolledBack()
		{
			// Arrange
			Author author = new Author()
			{
				FirstName = "John",
				LastName = "Smith",
				Books = new List<Book>()
			};

			List<Author> resultsAfterRollBack;
			List<Author> resultsAfterDelete;

			// Act
			new EntityQuery<Author>().Create(author).Execute();

			using (Transaction t = new Transaction())
			{
				t.Execute(new EntityQuery<Author>().Delete(author));
				t.RollBack();
			}

			resultsAfterRollBack = new EntityQuery<Author>().Read().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName).Execute();
			new EntityQuery<Author>().Delete().Execute();
			resultsAfterDelete = new EntityQuery<Author>().Read().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName).Execute();

			//Assert
			Assert.AreEqual(resultsAfterRollBack.Count, 1);
			Assert.AreEqual(resultsAfterDelete.Count, 0);
		}
	}
}
