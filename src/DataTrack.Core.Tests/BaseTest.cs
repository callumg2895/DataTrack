using DataTrack.Core.Enums;
using DataTrack.Core.Tests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

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

            DataTrackConfiguration.Init(false, ConfigType.ConnectionString, connectionString);

            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                string sqlInit = @"
                    if OBJECT_ID('authors','U') is null
                    begin
                        create table authors
                        (
                            id int not null identity(1,1),
                            first_name varchar(255) not null,
                            last_name varchar(255) not null
                            primary key (id)
                        )

                        create table books
                        (
                            id int not null identity(1,1),
                            author_id int not null,
                            title varchar(255) not null
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

        protected bool AuthorsAreEqual(Author author1, Author author2)
        {
            bool equal = true;

            equal &= author1.FirstName == author2.FirstName;
            equal &= author1.LastName == author2.LastName;
            equal &= author1.Books.Count == author2.Books.Count;

            if (equal)
            {
                for (int i = 0; i < author1.Books.Count; i++)
                {
                    equal &= BooksAreEqual(author1.Books[i], author2.Books[i]);
                }
            }

            return equal;
        }

        protected bool BooksAreEqual(Book book1, Book book2)
        {
            bool equal = true;

            equal &= book1.AuthorId == book2.AuthorId;
            equal &= book1.Title == book2.Title;

            return equal;
        }

    }
}
