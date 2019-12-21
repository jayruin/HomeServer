using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeServer.Utility;
using HomeServer.Areas.FileManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HomeServer.Areas.FileManager.Controllers
{
    [Area("FileManager")]
    [Authorize(Policy = "SiteAdmin")]
    public class FileExplorerController : Controller
    {
        [HttpGet]
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

        [HttpGet]
        public IActionResult Download(string base64Path)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            if (model.IsFile)
            {
                return File(System.IO.File.OpenRead(model.NodePath), model.MimeType, model.Name);
            }
            else
            {
                string zipName = model.Name + ".zip";
                return File(FileSystem.DirectoryGetZipStream(model), FileSystem.GetMimeType(zipName), zipName);
            }
        }

        [HttpPost]
        public IActionResult CreateDirectory(string base64Path, string newDirectoryName)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            if (model.IsDirectory)
            {
                FileSystem.CreateDirectory(model, newDirectoryName);
            }
            return RedirectToAction("Browse", "FileExplorer", new { area = "FileManager", base64Path = model.NodePathBase64 });
        }

        [HttpPost]
        public IActionResult Delete(string base64Path)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            return RedirectToAction("Browse", "FileExplorer", new { area = "FileManager", base64Path = FileSystem.Delete(model).NodePathBase64 });
        }

        [HttpPost]
        public IActionResult UploadFiles(string base64Path, IFormFileCollection uploadedFiles)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            if (model.IsDirectory)
            {
                FileSystem.SaveUploadedFiles(model, uploadedFiles);
            }
            return RedirectToAction("Browse", "FileExplorer", new { area = "FileManager", base64Path = model.NodePathBase64 });
        }
    }
}