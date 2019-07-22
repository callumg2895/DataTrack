using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Components.SQL;
using DataTrack.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.Components.Builders
{
	internal class EntityBeanSQLBuilder<TBase> : SQLBuilder where TBase : IEntityBean
	{
		internal EntityBeanSQLBuilder(EntityBeanMapping<TBase> mapping)
			: base(typeof(TBase), mapping)
		{

		}

		public void BuildSelectStatement()
		{
			EntityBeanMapping<TBase> mapping = GetMapping();

			foreach (EntityTable table in _mapping.Tables)
			{
				List<Column> columns = table.Columns;
				List<Column> foreignKeyColumns = table.GetForeignKeyColumns();

				if (table.Type != _baseType)
				{
					foreach (Column column in foreignKeyColumns)
					{
						EntityTable foreignTable = _mapping.Tables.Where(t => t.Name == column.ForeignKeyTableMapping).First();
						Column foreignColumn = foreignTable.GetPrimaryKeyColumn();

						column.Restrictions.Add(new Restriction(column, $"select {foreignColumn.Name} from {foreignTable.StagingTable.Name}", Enums.RestrictionTypes.In));
					}
				}
			}

			_sql.AppendLine();
			_sql.AppendLine(new SelectStatement(mapping.Tables, mapping.Columns).ToString());
		}

		public EntityBeanMapping<TBase> GetMapping()
		{
			return _mapping as EntityBeanMapping<TBase>;
		} 
	}
}
