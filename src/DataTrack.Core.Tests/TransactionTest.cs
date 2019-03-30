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
                new Query<Author>().Create(author),
                new Query<Author>().Read().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName),
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
                    new Query<Book>().Delete(bookResult).Execute();
                }

                new Query<Author>().Delete(authorResult).Execute();
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
                new Query<Author>().Create(authorBefore),
                new Query<Author>().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, authorBefore.FirstName)
            }))
            {
                results1 = t1.Execute();
                t1.Commit();
            }

            using (Transaction<Author> t2 = new Transaction<Author>(new List<Query<Author>>()
            {
                new Query<Author>().Update(authorAfter),
                new Query<Author>().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, authorAfter.FirstName),
            }))
            {
                results2 = t2.Execute();
                t2.Commit();
            }

            Author beforeUpdate = ((List<Author>)results1[1])[0];
            Author afterUpdate = ((List<Author>)results2[1])[0];
        
            new Query<Author>().Delete(beforeUpdate).Execute();
            new Query<Author>().Delete(afterUpdate).Execute();

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

            new Query<Author>().Create(author).Execute();

            using (Transaction<Author> t = new Transaction<Author>(new Query<Author>().Delete(author)))
            {
                t.Execute();
                t.RollBack();
            }

            resultsAfterRollBack = new Query<Author>().Read().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName).Execute();

            foreach(Author result in resultsAfterRollBack)
            {
                new Query<Author>().Delete(result).Execute();
            }

            resultsAfterDelete = new Query<Author>().Read().AddRestriction("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName).Execute();

            //Assert
            Assert.AreEqual(resultsAfterRollBack.Count, 1);
            Assert.AreEqual(resultsAfterDelete.Count, 0);


        }
    }
}
