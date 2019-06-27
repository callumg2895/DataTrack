﻿using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.BuilderObjects;
using DataTrack.Core.SQL.ExecutionObjects;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DataTrack.Core.SQL.DataStructures
{
	public class Query<TBase> where TBase : IEntity
	{
		#region Members

		private readonly Type baseType;
		private readonly Stopwatch stopwatch;

		internal Mapping<TBase> Mapping { get; set; }
		public CRUDOperationTypes OperationType { get; set; }
		public string QueryString { get; set; }

		#endregion

		#region Constructors

		public Query()
		{
			OperationType = CRUDOperationTypes.Read;

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
		public Query<TBase> Create(TBase item)
		{
			OperationType = CRUDOperationTypes.Create;
			Mapping.DataTableMapping = new BulkDataBuilder<TBase>(item, Mapping).YieldDataMap();
			return this;
		}

		public Query<TBase> Create(List<TBase> items)
		{
			OperationType = CRUDOperationTypes.Create;
			Mapping.DataTableMapping = new BulkDataBuilder<TBase>(items, Mapping).YieldDataMap();
			return this;
		}

		public Query<TBase> Read(int? id = null)
		{
			OperationType = CRUDOperationTypes.Read;

			if (id.HasValue)
			{
				AddRestriction("id", RestrictionTypes.EqualTo, id.Value);
			}

			return this;
		}

		public Query<TBase> Update(TBase item)
		{
			OperationType = CRUDOperationTypes.Update;
			UpdateParameters(item);
			AddPrimaryKeyRestriction(item);
			return this;
		}

		public Query<TBase> Delete()
		{
			OperationType = CRUDOperationTypes.Delete;
			return this;
		}

		public Query<TBase> Delete(TBase item)
		{
			OperationType = CRUDOperationTypes.Delete;
			AddPrimaryKeyDeleteRestriction(item);
			return this;
		}


		#endregion

		#region Methods

		public List<Parameter> GetParameters()
		{
			List<Parameter> parameters = new List<Parameter>();

			foreach (EntityTable table in Mapping.Tables)
			{
				foreach (Column column in table.Columns)
				{
					parameters.AddRange(column.Parameters);
				}
			}

			return parameters;
		}

		internal void UpdateParameters(IEntity item)
		{
			Type type = item.GetType();
			EntityTable table = Mapping.TypeTableMapping[type];

			foreach (Column column in table.Columns)
			{
				string propertyName = column.PropertyName;
				object propertyValue = item.GetPropertyValue(propertyName);

				if (propertyValue == null || (column.IsPrimaryKey() && propertyValue == default))
				{
					continue;
				}

				column.AddParameter(propertyValue);
			}

			foreach (EntityTable childTable in Mapping.ParentChildMapping[table])
			{
				foreach (dynamic childItem in item.GetChildPropertyValues(childTable.Name))
				{
					UpdateParameters(childItem);
				}
			}
		}

		public Query<TBase> AddRestriction(string property, RestrictionTypes type, object value)
		{
			Column column = Mapping.TypeTableMapping[baseType].Columns.Single(x => x.Name == property);
			column.AddRestriction(type, value);

			return this;
		}

		private void AddPrimaryKeyRestriction(TBase item)
		{
			Column primaryKeyColumn = Mapping.TypeTableMapping[baseType].GetPrimaryKeyColumn();
			string primaryKeyColumnPropertyName = primaryKeyColumn.PropertyName;
			object primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyName);

			AddRestriction(primaryKeyColumn.Name, RestrictionTypes.EqualTo, primaryKeyValue);
		}

		private void AddPrimaryKeyDeleteRestriction(TBase item)
		{
			Column primaryKeyColumn = Mapping.TypeTableMapping[baseType].GetPrimaryKeyColumn();
			string primaryKeyColumnPropertyName = primaryKeyColumn.PropertyName;
			object primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyName);

			AddRestriction(primaryKeyColumn.Name, RestrictionTypes.In, primaryKeyValue);
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
			{
				command.Transaction = transaction;
			}

			command.CommandType = CommandType.Text;

			foreach (Parameter parameter in GetParameters())
			{
				command.Parameters.Add(parameter.ToSqlParameter());
			}

			if (OperationType == CRUDOperationTypes.Create)
			{
				return new InsertQueryExecutor<TBase>(this, connection, transaction).Execute();
			}

			command.CommandText = ToString();

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

		public override string ToString()
		{
			switch (OperationType)
			{
				case CRUDOperationTypes.Read: return GetReadString();
				case CRUDOperationTypes.Update: return GetUpdateString();
				case CRUDOperationTypes.Delete: return GetDeleteString();
				default:
					stopwatch.Stop();
					Logger.Error(MethodBase.GetCurrentMethod(), "No valid operation to perform.");
					throw new ArgumentException("No valid operation to perform.", nameof(OperationType));
			}
		}

		private string GetReadString()
		{
			SQLBuilder<TBase> sqlBuilder = new SQLBuilder<TBase>(Mapping);

			sqlBuilder.BuildSelectStatement();

			string sql = sqlBuilder.ToString();

			Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

			return sql;
		}

		private string GetUpdateString()
		{
			SQLBuilder<TBase> sqlBuilder = new SQLBuilder<TBase>(Mapping);

			sqlBuilder.AppendLine();
			sqlBuilder.BuildUpdateStatement();

			// For update statements return the number of rows affected
			sqlBuilder.SelectRowCount();

			string sql = sqlBuilder.ToString();

			Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

			return sql;
		}

		private string GetDeleteString()
		{
			SQLBuilder<TBase> sqlBuilder = new SQLBuilder<TBase>(Mapping);

			sqlBuilder.BuildDeleteStatement();
			sqlBuilder.SelectRowCount();

			string sql = sqlBuilder.ToString();

			Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

			return sql;
		}

		#endregion
	}
}