using System;

namespace HomeServer.Areas.DataWarehouse.Models
{
    public class TableAddColumnModel
    {
        public string Column { get; set; }

        public string Type { get; set; }

        public bool PrimaryKey { get; set; }

        public bool NotNull { get; set; }

        public bool Unique { get; set; }
    }
}
