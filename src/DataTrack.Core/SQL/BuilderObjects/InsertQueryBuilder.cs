using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public class InsertQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : Entity, new()
    {

        #region Members

        public TBase Item { get; private set; }

        #endregion

        #region Constructors

        public InsertQueryBuilder(TBase item, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Create);

            Item = item;
            CurrentParameterIndex = parameterIndex;
        }

        #endregion

        #region Methods

        public override Query<TBase> GetQuery()
        {        
            Query.Mapping.DataTableMapping = new BulkDataBuilder<TBase>(Item, Query.Mapping).YieldDataMap();
            return Query;
        }

        #endregion Methods
    }
}
