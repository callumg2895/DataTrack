using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTrack.Core.Components.Data
{
	internal class EntityMapping<TBase> : Mapping where TBase : IEntity
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		internal Dictionary<IEntity, DataRow> EntityDataRowMapping { get; set; }

		internal EntityMapping()
			: base(typeof(TBase))
		{
			EntityDataRowMapping = new Dictionary<IEntity, DataRow>();

			MapType(BaseType);
		}
	}
}
