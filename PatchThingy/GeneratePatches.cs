using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

partial class DataHandler
{
    public static void GeneratePatches(DataFile vanilla, DataFile modded)
    {
        // Create output folder structure if not already present.
        Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Source/Code"));
        Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Source/Scripts"));
        Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Source/Game Objects"));
        Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Patches/Code"));

        // code files
        foreach (UndertaleCode modCode in modded.Data.Code)
        {
            // in UMT, these are the greyed out duplicates of a bunch of files.
            if (modCode.ParentEntry is not null)
                continue;

            // get name from vanilla data.win to check if it's a new file or not
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
                    // if there are no changes, ignore
                    continue;
                }

                File.WriteAllText(Path.Combine(Config.current.OutputPath, $"Patches/Code/{modCode.Name.Content}.gml.patch"), modChanges.ToString());
                Console.Write("▮");
            }
            // if it's a new file, export entire file to the Source folder
            else if (vanillaCode is null)
            {
                File.WriteAllLines(Path.Combine(Config.current.OutputPath, $"./Source/Code/{modCode.Name.Content}.gml"), modded.DecompileCode(modCode));
                Console.Write("▮");
            }
        }

        // separate script definitions and code
        // TODO: this actually doesnt look that cool i dont think
        Console.WriteLine();

        // script definitions
        foreach (UndertaleScript modScript in modded.Data.Scripts)
        {
            UndertaleScript vanillaScript = vanilla.Data.Scripts.ByName(modScript.Name.Content);

            // if the script isnt in vanilla, make a definition for it when applying
            if (vanillaScript is null)
            {
                string jsonText = JsonSerializer.Serialize(new ScriptDefinition(modScript.Name.Content, modScript.Code.Name.Content));
                File.WriteAllText(Path.Combine(Config.current.OutputPath, $"./Source/Scripts/{modScript.Name.Content}.json"), jsonText);
                Console.Write("▮");
            }
        }

        Console.WriteLine();
    }
}