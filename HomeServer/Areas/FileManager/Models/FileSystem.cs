using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.StaticFiles;
using HomeServer.Utility;
using Microsoft.AspNetCore.Http;

namespace HomeServer.Areas.FileManager.Models
{
    public static class FileSystem
    {
        private static FileExtensionContentTypeProvider systemMimeMapper = new FileExtensionContentTypeProvider();

        private static string root = "FileStorage";

        private static string tempFiles = "TempFiles";

        public static FileSystemNode GetNode(string path)
        {
            return new FileSystemNode(Path.Combine(root, path), systemMimeMapper);
        }

        public static Stream DirectoryGetZipStream(FileSystemNode directory)
        {
            string tmp = Path.Combine(tempFiles, directory.NodePathBase64);
            ZipFile.CreateFromDirectory(directory.NodePath, tmp);
            return new FileStream(tmp, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }

        public static string GetMimeType(string fileName)
        {
            string type = "";
            systemMimeMapper.TryGetContentType(fileName, out type);
            return type;
        }

        public static void CreateDirectory(FileSystemNode directory, string newDirectoryName)
        {
            if (directory.IsDirectory)
            {
                Directory.CreateDirectory(Path.Combine(directory.NodePath, newDirectoryName));
            }
        }

        public static FileSystemNode Delete(FileSystemNode node)
        {
            if (node.NodePath.Equals(root))
            {
                return node;
            }

            if (node.IsFile)
            {
                File.Delete(node.NodePath);
            }
            else if (node.IsDirectory)
            {
                Directory.Delete(node.NodePath, true);
            }
            return new FileSystemNode(Path.GetDirectoryName(node.NodePath), systemMimeMapper);
        }

        public static void SaveUploadedFiles(FileSystemNode directory, IFormFileCollection uploadedFiles)
        {
            if (directory.IsDirectory)
            {
                foreach (IFormFile uploadedFile in uploadedFiles)
                {
                    using (FileStream fileStream = File.Open(Path.Combine(directory.NodePath, uploadedFile.FileName), FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        uploadedFile.CopyTo(fileStream);
                    }
                }
            }
        }
    }

    public class FileSystemNode
    {
        public string NodePath { get; set; }

        public string NodePathBase64
        {
            get
            {
                if (!NodePath.Contains(Path.DirectorySeparatorChar))
                {
                    return "";
                }
                int rootIndex = NodePath.IndexOf(Path.DirectorySeparatorChar) + 1;
                return Base64.Base64Encode(NodePath.Substring(rootIndex, NodePath.Length - rootIndex));
            }
        }

        public string NodePathURL
        {
            get
            {
                return @$"\{NodePath}".Replace(@"\", "/");
            }
        }

        private FileExtensionContentTypeProvider mimeMapper;

        public string MimeType
        {
            get
            {
                string type = "";
                if (!mimeMapper.TryGetContentType(NodePath, out type))
                {
                    type = "application/octet-stream";
                }
                return type;
            }
        }

        public bool IsFile
        {
            get
            {
                return File.Exists(NodePath);
            }
        }

        public bool IsDirectory
        {
            get
            {
                return Directory.Exists(NodePath);
            }
        }

        public List<FileSystemNode> ChildNodes
        {
            get
            {
                List<FileSystemNode> childNodes = new List<FileSystemNode>();
                if (IsDirectory)
                {
                    foreach (string directory in Directory.GetDirectories(NodePath))
                    {
                        childNodes.Add(new FileSystemNode(directory, mimeMapper));
                    }
                    foreach (string file in Directory.GetFiles(NodePath))
                    {
                        childNodes.Add(new FileSystemNode(file, mimeMapper));
                    }
                }
                return childNodes;
            }
        }

        public string Name
        {
            get
            {
                if (IsDirectory)
                {
                    return new DirectoryInfo(NodePath).Name;
                }
                else if (IsFile)
                {
                    return new FileInfo(NodePath).Name;
                }
                else
                {
                    return "";
                }
            }
        }

        public long Size
        {
            get
            {
                if (IsFile)
                {
                    return new FileInfo(NodePath).Length;
                }
                else if (IsDirectory)
                {
                    long total = 0;
                    foreach (FileSystemNode childNode in ChildNodes)
                    {
                        total += childNode.Size;
                    }
                    return total;
                }
                return 0;
            }
        }

        public FileSystemNode(string path, FileExtensionContentTypeProvider systemMimeMapper)
        {
            NodePath = path;
            mimeMapper = systemMimeMapper;
        }
    }
}
