using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Components.SQL;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataTrack.Core.Components.Builders
{
	internal class EntitySQLBuilder<TBase> : SQLBuilder where TBase : IEntity
	{

		#region Constructors

		internal EntitySQLBuilder(EntityMapping<TBase> mapping)
			: base (typeof(TBase), mapping)
		{

		}

		#endregion

		#region Methods

		public void CreateStagingTable(EntityTable table)
		{
			_sql.AppendLine($"create table {table.StagingTable.Name}");
			_sql.AppendLine("(");

			for (int i = 0; i < table.EntityColumns.Count; i++)
			{
				EntityColumn column = table.EntityColumns[i];
				SqlDbType sqlDbType = column.GetSqlDbType();

				if (column.IsPrimaryKey())
				{
					continue;
				}
				else
				{
					_sql.Append($"{column.Name} {sqlDbType.ToSqlString()} not null");
				}

				_sql.AppendLine(i == table.EntityColumns.Count - 1 ? "" : ",");
			}

			_sql.AppendLine(")")
				.AppendLine();
		}

		public void BuildInsertFromStagingToMainWithOutputIds(EntityTable table)
		{
			List<Column> columns = table.Columns;
			List<EntityColumn> entityColumns = table.EntityColumns;
			Column primarKeyColumn = table.GetPrimaryKeyColumn();

			if (columns.Count == 0)
			{
				return;
			}

			bool isFirstElement = true;

			_sql.AppendLine($"create table #insertedIds ({primarKeyColumn.Name} {primarKeyColumn.GetSqlDbType().ToSqlString()});")
				.AppendLine()
				.Append("insert into " + table.Name + " (");

			for (int i = 0; i < entityColumns.Count; i++)
			{
				if (!columns[i].IsPrimaryKey())
				{
					_sql.Append((isFirstElement ? "" : ", ") + columns[i].Name);
					isFirstElement = false;
				}
			}

			_sql.AppendLine(")")
				.AppendLine()
				.AppendLine($"output inserted.{primarKeyColumn.Name} into #insertedIds({primarKeyColumn.Name})")
				.AppendLine();

			_sql.AppendLine(new SelectStatement(columns.Where(c => !c.IsPrimaryKey()).ToList()).From(table.StagingTable, ColumnTypes.EntityColumn).ToString())
				.AppendLine()
				.AppendLine("select * from #insertedIds")
				.AppendLine()
				.AppendLine("drop table #insertedIds")
				.AppendLine($"drop table {table.StagingTable.Name}")
				.AppendLine(); ;
		}

		public void BuildUpdateStatement()
		{
			EntityTable table = _mapping.TypeTableMapping[_baseType];
			List<Column> columns = table.Columns.Where(c => !c.IsPrimaryKey()).ToList();

			_sql.AppendLine(new UpdateStatement(columns).ToString());
		}

		public void BuildSelectStatement()
		{
			foreach (EntityTable table in _mapping.Tables)
			{
				List<EntityColumn> foreignKeyColumns = table.GetForeignKeyColumns();

				if (table.Type != _baseType)
				{
					foreach (EntityColumn column in foreignKeyColumns)
					{
						EntityTable foreignTable = _mapping.Tables.Where(t => t.Name == column.ForeignKeyTableMapping).First();
						EntityColumn foreignColumn = foreignTable.GetPrimaryKeyColumn();

						column.Restrictions.Add(new Restriction(column, $"select {foreignColumn.Name} from {foreignTable.StagingTable.Name}", Enums.RestrictionTypes.In));
					}
				}

				_sql.AppendLine();
				_sql.AppendLine(new SelectStatement(table).Into(table.StagingTable).ToString());
				_sql.AppendLine();
				_sql.AppendLine(new SelectStatement(table).From(table.StagingTable).ToString());
			}
		}

		public void BuildDeleteStatement()
		{
			_sql.AppendLine(new DeleteStatement(_mapping.Tables[0]).ToString());
		}

		#endregion
	}
}
