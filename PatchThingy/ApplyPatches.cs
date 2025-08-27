using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Compiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

// One class to manage all changes to the data.win
//
// This file contains the function that applies the
// patches in the output folder to the Active Data.
partial class DataHandler
{
    public static void ApplyPatches(ConsoleMenu menu, DataFile vandatailla) // typo but it was funny lmao
    {
        Console.Clear();
        menu.lines[1].SetText(" " + vandatailla.Data.GeneralInfo.DisplayName.Content);

        // Don't try to apply patches that don't exist.
        if (!Path.Exists(Config.current.OutputPath))
        {
            menu.lines[4].SetText("No output found. (Try generating patches)", true);
            menu.DrawAllLines();
            return;
        }

        CodeImportGroup importGroup = new(vandatailla.Data);

        // Patch files for code existing in vanilla
        foreach (string filePath in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Patches/Code")))
        {
            // read patches from file
            var patchFile = PatchFile.FromText(File.ReadAllText(filePath));

            // find and decompile the associated code
            var patchDest = vandatailla.Data.Code.ByName(Path.GetFileNameWithoutExtension(patchFile.basePath));
            var vanillaCode = vandatailla.DecompileCode(patchDest);

            // apply patches to vanilla code
            var patcher = new Patcher(patchFile.patches, vanillaCode);
            patcher.Patch(Patcher.Mode.FUZZY);

            // in any patches fail to apply here, don't save changes after applying.
            if (patcher.Results.Any(result => !result.success))
            {
                menu.lines[4].SetText($"Failed to apply patches to code", true);
                menu.lines[6].SetText(Path.GetFileName(patchFile.basePath));
                menu.DrawAllLines();
                return; // dont queue if a patch failed to apply
            }

            // write patched code to file and show progress
            importGroup.QueueReplace(patchDest, string.Join("\n", patcher.ResultLines));
            Console.WriteLine($" Patched code {Path.GetFileName(patchFile.basePath)}");
        }

        // Newly added code files
        foreach (string filePath in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Source/Code")))
        {
            // read code from file
            var codeFile = File.ReadAllText(filePath);

            // add file to data
            importGroup.QueueReplace(Path.GetFileNameWithoutExtension(filePath), codeFile);
            Console.WriteLine($" Added code {Path.GetFileName(filePath)}");
        }

        // Script Definitions
        foreach (string filePath in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Source/Scripts")))
        {
            // load the script definition from JSON
            ScriptDefinition scriptDef = JsonSerializer.Deserialize<ScriptDefinition>(File.ReadAllText(filePath))!;

            // if the definition couldn't be loaded for whatever reason
            if (scriptDef is null)
            {
                menu.lines[4].SetText($"Failed to load script definition", true);
                menu.lines[6].SetText(Path.GetFileName(filePath));
                menu.DrawAllLines();
                return; // dont queue if a patch failed to apply
            }

            // add script definition to data
            vandatailla.Data.Scripts.Add(scriptDef.Save(vandatailla.Data));
            Console.WriteLine($" Defined script {scriptDef.Name}");
        }

        // sprites (placeholder)
        // TODO: generate Texture Page
        foreach (string filePath in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Source/Sprites")))
        {
            if (!filePath.EndsWith(".json"))
            {
                continue; // dont try to load definition from image.
            }

            SpriteDefinition spriteDef = JsonSerializer.Deserialize<SpriteDefinition>(File.ReadAllText(filePath))!;

            // if the definition couldn't be loaded for whatever reason
            if (spriteDef is null)
            {
                menu.lines[4].SetText($"Failed to load sprite definition", true);
                menu.lines[6].SetText(Path.GetFileName(filePath));
                menu.DrawAllLines();
                return; // dont queue if a patch failed to apply
            }

            vandatailla.Data.Sprites.Add(spriteDef.Save(vandatailla.Data));
            Console.WriteLine($" Added placeholder sprite for {spriteDef.Name}");
        }

        importGroup.Import();
        vandatailla.SaveChanges(Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data.win"));

        // success popup
        menu.lines[3].SetText("SUCCESS", true);
        menu.lines[3].SetColor(ConsoleColor.Yellow);
        menu.lines[4].SetText("Patches applied successfully!", true);
        menu.DrawAllLines(true);
    }
}