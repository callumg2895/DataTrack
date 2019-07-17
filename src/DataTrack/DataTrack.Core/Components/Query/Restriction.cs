using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Enums;
using DataTrack.Logging;
using System.Data;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Query
{
	public struct Restriction
	{
		public string Alias;
		public string Handle;
		public RestrictionTypes RestrictionType;

		public Restriction(Column column, Parameter parameter, RestrictionTypes rType)
		{
			if (parameter.DatabaseType == SqlDbType.VarChar && (rType == RestrictionTypes.LessThan || rType == RestrictionTypes.MoreThan))
			{
				Logger.Error(MethodBase.GetCurrentMethod(), $"Cannot apply '{GetRestrictionString(rType)}' operator to values of type VarChar");
			}

			Alias = column.Alias;
			Handle = parameter.Handle;
			RestrictionType = rType;
		}

		public Restriction(Column column, string sql, RestrictionTypes rType)
		{
			Alias = column.Alias;
			Handle = sql;
			RestrictionType = rType;
		}

		public override string ToString()
		{
			StringBuilder restrictionBuilder = new StringBuilder();

			// Generate the SQL for the restriction clause
			switch (RestrictionType)
			{
				case RestrictionTypes.NotIn:
				case RestrictionTypes.In:
					restrictionBuilder.Append(Alias + " ");
					restrictionBuilder.Append(GetRestrictionString(RestrictionType) + " (");
					restrictionBuilder.Append(Handle);
					restrictionBuilder.Append(")");
					break;

				case RestrictionTypes.LessThan:
				case RestrictionTypes.MoreThan:
					restrictionBuilder.Append(Alias + " ");
					restrictionBuilder.Append(GetRestrictionString(RestrictionType) + " ");
					restrictionBuilder.Append(Handle);
					break;


				case RestrictionTypes.EqualTo:
				case RestrictionTypes.NotEqualTo:
				default:
					restrictionBuilder.Append(Alias + " ");
					restrictionBuilder.Append(GetRestrictionString(RestrictionType) + " ");
					restrictionBuilder.Append(Handle);
					break;
			}

			return restrictionBuilder.ToString();
		}

		private static string GetRestrictionString(RestrictionTypes type)
		{
			switch (type)
			{
				case RestrictionTypes.EqualTo: return "=";
				case RestrictionTypes.NotEqualTo: return "<>";
				case RestrictionTypes.LessThan: return "<";
				case RestrictionTypes.MoreThan: return ">";
				case RestrictionTypes.In: return "in";
				case RestrictionTypes.NotIn: return "not in";
				default:
					Logger.Error(MethodBase.GetCurrentMethod(), $"Invalid restriction '{type}'");
					return "";
			}
		}
	}
}
