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
        public int Chapter;

        public TempFile (string Name, string Text, FileType Type, int? Chapter = null)
        {
            this.Name = Name;
            this.Text = Text;
            this.Type = Type;
            this.Chapter = Chapter ?? DataFile.chapter;
        }
    }

    static List<TempFile> FileQueue = [];

    public static void QueueFile(string fileName, string fileText, FileType fileType)
    {
        FileQueue.Add(new TempFile(fileName, fileText, fileType));
    }
    public static void QueueFile(string fileName, string fileText, FileType fileType, int fileChapter)
    {
        FileQueue.Add(new TempFile(fileName, fileText, fileType, fileChapter));
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
            switch(queueFile.Type)
            {
                case FileType.Code:
                    path = Path.Combine(GetPath(queueFile.Chapter), codeFolder, $"{queueFile.Name}.gml");
                    break;

                case FileType.Script:
                    path = Path.Combine(GetPath(queueFile.Chapter), scriptFolder, $"{queueFile.Name}.json");
                    break;

                case FileType.Sprite:
                    path = Path.Combine(GetPath(queueFile.Chapter), spriteFolder, $"{queueFile.Name}.json");
                    break;

                case FileType.Patch:
                    path = Path.Combine(GetPath(queueFile.Chapter), patchFolder, $"{queueFile.Name}.gml.patch");
                    break;

                // failsafe to create an "Other" folder to save to
                default:
                    Directory.CreateDirectory(Path.Combine(GetPath(queueFile.Chapter), "./Other", $"{queueFile.Name}.txt"));
                    path = Path.Combine(GetPath(queueFile.Chapter), "./Other", $"{queueFile.Name}.txt");
                    break;
            }

            File.WriteAllText(path, queueFile.Text);
        }
    }

    static void ResetFolder(string folderPath, string toDelete)
    {
        string fullPath = Path.Combine(GetPath(DataFile.chapter), folderPath);
        string globalPath = Path.Combine(GetPath(0), folderPath);

        // Create folder if it doesn't already exist
        Directory.CreateDirectory(fullPath);
        Directory.CreateDirectory(globalPath);

        // Empty folder of all files of a given type
        foreach (string file in Directory.EnumerateFiles(folderPath))
        {
            if (file.EndsWith(toDelete))
            {
                File.Delete(file);
            }
        }
        // dont clear out global patches
    }
}