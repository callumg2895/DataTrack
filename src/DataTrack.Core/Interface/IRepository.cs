using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
    public interface IRepository<TBase> where TBase : IEntity
    {
        void Create(TBase item);
        void Create(List<TBase> items);
        List<TBase> GetAll();
        TBase GetByID(int id);
        List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue);
        int Update(TBase item);
        void Delete(TBase item);
        void DeleteAll();

    }
}
