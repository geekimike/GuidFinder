using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Dapper;

namespace GuidFinder
{
	class Program
	{
		static void Main(string[] args)
		{
            var guid = Guid.Parse("D7DA7F55-B5B7-4455-995B-CFA752DAB15B");
			var successfulColumns = new List<Column>();

			var columns = FindGuidColumns().ToList();

			for(var i = 0; i < columns.Count(); i++)
			{
				Console.Write("{0}/{1} Searching {2}.{3}... ", i, columns.Count(), columns[i].Table, columns[i].Name);

				var result = SearchColumn(columns[i], guid);

				if(result)
				{
					Console.WriteLine("Yes!!!");
					successfulColumns.Add(columns[i]);
				}
				else
				{
					Console.WriteLine("No...");
				}
			}

			if(successfulColumns.Count > 0)
			{
				Console.WriteLine("The GUID {0} was found in the following locations:", guid);
				foreach(var column in successfulColumns)
				{
					Console.WriteLine("{0}.{1}", column.Table, column.Name);
				}
			}
			else
			{
				Console.WriteLine("The GUID {0} was not found.", guid);
			}

			if(System.Diagnostics.Debugger.IsAttached)
			{
				Console.WriteLine("Press any key to finish...");
				Console.ReadKey();
			}
		}

		private static IEnumerable<Column> FindGuidColumns()
		{
			IEnumerable<Column> result = null;

			var sql =
				"SELECT c.TABLE_NAME AS [Table], c.COLUMN_NAME AS [Name] FROM INFORMATION_SCHEMA.Columns c INNER JOIN INFORMATION_SCHEMA.Tables t ON c.TABLE_NAME = t.TABLE_NAME AND t.TABLE_TYPE = 'BASE TABLE' WHERE DATA_TYPE = 'uniqueidentifier'";

			using(var connection = GetOpenConnection())
			{
				result = connection.Query<Column>(sql);
			}

			return result;
		}

		private static bool SearchColumn(
			Column column,
			Guid guid)
		{
			var sql = string.Format("SELECT COUNT(*) FROM {0} WHERE {1} = '{2}'", column.Table, column.Name, guid);

			using (var connection = GetOpenConnection())
			{
				var count = connection.ExecuteScalar<int>(sql, commandTimeout: 600); // 10 minutes....

				return count > 0;
			}
		}

		private static IDbConnection GetOpenConnection()
		{
            var connection = new SqlConnection("Server=GMSSQL28;Database=Touchpaper;Trusted_Connection=True;");
			connection.Open();
			return connection;
		}
	}
}
