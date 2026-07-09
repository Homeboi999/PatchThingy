using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

namespace TestThingy.Data;

class DataFile
{
    public int chapter;
    public DataType type;
    public UndertaleData Data;

    // find the relevant versions of data.win
    string fileName => GetFileName(type);

    GlobalDecompileContext globalDecompileContext;
    IDecompileSettings decompilerSettings;

    public DataFile(DataType type, int chapter)
    {
        // set variables first so
        // GetFilePath() can use them.
        this.type = type;
        this.chapter = chapter;

        // Read the DataFile from disk
        // using the chapter and type to
        // determine the file path.
        using (Stream file = File.Open(GetFilePath(), FileMode.Open))
        {
            Data = UndertaleIO.Read(file);
        }

        globalDecompileContext = new(Data);
        decompilerSettings = Data.ToolInfo.DecompilerSettings;
    }

    // Read the code from the Data file,
    // and convert it into a list of strings
    // for each line.
    public List<string> DecompileCode(UndertaleCode code)
    {
        string codeText = new DecompileContext(globalDecompileContext, code, decompilerSettings).DecompileToString();

        return codeText
            .Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .ToList();
    }

    // Write the file to disk
    public void SaveChanges()
    {
        using (Stream file = File.Open(GetFilePath(), FileMode.Create))
        {
            UndertaleIO.Write(file, Data);
        }
    }
    
    // Find the location of the DataFile
    // for the current chapter and type.
    public string GetFilePath()
    {
        string path = Path.Combine(Config.current.GamePath, $"./chapter{chapter}_windows", fileName);

        if (!Path.Exists(path))
        {
            // just crash, idk what else to do
            // if it somehow tries finding Chapter 58
            //
            // itll prob crash anyway later
            throw new FileNotFoundException($"Attempted to load nonexistent Chapter {chapter}.");
        }

        // TODO: support other versions of Deltarune.
        // ex. MacOS, Chapters 1&2, SURVEY_PROGRAM
        return path;
    }

    public static string GetFileName(DataType type)
    {
        switch (type)
        {
            // Active Data
            case DataType.Active:
                return "data.win";

            // Vanilla Data
            case DataType.Vanilla:
                return "data-vanilla.win";

            // Backup Data
            case DataType.Backup:
                return "data-backup.win";

            // For compiler
            default:
                throw new ArgumentException($"Invalid DataType ({type})", paramName: nameof(type));
        }
    }
}