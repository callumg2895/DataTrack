using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryBuilderObjects;
using DataTrack.Core.SQL.QueryObjects;
using System.Collections.Generic;

namespace DataTrack.Core.Interface
{
    public interface IQueryBuilder<TBase> where TBase : new()
    {
        Query<TBase> GetQuery();
        QueryBuilder<TBase> AddRestriction<TProp>(string property, RestrictionTypes rType, TProp value);
    }
}
