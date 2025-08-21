using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

class DataHandler
{
    public static void GeneratePatches(DataFile vanilla, DataFile modded)
    {
        Directory.CreateDirectory(Path.Combine(Config.current!.OutputPath, "./Patches/Code"));
        Directory.CreateDirectory(Path.Combine(Config.current!.OutputPath, "./Source/Code"));
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
                {
                    Console.Write("▯");
                    continue;
                }

                File.WriteAllText(Path.Combine(Config.current!.OutputPath, $"Patches/Code/{modCode.Name.Content}.gml.patch"), modChanges.ToString());
                Console.Write("▮");
            }
            else if (vanillaCode is null)
            {
                File.WriteAllLines(Path.Combine(Config.current!.OutputPath, $"./Source/Code/{modCode.Name.Content}.gml"), modded.DecompileCode(modCode));
                Console.Write("▮");
            }
        }

        Console.WriteLine();

        Directory.CreateDirectory(Path.Combine(Config.current!.OutputPath, "./Source/Scripts"));
        foreach (UndertaleScript modScript in modded.Data.Scripts)
        {
            UndertaleScript vanillaScript = vanilla.Data.Scripts.ByName(modScript.Name.Content);

            if (vanillaScript is null)
            {
                File.WriteAllText(Path.Combine(Config.current!.OutputPath, $"./Source/Scripts/{modScript.Name.Content}.json"), JsonSerializer.Serialize(new ScriptDefinition(modScript.Name.Content, modScript.Code.Name.Content)));
                Console.Write("◼");
            }
            else
            {
                Console.Write("◻");
            }
        }

        Console.WriteLine();
    }
}