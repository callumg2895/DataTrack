using DataTrack.Core.SQL.Read;
using DataTrack.Core.SQL.Insert;
using DataTrack.Core.SQL.Update;
using DataTrack.Core.SQL.Delete;
using DataTrack.Core.SQL;
using System;
using System.Collections.Generic;
using System.Text;


namespace DataTrack.Core.Repository
{
    public static class Repository<TBase> where TBase : new()
    {
        public static int Create(TBase item)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(
                new List<QueryBuilder<TBase>>()
                {
                    new InsertQueryBuilder<TBase>(item)
                });

            return (int)transaction.Execute()[0];
        }

        public static TBase GetByID(int id)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(
                new List<QueryBuilder<TBase>>()
                {
                    new ReadQueryBuilder<TBase>(id)
                });

            return (TBase)((List<TBase>)transaction.Execute()[0])[0];
        }

        public static int Update(TBase item)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(
                new List<QueryBuilder<TBase>>()
                {
                    new UpdateQueryBuilder<TBase>(item)
                });

            return (int)transaction.Execute()[0];
        }

        public static void Delete(TBase item)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(
                new List<QueryBuilder<TBase>>()
                {
                    new DeleteQueryBuilder<TBase>(item)
                });

            transaction.Execute();
        }
    }
}
