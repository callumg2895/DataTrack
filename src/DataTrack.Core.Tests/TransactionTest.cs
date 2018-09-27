using DataTrack.Core.Enums;
using DataTrack.Core.Sql.Read;
using DataTrack.Core.SQL;
using DataTrack.Core.SQL.Delete;
using DataTrack.Core.SQL.Insert;
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
    public class TransactionTest
    {

        private Stopwatch stopwatch = new Stopwatch();

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            string connectionString =
                "Data Source=(local);" +
                "Initial Catalog=data_track_testing;" +
                "User id=sa;" +
                "Password=password;";

            DataTrackConfiguration.Init(OutputTypes.All, ConfigType.ConnectionString, connectionString);

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
        public void TestTransaction_ShouldReturnCorrectObjectForReadWithRestriction()
        {
            // Arrange
            Author author = new Author() { ID = 1, FirstName = "John", LastName = "Smith" };

            // Act
            stopwatch.Start();
            Transaction<Author> t = new Transaction<Author>(new List<QueryBuilder<Author>>()
            {
                new InsertQueryBuilder<Author>(author),
                new ReadQueryBuilder<Author>()
                    .AddRestriction<Author, int>("id", RestrictionTypes.EqualTo, author.ID),
                new DeleteQueryBuilder<Author>(author)
            });

            List<object> results = t.Execute();
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Transaction executed in {stopwatch.ElapsedMilliseconds}ms");

            int affectedRows = (int)results[0];
            Author result = ((List<Author>)results[1])[0];

            // Assert
            Assert.AreEqual(affectedRows, 1);
            Assert.AreEqual(result.ID, author.ID);
            Assert.AreEqual(result.FirstName, author.FirstName);
            Assert.AreEqual(result.LastName, author.LastName);
        }
    }
}
