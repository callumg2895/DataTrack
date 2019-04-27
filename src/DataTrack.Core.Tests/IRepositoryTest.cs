using DataTrack.Core.Interface;
using DataTrack.Core.Repository;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Tests.TestObjects;
using DataTrack.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace DataTrack.Core.Tests
{
    [TestClass]
    public class IRepositoryTest : BaseTest
    {

        [TestMethod]
        public void TestIRepository_ShouldReturnCorrectObjectForReadWithRestriction()
        {
            // Arrange
            Author author = GetAuthors(1, 2)[0];
            IRepository<Author> authorRepository = new Repository<Author>();
            IRepository<Book> bookRepository = new Repository<Book>();

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
            IRepository<Author> authorRepository = new Repository<Author>();
            IRepository<Book> bookRepository = new Repository<Book>();

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
            IRepository<Author> authorRepository = new Repository<Author>();

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
            IRepository<Author> authorRepository = new Repository<Author>();
            IRepository<Book> bookRepository = new Repository<Book>();
            IRepository<Review> reviewRepository = new Repository<Review>();

            List<Author> authors =  new List<Author>(GetAuthors(authorsToInsert, booksPerAuthor, reviewsPerBook));

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
