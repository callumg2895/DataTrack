﻿using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.Repository;
using DataTrack.Core.Tests.TestClasses.TestBeans;
using DataTrack.Core.Tests.TestClasses.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DataTrack.Core.Tests
{
	[TestClass]
	public class BaseTest
	{
		private static int totalAuthors = 0;
		private static int totalBooks = 0;
		private static int totalReviews = 0;

		protected IEntityRepository<Author> AuthorRepository;
		protected IEntityRepository<Book> BookRepository;
		protected IEntityRepository<Review> ReviewRepository;

		protected IEntityBeanRepository<BookInfo> BookInfoRepository;

		public BaseTest()
		{
			this.AuthorRepository = new EntityRepository<Author>();
			this.BookRepository = new EntityRepository<Book>();
			this.ReviewRepository = new EntityRepository<Review>();

			this.BookInfoRepository = new EntityBeanRepository<BookInfo>();
		}

		[AssemblyInitialize]
		public static void AssemblyInit(TestContext testContext)
		{
			DataTrackConfiguration.Initialize(null);
			DataTrackConfiguration.Instance.StartAsync(CancellationToken.None);

			using (SqlConnection connection = DataTrackConfiguration.Instance.CreateConnection())
			{
				string sqlInit = @"
                    if OBJECT_ID('authors','U') is null
                    begin
                        create table authors
                        (
                            id int not null identity(1,1),
                            first_name varchar(255) not null,
                            last_name varchar(255) not null,
                            primary key (id)
                        )

                        create table books
                        (
                            id int not null identity(1,1),
                            author_id int not null,
                            title varchar(255) not null,
                            primary key (id),
                            foreign key (author_id) references authors(id) on delete cascade
                        )

                        create table reviews
                        (
                            identifier uniqueidentifier not null default newid(),
                            book_id int not null,
                            source varchar(255) not null,
                            score tinyint not null,
                            created datetime not null,
                            primary key (identifier),
                            foreign key (book_id) references books(id) on delete cascade
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

		[TestInitialize]
		public void TestInit()
		{
			
		}

		[TestCleanup]
		public void TestCleanup()
		{
			AuthorRepository.DeleteAll();
			BookRepository.DeleteAll();
			ReviewRepository.DeleteAll();
		}

		[TestMethod]
		public static void TestConfiguration_ConnectionStringShouldBePopulated()
		{
			Assert.IsFalse(string.IsNullOrEmpty(DataTrackConfiguration.ConnectionString));
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			using (SqlConnection connection = DataTrackConfiguration.Instance.CreateConnection())
			{
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText = @"
                        DECLARE @dropAllConstraints NVARCHAR(MAX) = N'';

                        SELECT @dropAllConstraints  += N'
                        ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id))
                            + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
                            ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
                        FROM sys.foreign_keys;
                        EXEC sp_executesql @dropAllConstraints 

                        exec sp_MSforeachtable @command1 = 'DROP TABLE ?'";

					command.CommandType = System.Data.CommandType.Text;
					command.ExecuteNonQuery();
				}
			}

			DataTrackConfiguration.Instance.StopAsync(CancellationToken.None);
		}

		protected List<Author> GetAuthors(int a, int b = 0, int r = 0)
		{
			List<Author> authors = new List<Author>();

			for (int i = totalAuthors; i < totalAuthors + a; i++)
			{
				authors.Add(new Author() { FirstName = $"FirstName{i}", LastName = $"LastName{i}", Books = GetBooks(b, r) });
			}

			totalAuthors += a;

			return authors;
		}

		protected List<Book> GetBooks(int b, int r = 0)
		{
			List<Book> books = new List<Book>();

			for (int i = totalBooks; i < totalBooks + b; i++)
			{
				books.Add(new Book() { Title = $"Title{i}", Reviews = GetReviews(r) });
			}

			totalBooks += b;

			return books;
		}

		protected List<Review> GetReviews(int r)
		{
			List<Review> reviews = new List<Review>();

			for (int i = totalReviews; i < totalReviews + r; i++)
			{
				reviews.Add(new Review() { Source = $"Source{i}", Score = (byte)new Random().Next(byte.MaxValue), Created = DateTime.UtcNow });
			}

			totalReviews += r;

			return reviews;
		}

		protected bool AuthorsAreEqual(Author author1, Author author2)
		{
			bool equal = true;

			equal &= author1.ID == author2.ID;
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

			equal &= book1.ID == book2.ID;
			equal &= book1.Title == book2.Title;

			if (equal)
			{
				for (int i = 0; i < book1.Reviews.Count; i++)
				{
					//equal &= ReviewsAreEqual(book1.Reviews[i], book2.Reviews[i]);
				}
			}

			return equal;
		}

		protected bool ReviewsAreEqual(Review review1, Review review2)
		{
			bool equal = true;

			equal &= review1.ID == review2.ID;
			equal &= review1.BookId == review2.BookId;

			return equal;
		}

	}
}
