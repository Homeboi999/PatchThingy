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
    const string codeFolder = "./Code";
    const string scriptFolder = "./Scripts";
    const string spriteFolder = "./Sprites";
    const string patchFolder = "./Patches";
    const string objectFolder = "./GameObjects";

    public enum FileType
    {
        Code,
        Script,
        Sprite,
        Patch,
        GameObject,
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
        ResetFolder(codeFolder, ".gml", true);
        ResetFolder(scriptFolder, ".json");
        ResetFolder(spriteFolder, ".json");
        ResetFolder(patchFolder, ".gml.patch");
        ResetFolder(objectFolder, ".json");

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

                case FileType.GameObject:
                    typeFolder = Path.Combine(objectFolder, $"{queueFile.Name}.json");
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

            // Don't replace source code.
            if (queueFile.Type == FileType.Code && File.Exists(path))
            {
                continue;
            }

            File.WriteAllText(path, queueFile.Text);
        }
    }

    // for ApplyPatches
    static bool FolderExists(int chapter)
    {
        return Path.Exists(GetPath(chapter)) && Directory.GetFileSystemEntries(GetPath(chapter)).Length > 0;
    }

    static void ResetFolder(string folderPath, string toDelete, bool keepFiles = false)
    {
        string fullPath = Path.Combine(GetPath(DataFile.chapter), folderPath);

        // if folder doesnt exist, dont need to delete
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        // exit early if told to
        if (keepFiles)
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
    }

    public static int ConvertPatches(DataFile data, int chapter)
    {
        int count = 0;

        foreach(string filePath in Directory.EnumerateFiles(Path.Combine(GetPath(chapter), codeFolder)))
        {
            if (!Path.GetFileName(filePath).EndsWith(".gml.patch"))
            {
                Console.WriteLine($"Skipping {Path.GetFileName(filePath)}");
                continue; // exit if not a patch file
            }

            // get code from Datafile
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string codeName = Path.GetFileNameWithoutExtension(fileName); // theres 2 extensions.
            UndertaleCode code = data.Data.Code.ByName(codeName);

            if (code is null)
            {
                Console.WriteLine($"No code for {codeName}");
                continue; // exit if code doesnt exist
            }

            // save new code file
            string newPath = Path.Combine(Path.GetDirectoryName(filePath)!, fileName);
            List<string> codeLines = data.DecompileCode(code);
            File.WriteAllLines(newPath, codeLines);

            // delete old patch file
            File.Delete(filePath);
            count++;
        }

        return count;
    }
}