using DataTrack.Core.Enums;
using System.Collections.Generic;

namespace DataTrack.Core.Interface
{
	public interface IEntityRepository<TBase> where TBase : IEntity
	{
		/// <summary>
		/// Creates a database entry for the Entity described by 'item'.
		/// </summary>
		/// <param name="item">The item of type TBase that will be created</param>
		void Create(TBase item);

		/// <summary>
		/// Creates a database entry for all Entities in the 'items' list.
		/// </summary>
		/// <param name="items">The list of items of type TBase that will be created</param>
		void Create(List<TBase> items);

		/// <summary>
		/// Returns all existing Entities of type TBase that currently exist.
		/// </summary>
		List<TBase> GetAll();

		/// <summary>
		/// Returns all existing Entities of type TBase that currently exist, which have an ID equal to 'id';
		/// </summary>
		/// <param name="id">The ID value of the Entity to retrieve</param>
		TBase GetByID(int id);

		/// <summary>
		/// Returns all existing Entities of type TBase that currently exist, which have a property (propName) that
		/// meets the restriction.
		/// </summary>
		/// <param name="propName">Name of the property of TBase that must meet the restriction</param>
		/// <param name="restriction">The type of the restriction</param>
		/// <param name="propValue">The value that the property is restricted by</param>
		List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue);

		/// <summary>
		/// Updates the Entity 'item' with new values.
		/// </summary>
		/// <param name="item">The item of type TBase to update</param>
		int Update(TBase item);

		/// <summary>
		/// Deletes the database entry for the Entity defined by 'item'.
		/// </summary>
		/// <param name="item">The item of type TBase to delete</param>
		void Delete(TBase item);

		/// <summary>
		/// Deletes all database entries for Entities of type TBase.
		/// </summary>
		void DeleteAll();

	}
}
