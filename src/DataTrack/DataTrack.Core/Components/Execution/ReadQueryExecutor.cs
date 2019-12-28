using DataTrack.Core.Components.Data;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Extensions;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTrack.Core.Components.Execution
{
	public class ReadQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : IEntity
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		private readonly List<TBase> results;
		private readonly List<EntityTable> tables;
		private readonly IDictionary<Table, Dictionary<object, IEntity>> entityPrimaryKeyDictionary;

		internal ReadQueryExecutor(EntityQuery<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
			: base(query, connection, transaction)
		{
			results = new List<TBase>();
			tables = mapping.Tables;
			entityPrimaryKeyDictionary = new Dictionary<Table, Dictionary<object, IEntity>>();
		}

		internal List<TBase> Execute(SqlDataReader reader)
		{
			stopwatch.Start();

			foreach (EntityTable table in tables)
			{
				ReadResultsForTable(reader, table);

				reader.NextResult();
			}

			stopwatch.Stop();

			Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Read statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {results.Count} result{(results.Count > 1 ? "s" : "")} retrieved");

			return results;
		}

		private void ReadResultsForTable(SqlDataReader reader, EntityTable table)
		{
			while (reader.Read())
			{
				IEntity entity = ReadEntity(reader, table);

				MapEntity(entity, table);
				AddResult(entity, table);
			}
		}

		private void MapEntity(IEntity entity, EntityTable table)
		{
			if (!entityPrimaryKeyDictionary.ContainsKey(table))
			{
				entityPrimaryKeyDictionary.Add(table, new Dictionary<object, IEntity>());
			}

			entityPrimaryKeyDictionary[table].Add(entity.GetID(), entity);
		}

		private void AddResult(IEntity entity, EntityTable table)
		{
			if (table.ParentTable != null)
			{
				AssociateWithParent(entity, table);
			}
			else
			{
				results.Add((TBase)entity);
			}
		}

		private void AssociateWithParent(IEntity entity, EntityTable table)
		{
			EntityTable parentTable = table.ParentTable;
			EntityColumn foreignKeyColumn = table.GetForeignKeyColumnFor(parentTable);
			object foreignKey = entity.GetPropertyValue(foreignKeyColumn.PropertyName);
			IEntity parentEntity = entityPrimaryKeyDictionary[parentTable][foreignKey];

			parentEntity.AddChildPropertyValue(table.Name, entity);		
		}

		private IEntity ReadEntity(SqlDataReader reader, EntityTable table)
		{
			Type type = table.Type;
			IEntity entity = (IEntity)table.EntityActivator();

			foreach (Column column in table.Columns)
			{
				PropertyInfo property = type.GetProperty(column.PropertyName);

				property.SetValue(entity, Convert.ChangeType(reader[column.Name], property.PropertyType));
			}

			entity.InstantiateChildProperties();

			return entity;
		}
	}
}
