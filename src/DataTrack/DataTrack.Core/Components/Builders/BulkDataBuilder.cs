﻿using DataTrack.Core.Components.Mapping;
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

		#region Members

		internal List<TBase> Data { get; private set; }
		internal List<EntityTable> Tables { get; private set; }
		internal EntityMapping<TBase> Mapping { get; private set; }

		private readonly Map<EntityTable, DataTable> DataMap = new Map<EntityTable, DataTable>();
		private readonly Map<Column, DataColumn> ColumnMap = new Map<Column, DataColumn>();
		private readonly Type BaseType = typeof(TBase);
		#endregion

		#region Constructors

		internal BulkDataBuilder(TBase data, EntityMapping<TBase> mapping)
			: this(new List<TBase>() { data }, mapping)
		{

		}

		internal BulkDataBuilder(List<TBase> data, EntityMapping<TBase> mapping)
		{
			Data = data;
			Tables = mapping.Tables;
			Mapping = mapping;
		}

		#endregion Constructors

		#region Methods

		internal Map<EntityTable, DataTable> YieldDataMap()
		{
			foreach (TBase item in Data)
			{
				BuildDataFor(item);
			}

			return DataMap;
		}

		private void BuildDataFor(IEntity item)
		{
			if (item == null)
			{
				return;
			}

			Type type = item.GetType();
			EntityTable table = Mapping.TypeTableMapping[type];

			if (!DataMap.ContainsKey(table))
			{
				DataMap[table] = new DataTable(table.Name);
				Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {Data?.GetType().ToString()}");
			}

			DataTable dataTable = DataMap[table];

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
					BuildDataFor(childItem);
					Mapping.ParentChildEntityMapping[item].Add(childItem);
				}
			}

		}

		private void SetColumns(DataTable dataTable)
		{
			List<Column> columns = DataMap[dataTable].Columns;

			foreach (Column column in columns)
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

			EntityTable table = DataMap[dataTable];
			DataRow dataRow = dataTable.NewRow();

			for (int i = 0; i < rowData.Count; i++)
			{
				Column column = table.Columns[i];
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