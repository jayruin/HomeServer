using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServer.Areas.DataWarehouse.Models
{
    public class TablesModel
    {
        public List<(string, string)> TablesList { get; set; }
    }
}
