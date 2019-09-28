using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTrack.Core.Components.Builders
{
	internal class BulkDataBuilder<TBase> where TBase : IEntity
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		#region Members

		internal List<EntityTable> Tables { get; private set; }
		internal EntityMapping<TBase> Mapping { get; private set; }

		private readonly Map<EntityColumn, DataColumn> ColumnMap = new Map<EntityColumn, DataColumn>();
		private readonly Type BaseType = typeof(TBase);
		#endregion

		#region Constructors

		internal BulkDataBuilder(EntityMapping<TBase> mapping)
		{
			Tables = mapping.Tables;
			Mapping = mapping;
		}

		#endregion Constructors

		#region Methods

		internal void BuildDataFor(List<TBase> items)
		{
			foreach (IEntity item in items)
			{
				BuildDataForEntity(item);
			}
		}

		internal void BuildDataFor(TBase item)
		{
			BuildDataForEntity(item);
		}

		private void BuildDataForEntity(IEntity item)
		{	
			if (item == null)
			{
				return;
			}

			Logger.Trace($"Building DataTable for: {item.GetType().ToString()}");

			Type type = item.GetType();
			EntityTable table = Mapping.TypeTableMapping[type];

			Mapping.UpdateTableEntities(table, item);
			Mapping.UpdateTableDataTable(table);

			DataTable dataTable = Mapping.DataTableMapping[table];

			SetColumns(dataTable);
			AddRow(dataTable, item);

			foreach (EntityTable childTable in Mapping.ParentChildMapping[table])
			{
				dynamic childItems = item.GetChildPropertyValues(childTable.Name);

				if (childItems == null)
				{
					continue;
				}

				if (!Mapping.ParentChildEntityMapping.ContainsKey(item))
				{
					Mapping.ParentChildEntityMapping[item] = new List<IEntity>();
				}

				foreach (dynamic childItem in childItems)
				{
					BuildDataForEntity(childItem);
					Mapping.ParentChildEntityMapping[item].Add(childItem);
				}
			}

		}

		private void SetColumns(DataTable dataTable)
		{
			/*
			 * Bulk inserts can only be performed on data that is mapped to a physics column in the database. These are
			 * represented by instances of the EntityColumn class, and contain specific methods which determine key type
			 * etc.
			 */

			EntityTable table = Mapping.DataTableMapping[dataTable];
			List <EntityColumn> columns = table.EntityColumns;

			foreach (EntityColumn column in columns)
			{
				if (ColumnMap.ContainsKey(column))
				{
					return;
				}

				DataColumn dataColumn = new DataColumn(column.Name);
				List<DataColumn> primaryKeys = new List<DataColumn>();

				if (!column.IsPrimaryKey())
				{
					dataTable.Columns.Add(dataColumn);
					ColumnMap[column] = dataColumn;
				}

				if (column.IsPrimaryKey())
				{
					primaryKeys.Add(dataColumn);
				}
			}
		}

		private void AddRow(DataTable dataTable, IEntity item)
		{
			List<object> rowData = item.GetPropertyValues();

			EntityTable table = Mapping.DataTableMapping[dataTable];
			DataRow dataRow = dataTable.NewRow();

			for (int i = 0; i < rowData.Count; i++)
			{
				EntityColumn column = table.EntityColumns[i];
				if (!column.IsPrimaryKey())
				{
					dataRow[column.Name] = rowData[i];
				}
			}

			dataTable.Rows.Add(dataRow);
			Mapping.EntityDataRowMapping.Add(item, dataRow);
		}
		#endregion
	}
}
