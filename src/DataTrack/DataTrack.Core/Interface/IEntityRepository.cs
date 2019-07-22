using DataTrack.Core.Enums;
using System.Collections.Generic;

namespace DataTrack.Core.Interface
{
	public interface IEntityRepository<TBase> where TBase : IEntity
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
