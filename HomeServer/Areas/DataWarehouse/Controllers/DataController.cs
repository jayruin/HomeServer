﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HomeServer.Areas.DataWarehouse.Models;
using HomeServer.Utility;
using Microsoft.Extensions.Configuration;

namespace HomeServer.Areas.DataWarehouse.Controllers
{
    [Area("DataWarehouse")]
    public class DataController : Controller
    {
        private readonly ILogger<DataController> _logger;

        private readonly IConfiguration configuration;

        private readonly string connectionString;

        public DataController(ILogger<DataController> logger, IConfiguration config)
        {
            _logger = logger;
            configuration = config;
            connectionString = this.configuration.GetConnectionString("SQLite");
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
            return View();
        }

        [HttpPost]
        public IActionResult CommandSubmit(string commandText)
        {
            SQLiteUtility.ExecuteCommand(connectionString, commandText);
            return RedirectToAction("Tables", "Data", new { area = "DataWarehouse" });
        }
    }
}
