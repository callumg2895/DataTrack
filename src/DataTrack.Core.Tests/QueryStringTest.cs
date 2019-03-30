using DataTrack.Core.SQL.BuilderObjects;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Tests.TestObjects;
using DataTrack.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Tests
{
    [TestClass]
    public class QueryStringTest : BaseTest
    {

        [TestMethod]
        public void TestQuery_CachesMappingData()
        {
            // Arrange
            Book book = new Book() { Title = string.Empty };
            Author author = new Author() { FirstName = string.Empty, LastName = string.Empty, Books = new List<Book>() { book } };

            //Act
            Query<Author> read = new Query<Author>().Read();
            Query<Author> insert = new Query<Author>().Create(author);
            Query<Author> update = new Query<Author>().Update(author);
            Query<Author> delete = new Query<Author>().Delete(author);

            //Assert
            Assert.IsTrue(Dictionaries.TypeMappingCache.ContainsKey(typeof(Author)));
            Assert.IsTrue(Dictionaries.TypeMappingCache.ContainsKey(typeof(Book)));
        }

        [TestMethod]
        public void TestReadQuery_ShouldReturnCorrectSQLForObjects()
        {
            // Arrange
            string testQuery;
            string expectedQuery;

            //Act
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("select Book.id, Book.author_id, Book.title");
            sqlBuilder.AppendLine("into #books_staging from books as Book");
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("select * from #books_staging");

            expectedQuery = sqlBuilder.ToString();
            testQuery = new Query<Book>().Read().ToString();

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestReadQuery_ShouldReturnCorrectSQLForObjectsWithChildProperty()
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
            testQuery = testQuery = new Query<Author>().Read().ToString();

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestDeleteQuery_ShouldReturnCorrectSQLForObjects()
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
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");

            expectedQuery = sqlBuilder.ToString();
            testQuery = new Query<Author>().Delete(author).ToString();

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

        [TestMethod]
        public void TestUpdateQuery_ShouldReturnCorrectSQLForObjects()
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
            sqlBuilder.AppendLine("where Author.id = @authors_id_2");
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");

            expectedQuery = sqlBuilder.ToString();
            testQuery = new Query<Author>().Update(author).ToString();

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

    }
}
