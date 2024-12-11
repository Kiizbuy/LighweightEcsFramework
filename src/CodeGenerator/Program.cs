using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EcsCore.Data.FixedArrays;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator
{
    
    
    class Program
    {
        private static readonly List<String> _exclusion = new List<String>
        {
            "obj",
            "bin",
            "Plugins",
            "Utils",
            "Dlls",
        };
        
    
        
        public static bool InExcludeDirectory(List<string> arr, string target)
        {
            foreach (var p in arr)
            {
                if (target.ToLower().Contains(p)) return true;
            }

            return false;
        }
        
        //TODO Parse Components folder paths from args
        static void Main(string[] args)
        {
            // var zalupaIvana = new zalupaIvana();
            // zalupaIvana.Manda();

            var solutionPath = ProjectSolutionProvider.TryGetSolutionDirectoryInfo();
            if (solutionPath == null)
            {
                throw new NullReferenceException("SLN null :(");
            }
            
            var codegenPath = Path.Combine(solutionPath.FullName, "CodeGenerator");
            var directories = CustomSearcher.GetDirectories(codegenPath, searchOption: SearchOption.TopDirectoryOnly)
                .Where(x => !InExcludeDirectory(_exclusion, x))
                .ToList();
            var listFiles = new List<string>();
            foreach (var directory in directories)
            {
                var files = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories);
                if (files.Any())
                {
                    listFiles.AddRange(files);
                }
            }
            
            var libs = ((string) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
            var roslynCodegen = new RoslynCodeGenerator();
            
            
            roslynCodegen.AddExternalLibraryPathReferences(libs);
            roslynCodegen.Generate(listFiles);            
        }
    }
}
