using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataTrack.Core.Configuration
{
	internal class DatabaseConfiguration
	{
		internal string DataSource { get; set; }
		internal string InitalCatalog { get; set; }
		internal string UserID { get; set; }
		internal string Password { get; set; }

		internal string GetConnectionString()
		{
			return new SqlConnectionStringBuilder()
			{
				DataSource = DataSource,
				InitialCatalog = InitalCatalog,
				UserID = UserID,
				Password = Password
			}.ToString();
		}
	}
}
