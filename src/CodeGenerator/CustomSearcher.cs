using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeGenerator
{
    public class CustomSearcher
    { 
        public static List<string> GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectoriesInternal(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectoriesInternal(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectoriesInternal(string path, string searchPattern = "*")
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }
    }
}