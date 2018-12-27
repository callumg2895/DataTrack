using DataTrack.Core.Repository;
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
            Book book1 = new Book() { ID = 1, AuthorId = 1, Title = "The Great Gatsby" };
            Book book2 = new Book() { ID = 2, AuthorId = 1, Title = "The Beautiful and Damned" };
            Author author = new Author()
            {
                ID = 1,
                FirstName = "F.Scott",
                LastName = "Fitzgerald",
                Books = new List<Book>() { book1, book2 }
            };

            Author authorReadResult;
            Book bookReadResult;

            //Act
            Repository<Author>.Create(author);
            authorReadResult = Repository<Author>.GetByID(author.ID);
            bookReadResult = Repository<Book>.GetByID(book1.ID);
            Repository<Book>.Delete(book1);
            Repository<Book>.Delete(book2);
            Repository<Author>.Delete(author);

            // Assert
            Assert.IsTrue(AuthorsAreEqual(authorReadResult, author));
        }

        [TestMethod]
        public void TestRepository_ShouldReturnCorrectObjectForGetByPropertyType()
        {
            // Arrange
            Book book1 = new Book() { ID = 1, AuthorId = 1, Title = "The Great Gatsby" };
            Book book2 = new Book() { ID = 2, AuthorId = 1, Title = "The Beautiful and Damned" };
            Author author = new Author()
            {
                ID = 1,
                FirstName = "F.Scott",
                LastName = "Fitzgerald",
                Books = new List<Book>() { book1, book2 }
            };

            List<Book> bookReadResult;

            //Act
            Repository<Author>.Create(author);
            bookReadResult = Repository<Book>.GetByProperty("title", Enums.RestrictionTypes.EqualTo, "The Great Gatsby");
            Repository<Book>.Delete(book1);
            Repository<Book>.Delete(book2);
            Repository<Author>.Delete(author);

            // Assert
            Assert.IsTrue(BooksAreEqual(bookReadResult[0], book1));
        }

        [TestMethod]
        public void TestRepository_ShouldReadCorrectNumberOfChildItemsAfterInsertingObjectWithLongListOfChildren()
        {
            // Arrange
            Book book1 = new Book() { ID = 1, AuthorId = 1, Title = "The Great Gatsby" };
            Book book2 = new Book() { ID = 2, AuthorId = 1, Title = "The Beautiful and Damned" };
            Book book3 = new Book() { ID = 3, AuthorId = 1, Title = "This Side of Paradise" };
            Book book4 = new Book() { ID = 4, AuthorId = 1, Title = "Tender is the Night" };
            Book book5 = new Book() { ID = 5, AuthorId = 1, Title = "The Last Tycoon" };
            Author author = new Author()
            {
                ID = 1,
                FirstName = "F.Scott",
                LastName = "Fitzgerald",
                Books = new List<Book>() { book1, book2, book3, book4, book5 }
            };

            Author authorReadResult;

            //Act
            Repository<Author>.Create(author);
            authorReadResult = Repository<Author>.GetByID(author.ID);
            Repository<Book>.Delete(book1);
            Repository<Book>.Delete(book2);
            Repository<Book>.Delete(book3);
            Repository<Book>.Delete(book4);
            Repository<Book>.Delete(book5);
            Repository<Author>.Delete(author);

            // Assert
            Assert.AreEqual(authorReadResult.Books.Count, 5);
        }

    }
}
