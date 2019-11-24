using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeServer.Utility;

namespace HomeServer.Areas.DataWarehouse.Models
{
    public class QueryResultModel
    {
        public string Query { get; set; }
        public List<SQLiteColumn> Columns { get; set; }
        public List<List<string>> Rows { get; set; }

        public QueryResultModel(string query, List<SQLiteColumn> columns, List<List<string>> rows)
        {
            this.Query = query;
            this.Columns = columns;
            this.Rows = rows;
        }

        public string ConvertToCSV()
        {
            string csvString = "";

            List<string> colNames = new List<string>();

            foreach (SQLiteColumn col in Columns)
            {
                colNames.Add($"\"{col.Name}\"");
            }

            csvString += String.Join(",", colNames) + "\n";

            foreach (List<string> row in Rows)
            {
                List<string> currRow = new List<string>();
                foreach(string item in row)
                {
                    currRow.Add($"\"{item}\"");
                }
                csvString += String.Join(",", currRow) + "\n";
            }

            return csvString;
        }
    }
}
