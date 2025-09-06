using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

class DataFile
{
    public static int chapter = 0; // hardcode to only look at ch2 for now
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