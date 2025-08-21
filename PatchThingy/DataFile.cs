using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

class DataFile
{
    string filePath = "chapter2_windows";
    public UndertaleData Data;

        GlobalDecompileContext globalDecompileContext;
        IDecompileSettings decompilerSettings;

    public DataFile(string fileName)
    {
        using (Stream file = File.Open(Path.Combine(Config.current!.GamePath, filePath, fileName), FileMode.Open))
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
}