// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;

DataFile vanilla = new("data-vanilla.win");
DataFile modded = new("data.win");

Console.WriteLine(vanilla.Data.GeneralInfo?.DisplayName?.Content.ToLower());
Console.WriteLine(modded.Data.GeneralInfo?.DisplayName?.Content.ToLower());

foreach (UndertaleCode modCode in modded.Data.Code)
{
    if (modCode.ParentEntry is not null)
        continue;

    UndertaleCode vanillaCode = vanilla.Data.Code.ByName(modCode.Name.Content);

    if (vanillaCode is not null && vanillaCode.ParentEntry is null)
    {
        LineMatchedDiffer differ = new();
        PatchFile modChanges = new();

        modChanges.patches = differ.MakePatches(vanilla.DecompileCode(vanillaCode), modded.DecompileCode(modCode));
        Console.WriteLine(modChanges.ToString());
    }
}
