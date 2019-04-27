using DataTrack.Core.Interface;
using DataTrack.Core.Repository;
using DataTrack.Core.Tests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests
{
    [TestClass]
    public class IEntityTest : BaseTest
    {
        [TestMethod]
        public void TestIEntity_ShouldGiveCorrectIDValue()
        {
            // Arrange
            Author author = (Author)GetAuthors(1)[0];
            author.ID = 999;

            // Act
            int ID = (int)author.GetID();

            // Assert
            Assert.AreEqual(author.ID, ID);
        }

        [TestMethod]
        public void TestIEntity_ShouldGiveCorrectPropertyValue()
        {
            // Arrange
            Author author = (Author)GetAuthors(1)[0];

            // Act
            string FirstName = (string)author.GetPropertyValue("FirstName");
            string LastName = (string)author.GetPropertyValue("LastName");

            // Assert
            Assert.AreEqual(author.FirstName, FirstName);
            Assert.AreEqual(author.LastName, LastName);
        }

        [TestMethod]
        public void TestIEntity_ShouldGiveCorrectPropertyValues()
        {
            // Arrange
            Author author = (Author)GetAuthors(1)[0];

            // Act
            List<object> propertyValues = author.GetPropertyValues();

            // Assert
            Assert.AreEqual(author.FirstName, propertyValues[0]);
            Assert.AreEqual(author.LastName, propertyValues[1]);
            Assert.AreEqual(author.ID, propertyValues[2]);
        }

        [TestMethod]
        public void TestIEntity_ShouldGiveCorrectChildPropertyValues()
        {
            // Arrange
            Author author = (Author)GetAuthors(1, 3)[0];

            // Act
            dynamic childPropertyValues = author.GetChildPropertyValues("books");

            // Assert
            Assert.AreEqual(author.Books, childPropertyValues);
        }

        [TestMethod]
        public void TestIEntity_ShouldInstantiateChildProperties()
        {
            // Arrange
            Author author = (Author)GetAuthors(1, 3)[0];
            author.Books = null;

            // Act
            author.InstantiateChildProperties();

            // Assert
            Assert.AreEqual(author.Books.Count, 0);
        }

        [TestMethod]
        public void TestIEntity_ShouldAddChildProperties()
        {
            // Arrange
            Author author = (Author)GetAuthors(1)[0];
            Book book = (Book)GetBooks(1)[0];

            // Act
            author.AddChildPropertyValue("books", book);

            // Assert
            Assert.AreEqual(author.Books.Count, 1);
            Assert.AreEqual(author.Books[0], book);
        }
    }
}
