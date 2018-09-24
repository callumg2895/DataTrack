using DataTrack.Core.Enums;
using DataTrack.Core.Sql.Read;
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
    public class QueryBuilderTest
    {
        private Stopwatch stopwatch = new Stopwatch();

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            DataTrackConfiguration.Init(OutputTypes.All, ConfigType.FilePath, "[insert db.config file path here]");

            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                string sqlInit = @"
                    if OBJECT_ID('authors','U') is null
                    begin
                        create table authors
                        (
                            id int not null,
                            first_name varchar(255) not null,
                            last_name varchar(255) not null
                            primary key (id)
                        )
                    end";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqlInit;
                    command.CommandType = System.Data.CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "exec sp_MSforeachtable @command1 = \"DROP TABLE ?\"";
                    command.CommandType = System.Data.CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
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
            sqlBuilder.AppendLine("select Author.id, Author.first_name, Author.last_name ");
            sqlBuilder.AppendLine("from authors as Author ");

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
            sqlBuilder.AppendLine("values (@authors_id, @authors_first_name, @authors_last_name)");

            expectedQuery = sqlBuilder.ToString();

            stopwatch.Start();
            testQuery = new InsertQueryBuilder<Author>(author).ToString();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"InsertQueryBuilder executed in {stopwatch.ElapsedMilliseconds}ms");

            //Assert
            Assert.AreNotEqual(testQuery, string.Empty);
            Assert.AreEqual(testQuery, expectedQuery);

            new Transaction<Author>(new InsertQueryBuilder<Author>(author)).Execute();
            string b = "h2";
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
            sqlBuilder.AppendLine("delete * from authors as Author");
            sqlBuilder.AppendLine("where Author.id = @authors_id");

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
            sqlBuilder.AppendLine("update authors as Author");
            sqlBuilder.AppendLine("set id = @authors_id,");
            sqlBuilder.AppendLine("first_name = @authors_first_name,");
            sqlBuilder.AppendLine("last_name = @authors_last_name");
            sqlBuilder.AppendLine("where Author.id = @authors_id");

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
