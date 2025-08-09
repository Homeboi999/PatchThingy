// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

DataFile vanilla = new("data-vanilla.win");
DataFile modded = new("data.win");

Directory.CreateDirectory("./Patches/Code");
Directory.CreateDirectory("./Source/Code");
foreach (UndertaleCode modCode in modded.Data.Code)
{
    if (modCode.ParentEntry is not null)
        continue;

    UndertaleCode vanillaCode = vanilla.Data.Code.ByName(modCode.Name.Content);

    if (vanillaCode is not null && vanillaCode.ParentEntry is null)
    {
        PatchFile modChanges = new();
        modChanges.basePath = $"a/Code/{vanillaCode.Name.Content}.gml";
        modChanges.patchedPath = $"b/Code/{modCode.Name.Content}.gml";
            
        LineMatchedDiffer differ = new();
        modChanges.patches = differ.MakePatches(vanilla.DecompileCode(vanillaCode), modded.DecompileCode(modCode));
        
        if (modChanges.patches.Count == 0)
            continue;

        File.WriteAllText($"./Patches/Code/{modCode.Name.Content}.gml.patch", modChanges.ToString());
        Console.WriteLine($"Created .patch file for {modCode.Name.Content}.gml");
    }
    else if (vanillaCode is null)
    {
        File.WriteAllLines($"./Source/Code/{modCode.Name.Content}.gml", modded.DecompileCode(modCode));
        Console.WriteLine($"Created source file for {modCode.Name.Content}.gml");
    }
}

Directory.CreateDirectory("./Source/Scripts");
foreach (UndertaleScript modScript in modded.Data.Scripts)
{
    UndertaleScript vanillaScript = vanilla.Data.Scripts.ByName(modScript.Name.Content);

    if (vanillaScript is null)
    {
        File.WriteAllText($"./Source/Scripts/{modScript.Name.Content}.json", JsonSerializer.Serialize(new ScriptDefinition(modScript.Name.Content, modScript.Code.Name.Content)));
        Console.WriteLine($"Created script definition for {modScript.Name.Content}");
    }
}
