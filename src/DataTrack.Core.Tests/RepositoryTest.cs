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
            Book book1 = new Book() { AuthorId = 1, Title = "The Great Gatsby" };
            Book book2 = new Book() { AuthorId = 1, Title = "The Beautiful and Damned" };
            Author author = new Author()
            {
                FirstName = "F.Scott",
                LastName = "Fitzgerald",
                Books = new List<Book>() { book1, book2 }
            };

            Author authorReadResult;
            Book book1ReadResult;

            //Act
            Repository<Author>.Create(author);

            authorReadResult = Repository<Author>.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
            book1ReadResult = Repository<Book>.GetByProperty("title", Enums.RestrictionTypes.EqualTo, book1.Title)[0];
            Repository<Author>.Delete(authorReadResult);

            // Assert
            Assert.IsTrue(AuthorsAreEqual(authorReadResult, author));
        }

        [TestMethod]
        public void TestRepository_ShouldReturnCorrectObjectForGetByPropertyType()
        {
            // Arrange
            Book book1 = new Book() { Title = "The Great Gatsby" };
            Book book2 = new Book() { Title = "The Beautiful and Damned" };
            Author author = new Author()
            {
                FirstName = "F.Scott",
                LastName = "Fitzgerald",
                Books = new List<Book>() { book1, book2 }
            };

            List<Book> bookReadResult;
            List<Author> authorReadResult;

            //Act
            Repository<Author>.Create(author);
            authorReadResult = Repository<Author>.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName);
            bookReadResult = Repository<Book>.GetByProperty("title", Enums.RestrictionTypes.EqualTo, "The Great Gatsby");

            foreach( Author authorResult in authorReadResult)
            {
                Repository<Author>.Delete(authorResult);
            }

            // Assert
            Assert.IsTrue(BooksAreEqual(bookReadResult[0], book1));
        }

        [TestMethod]
        public void TestRepository_ShouldReadCorrectNumberOfChildItemsAfterInsertingObjectWithLongListOfChildren()
        {
            // Arrange
            Book book1 = new Book() { Title = "The Great Gatsby" };
            Book book2 = new Book() { Title = "The Beautiful and Damned" };
            Book book3 = new Book() { Title = "This Side of Paradise" };
            Book book4 = new Book() { Title = "Tender is the Night" };
            Book book5 = new Book() { Title = "The Last Tycoon" };
            Author author = new Author()
            {
                FirstName = "F.Scott",
                LastName = "Fitzgerald",
                Books = new List<Book>() { book1, book2, book3, book4, book5 }
            };

            Author authorReadResult;

            //Act
            Repository<Author>.Create(author);
            authorReadResult = Repository<Author>.GetByProperty("first_name", Enums.RestrictionTypes.EqualTo, author.FirstName)[0];
            Repository<Book>.Delete(book1);
            Repository<Book>.Delete(book2);
            Repository<Book>.Delete(book3);
            Repository<Book>.Delete(book4);
            Repository<Book>.Delete(book5);
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

            List<IEntity> authors = new List<IEntity>();

            for (int i = 0; i < authorsToInsert; i++)
            {
                authors.Add(new Author() { FirstName = $"FirstName{i}", LastName = $"LastName{i}", Books = new List<Book>() });
            }

            foreach (Author author in authors)
            {
                for (int i = 0; i < booksPerAuthor; i++)
                {
                    author.Books.Add(new Book() { Title = $"Title{i}"});
                }
            }

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
