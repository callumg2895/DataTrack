using DataTrack.Core.Attributes;
using DataTrack.Core.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Mapping<TBase> where TBase : new()
    {
        public List<TableMappingAttribute> Tables { get; set; } = new List<TableMappingAttribute>();
        public List<ColumnMappingAttribute> Columns { get; set; } = new List<ColumnMappingAttribute>();
        internal Dictionary<TableMappingAttribute, string> TableAliases { get; set; } = new Dictionary<TableMappingAttribute, string>();
        internal Dictionary<ColumnMappingAttribute, string> ColumnAliases { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        internal Map<Type, TableMappingAttribute> TypeTableMapping { get; set; } = new Map<Type, TableMappingAttribute>();
        internal Map<Type, List<ColumnMappingAttribute>> TypeColumnMapping { get; set; } = new Map<Type, List<ColumnMappingAttribute>>();
        internal Dictionary<ColumnMappingAttribute, string> ColumnPropertyNames { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        internal Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> Parameters { get; set; } = new Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>>();
        internal Dictionary<ColumnMappingAttribute, string> Restrictions { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
    }
}
