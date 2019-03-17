using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.DataStructures;
using System.Collections.Generic;

namespace DataTrack.Core.Tests.TestObjects
{
    [TableMapping("authors")]
    public class Author : Entity
    {
        [ColumnMapping("authors", "id", (byte)KeyTypes.PrimaryKey)]
        public virtual int ID { get; set; }

        [ColumnMapping("authors", "first_name")]
        public virtual string FirstName { get; set; }

        [ColumnMapping("authors", "last_name")]
        public virtual string LastName { get; set; }

        [TableMapping("books")]
        public virtual List<Book> Books { get; set; }
    }
}
