using DataTrack.Core.Interface;
using DataTrack.Core.Repository;
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
    public class RepositoryTest : BaseTest
    {

        [TestMethod]
        public void TestRepository_ShouldReturnCorrectObjectForReadWithRestriction()
        {
            // Arrange
            Author author = Author.GetAuthors(1, 2)[0];

            //Act
            Repository<Author>.Create(author);
            Author authorReadResult = Repository<Author>.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
            Book book1ReadResult = Repository<Book>.GetByProperty("title", Enums.RestrictionTypes.EqualTo, author.Books[0].Title)[0];
            Repository<Author>.Delete(authorReadResult);

            // Assert
            Assert.IsTrue(AuthorsAreEqual(authorReadResult, author));
        }

        [TestMethod]
        public void TestRepository_ShouldReturnCorrectObjectForGetByPropertyType()
        {
            // Arrange
            Author author = Author.GetAuthors(1, 5)[0];

            //Act
            Repository<Author>.Create(author);
            List<Author> authorReadResult = Repository<Author>.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName);
            List<Book> bookReadResult = Repository<Book>.GetByProperty("title", Enums.RestrictionTypes.EqualTo, author.Books[0].Title);

            foreach( Author authorResult in authorReadResult)
            {
                Repository<Author>.Delete(authorResult);
            }

            // Assert
            Assert.IsTrue(BooksAreEqual(bookReadResult[0], author.Books[0]));
        }

        [TestMethod]
        public void TestRepository_ShouldReadCorrectNumberOfChildItemsAfterInsertingObjectWithLongListOfChildren()
        {
            // Arrange
            Author author = Author.GetAuthors(1, 5)[0];

            //Act
            Repository<Author>.Create(author);

            Author authorReadResult = Repository<Author>.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
            
            foreach(Book book in authorReadResult.Books)
            {
                Repository<Book>.Delete(book);
            }

            Repository<Author>.Delete(authorReadResult);

            // Assert
            Assert.AreEqual(authorReadResult.Books.Count, 5);
        }

        [TestMethod]
        public void TestRepository_ShouldInsertMultipleEntitiesFromList()
        {
            // Arrange
            int authorsToInsert = 10;
            int booksPerAuthor = 20;

            List<IEntity> authors =  new List<IEntity>(Author.GetAuthors(authorsToInsert, booksPerAuthor));

            // Act
            Repository<Author>.Create(authors);

            List<Author> createdAuthors = Repository<Author>.GetAll();
            List<Book> createdBooks = Repository<Book>.GetAll();

            foreach(Author author in createdAuthors)
            {
                Repository<Author>.Delete(author);
            }

            foreach(Book book in createdBooks)
            {
                Repository<Book>.Delete(book);
            }

            // Assert
            Assert.AreEqual(authorsToInsert, createdAuthors.Count);
            Assert.AreEqual(authorsToInsert * booksPerAuthor, createdBooks.Count);
        }
    }
}
