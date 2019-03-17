﻿using DataTrack.Core.Enums;
using DataTrack.Core.SQL.BuilderObjects;
using DataTrack.Core.SQL.DataStructures;

namespace DataTrack.Core.Interface
{
    public interface IQueryBuilder<TBase> where TBase : Entity, new()
    {
        Query<TBase> GetQuery();
        QueryBuilder<TBase> AddRestriction<TProp>(string property, RestrictionTypes rType, TProp value);
    }
}
