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
			IEntityRepository<Author> authorRepository = new EntityRepository<Author>();
			IEntityRepository<Book> bookRepository = new EntityRepository<Book>();

			//Act
			authorRepository.Create(author);
			Author authorReadResult = authorRepository.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
			Book book1ReadResult = bookRepository.GetByProperty("title", Enums.RestrictionTypes.EqualTo, author.Books[0].Title)[0];
			authorRepository.Delete(authorReadResult);

			// Assert
			Assert.IsTrue(AuthorsAreEqual(authorReadResult, author));
		}

		[TestMethod]
		public void TestIRepository_ShouldReturnCorrectObjectForGetByPropertyType()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];
			IEntityRepository<Author> authorRepository = new EntityRepository<Author>();
			IEntityRepository<Book> bookRepository = new EntityRepository<Book>();

			//Act
			authorRepository.Create(author);
			List<Author> authorReadResult = authorRepository.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName);
			List<Book> bookReadResult = bookRepository.GetByProperty("title", Enums.RestrictionTypes.EqualTo, author.Books[0].Title);
			authorRepository.DeleteAll();

			// Assert
			Assert.IsTrue(BooksAreEqual(bookReadResult[0], author.Books[0]));
		}

		[TestMethod]
		public void TestIRepository_ShouldReadCorrectNumberOfChildItemsAfterInsertingObjectWithLongListOfChildren()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];
			IEntityRepository<Author> authorRepository = new EntityRepository<Author>();

			//Act
			authorRepository.Create(author);
			Author authorReadResult = authorRepository.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
			authorRepository.Delete(authorReadResult);

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
			IEntityRepository<Author> authorRepository = new EntityRepository<Author>();
			IEntityRepository<Book> bookRepository = new EntityRepository<Book>();
			IEntityRepository<Review> reviewRepository = new EntityRepository<Review>();

			List<Author> authors = new List<Author>(GetAuthors(authorsToInsert, booksPerAuthor, reviewsPerBook));

			// Act
			authorRepository.Create(authors);
			List<Author> createdAuthors = authorRepository.GetAll();
			List<Book> createdBooks = bookRepository.GetAll();
			List<Review> createdReviews = reviewRepository.GetAll();
			authorRepository.DeleteAll();

			// Assert
			Assert.AreEqual(authorsToInsert, createdAuthors.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor, createdBooks.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor * reviewsPerBook, createdReviews.Count);
		}
	}
}
