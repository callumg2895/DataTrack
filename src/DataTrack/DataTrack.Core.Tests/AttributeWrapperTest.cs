using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Tests.TestClasses.TestBeans;
using DataTrack.Core.Tests.TestClasses.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.Tests
{
	[TestClass]
	public class AttributeWrapperTest : BaseTest
	{
		[TestMethod]
		public void TestAttrbuteWrapper_ShouldLoadAllCorrectAttributesForEntityWithoutForeignKey()
		{
			// Arrange
			// Act
			AttributeWrapper wrapper = new AttributeWrapper(typeof(Author));

			// Assert
			Assert.AreEqual(wrapper.MappingType, MappingTypes.TableBased);
			Assert.AreEqual(wrapper.TableAttribute.TableName, "authors");

			Assert.IsNotNull(wrapper.ColumnAttributes.Where(c => c.ColumnName == "id").FirstOrDefault());
			Assert.IsNotNull(wrapper.ColumnAttributes.Where(c => c.ColumnName == "first_name").FirstOrDefault());
			Assert.IsNotNull(wrapper.ColumnAttributes.Where(c => c.ColumnName == "last_name").FirstOrDefault());

			Assert.IsTrue(wrapper.ColumnForeignKeys.Keys.Count() == 0);
			Assert.IsTrue(wrapper.ColumnPrimaryKeys.Keys.Count() == 1);
			Assert.IsTrue(wrapper.ColumnPrimaryKeys.Keys.First().ColumnName == "id");
		}

		[TestMethod]
		public void TestAttrbuteWrapper_ShouldLoadAllCorrectAttributesForEntityWithForeignKey()
		{
			// Arrange
			// Act
			AttributeWrapper wrapper = new AttributeWrapper(typeof(Book));

			// Assert
			Assert.AreEqual(wrapper.MappingType, MappingTypes.TableBased);
			Assert.AreEqual(wrapper.TableAttribute.TableName, "books");

			Assert.IsNotNull(wrapper.ColumnAttributes.Where(c => c.ColumnName == "id").FirstOrDefault());
			Assert.IsNotNull(wrapper.ColumnAttributes.Where(c => c.ColumnName == "author_id").FirstOrDefault());
			Assert.IsNotNull(wrapper.ColumnAttributes.Where(c => c.ColumnName == "title").FirstOrDefault());
			Assert.IsNotNull(wrapper.FormulaAttributes.Where(f => f.Alias == "avg_score").FirstOrDefault());

			Assert.IsTrue(wrapper.ColumnForeignKeys.Keys.Count() == 1);
			Assert.IsTrue(wrapper.ColumnForeignKeys.Keys.First().ColumnName == "author_id");
			Assert.IsTrue(wrapper.ColumnPrimaryKeys.Keys.Count() == 1);
			Assert.IsTrue(wrapper.ColumnPrimaryKeys.Keys.First().ColumnName == "id");
		}

		[TestMethod]
		public void TestAttrbuteWrapper_ShouldLoadAllCorrectAttributesForEntityBean()
		{
			// Arrange
			// Act
			AttributeWrapper wrapper = new AttributeWrapper(typeof(BookInfo));

			// Assert
			Assert.AreEqual(wrapper.MappingType, MappingTypes.EntityBased);

			Assert.IsNotNull(wrapper.EntityAttributes.Where(e => e.EntityType == typeof(Author) && e.EntityProperty == "FirstName").FirstOrDefault());
			Assert.IsNotNull(wrapper.EntityAttributes.Where(e => e.EntityType == typeof(Book) && e.EntityProperty == "Title").FirstOrDefault());

			Assert.IsTrue(wrapper.EntityAttributes.Count == 2);
		}
	}
}
