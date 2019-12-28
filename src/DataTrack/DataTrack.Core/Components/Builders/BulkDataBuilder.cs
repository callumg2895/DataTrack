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

			table.Entities.Add(item);
			table.AddDataRow(item);

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
		#endregion
	}
}
