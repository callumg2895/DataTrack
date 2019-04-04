﻿using DataTrack.Core.Attributes;
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

        public TBase Data { get; private set; }
        public List<Table> Tables { get; private set; }

        private Map<Table, DataTable> DataMap = new Map<Table, DataTable>();
        private Map<Column, DataColumn> ColumnMap = new Map<Column, DataColumn>();
        private Type BaseType = typeof(TBase);
        #endregion

        #region Constructors

        public BulkDataBuilder(TBase data, Mapping<TBase> mapping)
        {
            Data = data;
            Tables = mapping.Tables;
        }

        #endregion Constructors

        #region Methods

        public Map<Table, DataTable> YieldDataMap()
        {
            ConstructData();
            return DataMap;
        }

        private void ConstructData()
        {
            // For inserts, we build a list of DataTables, where each 'table' in the list corresponds to the data for a table in the Query object
            Tables.ForEach(table => BuildDataFor(table));
            Logger.Info(MethodBase.GetCurrentMethod(), $"Created {DataMap.ForwardKeys.Count} DataTable{(DataMap.ForwardKeys.Count > 1 ? "s" : "")}");
        }

        private DataTable BuildDataFor(Table table)
        {
            DataTable dataTable = new DataTable(table.Name);
            DataMap[dataTable] = table;

            if (table.Type == BaseType)
            {
                Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {Data?.GetType().ToString()}");

                if (Data != null)
                {
                    List<object> items = Data.GetPropertyValues();

                    SetColumns(dataTable);
                    AddRow(dataTable, items);

                    Logger.Info($"Current table row count: {dataTable.Rows.Count}");
                    items.ForEach(item => Logger.Info(item?.ToString() ?? "NULL"));
                }
            }
            else
            {
                if (Data != null && Data.GetChildPropertyValues(table.Name) != null)
                {
                    SetColumns(dataTable);

                    dynamic childItems = Activator.CreateInstance(typeof(List<>).MakeGenericType(table.Type));

                    foreach (var item in Data.GetChildPropertyValues(table.Name))
                    {
                        childItems.Add(item);
                    }

                    Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {table.Name}");

                    foreach (var item in childItems)
                    {
                        List<object> values = item.GetPropertyValues();
                        AddRow(dataTable, values);

                        values.ForEach(value => Logger.Info(value?.ToString() ?? "NULL"));
                        Logger.Info($"Current table row count: {dataTable.Rows.Count}");
                    }
                }
            }

            return dataTable;
        }

        private void SetColumns(DataTable dataTable)
        {
            List<Column> columns = DataMap[dataTable].Columns;

            foreach (Column column in columns)
            {
                DataColumn dataColumn = new DataColumn(column.Name);
                List<DataColumn> primaryKeys = new List<DataColumn>();
                ForeignKeyConstraint fk;

                if (!column.IsPrimaryKey())
                {
                    dataTable.Columns.Add(dataColumn);
                    ColumnMap[column] = dataColumn;
                }

                if (column.IsPrimaryKey())
                    primaryKeys.Add(dataColumn);

                //if (column.IsForeignKey())
                //{
                //    foreach (Table table in Tables)
                //    {
                //        if (column.ForeignKeyTableMapping == table.Name)
                //        {
                //            DataColumn parentColumn = DataMap[table].Columns.Cast<DataColumn>().Where(c => ColumnMap[c].Name == column.ForeignKeyColumnMapping).First();
                //            fk = new ForeignKeyConstraint(parentColumn, dataColumn);
                //            dataTable.Constraints.Add(fk);
                //        }
                //    }
                //}

                //dataTable.PrimaryKey = primaryKeys.ToArray();
            }
        }

        private void AddRow(DataTable dataTable, List<object> rowData)
        {
            Table table = DataMap[dataTable];
            DataRow dataRow = dataTable.NewRow();

            for (int i = 0; i < rowData.Count; i++)
            {
                Column column = table.Columns[i];
                if (!column.IsPrimaryKey())
                    dataRow[column.Name] = rowData[i];
            }

            dataTable.Rows.Add(dataRow);
        }
        #endregion  
    }
}
