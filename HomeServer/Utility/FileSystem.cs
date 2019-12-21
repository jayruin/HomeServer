using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace HomeServer.Utility
{
    public static class FileSystem
    {
        private static FileExtensionContentTypeProvider systemMimeMapper = new FileExtensionContentTypeProvider();

        private static string root = "FileStorage";

        public static FileSystemNode GetNode(string path)
        {
            return new FileSystemNode(Path.Combine(root, path), systemMimeMapper);
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

        public FileSystemNode(string path, FileExtensionContentTypeProvider systemMimeMapper)
        {
            NodePath = path;
            mimeMapper = systemMimeMapper;
        }
    }
}
