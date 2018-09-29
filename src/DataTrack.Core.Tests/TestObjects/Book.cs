using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests.TestObjects
{
    [TableMapping("books")]
    public class Book
    {
        [ColumnMapping("books", "id", Enums.KeyTypes.PrimaryKey)]
        public virtual int ID { get; set; }

        [ColumnMapping("books", "author_id", Enums.KeyTypes.ForeignKey, "authors")]
        public virtual int AuthorId { get; set; }

        [ColumnMapping("books", "title")]
        public virtual string Title { get; set; }
    }
}
