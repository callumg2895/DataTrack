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

namespace DataTrack.Core.SQL.BuilderObjects
{
    public class BulkDataBuilder<TBase> where TBase : new()
    {

        #region Members

        public TBase Data { get; private set; }
        public List<Table> Tables { get; private set; }
        public List<ColumnMappingAttribute> Columns { get; private set; }
        public Map<Type, Table> TypeTableMapping { get; private set; }
        public Map<Type, List<ColumnMappingAttribute>> TypeColumnMapping { get; private set; }

        private Map<TableMappingAttribute, DataTable> DataMap { get; set; } = new Map<TableMappingAttribute, DataTable>();
        private Map<ColumnMappingAttribute, DataColumn> ColumnMap { get; set; } = new Map<ColumnMappingAttribute, DataColumn>();
        private Type BaseType = typeof(TBase);
        #endregion

        #region Constructors

        public BulkDataBuilder(TBase data, Mapping<TBase> mapping)
        {
            Data = data;
            Tables = mapping.Tables;
            Columns = new List<ColumnMappingAttribute>();
            foreach ( var columns in mapping.Tables.Select(t => t.ColumnAttributes))
            {
                Columns.AddRange(columns);
            }
            TypeTableMapping = mapping.TypeTableMapping;
        }

        #endregion Constructors

        #region Methods

        public Map<TableMappingAttribute, DataTable> YieldDataMap()
        {
            ConstructData();
            return DataMap;
        }

        private void ConstructData()
        {
            // For inserts, we build a list of DataTables, where each 'table' in the list corresponds to the data for a table in the Query object
            Tables.ForEach(table => DataMap[table.TableAttribute] = BuildDataFor(table));
            Logger.Info(MethodBase.GetCurrentMethod(), $"Created {DataMap.ForwardKeys.Count} DataTable{(DataMap.ForwardKeys.Count > 1 ? "s" : "")}");
        }

        private DataTable BuildDataFor(Table table)
        {
            DataTable dataTable = new DataTable(table.TableAttribute.TableName);

            if (TypeTableMapping[table] == BaseType)
            {
                Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {Data?.GetType().ToString()}");
                List<ColumnMappingAttribute> columns = Dictionaries.TableMappingCache[table.TableAttribute];

                if (Data != null)
                {
                    List<object> items = table.TableAttribute.GetPropertyValues(Data);

                    SetColumns(dataTable, columns);
                    AddRow(dataTable, columns, items);

                    Logger.Info($"Current table row count: {dataTable.Rows.Count}");
                    items.ForEach(item => Logger.Info(item?.ToString() ?? "NULL"));
                }
            }
            else
            {
                if (Data != null && Tables[0].TableAttribute.GetChildPropertyValues(Data, table.TableAttribute.TableName) != null)
                {

                    List<ColumnMappingAttribute> columns = Dictionaries.TableMappingCache[table.TableAttribute];
                    SetColumns(dataTable, columns);

                    dynamic childItems = Activator.CreateInstance(typeof(List<>).MakeGenericType(TypeTableMapping[table]));

                    foreach (var item in Tables[0].TableAttribute.GetChildPropertyValues(Data, table.TableAttribute.TableName))
                    {
                        childItems.Add(item);
                    }

                    Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {TypeTableMapping[table].ToString()}");

                    foreach (dynamic item in childItems)
                    {
                        List<object> values = table.TableAttribute.GetPropertyValues(item);
                        AddRow(dataTable, columns, values);

                        values.ForEach(value => Logger.Info(value?.ToString() ?? "NULL"));
                        Logger.Info($"Current table row count: {dataTable.Rows.Count}");
                    }
                }
            }

            return dataTable;
        }

        private void SetColumns(DataTable dataTable, List<ColumnMappingAttribute> columns)
        {
            foreach (ColumnMappingAttribute column in columns)
            {
                DataColumn dataColumn = new DataColumn(column.ColumnName);
                List<DataColumn> primaryKeys = new List<DataColumn>();
                ForeignKeyConstraint fk;

                dataTable.Columns.Add(dataColumn);
                ColumnMap[column] = dataColumn;

                if (column.IsPrimaryKey())
                    primaryKeys.Add(dataColumn);

                if (column.IsForeignKey())
                {
                    foreach(Table table in Tables)
                    {
                        if (column.ForeignKeyTableMapping == table.TableAttribute.TableName)
                        {
                            DataColumn parentColumn = DataMap[table.TableAttribute].Columns.Cast<DataColumn>().Where(c => ColumnMap[c].ColumnName == column.ForeignKeyColumnMapping).First();
                            fk = new ForeignKeyConstraint(parentColumn, dataColumn);
                            dataTable.Constraints.Add(fk);
                        }
                    }
                }

                dataTable.PrimaryKey = primaryKeys.ToArray();
            }
        }

        private void AddRow(DataTable dataTable, List<ColumnMappingAttribute> columns, List<object> rowData)
        {
            DataRow dataRow = dataTable.NewRow();

            for (int i = 0; i < rowData.Count; i++)
            {
                ColumnMappingAttribute column = columns[i];
                dataRow[column.ColumnName] = rowData[i];
            }

            dataTable.Rows.Add(dataRow);
        }
        #endregion  
    }
}
