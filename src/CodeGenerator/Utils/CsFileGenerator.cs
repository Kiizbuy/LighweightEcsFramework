using System;
using System.IO;

namespace CodeGenerator
{
    internal class CsFileGenerator
    {
        internal static void Generate(string outputFolderPath, 
            string fileName, 
            string fileContent)
        {
            var fullFilePath = Path.Combine(outputFolderPath, fileName + ".cs");
            
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
            }

            var generatedFile = File.CreateText(fullFilePath);
            
            generatedFile.Write(fileContent);
            generatedFile.Flush();
            generatedFile.Close();
            generatedFile.Dispose();
        }
    }
}