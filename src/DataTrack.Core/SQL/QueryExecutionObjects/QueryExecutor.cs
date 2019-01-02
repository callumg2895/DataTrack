using DataTrack.Core.SQL.QueryObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DataTrack.Core.SQL.QueryExecutionObjects
{
    public abstract class QueryExecutor<TBase> where TBase : new()
    {
        private protected Query<TBase> Query;
        private protected Stopwatch stopwatch;
        private protected Type baseType = typeof(TBase);
    }
}
