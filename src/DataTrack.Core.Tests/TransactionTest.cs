using DataTrack.Core.Enums;
using DataTrack.Core.SQL.Read;
using DataTrack.Core.SQL;
using DataTrack.Core.SQL.Delete;
using DataTrack.Core.SQL.Insert;
using DataTrack.Core.SQL.Update;
using DataTrack.Core.Tests.TestObjects;
using DataTrack.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Tests
{
    [TestClass]
    public class TransactionTest : BaseTest
    {

        private Stopwatch stopwatch = new Stopwatch();

        [TestMethod]
        public void TestTransaction_ShouldReturnCorrectObjectForReadWithRestriction()
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

            //Act
            stopwatch.Start();
            Transaction<Author> t1 = new Transaction<Author>(new List<QueryBuilder<Author>>()
            {
                new SQL.Insert.InsertQueryBuilder<Author>(author),
                new ReadQueryBuilder<Author>(author.ID),
                new SQL.Delete.DeleteQueryBuilder<Author>(author),
            });

            Transaction<Book> t2 = new Transaction<Book>(new List<QueryBuilder<Book>>()
            {
                new SQL.Delete.DeleteQueryBuilder<Book>(book)
            });

            List<object> results = t1.Execute();
            t2.Execute();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Transaction<Author> executed in {stopwatch.ElapsedMilliseconds}ms");

            int affectedRows = (int)results[0];
            Author result = ((List<Author>)results[1])[0];

            // Assert
            Assert.AreEqual(affectedRows, 2);
            Assert.AreEqual(result.ID, author.ID);
            Assert.AreEqual(result.FirstName, author.FirstName);
            Assert.AreEqual(result.LastName, author.LastName);
            Assert.AreEqual(result.Books.Count, 1);
            Assert.AreEqual(result.Books[0].AuthorId, result.ID);
        }

        [TestMethod]
        public void TestTransaction_ShouldReturnCorrectObjectBeforeAndAfterUpdate()
        {
            // Arrange
            Author authorBefore = new Author() { ID = 1, FirstName = "John", LastName = "Smith", Books = new List<Book>()};
            Author authorAfter = new Author() { ID = 1, FirstName = "James", LastName = "Smith", Books = new List<Book>() };

            // Act
            stopwatch.Start();
            Transaction<Author> t1 = new Transaction<Author>(new List<QueryBuilder<Author>>()
            {
                new SQL.Insert.InsertQueryBuilder<Author>(authorBefore),
                new ReadQueryBuilder<Author>(authorBefore.ID),
            });

            List<object> results1 = t1.Execute();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Transaction<Author> executed in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Start();
            Transaction<Author> t2 = new Transaction<Author>(new List<QueryBuilder<Author>>()
            {
                new UpdateQueryBuilder<Author>(authorAfter),
                new ReadQueryBuilder<Author>(authorAfter.ID),
                new SQL.Delete.DeleteQueryBuilder<Author>(authorAfter),
            });

            List<object> results2 = t2.Execute();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Transaction<Author> executed in {stopwatch.ElapsedMilliseconds}ms");

            int affectedInsertRows = (int)results1[0];
            Author beforeUpdate = ((List<Author>)results1[1])[0];
            int affectedUpdateRows = (int)results2[0];
            Author afterUpdate = ((List<Author>)results2[1])[0];

            // Assert
            Assert.AreEqual(affectedInsertRows, 1);
            Assert.AreEqual(affectedUpdateRows, 1);
            Assert.AreEqual(beforeUpdate.ID, authorBefore.ID);
            Assert.AreEqual(beforeUpdate.FirstName, authorBefore.FirstName);
            Assert.AreEqual(beforeUpdate.LastName, authorBefore.LastName);
            Assert.AreEqual(afterUpdate.ID, authorAfter.ID);
            Assert.AreEqual(afterUpdate.FirstName, authorAfter.FirstName);
            Assert.AreEqual(afterUpdate.LastName, authorAfter.LastName);
        }
    }
}
