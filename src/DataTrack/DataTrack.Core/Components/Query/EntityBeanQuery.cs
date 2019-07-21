using DataTrack.Core.Components.Execution;
using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Logging;
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

			return new List<TBase>();
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}
}
