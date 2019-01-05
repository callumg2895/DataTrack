﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.ExecutionObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Query<TBase> where TBase : new()
    {
        #region Members

        private Type baseType;
        private Stopwatch stopwatch;

        internal Mapping<TBase> Mapping { get; set; }
        public CRUDOperationTypes OperationType { get; set; }
        public string QueryString { get; set; }

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

            foreach (ColumnMappingAttribute column in Mapping.Columns)
                if (Mapping.Parameters.ContainsKey(column))
                    parameters.AddRange(Mapping.Parameters[column]);

            return parameters;
        }

        public void AddParameter(ColumnMappingAttribute column, (string Handle, object Value) parameter)
        {
            if (Mapping.Parameters.ContainsKey(column))
                Mapping.Parameters[column].Add(parameter);
            else
                Mapping.Parameters[column] = new List<(string Handle, object Value)>() { parameter };
        }

        public dynamic Execute()
        {
            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                SqlCommand command = connection.CreateCommand();

                return Execute(command, connection, null);
            }
        }

        internal dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction transaction = null)
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
                        return null;
                }
            }
        }

        #endregion
    }
}