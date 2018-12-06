using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataTrack.Core.Util.Extensions
{
    public static class SqlCommandExtension
    {

        public static SqlCommand AddParameter(this SqlCommand command, (string Handle, object Value) parameter)
        {
            command.Parameters.Add(new SqlParameter(parameter.Handle, Dictionaries.SQLDataTypes[parameter.Value.GetType()]) { Value = parameter.Value });

            return command;
        }

        public static SqlCommand AddParameters(this SqlCommand command, List<(string Handle, object Value)> parameters)
        {
            foreach ((string Handle, object Value) parameter in parameters) command.AddParameter(parameter);

            return command;
        }

    }
}
