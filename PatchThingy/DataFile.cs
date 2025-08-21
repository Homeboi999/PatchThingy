using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

class DataFile
{
    public const string chapterFolder = "chapter2_windows"; // hardcode to only look at ch2 for now
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
}