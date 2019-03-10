using DataTrack.Core.SQL.BuilderObjects;
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
    public class QueryBuilderTest : BaseTest
    {

        [TestMethod]
        public void TestQueryBuilders_CachesMappingData()
        {
            // Arrange
            Book book = new Book() { Title = string.Empty };
            Author author = new Author() { FirstName = string.Empty, LastName = string.Empty, Books = new List<Book>() { book } };

            //Act
            ReadQueryBuilder<Author> read = new ReadQueryBuilder<Author>();
            InsertQueryBuilder<Author> insert = new InsertQueryBuilder<Author>(author);
            UpdateQueryBuilder<Author> update = new UpdateQueryBuilder<Author>(author);
            DeleteQueryBuilder<Author> delete = new DeleteQueryBuilder<Author>(author);

            //Assert
            Assert.IsTrue(Dictionaries.TypeMappingCache.ContainsKey(typeof(Author)));
            Assert.IsTrue(Dictionaries.TypeMappingCache.ContainsKey(typeof(Book)));
        }

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
            sqlBuilder.AppendLine("into #books_staging from books as Book");
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine("select * from #books_staging");

            expectedQuery = sqlBuilder.ToString();
            testQuery = new ReadQueryBuilder<Book>().GetQuery().QueryString;

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
            testQuery = new ReadQueryBuilder<Author>().GetQuery().QueryString;

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
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");

            expectedQuery = sqlBuilder.ToString();
            testQuery = new DeleteQueryBuilder<Author>(author).GetQuery().QueryString;

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
            sqlBuilder.AppendLine("where Author.id = @authors_id_2");
            sqlBuilder.AppendLine("select @@rowcount as affected_rows");

            expectedQuery = sqlBuilder.ToString();
            testQuery = new UpdateQueryBuilder<Author>(author).GetQuery().QueryString;

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);
        }

    }
}
