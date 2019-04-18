using DataTrack.Core.Attributes;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Interface;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public class BulkDataBuilder<TBase> where TBase : IEntity
    {

        #region Members

        public List<IEntity> Data { get; private set; }
        public List<Table> Tables { get; private set; }
        public Mapping<TBase> Mapping { get; private set; }

        private Map<Table, DataTable> DataMap = new Map<Table, DataTable>();
        private Map<Column, DataColumn> ColumnMap = new Map<Column, DataColumn>();
        private Type BaseType = typeof(TBase);
        #endregion

        #region Constructors

        public BulkDataBuilder(IEntity data, Mapping<TBase> mapping)
            : this(new List<IEntity>() { data }, mapping)
        {

        }

        public BulkDataBuilder(List<IEntity> data, Mapping<TBase> mapping)
        {
            Data = data;
            Tables = mapping.Tables;
            Mapping = mapping;
        }

        #endregion Constructors

        #region Methods

        public Map<Table, DataTable> YieldDataMap()
        {
            foreach(var item in Data)
            {
                BuildDataFor(item);
            }

            return DataMap;
        }

        private void BuildDataFor(IEntity item)
        {
            if (item == null)
                return;

            Type type = item.GetType();
            Table table = Mapping.TypeTableMapping[type];

            if (!DataMap.ContainsKey(table))
            {
                DataMap[table] = new DataTable(table.Name);
                Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {Data?.GetType().ToString()}");
            }

            DataTable dataTable = DataMap[table];

            SetColumns(dataTable);
            AddRow(dataTable, item);

            foreach (var childTable in Mapping.ParentChildMapping[table])
            {
                var childItems = item.GetChildPropertyValues(childTable.Name);

                if (childItems == null)
                    continue;

                if (!Mapping.ParentChildEntityMapping.ContainsKey(item))
                {
                    Mapping.ParentChildEntityMapping[item] = new List<IEntity>();
                }

                foreach (var childItem in childItems)
                {
                    BuildDataFor(childItem);
                    Mapping.ParentChildEntityMapping[item].Add(childItem);
                }
            }
            
        }

        private void SetColumns(DataTable dataTable)
        {
            List<Column> columns = DataMap[dataTable].Columns;

            foreach (Column column in columns)
            {
                if (ColumnMap.ContainsKey(column))
                    return;

                DataColumn dataColumn = new DataColumn(column.Name);
                List<DataColumn> primaryKeys = new List<DataColumn>();

                if (!column.IsPrimaryKey())
                {
                    dataTable.Columns.Add(dataColumn);
                    ColumnMap[column] = dataColumn;
                }

                if (column.IsPrimaryKey())
                    primaryKeys.Add(dataColumn);
            }
        }

        private void AddRow(DataTable dataTable, IEntity item)
        {
            List<object> rowData = item.GetPropertyValues();

            Table table = DataMap[dataTable];
            DataRow dataRow = dataTable.NewRow();

            for (int i = 0; i < rowData.Count; i++)
            {
                Column column = table.Columns[i];
                if (!column.IsPrimaryKey())
                    dataRow[column.Name] = rowData[i];
            }

            dataTable.Rows.Add(dataRow);
            Mapping.EntityDataRowMapping.Add(item, dataRow);
        }
        #endregion  
    }
}
