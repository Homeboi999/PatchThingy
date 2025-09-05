using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Compiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using ImageMagick;
using RectpackSharp;

// One class to manage all changes to the data.win
//
// This file contains the function that applies the
// patches in the output folder to the Active Data.
partial class DataHandler
{
    public static void ApplyPatches(ConsoleMenu menu, DataFile vandatailla) // typo but it was funny lmao
    {
        bool warned = false;

        // set up the menu for console output
        menu.ResizeBox(80);
        menu.AddSeparator();        // 1
        menu.AddSeparator(false);   // 2
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);   // 9
        menu.AddSeparator();        // 10
        menu.AddText($"{vandatailla.Data.GeneralInfo.DisplayName.Content} - Applying Patches...", Alignment.Center);

        // Don't try to apply patches that don't exist.
        if (!Path.Exists(Config.current.OutputPath))
        {
            menu.ReplaceText(11, "! ERROR !", Alignment.Center, ConsoleColor.Red);
            menu.AddText("No prior output found.", Alignment.Center);
            menu.AddText("(Try generating new patches first!)", Alignment.Center);
            menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
            menu.Draw();
            menu.PromptChoicer(14);
            return;
        }

        CodeImportGroup importGroup = new(vandatailla.Data);

        // Patch files for code existing in vanilla
        foreach (string filePath in Directory.EnumerateFiles(patchFolder))
        {
            PatchFile patchFile;

            // read patches from file
            try
            {
                patchFile = PatchFile.FromText(File.ReadAllText(filePath));
            }
            catch
            {
                // build error message
                menu.ReplaceText(11, "! ERROR !", Alignment.Center, ConsoleColor.Red);
                menu.AddText($"Unable to load patches from {Path.GetFileNameWithoutExtension(filePath)}", Alignment.Center);
                menu.AddSeparator(false);
                menu.AddChoicer(ChoicerType.Grid, ["Exit PatchThingy", "Ignore and continue"]);
                menu.Draw();

                // give option to continue anyway
                if (menu.PromptChoicer(14) == 1)
                {
                    warned = true;
                    menu.Remove(12, 14);
                    menu.ReplaceText(11, $"{vandatailla.Data.GeneralInfo.DisplayName.Content} - Applying Patches...", Alignment.Center);
                    continue; // keep importing
                }
                else
                {
                    throw; // stop trying to import
                }
            }

            // find and decompile the associated code
            var patchDest = vandatailla.Data.Code.ByName(Path.GetFileNameWithoutExtension(patchFile.basePath));
            var vanillaCode = vandatailla.DecompileCode(patchDest);

            // apply patches to vanilla code
            var patcher = new Patcher(patchFile.patches, vanillaCode);
            patcher.Patch(Patcher.Mode.FUZZY);

            // in any patches fail to apply here, print a warning.
            if (patcher.Results.Any(result => !result.success))
            {
                // build error message
                menu.ReplaceText(11, "! ERROR !", Alignment.Center, ConsoleColor.Red);
                menu.AddText($"Unable to cleanly apply patches for  {Path.GetFileName(patchFile.basePath)}", Alignment.Center);
                menu.AddSeparator(false);
                menu.AddChoicer(ChoicerType.Grid, ["Exit PatchThingy", "Ignore and continue"]);
                menu.Draw();

                // give option to continue anyway
                if (menu.PromptChoicer(14) == 1)
                {
                    warned = true;
                    menu.Remove(12, 14);
                    menu.ReplaceText(11, $"{vandatailla.Data.GeneralInfo.DisplayName.Content} - Applying Patches...", Alignment.Center);
                    continue; // keep importing
                }
                else
                {
                    return; // stop trying to import
                }
            }

            // write patched code to file and show progress
            importGroup.QueueReplace(patchDest, string.Join("\n", patcher.ResultLines));

            // scroll log output in menu
            menu.Remove(2);
            menu.InsertText(9, $"Patched code {Path.GetFileName(patchFile.basePath)}");
            menu.Draw();
        }

        // Newly added code files
        foreach (string filePath in Directory.EnumerateFiles(codeFolder))
        {
            // read code from file
            var codeFile = File.ReadAllText(filePath);

            // add file to data
            importGroup.QueueReplace(Path.GetFileNameWithoutExtension(filePath), codeFile);

            // scroll log output in menu
            menu.Remove(2);
            menu.InsertText(9, $"Added code {Path.GetFileName(filePath)}");
            menu.Draw();
        }

        // Script Definitions
        foreach (string filePath in Directory.EnumerateFiles(scriptFolder))
        {
            // load the script definition from JSON
            ScriptDefinition scriptDef = JsonSerializer.Deserialize<ScriptDefinition>(File.ReadAllText(filePath))!;

            // if the definition couldn't be loaded for whatever reason
            if (scriptDef is null)
            {
                // build error message
                menu.ReplaceText(11, "! ERROR !", Alignment.Center, ConsoleColor.Red);
                menu.AddText($"Failed to load script definition for {Path.GetFileNameWithoutExtension(filePath)}", Alignment.Center);
                menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
                menu.Draw();
                menu.PromptChoicer(13);

                return; // stop trying to import
            }

            // add script definition to data
            vandatailla.Data.Scripts.Add(scriptDef.Save(vandatailla.Data));

            // scroll log output in menu
            menu.Remove(2);
            menu.InsertText(9, $"Defined script {scriptDef.Name}");
            menu.Draw();
        }

        // sprite loading
        List<SpriteDefinition> spriteList = [];
        TextureAtlas atlas = new TextureAtlas();

        foreach (string filePath in Directory.EnumerateFiles(spriteFolder))
        {
            if (!filePath.EndsWith(".json"))
            {
                continue; // images are loaded when saved.
            }
            
            SpriteDefinition spriteDef = JsonSerializer.Deserialize<SpriteDefinition>(File.ReadAllText(filePath))!;

            // if the definition couldn't be loaded for whatever reason
            if (spriteDef is null)
            {
                // build error message
                menu.ReplaceText(11, "! ERROR !", Alignment.Center, ConsoleColor.Red);
                menu.AddText($"Failed to load sprite definition for {Path.GetFileNameWithoutExtension(filePath)}", Alignment.Center);
                menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
                menu.Draw();
                menu.PromptChoicer(13);

                return; // stop trying to import
            }

            string imagePath = Path.Combine(spriteFolder, spriteDef.ImageFile);

            // check if the image exists
            if (!File.Exists(imagePath))
            {
                // build error message
                menu.ReplaceText(11, "! ERROR !", Alignment.Center, ConsoleColor.Red);
                menu.AddText($"Failed to load sprite image for {spriteDef.Name}", Alignment.Center);
                menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
                menu.Draw();
                menu.PromptChoicer(13);
                return; // stop trying to import
            }

            // add sprite definition to atlas
            atlas.Add(spriteDef, imagePath);
            spriteList.Add(spriteDef);
        }

        if (spriteList.Count > 0)
        {
            // pack sprites to atlas, and add to data
            atlas.Save(vandatailla.Data);
        }

        foreach (SpriteDefinition spriteDef in spriteList)
        {
            // add texture entries to data
            spriteDef.AddFrames(atlas, vandatailla.Data);

            // add sprite definition to data
            vandatailla.Data.Sprites.Add(spriteDef.Save(vandatailla.Data));

            // scroll log output in menu
            menu.Remove(2);
            menu.InsertText(9, $"Defined script {spriteDef.Name}");
            menu.Draw();
        }

        if (warned)
        {
            // build error message
            menu.ReplaceText(11, "WARNING", Alignment.Center, ConsoleColor.Yellow);
            menu.AddText($"Some patches failed to apply cleanly, possibly making the game unstable.", Alignment.Center);
            menu.AddSeparator(false);
            menu.AddChoicer(ChoicerType.Grid, ["Apply patches anyway", "Do not apply patches"]);
            menu.Draw();

            if (menu.PromptChoicer(14) == 0)
            {
                menu.Remove(12, 14);
            }
            else
            {
                menu.Remove(12, 14);
                menu.ReplaceText(11, "Patches were not applied.", Alignment.Center);
                menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
                menu.Draw();
                menu.PromptChoicer(12);
                return;
            }
        }

        importGroup.Import();
        vandatailla.SaveChanges(Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data.win"));

        // success popup
        menu.ReplaceText(11, "SUCCESS", Alignment.Center, ConsoleColor.Yellow);
        menu.AddText("Successfully applied patches!", Alignment.Center);
        menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
        menu.Draw();
        menu.PromptChoicer(13);
    }
}