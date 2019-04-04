using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.DataStructures;

namespace DataTrack.Core.Tests.TestObjects
{
    [TableMapping("books")]
    public class Book : Entity<int>
    {
        [ColumnMapping("books", "author_id", (byte)KeyTypes.ForeignKey, "authors", "id")]
        public virtual int AuthorId { get; set; }

        [ColumnMapping("books", "title")]
        public virtual string Title { get; set; }
    }
}
