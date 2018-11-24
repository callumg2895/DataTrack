using DataTrack.Core.SQL;
using DataTrack.Core.SQL.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Reflection;
using DataTrack.Core.Enums;

namespace DataTrack.Core.Repository
{
    public static class Repository<TBase> where TBase : new()
    {
        #region Create

        public static int Create(TBase item)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(new InsertQueryBuilder<TBase>(item));

            return (int)transaction.Execute()[0];
        }

        #endregion

        #region Read

        public static TBase GetByID(int id)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(new ReadQueryBuilder<TBase>(id));

            return (TBase)((List<TBase>)transaction.Execute()[0])[0];
        }

        public static List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue)
        {
            Type propType = propValue.GetType();

            ReadQueryBuilder<TBase> readBuilder = new ReadQueryBuilder<TBase>();

            MethodInfo addRestriction;
            addRestriction = readBuilder.GetType().GetMethod("AddRestriction", BindingFlags.Instance | BindingFlags.Public);
            addRestriction.MakeGenericMethod(propType).Invoke(readBuilder, new object[] { propName, restriction, propValue });

            Transaction<TBase> transaction = new Transaction<TBase>(readBuilder);

            return (List<TBase>)transaction.Execute()[0];
        }

        #endregion

        #region Update

        public static int Update(TBase item)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(new UpdateQueryBuilder<TBase>(item));

            return (int)transaction.Execute()[0];
        }

        #endregion

        #region Delete

        public static void Delete(TBase item)
        {
            Transaction<TBase> transaction = new Transaction<TBase>(new DeleteQueryBuilder<TBase>(item));

            transaction.Execute();
        }

        #endregion
    }
}
