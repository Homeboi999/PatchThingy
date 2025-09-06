using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

// One class to manage all changes to the data.win
//
// This file contains functions that let me delay
// file output until all patches are generated
partial class DataHandler
{
    public static string GetPath(int chapter)
    {
        // TODO: support other versions of Deltarune.
        // ex. MacOS, Chapters 1&2, SURVEY_PROGRAM
        switch (chapter)
        {
            // global patches
            case 0:
                return Path.Combine(Config.current.OutputPath, "./Global");
                
            default:
                return Path.Combine(Config.current.OutputPath, $"./Chapter{DataFile.chapter}");
        }
    }

    // Pre-generate path strings for ease-of-access
    const string codeFolder = "./Source/Code";
    const string scriptFolder = "./Source/Scripts";
    const string spriteFolder = "./Source/Sprites";
    const string patchFolder = "./Patches/Code";

    public enum FileType
    {
        Code,
        Script,
        Sprite,
        Patch,
    }

    public record TempFile
    {
        public string Name;
        public string Text;
        public FileType Type;
        
        public int Chapter = DataFile.chapter;

        public TempFile (string Name, string Text, FileType Type)
        {
            this.Name = Name;
            this.Text = Text;
            this.Type = Type;
        }
    }

    static List<TempFile> FileQueue = [];

    public static void QueueFile(string fileName, string fileText, FileType fileType)
    {
        FileQueue.Add(new TempFile(fileName, fileText, fileType));
    }

    static void SaveModFiles()
    {
        // Reset output folder structure.
        ResetFolder(codeFolder, ".gml");
        ResetFolder(scriptFolder, ".json");
        ResetFolder(spriteFolder, ".json");
        ResetFolder(patchFolder, ".gml.patch");

        // Write file to the correct folder
        // based on the file type.
        string path;
        foreach (TempFile queueFile in FileQueue)
        {
            string typeFolder = "";

            switch(queueFile.Type)
            {
                case FileType.Code:
                    typeFolder = Path.Combine(codeFolder, $"{queueFile.Name}.gml");
                    break;

                case FileType.Script:
                    typeFolder = Path.Combine(scriptFolder, $"{queueFile.Name}.json");
                    break;

                case FileType.Sprite:
                    typeFolder = Path.Combine(spriteFolder, $"{queueFile.Name}.json");
                    break;

                case FileType.Patch:
                    typeFolder = Path.Combine(patchFolder, $"{queueFile.Name}.gml.patch");
                    break;

                // get the compiler to shut up
                default:
                    break;
            }

            if (File.Exists(Path.Combine(GetPath(0), typeFolder)))
            {
                queueFile.Chapter = 0;
            }

            path = Path.Combine(GetPath(queueFile.Chapter), typeFolder);
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }

            File.WriteAllText(path, queueFile.Text);
        }
    }

    // for ApplyPatches
    static bool FolderExists(int chapter)
    {
        return Path.Exists(GetPath(chapter)) && Directory.GetFileSystemEntries(GetPath(chapter)).Length > 0;
    }

    static void ResetFolder(string folderPath, string toDelete)
    {
        string fullPath = Path.Combine(GetPath(DataFile.chapter), folderPath);

        // if folder doesnt exist, dont need to delete
        if (!Directory.Exists(fullPath))
        {
            return;
        }

        // Empty folder of all files of a given type
        foreach (string file in Directory.EnumerateFiles(fullPath))
        {
            if (file.EndsWith(toDelete))
            {
                File.Delete(file);
            }
        }

        if (Directory.GetFileSystemEntries(fullPath).Length == 0)
        {
            Directory.Delete(fullPath);
        }
        // dont clear out global patches
    }

    public static void ConvertPatches(DataFile data)
    {
        foreach(string filePath in Directory.EnumerateFiles(Path.Combine(GetPath(DataFile.chapter), codeFolder)))
        {
            if (!Path.GetFileName(filePath).EndsWith(".gml.patch"))
            {
                continue; // exit if not a patch file
            }

            // get code from Datafile
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            UndertaleCode code = data.Data.Code.ByName(fileName);

            if (code is null)
            {
                continue; // exit if code doesnt exist
            }

            // save new code file
            string newPath = Path.Combine(Path.GetFullPath(filePath), $"{fileName}.gml");
            List<string> codeLines = data.DecompileCode(code);
            File.WriteAllLines(newPath, codeLines);

            // delete old patch file
            File.Delete(filePath);
        }
    }
}