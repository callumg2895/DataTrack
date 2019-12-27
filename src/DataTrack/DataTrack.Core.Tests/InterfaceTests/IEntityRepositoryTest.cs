using DataTrack.Core.Interface;
using DataTrack.Core.Repository;
using DataTrack.Core.Tests.TestClasses.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataTrack.Core.Tests.InterfaceTests
{
	[TestClass]
	public class IEntityRepositoryTest : BaseTest
	{

		[TestMethod]
		public void TestIRepository_GetByProperty_SingleParentEntity_RestrictionOnParentEntity_SingleRestriction_MatchExpected()
		{
			// Arrange
			Author author = GetAuthors(1, 2, 3)[0];

			//Act
			AuthorRepository.Create(author);
			Author authorReadResult = AuthorRepository.GetByProperty("FirstName", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];

			// Assert
			Assert.IsTrue(AuthorsAreEqual(authorReadResult, author));
		}

		[TestMethod]
		public void TestIRepository_GetByProperty_MultipleParentEntities_RestrictionOnParentEntity_SingleRestriction_MatchExpected()
		{
			// Arrange
			List<Author> authors = GetAuthors(2, 2, 3);
			Author author = authors[0];

			//Act
			AuthorRepository.Create(author);
			Author authorReadResult = AuthorRepository.GetByProperty("FirstName", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];

			// Assert
			Assert.IsTrue(AuthorsAreEqual(authorReadResult, author));
		}

		[TestMethod]
		[ExpectedException(typeof(SqlException))]
		public void TestIRepository_GetByProperty_NoParentEntity_RestrictionOnChildEntity_SingleRestriction_ShouldFailToInsertChildEntities()
		{
			// Arrange
			List<Book> books = GetBooks(3, 3);
			Book book = books[0];

			//Act
			BookRepository.Create(books);
			Book bookReadResult = BookRepository.GetByProperty("Title", Enums.RestrictionTypes.EqualTo, book.Title)[0];

			// Assert
			Assert.IsTrue(BooksAreEqual(bookReadResult, book));
		}

		[TestMethod]
		public void TestIRepository_GetByProperty_SingleParentEntity_RestrictionOnChildEntity_SingleRestriction_MatchExpected()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];

			//Act
			AuthorRepository.Create(author);
			List<Book> bookReadResult = BookRepository.GetByProperty("Title", Enums.RestrictionTypes.EqualTo, author.Books[0].Title);

			// Assert
			Assert.IsTrue(bookReadResult.Count == 1);
			Assert.IsTrue(BooksAreEqual(bookReadResult[0], author.Books[0]));
		}

		[TestMethod]
		public void TestIRepository_GetByProperty_SingleParentEntity_RestrictionOnChildEntity_SingleRestriction_NoMatchExpected()
		{
			// Arrange
			Author author = GetAuthors(1, 5)[0];

			//Act
			AuthorRepository.Create(author);
			List<Book> bookReadResult = BookRepository.GetByProperty("Title", Enums.RestrictionTypes.EqualTo, "Something that doesn't exist");

			// Assert
			Assert.IsTrue(bookReadResult.Count == 0);
		}

		[TestMethod]
		public void TestIRepository_Create_SingleParentEntity()
		{
			// Arrange
			int authorsToInsert = 1;
			int booksPerAuthor = 20;
			int reviewsPerBook = 2;

			List<Author> authors = new List<Author>(GetAuthors(authorsToInsert, booksPerAuthor, reviewsPerBook));

			// Act
			AuthorRepository.Create(authors);
			List<Author> createdAuthors = AuthorRepository.GetAll();
			List<Book> createdBooks = BookRepository.GetAll();
			List<Review> createdReviews = ReviewRepository.GetAll();

			// Assert
			Assert.AreEqual(authorsToInsert, createdAuthors.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor, createdBooks.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor * reviewsPerBook, createdReviews.Count);
		}

		[TestMethod]
		public void TestIRepository_Create_MultipleParentEntities()
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

			// Assert
			Assert.AreEqual(authorsToInsert, createdAuthors.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor, createdBooks.Count);
			Assert.AreEqual(authorsToInsert * booksPerAuthor * reviewsPerBook, createdReviews.Count);
		}
	}
}
