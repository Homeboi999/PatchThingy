// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;

DataFile vanilla = new("data-vanilla.win");
DataFile modded = new("data.win");

Directory.CreateDirectory("./Patches/Code");
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
}
