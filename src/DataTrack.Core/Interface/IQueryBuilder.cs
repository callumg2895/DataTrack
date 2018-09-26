using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
    public interface IQueryBuilder<TBase>
    {

        string ToString();

        QueryBuilder<TBase> AddRestriction<T, TProp>(string property, RestrictionTypes rType, TProp value);

        List<(string Handle, object Value)> GetParameters();
    }
}
