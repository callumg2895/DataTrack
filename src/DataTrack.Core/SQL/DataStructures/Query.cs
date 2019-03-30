using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.ExecutionObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Logging;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Query<TBase> where TBase : Entity, new()
    {
        #region Members

        private Type baseType;
        private Stopwatch stopwatch;

        internal Mapping<TBase> Mapping { get; set; }
        public CRUDOperationTypes OperationType { get; set; }
        public string QueryString { get; set; }

        #endregion

        #region Constructors

        public Query(CRUDOperationTypes operationType)
        {
            OperationType = operationType;

            Mapping = new Mapping<TBase>();
            QueryString = string.Empty;
            baseType = typeof(TBase);
            stopwatch = new Stopwatch();

            // Check for valid Table/Columns
            if (Mapping.Tables.Count == 0 || Mapping.Tables.Any(t => t.Columns.Count == 0))
            {
                string message = $"Mapping data for class '{baseType.Name}' was incomplete/empty";
                Logger.Error(MethodBase.GetCurrentMethod(), message);
                throw new Exception(message);
            }
        }

        #endregion

        #region Methods

        public List<Parameter> GetParameters()
        {
            List<Parameter> parameters = new List<Parameter>();

            var tableColumns = new List<Column>();

            foreach (var columns in Mapping.Tables.Select(t => t.Columns))
            {
                tableColumns.AddRange(columns);
            }

            foreach (Column column in tableColumns)
                if (Mapping.Parameters.ContainsKey(column))
                    parameters.AddRange(Mapping.Parameters[column]);

            return parameters;
        }

        internal void UpdateParameters(TBase item)
        {
            foreach(Table table in Mapping.Tables)
            {
                foreach (Column column in table.Columns)
                {
                    // For each column in the Query, find the value of the property which is decorated by that column attribute
                    // Then update the dictionary of parameters with this value.
                    if (column.TryGetPropertyName(baseType, out string? propertyName))
                    {
                        object propertyValue = item.GetPropertyValue(propertyName);

                        if (propertyValue == null || (column.IsPrimaryKey() && (int)propertyValue == 0))
                            continue;

                        AddParameter(column, new Parameter(column, propertyValue));
                    }
                }
            }
        }

        public void AddParameter(Column column, Parameter parameter)
        {
            if (Mapping.Parameters.ContainsKey(column))
                Mapping.Parameters[column].Add(parameter);
            else
                Mapping.Parameters[column] = new List<Parameter>() { parameter };
        }

        public Query<TBase> AddRestriction(string property, RestrictionTypes type, object value)
        {
            Column column = Mapping.TypeTableMapping[baseType].Columns.Single(x => x.Name == property);
            Parameter parameter = new Parameter(column, value);

            // Store the SQL for the restriction clause against the column attribute for the 
            // property, then store the value of the parameter against its handle if no error occurs.
            Mapping.Restrictions[column] = new Restriction(column, parameter, type);
            AddParameter(column, parameter);

            return this;
        }

        public dynamic Execute()
        {
            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                SqlCommand command = connection.CreateCommand();

                return Execute(command, connection, null);
            }
        }

        internal dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction? transaction = null)
        {
            if (transaction != null)
                command.Transaction = transaction;

            command.CommandType = CommandType.Text;
            command.CommandText = QueryString;
            command.AddParameters(GetParameters());

            if (OperationType == CRUDOperationTypes.Create)
            {
                return new InsertQueryExecutor<TBase>(this, connection, transaction).Execute();
            }

            using (SqlDataReader reader = command.ExecuteReader())
            {
                switch (OperationType)
                {
                    case CRUDOperationTypes.Read: return new ReadQueryExecutor<TBase>(this, connection, transaction).Execute(reader);
                    case CRUDOperationTypes.Update: return new UpdateQueryExecutor<TBase>(this, connection, transaction).Execute(reader);
                    case CRUDOperationTypes.Delete: return new DeleteQueryExecutor<TBase>(this, connection, transaction).Execute(reader);
                    default:
                        stopwatch.Stop();
                        Logger.Error(MethodBase.GetCurrentMethod(), "No valid operation to perform.");
                        throw new ArgumentException("No valid operation to perform.", nameof(OperationType));
                }
            }
        }

        #endregion
    }
}
