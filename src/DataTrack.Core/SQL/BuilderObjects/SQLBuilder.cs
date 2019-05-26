using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.BuilderObjects
{
	internal class SQLBuilder<TBase> where TBase : IEntity
	{
		#region Members

		private readonly Type _baseType;
		private readonly Mapping<TBase> _mapping;
		private readonly StringBuilder _sql;

		#endregion

		#region Constructors

		internal SQLBuilder(Mapping<TBase> mapping)
		{
			_baseType = typeof(TBase);
			_mapping = mapping;
			_sql = new StringBuilder();
		}

		#endregion

		#region Methods

		public void CreateStagingTable(Table table)
		{
			_sql.AppendLine($"create table {table.StagingName}");
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

		public void BuildInsertFromStagingToMainWithOutputIds(Table table)
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
				.AppendLine()
				.Append("select ");

			isFirstElement = true;
			for (int i = 0; i < columns.Count; i++)
			{
				if (!columns[i].IsPrimaryKey())
				{
					_sql.Append((isFirstElement ? "" : ", ") + columns[i].Name);
					isFirstElement = false;
				}
			}

			_sql.AppendLine()
				.AppendLine($"from {table.StagingName}")
				.AppendLine()
				.AppendLine("select * from #insertedIds")
				.AppendLine()
				.AppendLine("drop table #insertedIds")
				.AppendLine($"drop table {table.StagingName}")
				.AppendLine();
		}

		public void BuildUpdateStatement()
		{
			StringBuilder setBuilder = new StringBuilder();
			StringBuilder restrictionBuilder = new StringBuilder();

			Table table = _mapping.TypeTableMapping[_baseType];
			List<Column> columns = table.Columns.Where(c => !c.IsPrimaryKey()).ToList();

			int processedRestrictions = 0;
			int totalColumns = columns.Count;

			_sql.AppendLine($"update {table.Alias}");
			_sql.Append("set ");

			setBuilder.Append($"{columns[0].Alias} = {columns[0].Parameters[0].Handle}");

			for (int i = 1; i < totalColumns; i++)
			{
				setBuilder.Append($", {columns[i].Alias} = {columns[i].Parameters[0].Handle}");
			}

			restrictionBuilder.AppendLine($" from {table.Name} {table.Alias}");

			foreach (Column column in columns)
			{
				foreach (Restriction restriction in column.Restrictions)
				{
					restrictionBuilder.Append(processedRestrictions++ == 0
						? "where "
						: "and ");
					restrictionBuilder.AppendLine(restriction.ToString());
				}
			}

			_sql.Append(setBuilder.ToString());
			_sql.Append(restrictionBuilder.ToString());
		}

		public void BuildSelectStatement()
		{
			foreach (Table table in _mapping.Tables)
			{
				List<Column> columns = table.Columns;
				List<Column> foreignKeyColumns = table.GetForeignKeyColumns();

				int RestrictionCount = 0;

				_sql.AppendLine()
					.Append($"select {columns[0].Alias}");

				for (int i = 1; i < columns.Count; i++)
				{
					_sql.Append(", ")
						.Append(columns[i].Alias);
				}

				_sql.AppendLine()
					.AppendLine($"into {table.StagingName} from {table.Name} as {table.Alias}");

				if (table.Type != _baseType)
				{
					foreach (Column column in foreignKeyColumns)
					{
						Table foreignTable = _mapping.Tables.Where(t => t.Name == column.ForeignKeyTableMapping).First();
						Column foreignColumn = foreignTable.GetPrimaryKeyColumn();

						_sql.Append($"{GetRestrictionKeyWord(RestrictionCount++)} ")
						   .AppendLine($"{column.Alias} in (select {foreignColumn.Name} from {foreignTable.StagingName})");
					}
				}

				foreach (Column column in columns)
				{
					foreach (Restriction restriction in column.Restrictions)
					{
						_sql.Append($"{GetRestrictionKeyWord(RestrictionCount++)} ")
						   .AppendLine(restriction.ToString());
					}
				}

				_sql.AppendLine();
				_sql.AppendLine($"select * from {table.StagingName}");
			}
		}

		public void BuildDeleteStatement()
		{
			StringBuilder restrictionsBuilder = new StringBuilder();
			int restrictionCount = 0;

			if (_mapping.Tables.Any(t => t.Columns.Any(c => c.Parameters.Count > 0)))
			{
				for (int i = 0; i < _mapping.Tables.Count; i++)
				{
					for (int j = 0; j < _mapping.Tables[i].Columns.Count; j++)
					{
						Column column = _mapping.Tables[i].Columns[j];

						foreach (Restriction restriction in column.Restrictions)
						{
							restrictionsBuilder.Append($"{GetRestrictionKeyWord(restrictionCount++)} ");
							restrictionsBuilder.AppendLine(restriction.ToString());
						}
					}
				}
			}

			_sql.AppendLine();
			_sql.AppendLine($"delete {_mapping.Tables[0].Alias} from {_mapping.Tables[0].Name} {_mapping.Tables[0].Alias}");
			_sql.Append(restrictionsBuilder.ToString());
		}

		private string GetRestrictionKeyWord(int restrictionCount)
		{
			return restrictionCount > 0 ? "and" : "where";
		}

		private string GetPrimaryKeyColumnDefinition(Column column)
		{
            SqlDbType dbType = column.GetSqlDbType();

            switch (dbType)
            {
                case SqlDbType.UniqueIdentifier:
                    return $"{column.Name} {dbType.ToSqlString()} not null default newid(),";
                default:
                    return $"{column.Name} {dbType.ToSqlString()} not null identity(1,1),";
            }
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
