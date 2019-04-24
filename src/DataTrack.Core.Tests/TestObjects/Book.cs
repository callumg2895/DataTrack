﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using System.Collections.Generic;

namespace DataTrack.Core.Tests.TestObjects
{
    [Table("books")]
    public class Book : Entity<int>
    {
        [Column("author_id")]
        [ForeignKey("authors")]
        public virtual int AuthorId { get; set; }

        [Column("title")]
        public virtual string Title { get; set; }
    }
}
