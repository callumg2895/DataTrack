using DataTrack.Core.Components.Query;
using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataTrack.Core.Interface
{
	public interface IQuery
	{
		IQuery AddRestriction(string property, RestrictionTypes type, object value);

		dynamic Execute();

		dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction? transaction = null);

		List<Parameter> GetParameters();
	}
}
