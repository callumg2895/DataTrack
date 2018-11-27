using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.QueryObjects
{
    public class Query<TBase>
    {
        public List<TableMappingAttribute> Tables { get; set; } = new List<TableMappingAttribute>();
        public List<ColumnMappingAttribute> Columns { get; set; } = new List<ColumnMappingAttribute>();
        public Mapping<Type, TableMappingAttribute> TypeTableMapping { get; set; } = new Mapping<Type, TableMappingAttribute>();
        public Mapping<Type, List<ColumnMappingAttribute>> TypeColumnMapping { get; set; } = new Mapping<Type, List<ColumnMappingAttribute>>();
        public Dictionary<ColumnMappingAttribute, string> ColumnPropertyNames { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        public Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> Parameters { get; set; } = new Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>>();
        public CRUDOperationTypes OperationType { get; set; }

        public string QueryString { get; set; }
    }
}
