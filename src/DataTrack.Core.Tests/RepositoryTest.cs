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
    public class RepositoryTest
    {

        private Stopwatch stopwatch = new Stopwatch();

        [TestMethod]
        public void TestRepository_ShouldReturnCorrectObjectForReadWithRestriction()
        {
            // Arrange
            Book book = new Book() { ID = 1, AuthorId = 1, Title = "The Great Gatsby" };
            Author author = new Author()
            {
                ID = 1,
                FirstName = "John",
                LastName = "Smith",
                Books = new List<Book>() { book }
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
            bookReadResult = Repository<Book>.GetByID(book.ID);
            stopwatch.Stop();
            long bookReadTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Book>.Delete(book);
            stopwatch.Stop();
            long bookDeleteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            Repository<Author>.Delete(author);
            stopwatch.Stop();
            long authorDeleteTime = stopwatch.ElapsedMilliseconds;

            Logger.Info(MethodBase.GetCurrentMethod(), $"Inserted 'Author' object in in {authorInsertTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Read 'Author' object in {authorReadTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Read 'Book' object in {bookReadTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Book' object in {bookDeleteTime}ms");
            Logger.Info(MethodBase.GetCurrentMethod(), $"Deleted 'Author' object in {authorDeleteTime}ms");

            // Assert
            Assert.AreEqual(createResult, 2);
            Assert.AreEqual(authorReadResult.ID, author.ID);
            Assert.AreEqual(authorReadResult.FirstName, author.FirstName);
            Assert.AreEqual(authorReadResult.LastName, author.LastName);
            Assert.AreEqual(bookReadResult.ID, book.ID);
            Assert.AreEqual(bookReadResult.AuthorId, book.AuthorId);
            Assert.AreEqual(bookReadResult.Title, book.Title);
        }
    }
}
