using DataTrack.Core.Tests.TestObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace DataTrack.Core.Tests
{
    [TestClass]
    public class RepositoryTest : BaseTest
    {

        private Stopwatch stopwatch = new Stopwatch();

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

            int createResult;
            Author authorReadResult;
            Book bookReadResult;

            //Act
            stopwatch.Start();
            createResult = Repository<Author>.Create(author);
            stopwatch.Stop();
            long authorInsertTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            authorReadResult = Repository<Author>.GetByID(author.ID);
            stopwatch.Stop();
            long authorReadTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            bookReadResult = Repository<Book>.GetByID(book1.ID);
            stopwatch.Stop();
            long bookReadTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book1);
            stopwatch.Stop();
            long book1DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book2);
            stopwatch.Stop();
            long book2DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Author>.Delete(author);
            stopwatch.Stop();
            long authorDeleteTime = stopwatch.ElapsedMilliseconds;

            Logger.Info(MethodBase.GetCurrentMethod(), $"Inserted 'Author' object in in {authorInsertTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Read 'Author' object in {authorReadTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Read 'Book' object in {bookReadTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book1DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book2DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Author' object in {authorDeleteTime}ms");

            // Assert
            Assert.AreEqual(createResult, 3);
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

            int createResult;
            List<Book> bookReadResult;

            //Act
            stopwatch.Start();
            createResult = Repository<Author>.Create(author);
            stopwatch.Stop();
            long authorInsertTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            bookReadResult = Repository<Book>.GetByProperty("title", Enums.RestrictionTypes.EqualTo, "The Great Gatsby");
            stopwatch.Stop();
            long bookReadTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book1);
            stopwatch.Stop();
            long book1DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book2);
            stopwatch.Stop();
            long book2DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Author>.Delete(author);
            stopwatch.Stop();
            long authorDeleteTime = stopwatch.ElapsedMilliseconds;

            Logger.Info(MethodBase.GetCurrentMethod(), $"Inserted 'Author' object in in {authorInsertTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Read 'Book' object in {bookReadTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book1DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book2DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Author' object in {authorDeleteTime}ms");

            // Assert
            Assert.AreEqual(createResult, 3);
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

            int createResult;
            Author authorReadResult;
            Book bookReadResult;

            //Act
            stopwatch.Start();
            createResult = Repository<Author>.Create(author);
            stopwatch.Stop();
            long authorInsertTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            authorReadResult = Repository<Author>.GetByID(author.ID);
            stopwatch.Stop();
            long authorReadTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book1);
            stopwatch.Stop();
            long book1DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book2);
            stopwatch.Stop();
            long book2DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book3);
            stopwatch.Stop();
            long book3DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book4);
            stopwatch.Stop();
            long book4DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book5);
            stopwatch.Stop();
            long book5DeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Author>.Delete(author);
            stopwatch.Stop();
            long authorDeleteTime = stopwatch.ElapsedMilliseconds;

            Logger.Info(MethodBase.GetCurrentMethod(), $"Inserted 'Author' object in in {authorInsertTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Read 'Author' object in {authorReadTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book1DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book2DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book3DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book4DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {book5DeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Author' object in {authorDeleteTime}ms");

            // Assert
            Assert.AreEqual(createResult, 6);
            Assert.AreEqual(authorReadResult.Books.Count, 5);
        }

    }
}
