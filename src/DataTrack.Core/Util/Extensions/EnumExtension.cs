﻿using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Util.Extensions
{
    public static class EnumExtension
    {

        public static string ToSqlString(this RestrictionTypes type)
        {
            switch (type)
            {
                case RestrictionTypes.EqualTo:      return "=";
                case RestrictionTypes.NotEqualTo:   return "<>";
                case RestrictionTypes.LessThan:     return "<";
                case RestrictionTypes.MoreThan:     return ">";
                case RestrictionTypes.In:           return "in";
                case RestrictionTypes.NotIn:        return "not in";
                default:
                    Logger.Error(MethodBase.GetCurrentMethod(), $"Invalid restriction '{type}'");
                    return "";
            }
        }

    }
}