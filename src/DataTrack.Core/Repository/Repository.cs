using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryBuilderObjects;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Repository
{
    public static class Repository<TBase> where TBase : new()
    {
        #region Create

        public static void Create(TBase item) => new InsertQueryBuilder<TBase>(item).GetQuery().Execute();

        #endregion

        #region Read

        public static TBase GetByID(int id) => new ReadQueryBuilder<TBase>().AddRestriction("id", RestrictionTypes.EqualTo, id).GetQuery().Execute()[0];

        public static List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue)
        {
            Type propType = propValue.GetType();

            ReadQueryBuilder<TBase> readBuilder = new ReadQueryBuilder<TBase>();

            MethodInfo addRestriction;
            addRestriction = readBuilder.GetType().GetMethod("AddRestriction", BindingFlags.Instance | BindingFlags.Public);
            addRestriction.MakeGenericMethod(propType).Invoke(readBuilder, new object[] { propName, restriction, propValue });

            return readBuilder.GetQuery().Execute();
        }

        #endregion

        #region Update

        public static int Update(TBase item) => new UpdateQueryBuilder<TBase>(item).GetQuery().Execute();


        #endregion

        #region Delete

        public static void Delete(TBase item) => new DeleteQueryBuilder<TBase>(item).GetQuery().Execute();

        #endregion
    }
}
