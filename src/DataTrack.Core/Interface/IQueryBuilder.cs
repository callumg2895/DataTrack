using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
    public interface IQueryBuilder<TBase>
    {

        string ToString();

        IQueryBuilder<TBase> AddRestriction<T, TProp>(string property, RestrictionTypes rType, TProp value);

        List<TableMappingAttribute> GetTables();

        List<ColumnMappingAttribute> GetColumns();

        List<(string Handle, object Value)> GetParameters();
    }
}
