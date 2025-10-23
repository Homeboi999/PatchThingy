using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

class DataFile
{
    // find the relevant versions of data.win

    // Active Data: the data.win that Deltarune loads, and that Steam would replace
    // Vanilla Data: the version of data.win that the patches were based on
    // Backup Data: a second copy of the patched data.win, in case of an update
    public static string active
    {
        get
        {
            return Path.Combine(Config.current.GamePath, GetPath(), "data.win");
        }
    }
    public static string vanilla
    {
        get
        {
            return Path.Combine(Config.current.GamePath, GetPath(), "data-vanilla.win");
        }
    }
    public static string backup
    {
        get
        {
            return Path.Combine(Config.current.GamePath, GetPath(), "data-backup.win");
        }
    }

    public static int chapter = 0;
    public UndertaleData Data;

    GlobalDecompileContext globalDecompileContext;
    IDecompileSettings decompilerSettings;

    public DataFile(string filePath)
    {
        using (Stream file = File.Open(filePath, FileMode.Open))
        {
            Data = UndertaleIO.Read(file);
        }

        globalDecompileContext = new(Data);
        decompilerSettings = Data.ToolInfo.DecompilerSettings;
    }

    public List<string> DecompileCode(UndertaleCode code)
    {
        string codeText = new DecompileContext(globalDecompileContext, code, decompilerSettings).DecompileToString();

        return codeText
            .Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .ToList();
    }

    public void SaveChanges(string filePath)
    {
        using (Stream file = File.Open(filePath, FileMode.Create))
        {
            UndertaleIO.Write(file, Data);
        }
    }
    
    public static string GetPath()
    {
        string path = Path.Combine(Config.current.GamePath, $"./chapter{DataFile.chapter}_windows");

        if (!Path.Exists(path))
        {
            // just crash, idk what else to do
            // if it somehow tries finding Chapter 58
            //
            // itll prob crash anyway later
            throw new Exception("Attempted to load nonexistent chapter.");
        }

        // TODO: support other versions of Deltarune.
        // ex. MacOS, Chapters 1&2, SURVEY_PROGRAM
        return path;
    }
}