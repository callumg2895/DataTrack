﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public abstract class QueryBuilder<TBase> : IQueryBuilder<TBase> where TBase : Entity, new()
    {
        #region Members

        private protected Type BaseType;
        internal Query<TBase> Query { get; set; }

        #endregion

        #region Methods

        private protected void Init(CRUDOperationTypes opType)
        {
            BaseType = typeof(TBase);

            // Define the operation type used for transactions
            Query = new Query<TBase>(opType);
        }

        private protected void AddPrimaryKeyRestriction(TBase item)
        {
            Column primaryKeyColumn = Query.Mapping.TypeTableMapping[BaseType].GetPrimaryKeyColumn(); 

            // Find the name and value of the primary key property in the 'item' object
            if (primaryKeyColumn.TryGetPropertyName(BaseType, out string? primaryKeyColumnPropertyname))
            {
                var primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyname);
                this.AddRestriction<object>(primaryKeyColumn.Name, RestrictionTypes.EqualTo, primaryKeyValue);
            }
        }

        abstract public Query<TBase> GetQuery();

        public virtual QueryBuilder<TBase> AddRestriction<TProp>(string property, RestrictionTypes rType, TProp value)
        {
            Query.AddRestriction(property, rType, value);

            return this;
        }

        #endregion
    }
}
