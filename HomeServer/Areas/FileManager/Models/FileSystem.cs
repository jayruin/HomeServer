using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.StaticFiles;
using HomeServer.Utility;

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
    }

    public class FileSystemNode
    {
        public string NodePath { get; set; }

        public string NodePathBase64
        {
            get
            {
                int rootIndex = NodePath.IndexOf(@"\") + 1;
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
                mimeMapper.TryGetContentType(NodePath, out type);
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
