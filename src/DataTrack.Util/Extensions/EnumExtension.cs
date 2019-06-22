using DataTrack.Logging;
using System.Data;
using System.Reflection;

namespace DataTrack.Util.Extensions
{
	public static class EnumExtension
	{
		public static string ToSqlString(this SqlDbType type)
		{
			switch (type)
			{
				case SqlDbType.VarChar:
					return "varchar(255)";
				default:
					return type.ToString().ToLower();
			}
		}

	}
}
