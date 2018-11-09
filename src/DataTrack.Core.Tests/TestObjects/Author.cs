using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Tests.TestObjects
{
    [TableMapping("authors")]
    public class Author
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
