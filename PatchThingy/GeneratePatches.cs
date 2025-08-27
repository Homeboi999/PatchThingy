using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

// One class to manage all changes to the data.win
//
// This file contains the function that generates
// patches and writes them to the output folder.
partial class DataHandler
{
    public static void GeneratePatches(ConsoleMenu menu, DataFile vanilla, DataFile modded)
    {
        // Create output folder structure if not already present.
        Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Source/Code"));
        Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Source/Scripts"));
        Directory.CreateDirectory(Path.Combine(Config.current.OutputPath, "./Source/Sprites"));
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
                Console.WriteLine($"Generated patches for {modCode.Name.Content}.gml");
            }

            // if it's a new file, export entire file to the Source folder
            else if (vanillaCode is null)
            {
                File.WriteAllLines(Path.Combine(Config.current.OutputPath, $"./Source/Code/{modCode.Name.Content}.gml"), modded.DecompileCode(modCode));
                Console.WriteLine($"Created source code for {modCode.Name.Content}.gml");
            }
        }

        // script definitions
        foreach (UndertaleScript modScript in modded.Data.Scripts)
        {
            UndertaleScript vanillaScript = vanilla.Data.Scripts.ByName(modScript.Name.Content);

            // if the script isnt in vanilla, make a definition for it when applying
            if (vanillaScript is null)
            {
                string jsonText = JsonSerializer.Serialize(new ScriptDefinition(modScript.Name.Content, modScript.Code.Name.Content));
                File.WriteAllText(Path.Combine(Config.current.OutputPath, $"./Source/Scripts/{modScript.Name.Content}.json"), jsonText);
                
                Console.WriteLine($"Created script definition for {modScript.Name.Content}");
            }
        }

        // sprite definitions
        foreach (UndertaleSprite modSprite in modded.Data.Sprites)
        {
            
        }

        // success popup
        menu.lines[3].SetText("SUCCESS", true);
        menu.lines[3].SetColor(ConsoleColor.Yellow);
        menu.lines[4].SetText("Patches applied successfully!", true);
        menu.DrawAllLines();

        Console.WriteLine();
    }
}