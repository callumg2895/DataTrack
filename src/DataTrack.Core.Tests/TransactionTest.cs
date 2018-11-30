using DataTrack.Core.SQL;
using DataTrack.Core.SQL.QueryBuilderObjects;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Tests.TestObjects;
using DataTrack.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

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

            List<object> results = null;

            //Act
            stopwatch.Start();

            using (Transaction<Author> t1 = new Transaction<Author>(new List<Query<Author>>(){
                new InsertQueryBuilder<Author>(author).GetQuery(),
                new ReadQueryBuilder<Author>(author.ID).GetQuery(),
                new DeleteQueryBuilder<Author>(author).GetQuery(),
            }))
            {
                results = t1.Execute();
                t1.Commit();
            }

            using (Transaction<Book> t2 = new Transaction<Book>(new DeleteQueryBuilder<Book>(book).GetQuery()))
            {
                t2.Execute();
                t2.Commit();
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Transaction<Author> executed in {stopwatch.ElapsedMilliseconds}ms");

            int affectedRows = (int)results[0];
            Author result = ((List<Author>)results[1])[0];

            // Assert
            Assert.AreEqual(affectedRows, 2);
            Assert.IsTrue(AuthorsAreEqual(result, author));
        }

        [TestMethod]
        public void TestTransaction_ShouldReturnCorrectObjectBeforeAndAfterUpdate()
        {
            // Arrange
            Author authorBefore = new Author() { ID = 1, FirstName = "John", LastName = "Smith", Books = new List<Book>()};
            Author authorAfter = new Author() { ID = 1, FirstName = "James", LastName = "Smith", Books = new List<Book>() };
            List<object> results1 = null;
            List<object> results2 = null;

            // Act
            stopwatch.Start();

            using (Transaction<Author> t1 = new Transaction<Author>(new List<Query<Author>>()
            {
                new InsertQueryBuilder<Author>(authorBefore).GetQuery(),
                new ReadQueryBuilder<Author>(authorBefore.ID).GetQuery(),
            }))
            {
                results1 = t1.Execute();
                t1.Commit();
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Transaction<Author> executed in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Start();

            using (Transaction<Author> t2 = new Transaction<Author>(new List<Query<Author>>()
            {
                new UpdateQueryBuilder<Author>(authorAfter).GetQuery(),
                new ReadQueryBuilder<Author>(authorAfter.ID).GetQuery(),
                new DeleteQueryBuilder<Author>(authorAfter).GetQuery(),
            }))
            {
                results2 = t2.Execute();
                t2.Commit();
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Transaction<Author> executed in {stopwatch.ElapsedMilliseconds}ms");

            int affectedInsertRows = (int)results1[0];
            Author beforeUpdate = ((List<Author>)results1[1])[0];
            int affectedUpdateRows = (int)results2[0];
            Author afterUpdate = ((List<Author>)results2[1])[0];

            // Assert
            Assert.AreEqual(affectedInsertRows, 1);
            Assert.AreEqual(affectedUpdateRows, 1);
            Assert.IsTrue(AuthorsAreEqual(beforeUpdate, authorBefore));
            Assert.IsTrue(AuthorsAreEqual(afterUpdate, authorAfter));
        }
    }
}
