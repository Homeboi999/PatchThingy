using System.Text.Json;
using CodeChicken.DiffPatch;
using PatchThingy.Data;
using PatchThingy.Data.Sources;
using PatchThingy.Pages;
using UndertaleModLib;
using UndertaleModLib.Models;
using static PatchThingy.Data.OutputManager;

namespace PatchThingy.Operations;

class GeneratePatches(IOperation menu)
{
    // Output Manager
    OutputManager manager = new OutputManager();

    public void AllChapters(int globalChapter)
    {
        // TODO: my pc fucking HATES this
        for (int i = 1; i <= ChapterPage.chapterCount; i++)
        {
            SingleChapter(i, i == globalChapter);

            // Add a space between chapters,
            // excluding the final one
            if (i < ChapterPage.chapterCount)
            {
                menu.AddLog("");
            }
        }
    }

    public void SingleChapter(int chapter, bool makeGlobal = true)
    {
        DataFile active;
        DataFile vanilla;

        // Load Active Data
        if (!menu.TryLoadData(DataType.Active, chapter, out active))
        {
            menu.ErrorMessage($"Unable to locate {DataFile.GetFileName(DataType.Active)} for Chapter {chapter}.");
            return;
        }

        // Load Vanilla Data
        if (!menu.TryLoadData(DataType.Vanilla, chapter, out vanilla))
        {
            menu.ErrorMessage($"Unable to locate {DataFile.GetFileName(DataType.Vanilla)} for Chapter {chapter}.");
            return;
        }

        // Change Json Format
        JsonSerializerOptions defOptions = new JsonSerializerOptions();
        defOptions.WriteIndented = true;

        // Export Changes
        ExportCode(vanilla, active, chapter);
        DefineGameObjects(vanilla, active, chapter, defOptions);
        DefineScripts(vanilla, active, chapter, defOptions);
        DefineSprites(vanilla, active, chapter, defOptions);

        // Save Files
        menu.AddLog("Saving output...");
        manager.SaveModFiles(chapter, makeGlobal);

        // Placeholder Success
        menu.AddLog($"Successfully generated patches for Chapter {chapter}", MessageType.Success);
        menu.OnComplete();
    }

    // code files
    void ExportCode(DataFile vanilla, DataFile modded, int chapter)
    {
        foreach (UndertaleCode modCode in modded.data.Code)
        {
            // in UMT, these are the greyed out duplicates of a bunch of files.
            if (modCode.ParentEntry is not null)
                continue;

            // get name from vanilla data.win to check if it's a new file or not
            UndertaleCode vanillaCode = vanilla.data.Code.ByName(modCode.Name.Content);

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

                // Add TempFile to queue
                if (manager.QueueFile(new TempFile(modCode.Name.Content, modChanges.ToString(), chapter, FileType.Patch)))
                {
                    // scroll log output in menu if patched
                    menu.AddLog($"Generated patches for {modCode.Name.Content}.gml");
                }
            }
            // if it's a new file, export entire file to the Source folder
            else if (vanillaCode is null)
            {
                string fileText = string.Join("\n", modded.DecompileCode(modCode));

                // add to queue
                if (manager.QueueFile(new TempFile(modCode.Name.Content, fileText, chapter, FileType.Code)))
                {
                    // scroll log output in menu if patched
                    menu.AddLog($"Created source code for {modCode.Name.Content}.gml");
                }
            }
        }
    }

    // game object definitions
    void DefineGameObjects(DataFile vanilla, DataFile modded, int chapter, JsonSerializerOptions defOptions)
    {
        foreach (UndertaleGameObject modObject in modded.data.GameObjects)
        {
            UndertaleGameObject vanillaObject = vanilla.data.GameObjects.ByName(modObject.Name.Content);
            GameObjectDefinition objectDef;

            // ofc, only save new objects
            if (vanillaObject is null)
            {
                objectDef = GameObjectDefinition.Load(modObject);
                string jsonText = JsonSerializer.Serialize(objectDef, defOptions);

                // Make TempFile

                // add to queue
                if (manager.QueueFile(new TempFile(objectDef.Name, jsonText, chapter, FileType.GameObject)))
                {
                    // scroll log output in menu if patched
                    menu.AddLog($"Created game object definition for {objectDef.Name}");
                }
            }
        }
    }
    
    void DefineScripts(DataFile vanilla, DataFile modded, int chapter, JsonSerializerOptions defOptions)
    {
        // script definitions
        foreach (UndertaleScript modScript in modded.data.Scripts)
        {
            // ignore definition if a part is null
            if (modScript.Name is null || modScript.Code is null)
            {
                // but still print a warning just in case
                menu.AddLog($"Skipped definition containing a null value", MessageType.Warning);
                continue;
            }

            // skip the extra definitions UMT made automatically
            if (modded.data.Code.ByName(modScript.Code.Name.Content).ParentEntry is not null)
            {
                continue;
            }

            UndertaleScript vanillaScript = vanilla.data.Scripts.ByName(modScript.Name.Content);
            ScriptDefinition scriptDef;

            // if the script isnt in vanilla and this is where we're getting, 
            // make a definition for it when applying
            if (vanillaScript is null)
            {
                scriptDef = ScriptDefinition.Load(modScript);
                string jsonText = JsonSerializer.Serialize(scriptDef, defOptions);

                // add to queue
                if (manager.QueueFile(new TempFile(scriptDef.Name, jsonText, chapter, FileType.Script)))
                {
                    // scroll log output in menu if patched
                    menu.AddLog($"Created script definition for {scriptDef.Name}");
                }
            }
        }
    }

    void DefineSprites(DataFile vanilla, DataFile modded, int chapter, JsonSerializerOptions defOptions)
    {
        // sprite definitions
        foreach (UndertaleSprite modSprite in modded.data.Sprites)
        {
            // get equivalent in Vanilla Data 
            UndertaleSprite vanillaSprite = vanilla.data.Sprites.ByName(modSprite.Name.Content);
            SpriteDefinition spriteDef;

            // get the index of the Sprite
            int spriteIndex = modded.data.Sprites.IndexOfName(modSprite.Name.Content);

            if (vanillaSprite is null)
            {
                // assemble sprite definition
                spriteDef = SpriteDefinition.Load(modSprite, spriteIndex);
                string jsonText = JsonSerializer.Serialize(spriteDef, defOptions);

                // add to queue
                if (manager.QueueFile(new TempFile(spriteDef.Name, jsonText, chapter, FileType.Sprite)))
                {
                    // scroll log output in menu if patched
                    menu.AddLog($"Created sprite definition for {spriteDef.Name}");

                    string imagePath = Path.Combine(GetChapterPath(chapter), GetTypeFolder(FileType.Sprite), spriteDef.ImageFile);

                    // check if the image exists
                    if (!File.Exists(imagePath))
                    {
                        // build error message
                        menu.AddLog($"Sprite image for {spriteDef.Name} is missing.");

                        // TODO: regenerate images if not there?
                    }
                }
            }
        }
    }
}