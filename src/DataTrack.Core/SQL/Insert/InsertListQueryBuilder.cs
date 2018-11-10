using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.Insert
{
    public class InsertListQueryBuilder<TBase> : QueryBuilder<TBase>
    {

        #region Members

        public List<TBase> Items { get; private set; }

        #endregion

        #region Constructors

        public InsertListQueryBuilder(List<TBase> items, int parameterIndex = 1)
        {
            // Define the operation type used for transactions
            OperationType = CRUDOperationTypes.Create;

            // Fetch the table and column names for TBase
            GetTable();
            GetColumns();
            CacheMappingData();

            if (!Dictionaries.MappingCache.ContainsKey(typeof(TBase)))
            {
                Dictionaries.MappingCache[typeof(TBase)] = (TypeTableMapping[typeof(TBase)], TypeColumnMapping[typeof(TBase)]);
            }

            // Check for valid Table/Columns
            if (Tables.Count < 0 || Columns.Count < 0)
            {
                string message = $"Mapping data for class '{BaseType.Name}' was incomplete/empty";
                Logger.Error(MethodBase.GetCurrentMethod(), message);
                throw new Exception(message);
            }

            Items = items;
            CurrentParameterIndex = parameterIndex;

            UpdateParameters(Items);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            SQLBuilder sqlBuilder = new SQLBuilder(Parameters);

            sqlBuilder.AppendLine();

            for (int i = 0; i < Tables.Count; i++)
            {
                if (i == 0)
                {
                    sqlBuilder.BuildInsertStatement(Columns, Tables[i]);
                    sqlBuilder.BuildValuesStatement(Columns, Tables[i]);
                }
            }

            // For insert statements return the number of rows affected
            SelectRowCount(ref sqlBuilder);

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            return sql;
        }

        #endregion Methods
    }
}
