using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
	public interface IEntityBeanRepository<TBase> where TBase : IEntityBean
	{
		List<TBase> GetAll();
	}
}
