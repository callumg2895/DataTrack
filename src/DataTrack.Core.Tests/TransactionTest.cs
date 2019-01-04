using DataTrack.Core.SQL;
using DataTrack.Core.SQL.BuilderObjects;
using DataTrack.Core.SQL.DataStructures;
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
            Book book = new Book() {Title = "The Great Gatsby" };

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
                new ReadQueryBuilder<Author>().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName).GetQuery(),
            }))
            {
                results = t1.Execute();
                t1.Commit();
            }

            List<Author> authorResults = (List<Author>)results[1];

            foreach (Author authorResult in authorResults)
            {
                foreach (Book bookResult in authorResult.Books)
                {
                    new DeleteQueryBuilder<Book>(bookResult).GetQuery().Execute();
                }

                new DeleteQueryBuilder<Author>(authorResult).GetQuery().Execute();
            }

            Author result = ((List<Author>)results[1])[0];

            // Assert
            Assert.IsTrue(AuthorsAreEqual(result, author));
        }

        [TestMethod]
        public void TestTransaction_ShouldReturnCorrectObjectBeforeAndAfterUpdate()
        {
            // Arrange
            Author authorBefore = new Author() { FirstName = "John", LastName = "Smith", Books = new List<Book>()};
            Author authorAfter = new Author() { FirstName = "James", LastName = "Smith", Books = new List<Book>()};
            List<object> results1 = null;
            List<object> results2 = null;

            // Act

            using (Transaction<Author> t1 = new Transaction<Author>(new List<Query<Author>>()
            {
                new InsertQueryBuilder<Author>(authorBefore).GetQuery(),
                new ReadQueryBuilder<Author>()
                    .AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, authorBefore.FirstName)
                    .GetQuery(),
            }))
            {
                results1 = t1.Execute();
                t1.Commit();
            }

            using (Transaction<Author> t2 = new Transaction<Author>(new List<Query<Author>>()
            {
                new UpdateQueryBuilder<Author>(authorAfter).GetQuery(),
                new ReadQueryBuilder<Author>()
                    .AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, authorAfter.FirstName)
                    .GetQuery(),
            }))
            {
                results2 = t2.Execute();
                t2.Commit();
            }

            Author beforeUpdate = ((List<Author>)results1[1])[0];
            Author afterUpdate = ((List<Author>)results2[1])[0];
        
            new DeleteQueryBuilder<Author>(beforeUpdate).GetQuery().Execute();
            new DeleteQueryBuilder<Author>(afterUpdate).GetQuery().Execute();

            // Assert
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

            List<Author> resultsAfterRollBack;
            List<Author> resultsAfterDelete;

            // Act

            new InsertQueryBuilder<Author>(author).GetQuery().Execute();

            using (Transaction<Author> t = new Transaction<Author>(new DeleteQueryBuilder<Author>(author).GetQuery()))
            {
                t.Execute();
                t.RollBack();
            }

            resultsAfterRollBack = new ReadQueryBuilder<Author>().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName).GetQuery().Execute();

            foreach(Author result in resultsAfterRollBack)
            {
                new DeleteQueryBuilder<Author>(result).GetQuery().Execute();
            }

            resultsAfterDelete = new ReadQueryBuilder<Author>().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName).GetQuery().Execute();

            //Assert
            Assert.AreEqual(resultsAfterRollBack.Count, 1);
            Assert.AreEqual(resultsAfterDelete.Count, 0);


        }
    }
}
