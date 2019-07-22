using DataTrack.Core.Components.Query;
using DataTrack.Core.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Repository
{
	public class EntityBeanRepository<TBase> : IEntityBeanRepository<TBase> where TBase : IEntityBean
	{
		public List<TBase> GetAll()
		{
			return new EntityBeanQuery<TBase>().Execute();
		}
	}
}
