using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataTrack.Core.SQL.QueryObjects
{
    public class Query<TBase> where TBase : new()
    {
        #region Members

        private Type baseType;
        private Stopwatch stopwatch;

        public List<TableMappingAttribute> Tables { get; set; } = new List<TableMappingAttribute>();
        public List<ColumnMappingAttribute> Columns { get; set; } = new List<ColumnMappingAttribute>();
        internal Dictionary<TableMappingAttribute, string> TableAliases { get; set; }  = new Dictionary<TableMappingAttribute, string>();
        internal Dictionary<ColumnMappingAttribute, string> ColumnAliases { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        public Map<Type, TableMappingAttribute> TypeTableMapping { get; set; } = new Map<Type, TableMappingAttribute>();
        public Map<Type, List<ColumnMappingAttribute>> TypeColumnMapping { get; set; } = new Map<Type, List<ColumnMappingAttribute>>();
        public Dictionary<ColumnMappingAttribute, string> ColumnPropertyNames { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        public Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> Parameters { get; set; } = new Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>>();
        public CRUDOperationTypes OperationType { get; set; }
        public string QueryString { get; set; }
        public Map<TableMappingAttribute, DataTable> DataMap { get; set; } = new Map<TableMappingAttribute, DataTable>();

        #endregion

        #region Constructors

        public Query()
        {
            baseType = typeof(TBase);
            stopwatch = new Stopwatch();
        }

        #endregion

        #region Methods

        public List<(string Handle, object Value)> GetParameters()
        {
            List<(string Handle, object Value)> parameters = new List<(string Handle, object Value)>();

            foreach (ColumnMappingAttribute column in Columns)
                if (Parameters.ContainsKey(column))
                    parameters.AddRange(Parameters[column]);

            return parameters;
        }

        public void AddParameter(ColumnMappingAttribute column, (string Handle, object Value) parameter)
        {
            if (Parameters.ContainsKey(column))
                Parameters[column].Add(parameter);
            else
                Parameters[column] = new List<(string Handle, object Value)>() { parameter };
        }

        public dynamic Execute()
        {
            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                SqlCommand command = connection.CreateCommand();

                return Execute(command, connection, null);
            }
        }

        internal bool ExecuteBulkInsert(SqlConnection connection, SqlTransaction transaction = null)
        {
            stopwatch.Start();

            foreach (TableMappingAttribute table in DataMap.ForwardKeys)
            {
                Logger.Info($"Executing Bulk Insert for {table.TableName}");

                SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default;
                SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, copyOptions, transaction);

                bulkCopy.DestinationTableName = DataMap[table].TableName;
                bulkCopy.WriteToServer(DataMap[table]);
            }

            stopwatch.Stop();
            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Bulk Insert ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");


            return true;
        }

        internal dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction transaction = null)
        {
            stopwatch.Start();

            if (transaction != null)
                command.Transaction = transaction;

            command.CommandType = CommandType.Text;
            command.CommandText = QueryString;
            command.AddParameters(GetParameters());

            if (OperationType == CRUDOperationTypes.Create)
            {
                return ExecuteBulkInsert(connection, transaction);
            }

            using (SqlDataReader reader = command.ExecuteReader())
            {
                switch (OperationType)
                {
                    case CRUDOperationTypes.Read: return GetResultsForReadQuery(reader);
                    case CRUDOperationTypes.Update: return GetResultsForUpdateQuery(reader);
                    case CRUDOperationTypes.Delete: return GetResultsForDeleteQuery(reader);
                    default:
                        stopwatch.Stop();
                        Logger.Error(MethodBase.GetCurrentMethod(), "No valid operation to perform.");
                        return null;
                }
            }
        }

        private int GetResultForInsertQuery(SqlDataReader reader)
        {
            int affectedRows = 0;

            // Create operations always check the number of rows affected after the query has executed
            for (int tableCount = 0; tableCount < Tables.Count; tableCount++)
            {
                if (tableCount == 0)
                {
                    if (reader.Read())
                        affectedRows += (int)reader["affected_rows"];

                    reader.NextResult();
                }
                else
                {

                    TableMappingAttribute table = Tables[tableCount];
                    int childObjects = 0;

                    // TODO: needs improving
                    // The idea here is that the number of parameters associated with each column of a table is equal to the number of total child objects
                    foreach (var key in Parameters.Keys)
                    {
                        if (key.TableName == table.TableName)
                            childObjects = Parameters[key].Count;
                    }

                    for (int i = 0; i < childObjects; i++)
                    {
                        if (reader.Read())
                            affectedRows += (int)reader["affected_rows"];

                        reader.NextResult();
                    }
                }
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Insert statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {affectedRows} row{(affectedRows > 1 ? "s" : "")} affected");
            
            return affectedRows;
        }

        private List<TBase> GetResultsForReadQuery(SqlDataReader reader)
        {
            List<TBase> results = new List<TBase>();

            List<ColumnMappingAttribute> mainColumns = TypeColumnMapping[baseType];

            int columnCount = 0;

            while (reader.Read())
            {
                TBase obj = new TBase();

                foreach (ColumnMappingAttribute column in mainColumns)
                {
                    if (ColumnPropertyNames.ContainsKey(column))
                    {
                        PropertyInfo property = baseType.GetProperty(ColumnPropertyNames[column]);

                        if (reader[column.ColumnName] != DBNull.Value)
                            property.SetValue(obj, Convert.ChangeType(reader[column.ColumnName], property.PropertyType));
                        else
                            property.SetValue(obj, null);
                    }
                    else
                    {
                        Logger.Error(MethodBase.GetCurrentMethod(), $"Could not find property in class {typeof(TBase)} mapped to column {column.ColumnName}");
                        break;
                    }

                    columnCount++;
                }

                results.Add(obj);
            }

            for (int tableCount = 1; tableCount < Tables.Count; tableCount++)
            {
                reader.NextResult();
                Type childType = TypeTableMapping[Tables[tableCount]];
                dynamic childCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));
                int originalColumnCount = columnCount;

                while (reader.Read())
                {
                    var childItem = Activator.CreateInstance(childType);
                    columnCount = originalColumnCount;

                    childType.GetProperties()
                             .ForEach(prop => prop.SetValue(childItem, Convert.ChangeType(reader[Columns[columnCount++].ColumnName], prop.PropertyType)));

                    MethodInfo addItem = childCollection.GetType().GetMethod("Add");
                    addItem.Invoke(childCollection, new object[] { childItem });
                }

                foreach (TBase obj in results)
                {
                    PropertyInfo childProperty = Tables[0].GetChildProperty(baseType, Tables[tableCount].TableName);
                    childProperty.SetValue(obj, childCollection);
                }
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Read statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {results.Count} result{(results.Count > 1 ? "s" : "")} retrieved");

            return results;
        }

        private int GetResultsForUpdateQuery(SqlDataReader reader)
        { 
            // Update operations always check the number of rows affected after the query has executed
            int affectedRows = reader.Read() ? (int)reader["affected_rows"] : 0;

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Update statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {affectedRows} row{(affectedRows > 1 ? "s" : "")} affected");

            return affectedRows;
        }

        private int GetResultsForDeleteQuery(SqlDataReader reader)
        {
            // Update operations always check the number of rows affected after the query has executed
            int affectedRows = reader.Read() ? (int)reader["affected_rows"] : 0;

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Delete statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {affectedRows} row{(affectedRows > 1 ? "s" : "")} affected");

            return affectedRows;
        }

        #endregion
    }
}
