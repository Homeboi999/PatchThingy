using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Compiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using ImageMagick;
using RectpackSharp;
using System.Diagnostics;

// One class to manage all changes to the data.win
//
// This file contains the functions that apply the
// patches in the output folder to the Active Data.
partial class DataHandler
{
    public static void ApplyPatches(ConsoleMenu menu, bool sourceCodeOnly = false, bool lastChapter = true) // typo but it was funny lmao
    {
        // check if Vanilla Data exists. If not, assume
        // Active Data is an unmodified version of Deltarune.
        if (!File.Exists(DataFile.vanilla))
        {
            // If Active Data ALSO doesn't exist, then panic.
            if (!File.Exists(DataFile.active))
            {
                menu.MessagePopup(PopupType.Error, ["Could not find game data."]);
                return; // for compiler
            }

            // Rename Active Data to create Vanilla Data.
            // 
            // Vanilla Data is used to apply patches so I
            // don't generate patches for the modified version.
            File.Move(DataFile.active, DataFile.vanilla);
        }

        // set up data variable
        DataFile vandatailla;

        // Apply patches to Vanilla Data, then save changes to Active Data.
        if (sourceCodeOnly)
        {
            // If Active Data doesn't exist, then panic.
            if (!File.Exists(DataFile.active))
            {
                menu.MessagePopup(PopupType.Error, [$"Could not find game data for chapter {DataFile.chapter}."]);
                return; // for compiler
            }

            vandatailla = new(DataFile.active);
        }
        else
        {
            vandatailla = new(DataFile.vanilla);
        }

        menu.ReplaceText(11, $"Deltarune Chapter {DataFile.chapter} - Applying Patches...", Alignment.Center);
        menu.Draw();

        // Don't try to apply patches that don't exist.
        if (!Path.Exists(Config.current.OutputPath))
        {
            menu.MessagePopup(PopupType.Error, ["Missing prior output in directory "]);
            return;
        }
        else if (!FolderExists(DataFile.chapter) && !FolderExists(0))
        {
            menu.MessagePopup(PopupType.Error, [$"No patches found for Chapter {DataFile.chapter}.", "(Try creating global patches)"]);
            return;
        }

        CodeImportGroup importGroup = new(vandatailla.Data);

        // use new functions to add global patches
        // after chapter-specific ones
        bool success = true;

        success = LoadPatchesFromFiles(DataFile.chapter, menu, vandatailla, importGroup, sourceCodeOnly);

        // silently exit if failed
        if (!success)
        {
            // error popups happened already
            return;
        }

        success = LoadPatchesFromFiles(0, menu, vandatailla, importGroup, sourceCodeOnly);

        if (!success)
        {
            return;
        }

        importGroup.Import();
        vandatailla.SaveChanges(Path.Combine(Config.current.GamePath, DataFile.GetPath(), "data.win"));

        if (lastChapter)
        {
            // success popup
            menu.MessagePopup(PopupType.Success, ["Successfully applied patches!"]);
        }
    }

    // moved all this code out to a separate function
    // so i can call it twice (for chapter/global patches)
    // without separating the logic for each set of files
    //
    // this seems stupid, i hope it works first try
    static bool LoadPatchesFromFiles(int chapter, ConsoleMenu menu, DataFile vandatailla, CodeImportGroup importGroup, bool sourceCodeOnly)
    {
        string curPath;

        // skip if only code files
        if (!sourceCodeOnly)
        {
            // Game Object Definitions
            // (must come before code definitions so we dont make them from code files)
            curPath = Path.Combine(GetPath(chapter), objectFolder);
            if (Path.Exists(curPath))
            {
                foreach (string filePath in Directory.EnumerateFiles(curPath))
                {
                    // load the script definition from JSON
                    GameObjectDefinition objectDef = JsonSerializer.Deserialize<GameObjectDefinition>(File.ReadAllText(filePath))!;

                    // if the definition couldn't be loaded for whatever reason
                    if (objectDef is null)
                    {
                        // build error message
                        menu.MessagePopup(PopupType.Error, [$"Failed to load game object definition for {Path.GetFileNameWithoutExtension(filePath)}"]);
                        return false; // stop trying to import
                    }

                    // add script definition to data
                    vandatailla.Data.GameObjects.Add(objectDef.Save(vandatailla.Data));

                    // scroll log output in menu
                    menu.Remove(2);
                    menu.InsertText(9, $"Defined script {objectDef.Name}");
                    menu.Draw();
                }
            }
        }

        // Newly added code files
        curPath = Path.Combine(GetPath(chapter), codeFolder);
        if (Path.Exists(curPath))
        {
            foreach (string filePath in Directory.EnumerateFiles(curPath))
            {
                // read code from file
                var codeFile = File.ReadAllText(filePath);
                string codeName = Path.GetFileNameWithoutExtension(filePath);

                // add file to data
                try
                {
                    importGroup.QueueReplace(codeName, codeFile);
                }
                catch (Exception error) when (error.Message == $"Collision event cannot be automatically resolved; must attach to object manually ({Path.GetFileNameWithoutExtension(filePath)})")
                {
                    // build error message
                    menu.MessagePopup(PopupType.Error, [$"Failed to import code file {Path.GetFileNameWithoutExtension(filePath)}", $"Collision event cannot be automatically resolved; must attach to object manually."]);
                    return false; // stop trying to import
                }

                // scroll log output in menu
                menu.Remove(2);
                menu.InsertText(9, $"Added code {Path.GetFileName(filePath)}");
                menu.Draw();
            }
        }

        // stop here if only source code
        if (sourceCodeOnly)
        {
            return true;
        }

        // Script Definitions
        curPath = Path.Combine(GetPath(chapter), scriptFolder);
        if (Path.Exists(curPath))
        {
            foreach (string filePath in Directory.EnumerateFiles(curPath))
            {
                // load the script definition from JSON
                ScriptDefinition scriptDef = JsonSerializer.Deserialize<ScriptDefinition>(File.ReadAllText(filePath))!;

                // if the definition couldn't be loaded for whatever reason
                if (scriptDef is null)
                {
                    // build error message
                    menu.MessagePopup(PopupType.Error, [$"Failed to load script definition for {Path.GetFileNameWithoutExtension(filePath)}"]);
                    return false; // stop trying to import
                }

                // add script definition to data
                vandatailla.Data.Scripts.Add(scriptDef.Save(vandatailla.Data));

                // scroll log output in menu
                menu.Remove(2);
                menu.InsertText(9, $"Defined script {scriptDef.Name}");
                menu.Draw();
            }
        }

        // Patch files for code existing in vanilla
        curPath = Path.Combine(GetPath(chapter), patchFolder);
        if (Path.Exists(curPath))
        {
            foreach (string filePath in Directory.EnumerateFiles(curPath))
            {
                PatchFile patchFile;

                // read patches from file
                try
                {
                    patchFile = PatchFile.FromText(File.ReadAllText(filePath));
                }
                catch
                {
                    // give option to continue anyway
                    if (menu.MessagePopup(PopupType.Warning, [$"Unable to load patches from {Path.GetFileNameWithoutExtension(filePath)}"]))
                    {
                        continue; // keep importing
                    }
                    else
                    {
                        throw; // stop trying to import
                    }
                }

                // find and decompile the associated code
                var patchDest = vandatailla.Data.Code.ByName(Path.GetFileNameWithoutExtension(patchFile.basePath));

                if (patchDest is not null)
                {
                    var vanillaCode = vandatailla.DecompileCode(patchDest);

                    // apply patches to vanilla code
                    var patcher = new Patcher(patchFile.patches, vanillaCode);
                    patcher.Patch(Patcher.Mode.FUZZY);

                    // in any patches fail to apply here, print a warning.
                    if (patcher.Results.Any(result => !result.success))
                    {
                        // give option to continue anyway
                        if (!menu.MessagePopup(PopupType.Warning, [$"Unable to cleanly apply patches for  {patchDest}"]))
                        {
                            continue; // keep importing
                        }
                        else
                        {
                            return false; // stop trying to import
                        }
                    }

                    // write patched code to file and show progress
                    importGroup.QueueReplace(patchDest, string.Join("\n", patcher.ResultLines));

                    // scroll log output in menu
                    menu.Remove(2);
                    menu.InsertText(9, $"Patched code {Path.GetFileName(patchFile.basePath)}");
                    menu.Draw();
                }
            }
        }

        // sprite loading
        List<SpriteDefinition> spriteList = [];
        TextureAtlas atlas = new TextureAtlas();

        curPath = Path.Combine(GetPath(chapter), spriteFolder);
        if (Path.Exists(curPath))
        {
            foreach (string filePath in Directory.EnumerateFiles(curPath))
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
                    menu.MessagePopup(PopupType.Error, [$"Failed to load sprite definition for {Path.GetFileNameWithoutExtension(filePath)}"]);

                    return false; // stop trying to import
                }

                string imagePath = Path.Combine(GetPath(DataFile.chapter), spriteFolder, spriteDef.ImageFile);

                // check if the image exists
                if (!File.Exists(imagePath))
                {
                    // build error message
                    menu.MessagePopup(PopupType.Error, [$"Failed to load sprite image for {spriteDef.Name}"]);
                    return false; // stop trying to import
                }

                // add sprite definition to atlas
                atlas.Add(spriteDef, imagePath);
                spriteList.Add(spriteDef);
            }
        }

        // dont make an atlas if theres no sprites lmao
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
            menu.InsertText(9, $"Added sprite {spriteDef.Name}");
            menu.Draw();
        }

        // return success
        return true;
    }
}