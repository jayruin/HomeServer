using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HomeServer.Areas.DataWarehouse.Models;
using HomeServer.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace HomeServer.Areas.DataWarehouse.Controllers
{
    [Area("DataWarehouse")]
    [Authorize(Policy = "SiteAdmin")]
    public class DataController : Controller
    {
        private readonly ILogger<DataController> _logger;

        private readonly IConfiguration configuration;

        private readonly string connectionString;

        private readonly SQLiteDictionary savedCommands;

        private readonly SQLiteDictionary savedQueries;

        public DataController(ILogger<DataController> logger, IConfiguration config)
        {
            _logger = logger;
            configuration = config;
            connectionString = this.configuration.GetConnectionString("SQLite");
            savedCommands = new SQLiteDictionary(this.configuration.GetConnectionString("SavedCommandsQueries"), "SavedCommands");
            savedQueries = new SQLiteDictionary(this.configuration.GetConnectionString("SavedCommandsQueries"), "SavedQueries");
        }

        public IActionResult CreateTable()
        {
            return View();
        }

        public IActionResult Tables()
        {
            TablesModel model = new TablesModel();
            model.TablesList = SQLiteUtility.GetTables(connectionString);
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateTableSubmit(string tableName, IList<TableAddColumnModel> tableColumns)
        {
            SQLiteUtility.CreateTable(connectionString, tableName, tableColumns);

            return RedirectToAction("Tables", "Data", new { area = "DataWarehouse"});
        }

        [HttpGet]
        public IActionResult EditTable(string tableName)
        {
            TableModel model = SQLiteUtility.GetTableModel(connectionString, tableName);
            return View(model);
        }

        [HttpPost]
        public IActionResult AddRow(string tableName, IList<string> row)
        {
            SQLiteUtility.InsertRow(connectionString, tableName, row);

            return RedirectToAction("EditTable", "Data", new { area = "DataWarehouse", tableName = tableName });
        }

        [HttpPost]
        public IActionResult DeleteRow(string tableName, IList<string> row)
        {
            SQLiteUtility.DeleteRow(connectionString, tableName, row);

            return RedirectToAction("EditTable", "Data", new { area = "DataWarehouse", tableName = tableName });
        }

        [HttpPost]
        public IActionResult EditRow(string tableName, IList<string> row, IList<string> newRow)
        {
            SQLiteUtility.EditRow(connectionString, tableName, row, newRow);
            return RedirectToAction("EditTable", "Data", new { area = "DataWarehouse", tableName = tableName });
        }

        public IActionResult Command()
        {
            List<KeyValuePair<string, string>> model = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, string> item in savedCommands)
            {
                model.Add(item);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult CommandSubmit(string commandText)
        {
            SQLiteUtility.ExecuteCommand(connectionString, commandText);
            return RedirectToAction("Command", "Data", new { area = "DataWarehouse" });
        }

        [HttpPost]
        public IActionResult CommandSave(string name, string commandText)
        {
            savedCommands[name] = commandText;
            return RedirectToAction("Command", "Data", new { area = "DataWarehouse" });
        }

        [HttpPost]
        public IActionResult CommandDelete(string name)
        {
            savedCommands.Remove(name);
            return RedirectToAction("Command", "Data", new { area = "DataWarehouse" });
        }

        [HttpGet]
        public IActionResult CommandExecute(string name)
        {
            SQLiteUtility.ExecuteCommand(connectionString, savedCommands[name]);
            return RedirectToAction("Command", "Data", new { area = "DataWarehouse" });
        }

        public IActionResult Query()
        {
            List<KeyValuePair<string, string>> model = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, string> item in savedQueries)
            {
                model.Add(item);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult QuerySubmit(string query)
        {
            return View(SQLiteUtility.GetQueryResult(connectionString, query));
        }

        [HttpPost]
        public IActionResult QuerySave(string name, string query)
        {
            savedQueries[name] = query;
            return RedirectToAction("Query", "Data", new { area = "DataWarehouse" });
        }

        [HttpPost]
        public IActionResult QueryDelete(string name)
        {
            savedQueries.Remove(name);
            return RedirectToAction("Query", "Data", new { area = "DataWarehouse" });
        }

        [HttpGet]
        public IActionResult QueryExecute(string name)
        {
            return View("QuerySubmit", SQLiteUtility.GetQueryResult(connectionString, savedQueries[name]));
        }
    }
}
