using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace DataStructureTest.Extensions
{
    public static class SQLiteWhereInParametersExtension
    {
        public static string BuildWhereInClause<T>(string partialClause, string paramPrefix, IEnumerable<T> parameters)
        {
            string[] parameterNames = parameters.Select(
                (paramText, paramNumber) => "@" + paramPrefix + paramNumber.ToString())
                .ToArray();

            string inClause = string.Join(",", parameterNames);
            string whereInClause = string.Format(partialClause.Trim(), inClause);

            return whereInClause;
        }

        public static void AddParamsToCommand<T>(this SQLiteCommand cmd, string paramPrefix, IEnumerable<T> parameters)
        {
            string[] parameterValues = parameters.Select((paramText) => paramText.ToString()).ToArray();

            string[] parameterNames = parameterValues.Select(
                (paramText, paramNumber) => "@" + paramPrefix + paramNumber.ToString()
                ).ToArray();

            for (int i = 0; i < parameterNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(parameterNames[i], parameterValues[i]);
            }
        }
    }
}
