using DataTrack.Core.SQL.DataStructures;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataTrack.Core.Util.Extensions
{
    public static class SqlCommandExtension
    {

        public static SqlCommand AddParameter(this SqlCommand command, Parameter parameter)
        {
            command.Parameters.Add(new SqlParameter(parameter.Handle, parameter.DatabaseType) { Value = parameter.Value });

            return command;
        }

        public static SqlCommand AddParameters(this SqlCommand command, List<Parameter> parameters)
        {
            foreach (Parameter parameter in parameters) command.AddParameter(parameter);

            return command;
        }

    }
}
