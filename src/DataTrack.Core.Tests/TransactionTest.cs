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

        [TestMethod]
        public void TestTransaction_ShouldReturnCorrectObjectForReadWithRestriction()
        {
            // Arrange
            Book book = new Book() { AuthorId = 1, Title = "The Great Gatsby" };

            Author author = new Author()
            {
                FirstName = "John",
                LastName = "Smith",
                Books = new List<Book>() { book }
            };

            List<object> results = null;

            //Act
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
            Author authorBefore = new Author() { FirstName = "John", LastName = "Smith", Books = new List<Book>()};
            Author authorAfter = new Author() { FirstName = "James", LastName = "Smith", Books = new List<Book>() };
            List<object> results1 = null;
            List<object> results2 = null;

            // Act

            using (Transaction<Author> t1 = new Transaction<Author>(new List<Query<Author>>()
            {
                new InsertQueryBuilder<Author>(authorBefore).GetQuery(),
                new ReadQueryBuilder<Author>().GetQuery(),
            }))
            {
                results1 = t1.Execute();
                t1.Commit();
            }

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

        [TestMethod]
        public void TestTransaction_ShouldNotDeleteIfTransactionRolledBack()
        {
            // Arrange
            Author author = new Author()
            {
                FirstName = "John",
                LastName = "Smith",
            };

            int resultsAfterRollBack;
            int resultsAfterDelete;

            // Act

            new InsertQueryBuilder<Author>(author).GetQuery().Execute();

            using (Transaction<Author> t = new Transaction<Author>(new DeleteQueryBuilder<Author>(author).GetQuery()))
            {
                t.Execute();
                t.RollBack();
            }

            resultsAfterRollBack = new ReadQueryBuilder<Author>(author.ID).GetQuery().Execute().Count;

            new DeleteQueryBuilder<Author>(author).GetQuery().Execute();

            resultsAfterDelete = new ReadQueryBuilder<Author>(author.ID).GetQuery().Execute().Count;

            //Assert
            Assert.AreEqual(resultsAfterRollBack, 1);
            Assert.AreEqual(resultsAfterDelete, 0);


        }
    }
}
