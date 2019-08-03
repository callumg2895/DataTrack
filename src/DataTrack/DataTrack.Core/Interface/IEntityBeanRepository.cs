using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
	public interface IEntityBeanRepository<TBase> where TBase : IEntityBean
	{
		/// <summary>
		/// Returns all existing EntityBeans of type TBase that currently exist.
		/// </summary>
		List<TBase> GetAll();

		/// <summary>
		/// Returns all existing EntityBeans of type TBase that currently exist, which have a property (propName) that
		/// meets the restriction.
		/// </summary>
		/// <param name="propName">Name of the property of TBase that must meet the restriction</param>
		/// <param name="restriction">The type of the restriction</param>
		/// <param name="propValue">The value that the property is restricted by</param>
		/// <returns></returns>
		List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue);
	}
}
