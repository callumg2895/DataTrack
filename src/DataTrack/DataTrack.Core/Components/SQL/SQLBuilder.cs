using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Interface;
using DataTrack.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataTrack.Core.Components.SQL
{
	internal class SQLBuilder<TBase> where TBase : IEntity
	{
		#region Members

		private readonly Type _baseType;
		private readonly EntityMapping<TBase> _mapping;
		private readonly StringBuilder _sql;

		#endregion

		#region Constructors

		internal SQLBuilder(EntityMapping<TBase> mapping)
		{
			_baseType = typeof(TBase);
			_mapping = mapping;
			_sql = new StringBuilder();
		}

		#endregion

		#region Methods

		public void CreateStagingTable(EntityTable table)
		{
			_sql.AppendLine($"create table {table.StagingTable.Name}");
			_sql.AppendLine("(");

			for (int i = 0; i < table.Columns.Count; i++)
			{
				Column column = table.Columns[i];
				SqlDbType sqlDbType = column.GetSqlDbType();

				if (column.IsPrimaryKey())
				{
					continue;
				}
				else
				{
					_sql.Append($"{column.Name} {sqlDbType.ToSqlString()} not null");
				}

				_sql.AppendLine(i == table.Columns.Count - 1 ? "" : ",");
			}

			_sql.AppendLine(")")
				.AppendLine();
		}

		public void BuildInsertFromStagingToMainWithOutputIds(EntityTable table)
		{
			List<Column> columns = table.Columns;
			Column primarKeyColumn = table.GetPrimaryKeyColumn();

			if (columns.Count == 0)
			{
				return;
			}

			bool isFirstElement = true;

			_sql.AppendLine($"create table #insertedIds ({primarKeyColumn.Name} {primarKeyColumn.GetSqlDbType().ToSqlString()});")
				.AppendLine()
				.Append("insert into " + table.Name + " (");

			for (int i = 0; i < columns.Count; i++)
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

			_sql.AppendLine(new SelectStatement(columns.Where(c => !c.IsPrimaryKey()).ToList()).From(table.StagingTable).ToString())
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

		public void Append(string text)
		{
			_sql.Append(text);
		}

		public void AppendLine()
		{
			_sql.AppendLine();
		}

		public void AppendLine(string text)
		{
			_sql.AppendLine(text);
		}

		public override string ToString()
		{
			return _sql.ToString();
		}

		public void SelectRowCount()
		{
			_sql.AppendLine("select @@rowcount as affected_rows");
		}

		#endregion
	}
}
