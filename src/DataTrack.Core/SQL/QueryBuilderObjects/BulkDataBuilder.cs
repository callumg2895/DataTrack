using DataTrack.Core.Attributes;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class BulkDataBuilder<TBase>
    {

        #region Members

        public TBase Data { get; private set; }
        public List<TableMappingAttribute> Tables { get; private set; }
        public List<ColumnMappingAttribute> Columns { get; private set; }
        public Mapping<Type, TableMappingAttribute> TypeTableMapping { get; private set; }
        public Mapping<Type, List<ColumnMappingAttribute>> TypeColumnMapping { get; private set; }

        private Mapping<TableMappingAttribute, DataTable> DataMap { get; set; } = new Mapping<TableMappingAttribute, DataTable>();
        private Type BaseType = typeof(TBase);
        #endregion

        #region Constructors

        public BulkDataBuilder(TBase data, List<TableMappingAttribute> tables, List<ColumnMappingAttribute> columns, Mapping<Type, TableMappingAttribute> typeTableMapping, Mapping<Type, List<ColumnMappingAttribute>> typeColumnMapping)
        {
            Data = data;
            Tables = tables;
            Columns = columns;
            TypeTableMapping = typeTableMapping;
            TypeColumnMapping = typeColumnMapping;
        }

        #endregion Constructors

        #region Methods

        public Mapping<TableMappingAttribute, DataTable> YieldDataMap()
        {
            ConstructData();
            return DataMap;
        }

        private void ConstructData()
        {
            // For inserts, we build a list of DataTables, where each 'table' in the list corresponds to the data for a table in the Query object
            Tables.ForEach(table => DataMap[table] = BuildDataFor(table));
            Logger.Info(MethodBase.GetCurrentMethod(), $"Created {DataMap.ForwardKeys.Count} DataTable{(DataMap.ForwardKeys.Count > 1 ? "s" : "")}");
        }

        private DataTable BuildDataFor(TableMappingAttribute table)
        {
            DataTable dataTable = new DataTable(table.TableName);

            if (TypeTableMapping[table] == BaseType)
            {
                Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {Data.GetType().ToString()}");
                List<ColumnMappingAttribute> columns = TypeColumnMapping[TypeTableMapping[table]];
                List<object> items = table.GetPropertyValues(Data);

                dataTable.SetColumns(columns);
                dataTable.AddRow(columns, items);

                Logger.Info($"Current table row count: {dataTable.Rows.Count}");
                items.ForEach(item => Logger.Info(item?.ToString() ?? "NULL"));
            }
            else
            {
                if (Tables[0].GetChildPropertyValues(Data, table.TableName) != null)
                {

                    List<ColumnMappingAttribute> columns = Dictionaries.MappingCache[TypeTableMapping[table]].Columns;
                    dataTable.SetColumns(columns);

                    dynamic childItems = Activator.CreateInstance(typeof(List<>).MakeGenericType(TypeTableMapping[table]));

                    foreach (var item in Tables[0].GetChildPropertyValues(Data, table.TableName))
                    {
                        childItems.Add(item);
                    }

                    Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {TypeTableMapping[table].ToString()}");

                    foreach (dynamic item in childItems)
                    {
                        List<object> values = table.GetPropertyValues(item);
                        dataTable.AddRow(columns, values);

                        values.ForEach(value => Logger.Info(value?.ToString() ?? "NULL"));
                        Logger.Info($"Current table row count: {dataTable.Rows.Count}");
                    }
                }
            }

            return dataTable;
        }


        #endregion  
    }
}
