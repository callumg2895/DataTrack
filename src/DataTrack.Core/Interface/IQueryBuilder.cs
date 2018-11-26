using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryBuilderObjects;
using System.Collections.Generic;

namespace DataTrack.Core.Interface
{
    public interface IQueryBuilder<TBase>
    {

        string ToString();

        QueryBuilder<TBase> AddRestriction<TProp>(string property, RestrictionTypes rType, TProp value);

        List<(string Handle, object Value)> GetParameters();
    }
}
