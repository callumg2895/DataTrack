using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.BuilderObjects;
using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Repository
{
    public class Repository<TBase> : IRepository<TBase> where TBase : IEntity
    {
        #region Create

        public void Create(TBase item) => new Query<TBase>().Create(item).Execute();

        public void Create(List<TBase> items) => new Query<TBase>().Create(items).Execute();

        #endregion

        #region Read

        public List<TBase> GetAll() => new Query<TBase>().Read().Execute();

        public TBase GetByID(int id) => new Query<TBase>().AddRestriction("id", RestrictionTypes.EqualTo, id).Execute()[0];

        public List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue)
        {
            Type propType = propValue.GetType();

            Query<TBase> query = new Query<TBase>().Read();

            MethodInfo addRestriction;
            addRestriction = query.GetType().GetMethod("AddRestriction", BindingFlags.Instance | BindingFlags.Public);
            addRestriction.Invoke(query, new object[] { propName, restriction, propValue });

            return query.Execute();
        }

        #endregion

        #region Update

        public int Update(TBase item) => new Query<TBase>().Update(item).Execute();


        #endregion

        #region Delete

        public void Delete(TBase item) => new Query<TBase>().Delete(item).Execute();

        public void DeleteAll() => new Query<TBase>().Delete().Execute();

        #endregion
    }
}
