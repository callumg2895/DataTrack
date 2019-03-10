using DataTrack.Core.Enums;
using System.Reflection;
using DataTrack.Core.Logging;
using System.Data;

namespace DataTrack.Core.Util.Extensions
{
    public static class EnumExtension
    {

        public static string ToSqlString(this RestrictionTypes type)
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
