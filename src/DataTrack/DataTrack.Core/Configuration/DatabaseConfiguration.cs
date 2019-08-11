using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

namespace DataTrack.Core.Configuration
{
	internal class DatabaseConfiguration
	{
		internal string DataSource { get; set; }
		internal string InitalCatalog { get; set; }
		internal string UserID { get; set; }
		internal string Password { get; set; }

		internal DatabaseConfiguration(XmlNode databaseNode)
		{
			XmlNode connectionNode = databaseNode.SelectSingleNode("Connection");

			DataSource = connectionNode.Attributes.GetNamedItem("source").Value;
			InitalCatalog = connectionNode.Attributes.GetNamedItem("catalog").Value;
			UserID = connectionNode.Attributes.GetNamedItem("id").Value;
			Password = connectionNode.Attributes.GetNamedItem("password").Value;
		}

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
