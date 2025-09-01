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
    // Pre-generate path strings for ease-of-access
    static string codeFolder = Path.Combine(Config.current.OutputPath, "./Source/Code");
    static string scriptFolder = Path.Combine(Config.current.OutputPath, "./Source/Scripts");
    static string spriteFolder = Path.Combine(Config.current.OutputPath, "./Source/Sprites");
    static string patchFolder = Path.Combine(Config.current.OutputPath, "./Patches/Code");

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
        ResetDirectory(codeFolder, ".gml");
        ResetDirectory(scriptFolder, ".json");
        ResetDirectory(spriteFolder, ".json");
        ResetDirectory(patchFolder, ".gml.patch");

        // Write file to the correct folder
        // based on the file type.
        string path;
        foreach (TempFile queueFile in FileQueue)
        {
            switch(queueFile.Type)
            {
                case FileType.Code:
                    path = Path.Combine(codeFolder, $"{queueFile.Name}.gml");
                    break;

                case FileType.Script:
                    path = Path.Combine(scriptFolder, $"{queueFile.Name}.json");
                    break;

                case FileType.Sprite:
                    path = Path.Combine(spriteFolder, $"{queueFile.Name}.json");
                    break;

                case FileType.Patch:
                    path = Path.Combine(patchFolder, $"{queueFile.Name}.gml.patch");
                    break;

                // failsafe to create an "Other" folder to save to
                default:
                    Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Other"));
                    path = Path.Combine(Config.current.OutputPath, "./Other");
                    break;
            }

            File.WriteAllText(path, queueFile.Text);
        }
    }

    static void ResetDirectory(string folderPath, string toDelete = "")
    {
        // Create folder if it doesn't already exist
        Directory.CreateDirectory(folderPath);

        // Empty folder of all files of a given type
        foreach (string file in Directory.EnumerateFiles(folderPath))
        {
            if (file.EndsWith(toDelete))
            {
                File.Delete(file);
            }
        }
    }
}