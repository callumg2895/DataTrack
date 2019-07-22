﻿using DataTrack.Core.Components.Builders;
using DataTrack.Core.Components.Execution;
using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Query
{
	public class EntityBeanQuery<TBase> : Query where TBase : IEntityBean
	{
		public EntityBeanQuery()
			: base(typeof(TBase), new EntityBeanMapping<TBase>())
		{
			ValidateMapping(Mapping);
		}

		public override dynamic Execute()
		{
			using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
			{
				SqlCommand command = connection.CreateCommand();

				return Execute(command, connection, null);
			}
		}

		internal override dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction? transaction = null)
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

			command.CommandText = ToString();

			List<TBase> results = new List<TBase>();
			EntityBeanMapping<TBase> mapping = GetMapping();

			stopwatch.Start();

			using (SqlDataReader reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					IEntityBean entityBean = (IEntityBean)Activator.CreateInstance(baseType);

					foreach (PropertyInfo property in baseType.GetProperties())
					{
						string columnName = mapping.PropertyMapping[property.Name].Name;
						property.SetValue(entityBean, Convert.ChangeType(reader[columnName], property.PropertyType));
					}

					results.Add((TBase)entityBean);
				}
			}

			stopwatch.Stop();

			Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Read statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {results.Count} result{(results.Count > 1 ? "s" : "")} retrieved");

			return (List<TBase>)results;
		}

		public override string ToString()
		{
			EntityBeanSQLBuilder<TBase> sqlBuilder = new EntityBeanSQLBuilder<TBase>(GetMapping());

			sqlBuilder.BuildSelectStatement();

			return sqlBuilder.ToString();
		}

		internal EntityBeanMapping<TBase> GetMapping()
		{
			return Mapping as EntityBeanMapping<TBase> ?? throw new NullReferenceException();
		}
	}
}
