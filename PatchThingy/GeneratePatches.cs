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
    public static void GeneratePatches(ConsoleMenu menu, bool skipGlobal = false, bool lastChapter = true)
    {
        DataFile vanilla;
        DataFile modded;

        try
        {
            // load data files
            vanilla = new(DataFile.vanilla);
            modded = new(DataFile.active);
        }
        catch (FileNotFoundException)
        {
            menu.MessagePopup(PopupType.Error, [$"Unable to locate required data files for Chapter {DataFile.chapter}.", "(Does data-vanilla.win exist?)"]);
            return; // exit, of course
        }

        // change output format
        JsonSerializerOptions defOptions = new JsonSerializerOptions();
        defOptions.WriteIndented = true;

        // menu is set up so 11 is the start
        menu.ReplaceText(11, $"{modded.Data.GeneralInfo.DisplayName.Content} - Generating Patches...", Alignment.Center);
        menu.Draw();

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

                // add to queue
                QueueFile(modCode.Name.Content, modChanges.ToString(), FileType.Patch);

                // scroll log output in menu
                menu.Remove(2);
                menu.InsertText(9, $"Generated patches for {modCode.Name.Content}.gml");
                menu.Draw();
            }
            // if it's a new file, export entire file to the Source folder
            else if (vanillaCode is null)
            {
                string fileText = string.Join("\n", modded.DecompileCode(modCode));

                // scroll log output in menu
                if (QueueFile(modCode.Name.Content, fileText, FileType.Code))
                {
                    menu.Remove(2);
                    menu.InsertText(9, $"Created source code for {modCode.Name.Content}.gml");
                    menu.Draw();
                }
            }
        }

        // game object definitions
        foreach (UndertaleGameObject modObject in modded.Data.GameObjects)
        {
            UndertaleGameObject vanillaObject = vanilla.Data.GameObjects.ByName(modObject.Name.Content);
            GameObjectDefinition objectDef;

            // ofc, only save new objects
            if (vanillaObject is null && !skipGlobal)
            {
                objectDef = GameObjectDefinition.Load(modObject);
                string jsonText = JsonSerializer.Serialize(objectDef, defOptions);

                // scroll log output in menu
                if (QueueFile(objectDef.Name, jsonText, FileType.GameObject))
                {
                    menu.Remove(2);
                    menu.InsertText(9, $"Created game object definition for {objectDef.Name}");
                    menu.Draw();
                }
            }
        }

        // script definitions
        foreach (UndertaleScript modScript in modded.Data.Scripts)
        {
            // ignore definition if a part is null,
            // but still print a warning in case so
            // i can check if it causes problems afterwards
            if (modScript.Name is null || modScript.Code is null)
            {
                menu.Remove(2);
                menu.InsertText(9, $"Skipped definition containing a null value", Alignment.Left, ConsoleColor.Yellow);
                menu.Draw();
                continue;
            }

            // skip the extra definitions UMT made automatically
            if (modded.Data.Code.ByName(modScript.Code.Name.Content).ParentEntry is not null)
            {
                continue;
            }

            UndertaleScript vanillaScript = vanilla.Data.Scripts.ByName(modScript.Name.Content);
            ScriptDefinition scriptDef;

            // if the script isnt in vanilla and this is where we're getting, 
            // make a definition for it when applying
            if (vanillaScript is null && !skipGlobal)
            {
                scriptDef = ScriptDefinition.Load(modScript);
                string jsonText = JsonSerializer.Serialize(scriptDef, defOptions);

                

                if (QueueFile(scriptDef.Name, jsonText, FileType.Script))
                {
                    menu.Remove(2);
                    menu.InsertText(9, $"Created script definition for {scriptDef.Name}");
                    menu.Draw();
                }
            }
        }

        // sprite definitions
        foreach (UndertaleSprite modSprite in modded.Data.Sprites)
        {
            UndertaleSprite vanillaSprite = vanilla.Data.Sprites.ByName(modSprite.Name.Content);
            SpriteDefinition spriteDef;

            if (vanillaSprite is null && !skipGlobal)
            {
                // assemble sprite definition
                spriteDef = SpriteDefinition.Load(modSprite);
                string jsonText = JsonSerializer.Serialize(spriteDef, defOptions);

                if (QueueFile(spriteDef.Name, jsonText, FileType.Sprite))
                {
                    menu.Remove(2);
                    menu.InsertText(9, $"Created sprite definition for {spriteDef.Name}");
                    menu.Draw();
                }
            }
        }

        // since patches were generated successfully, 
        // it's safe to overwrite previous patches
        SaveModFiles(skipGlobal);

        // immediately advance if its the last chapter
        if (lastChapter)
        {
            // success popup
            menu.MessagePopup(PopupType.Success, ["Successfully generated patches!"]);
        }
    }
}
