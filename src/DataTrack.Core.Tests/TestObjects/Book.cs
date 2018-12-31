using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;

namespace DataTrack.Core.Tests.TestObjects
{
    [TableMapping("books")]
    public class Book
    {
        [ColumnMapping("books", "id", (byte)KeyTypes.PrimaryKey)]
        public virtual int ID { get; set; }

        [ColumnMapping("books", "author_id", (byte)KeyTypes.ForeignKey, "authors", "id")]
        public virtual int AuthorId { get; set; }

        [ColumnMapping("books", "title")]
        public virtual string Title { get; set; }
    }
}
