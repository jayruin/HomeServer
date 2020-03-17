using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeServer.Utility;
using HomeServer.Areas.FileManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net;

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
        public IActionResult Rename(string base64Path, string newName)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            return RedirectToAction("Browse", "FileExplorer", new { area = "FileManager", base64Path = FileSystem.Rename(model, newName).NodePathBase64 });
        }

        [HttpPost]
        public IActionResult Move(string base64Path, string urlBase64NewPath)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            string newPath = Base64.Base64Decode(WebUtility.UrlDecode(urlBase64NewPath) ?? "");
            return RedirectToAction("Browse", "FileExplorer", new { area = "FileManager", base64Path = FileSystem.Move(model, newPath).NodePathBase64 });
        }

        [HttpPost]
        public IActionResult Delete(string base64Path)
        {
            FileSystemNode model = FileSystem.GetNode(Base64.Base64Decode(base64Path ?? ""));
            return RedirectToAction("Browse", "FileExplorer", new { area = "FileManager", base64Path = FileSystem.Delete(model).NodePathBase64 });
        }

        [HttpPost]
        [RequestSizeLimit(4294967295)]
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