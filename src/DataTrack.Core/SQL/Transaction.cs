﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryBuilderObjects;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Core.SQL
{
    public class Transaction<TBase> : IDisposable where TBase : new()
    {
        #region Members

        private SqlTransaction transaction;
        private SqlConnection connection;
        private List<Query<TBase>> queries;

        #endregion

        #region Constructors

        private Transaction()
        {
            connection = DataTrackConfiguration.CreateConnection();
            transaction = connection.BeginTransaction();      
        }

        public Transaction(Query<TBase> query)
            : this()
        {
            this.queries = new List<Query<TBase>>() { query };
        }

        public Transaction(List<Query<TBase>> queries)
            : this()
        {
            this.queries = queries;
        }

        #endregion

        #region Methods

        public List<object> Execute()
        {
            List<object> results = new List<object>();

            queries.ForEach(query => results.Add(query.Execute(connection.CreateCommand(), transaction)));

            return results;
        }

        public void RollBack()
        {
            transaction.Rollback();
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Dispose()
        {
            transaction.Dispose();
        }

        #endregion
    }
}
