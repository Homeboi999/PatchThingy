using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

public record ScriptDefinition (string Name, string Code)
{
    // This is all the necessary data to define a script.
    
    public UndertaleScript Import (UndertaleData Data)
    {
        return new UndertaleScript() { Name = Data.Strings.MakeString(Name), Code = Data.Code.ByName(Code) };
    }
}

public record SpriteDefinition (string Name)
{
    // TODO: create the texture atlas
}