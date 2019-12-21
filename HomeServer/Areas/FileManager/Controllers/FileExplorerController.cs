using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeServer.Utility;
using Microsoft.AspNetCore.Authorization;

namespace HomeServer.Areas.FileManager.Controllers
{
    [Area("FileManager")]
    [Authorize(Policy = "SiteAdmin")]
    public class FileExplorerController : Controller
    {
        public IActionResult Browse(string base64Path)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            if (model.IsFile)
            {
                return View("File", model);
            }
            else
            {
                return View("Directory", model);
            }
        }
    }
}