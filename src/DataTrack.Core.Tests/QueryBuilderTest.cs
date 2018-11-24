using DataTrack.Core.Enums;
using DataTrack.Core.SQL;
using DataTrack.Core.SQL.QueryBuilders;
using DataTrack.Core.Tests.TestObjects;
using DataTrack.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Tests
{
    [TestClass]
    public class QueryBuilderTest : BaseTest
    {
        private Stopwatch stopwatch = new Stopwatch();

        [TestMethod]
        public void TestReadQueryBuilder_ShouldReturnCorrectSQLForObjects()
        {
            // Arrange
            string testQuery;
            string expectedQuery;

            //Act
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("select Book.id, Book.author_id, Book.title");
            sqlBuilder.AppendLine("from books as Book");

            expectedQuery = sqlBuilder.ToString();

            stopwatch.Start();
            testQuery = new ReadQueryBuilder<Book>().ToString();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"ReadQueryBuilder executed in {stopwatch.ElapsedMilliseconds}ms");

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestReadQueryBuilder_ShouldReturnCorrectSQLForObjectsWithChildProperty()
        {
            // Arrange
            string testQuery;
            string expectedQuery;

            //Act
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("select Author.id, Author.first_name, Author.last_name");
            sqlBuilder.AppendLine("from authors as Author");
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("select Book.id, Book.author_id, Book.title");
            sqlBuilder.AppendLine("from books as Book");

            expectedQuery = sqlBuilder.ToString();

            stopwatch.Start();
            testQuery = new ReadQueryBuilder<Author>().ToString();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"ReadQueryBuilder executed in {stopwatch.ElapsedMilliseconds}ms");

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestInsertQueryBuilder_ShouldReturnCorrectSQLForObjects()
        {
            // Arrange
            string testQuery;
            string expectedQuery;
            Author author = new Author() { ID = 1, FirstName = "John", LastName = "Smith" };

            //Act
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("insert into authors (id, first_name, last_name)");
            sqlBuilder.AppendLine("values (@authors_id_1, @authors_first_name_1, @authors_last_name_1)");
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");

            expectedQuery = sqlBuilder.ToString();

            stopwatch.Start();
            testQuery = new InsertQueryBuilder<Author>(author).ToString();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"InsertQueryBuilder executed in {stopwatch.ElapsedMilliseconds}ms");

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestInsertQueryBuilder_ShouldReturnCorrectSQLForObjectsWithChildren()
        {
            // Arrange
            string testQuery;
            string expectedQuery;
            Author author = new Author()
            {
                ID = 1,
                FirstName = "John",
                LastName = "Smith",
                Books = new List<Book>() { new Book() { ID = 1, AuthorId = 1, Title = "The Great Gatsby" } }
            };

            //Act
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("insert into authors (id, first_name, last_name)");
            sqlBuilder.AppendLine("values (@authors_id_1, @authors_first_name_1, @authors_last_name_1)");
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("insert into books (id, author_id, title)");
            sqlBuilder.AppendLine("values (@books_id_2, @books_author_id_2, @books_title_2)");
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");

            expectedQuery = sqlBuilder.ToString();

            stopwatch.Start();
            testQuery = new InsertQueryBuilder<Author>(author).ToString();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"InsertQueryBuilder executed in {stopwatch.ElapsedMilliseconds}ms");

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestDeleteQueryBuilder_ShouldReturnCorrectSQLForObjects()
        {
            // Arrange
            string testQuery;
            string expectedQuery;
            Author author = new Author() { ID = 1, FirstName = "John", LastName = "Smith" };

            //Act
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("delete Author from authors Author");
            sqlBuilder.AppendLine("where Author.id in (@authors_id_1)");

            expectedQuery = sqlBuilder.ToString();

            stopwatch.Start();
            testQuery = new DeleteQueryBuilder<Author>(author).ToString();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"DeleteQueryBuilder executed in {stopwatch.ElapsedMilliseconds}ms");

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestUpdateQueryBuilder_ShouldReturnCorrectSQLForObjects()
        {
            // Arrange
            string testQuery;
            string expectedQuery;
            Author author = new Author() { ID = 1, FirstName = "John", LastName = "Smith" };

            //Act
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("update Author");
            sqlBuilder.AppendLine("set Author.id = @authors_id_1,");
            sqlBuilder.AppendLine("Author.first_name = @authors_first_name_1,");
            sqlBuilder.AppendLine("Author.last_name = @authors_last_name_1");
            sqlBuilder.AppendLine("from authors Author");
            sqlBuilder.AppendLine("where Author.id = @authors_id_1");
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");

            expectedQuery = sqlBuilder.ToString();

            stopwatch.Start();
            testQuery = new UpdateQueryBuilder<Author>(author).ToString();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"UpdateQueryBuilder executed in {stopwatch.ElapsedMilliseconds}ms");

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

    }
}
