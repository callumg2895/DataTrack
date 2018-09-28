using DataTrack.Core.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataTrack.Core.Tests
{
    [TestClass]
    public class BaseTest
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext testContext)
        {
            string connectionString =
                "Data Source=(local);" +
                "Initial Catalog=data_track_testing;" +
                "User id=sa;" +
                "Password=password;";

            DataTrackConfiguration.Init(OutputTypes.None, ConfigType.ConnectionString, connectionString);

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

        [TestMethod]
        public static void TestConfiguration_ConnectionStringShouldBePopulated()
        {
            Assert.IsFalse(string.IsNullOrEmpty(DataTrackConfiguration.ConnectionString));
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
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

            DataTrackConfiguration.Dispose();
        }

    }
}
