using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
	public interface IEntityBeanRepository<TBase> where TBase : IEntityBean
	{
		List<TBase> GetAll();

		List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue);
	}
}
