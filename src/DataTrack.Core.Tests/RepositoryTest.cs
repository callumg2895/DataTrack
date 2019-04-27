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
    public class RepositoryTest : BaseTest
    {

        [TestMethod]
        public void TestRepository_ShouldReturnCorrectObjectForReadWithRestriction()
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
        public void TestRepository_ShouldReturnCorrectObjectForGetByPropertyType()
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
        public void TestRepository_ShouldReadCorrectNumberOfChildItemsAfterInsertingObjectWithLongListOfChildren()
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
        public void TestRepository_ShouldInsertMultipleEntitiesFromList()
        {
            // Arrange
            int authorsToInsert = 100;
            int booksPerAuthor = 20;
            IRepository<Author> authorRepository = new Repository<Author>();
            IRepository<Book> bookRepository = new Repository<Book>();

            List<Author> authors =  new List<Author>(GetAuthors(authorsToInsert, booksPerAuthor));

            // Act
            authorRepository.Create(authors);
            List<Author> createdAuthors = authorRepository.GetAll();
            List<Book> createdBooks = bookRepository.GetAll();
            authorRepository.DeleteAll();

            // Assert
            Assert.AreEqual(authorsToInsert, createdAuthors.Count);
            Assert.AreEqual(authorsToInsert * booksPerAuthor, createdBooks.Count);
        }
    }
}
