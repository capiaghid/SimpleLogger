#region

using System;
using System.Data;

#endregion

namespace SimpleLogger.Logging.Module.Database
{
    internal static class DatabaseExtensions
    {
        internal static void ExecuteMultipleNonQuery(this IDbCommand dbCommand)
        {
            string[ ] sqlStatementArray = dbCommand.CommandText
                                                   .Split(new[ ] { ";" },
                                                        StringSplitOptions.RemoveEmptyEntries);

            foreach (string sqlStatement in sqlStatementArray)
            {
                dbCommand.CommandText = sqlStatement;
                dbCommand.ExecuteNonQuery();
            }
        }
    }
}