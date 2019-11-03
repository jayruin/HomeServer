using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeServer.Utility;

namespace HomeServer.Areas.DataWarehouse.Models
{
    public class TableModel : QueryResultModel
    {
        public string Name { get; set; }
        public string Definition { get; set; }

        public TableModel(List<SQLiteColumn> columns, List<List<string>> rows, string name, string definition) : base($"SELECT * FROM {name}", columns, rows)
        {
            this.Name = name;
            this.Definition = definition;
        }
    }
}
