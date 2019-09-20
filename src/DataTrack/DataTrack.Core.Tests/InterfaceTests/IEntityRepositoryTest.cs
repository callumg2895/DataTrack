using DataTrack.Core.Interface;
using DataTrack.Core.Repository;
using DataTrack.Core.Tests.TestClasses.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DataTrack.Core.Tests.InterfaceTests
{
	[TestClass]
	public class IEntityRepositoryTest : BaseTest
	{

		[TestMethod]
		public void TestIRepository_ShouldReturnCorrectObjectForReadWithRestriction()
		{
			// Arrange
			Author author = GetAuthors(1, 2)[0];

			//Act
			AuthorRepository.Create(author);
			Author authorReadResult = AuthorRepository.GetByProperty("FirstName", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
			AuthorRepository.Delete(authorReadResult);

			// Assert
			Assert.IsTrue(AuthorsAreEqual(authorReadResult, author));
		}

		[TestMethod]
		public void TestIRepository_ShouldReturnCorrectObjectForGetByPropertyType()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];

			//Act
			AuthorRepository.Create(author);
			List<Book> bookReadResult = BookRepository.GetByProperty("Title", Enums.RestrictionTypes.EqualTo, author.Books[0].Title);
			AuthorRepository.DeleteAll();

			// Assert
			Assert.IsTrue(BooksAreEqual(bookReadResult[0], author.Books[0]));
		}

		[TestMethod]
		public void TestIRepository_ShouldReadCorrectNumberOfChildItemsAfterInsertingObjectWithLongListOfChildren()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];

			//Act
			AuthorRepository.Create(author);
			Author authorReadResult = AuthorRepository.GetByProperty("FirstName", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
			AuthorRepository.Delete(authorReadResult);

			// Assert
			Assert.AreEqual(authorReadResult.Books.Count, 5);
		}

		[TestMethod]
		public void TestIRepository_ShouldInsertMultipleEntitiesFromList()
		{
			// Arrange
			int authorsToInsert = 10;
			int booksPerAuthor = 20;
			int reviewsPerBook = 2;

			List<Author> authors = new List<Author>(GetAuthors(authorsToInsert, booksPerAuthor, reviewsPerBook));

			// Act
			AuthorRepository.Create(authors);
			List<Author> createdAuthors = AuthorRepository.GetAll();
			List<Book> createdBooks = BookRepository.GetAll();
			List<Review> createdReviews = ReviewRepository.GetAll();
			AuthorRepository.DeleteAll();

			// Assert
			Assert.AreEqual(authorsToInsert, createdAuthors.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor, createdBooks.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor * reviewsPerBook, createdReviews.Count);
		}
	}
}
