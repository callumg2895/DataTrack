﻿using DataTrack.Core.Components.Builders;
using DataTrack.Core.Components.Execution;
using DataTrack.Core.Components.Data;
using DataTrack.Core.Components.SQL;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DataTrack.Core.Components.Query
{
	public class EntityQuery<TBase> : Query where TBase : IEntity
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		#region Constructors

		public EntityQuery() 
			: base(typeof(TBase), new EntityMapping<TBase>())
		{
			ValidateMapping(Mapping);
		}

		public EntityQuery<TBase> Create(TBase item)
		{
			OperationType = CRUDOperationTypes.Create;

			EntityTable table = Mapping.TypeTableMapping[baseType];

			table.StageForInsertion(item as IEntity);

			return this;
		}

		public EntityQuery<TBase> Create(List<TBase> items)
		{
			OperationType = CRUDOperationTypes.Create;

			EntityTable table = Mapping.TypeTableMapping[baseType];

			foreach (IEntity item in items)
			{
				table.StageForInsertion(item as IEntity);
			}

			return this;
		}

		public EntityQuery<TBase> Read(int? id = null)
		{
			OperationType = CRUDOperationTypes.Read;

			if (id.HasValue)
			{
				AddRestriction("ID", RestrictionTypes.EqualTo, id.Value);
			}

			return this;
		}

		public EntityQuery<TBase> Update(TBase item)
		{
			OperationType = CRUDOperationTypes.Update;
			UpdateParameters(item);
			AddPrimaryKeyRestriction(item);
			return this;
		}

		public EntityQuery<TBase> Delete()
		{
			OperationType = CRUDOperationTypes.Delete;
			return this;
		}

		public EntityQuery<TBase> Delete(TBase item)
		{
			OperationType = CRUDOperationTypes.Delete;
			AddPrimaryKeyDeleteRestriction(item);
			return this;
		}


		#endregion

		#region Methods

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

			foreach (EntityTable childTable in table.ChildTables)
			{
				foreach (dynamic childItem in item.GetChildPropertyValues(childTable.Name))
				{
					UpdateParameters(childItem);
				}
			}
		}

		public override IQuery AddRestriction(string property, RestrictionTypes type, object value)
		{
			Column column = Mapping.TypeTableMapping[baseType].Columns.Single(x => x.PropertyName == property);
			column.AddRestriction(type, value);

			return this;
		}

		private void AddPrimaryKeyRestriction(TBase item)
		{
			Column primaryKeyColumn = Mapping.TypeTableMapping[baseType].GetPrimaryKeyColumn();
			string primaryKeyColumnPropertyName = primaryKeyColumn.PropertyName;
			object primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyName);

			AddRestriction(primaryKeyColumn.PropertyName, RestrictionTypes.EqualTo, primaryKeyValue);
		}

		private void AddPrimaryKeyDeleteRestriction(TBase item)
		{
			Column primaryKeyColumn = Mapping.TypeTableMapping[baseType].GetPrimaryKeyColumn();
			string primaryKeyColumnPropertyName = primaryKeyColumn.PropertyName;
			object primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyName);

			AddRestriction(primaryKeyColumn.PropertyName, RestrictionTypes.In, primaryKeyValue);
		}

		public override dynamic Execute()
		{
			using (SqlConnection connection = DataTrackConfiguration.Instance.CreateConnection())
			{
				SqlCommand command = connection.CreateCommand();

				return Execute(command, connection, null);
			}
		}

		public override dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction? transaction = null)
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

			try
			{
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
			catch (SqlException ex)
			{
				Logger.ErrorFatal($"DataTrack encountered a SQL exception - {ex.Message}");
				throw ex;
			}
			catch (Exception ex)
			{
				Logger.ErrorFatal($"DataTrack encountered an exception - {ex.Message}");
				throw ex;
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
			EntitySQLBuilder<TBase> sqlBuilder = new EntitySQLBuilder<TBase>(GetMapping());

			sqlBuilder.BuildSelectStatement();

			string sql = sqlBuilder.ToString();

			Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

			return sql;
		}

		private string GetUpdateString()
		{
			EntitySQLBuilder<TBase> sqlBuilder = new EntitySQLBuilder<TBase>(GetMapping());

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
			EntitySQLBuilder<TBase> sqlBuilder = new EntitySQLBuilder<TBase>(GetMapping());

			sqlBuilder.BuildDeleteStatement();
			sqlBuilder.SelectRowCount();

			string sql = sqlBuilder.ToString();

			Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

			return sql;
		}

		internal EntityMapping<TBase> GetMapping()
		{
			return Mapping as EntityMapping<TBase> ?? throw new NullReferenceException();
		}

		#endregion
	}
}
