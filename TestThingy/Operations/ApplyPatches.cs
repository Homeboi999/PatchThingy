using System.Text.Json;
using CodeChicken.DiffPatch;
using TestThingy.Data;
using TestThingy.Data.Sources;
using UndertaleModLib;
using UndertaleModLib.Compiler;
using static TestThingy.Data.OutputManager;

namespace TestThingy.Operations;

class ApplyPatches(IOperation menu)
{
    int chapter;

    public void ApplyAllToChapter(int chapter)
    {
        this.chapter = chapter;
        DataFile vanilla;
        bool success = true;

        // Load Vanilla Data
        if (!menu.TryLoadData(DataType.Vanilla, chapter, out vanilla))
        {
            // Missing Data Warning
            string[] warningMessage = [
                $"Unable to locate {DataFile.GetFileName(DataType.Vanilla)} for Chapter {chapter}.",
                $"Create Vanilla Data from Active Data? ({DataFile.GetFileName(DataType.Active)})"
            ];

            // TODO: make a better way to read the output
            // of another page's choicer
            if (!menu.WarningMessage(warningMessage))
            {
                return;
            }

            // Load Active Data instead
            if (!menu.TryLoadData(DataType.Active, chapter, out vanilla))
            {
                string errorMessage = $"Unable to locate {DataFile.GetFileName(DataType.Vanilla)} for Chapter {chapter}.";
                menu.AddLog(errorMessage, MessageType.Error);
                return;
            }

            // Rename data.win to data-vanilla.win,
            // and put a warning in the logs about it.
            File.Move(DataFile.GetFileName(DataType.Active), DataFile.GetFileName(DataType.Vanilla));
        }

        // Don't try to apply patches that don't exist.
        if (!Path.Exists(Config.current.OutputPath))
        {
            menu.ErrorMessage("Missing prior output in directory");
            return;
        }

        bool chapterExists = Path.Exists(GetChapterPath(chapter)) && Directory.GetFileSystemEntries(GetChapterPath(chapter)).Length > 0;
        bool globalExists = Path.Exists(GetChapterPath(0)) && Directory.GetFileSystemEntries(GetChapterPath(0)).Length > 0;
        
        if (!chapterExists && !globalExists)
        {
            menu.ErrorMessage([$"No patches available for Chapter {chapter}.", "(Try creating global patches)"]);
            return;
        }

        // Create ImportGroup
        CodeImportGroup importGroup = new(vanilla.data);

        // Start applying patches in the
        // correct order, exiting if it fails.
        #region ImportSteps
        menu.AddLog("Importing Sprites...");
        success = ImportSprites(vanilla, 0);

        if (!success)
            return;

        success = ImportSprites(vanilla, chapter);

        if (!success)
            return;
        
        menu.AddLog("Defining Game Objects...");
        success = DefineGameObjects(vanilla, 0);

        if (!success)
            return;

        success = DefineGameObjects(vanilla, chapter);

        if (!success)
            return;
        
        menu.AddLog("Importing Code...");
        success = ImportCode(vanilla, 0, importGroup);

        if (!success)
            return;

        success = ImportCode(vanilla, chapter, importGroup);

        if (!success)
            return;
        
        menu.AddLog("Defining Scripts...");
        success = DefineScripts(vanilla, 0);

        if (!success)
            return;

        success = DefineScripts(vanilla, chapter);

        if (!success)
            return;

        // Show patching step
        menu.AddLog("Patching Code...");
        success = PatchCode(vanilla, 0, importGroup);

        if (!success)
            return;
        success = PatchCode(vanilla, chapter, importGroup);

        if (!success)
            return;
        #endregion

        // Save File
        importGroup.Import();
        vanilla.type = DataType.Active;
        vanilla.SaveChanges();
        menu.AddLog($"Successfully applied patches to Chapter {chapter}!", MessageType.Success);
        menu.OnComplete();
    }

    public void ImportCodeForChapter(int chapter)
    {
        this.chapter = chapter;
        DataFile active;
        bool success = true;

        // Load Active Data
        if (!menu.TryLoadData(DataType.Active, chapter, out active))
        {
            string errorMessage = $"Unable to locate {DataFile.GetFileName(DataType.Active)} for Chapter {chapter}.";
            menu.AddLog(errorMessage, MessageType.Error);
            return;
        }

        // Don't try to apply patches that don't exist.
        if (!Path.Exists(Config.current.OutputPath))
        {
            menu.ErrorMessage("Missing prior output in directory");
            return;
        }

        bool chapterExists = Path.Exists(GetChapterPath(chapter)) && Directory.GetFileSystemEntries(GetChapterPath(chapter)).Length > 0;
        bool globalExists = Path.Exists(GetChapterPath(0)) && Directory.GetFileSystemEntries(GetChapterPath(0)).Length > 0;
        
        if (!chapterExists && !globalExists)
        {
            menu.ErrorMessage([$"No patches available for Chapter {chapter}.", "(Try creating global patches)"]);
            return;
        }

        // Create ImportGroup
        CodeImportGroup importGroup = new(active.data);

        // Start importing code, exiting if it fails.
        menu.AddLog("Importing Code...");
        success = ImportCode(active, 0, importGroup);

        if (!success)
            return;

        success = ImportCode(active, chapter, importGroup);

        if (!success)
            return;

        // Save File
        importGroup.Import();
        active.SaveChanges();
        menu.AddLog($"Successfully updated source code for Chapter {chapter}!", MessageType.Success);
        menu.OnComplete();
    }

    // Sprite Definitions
    // (must come before Game Objects so that sprite IDs exist)
    bool ImportSprites(DataFile data, int chapter)
    {
        List<SpriteDefinition> spriteList = [];
        TextureAtlas atlas = new TextureAtlas();

        string spriteFolder = GetTypeFolder(FileType.Sprite);
        string curPath = Path.Combine(GetChapterPath(chapter), spriteFolder);

        if (!Path.Exists(curPath))
        {
            // Add warning to Log
            menu.AddLog($"Couldn't find path {curPath}", MessageType.Warning);
            return true;
        }
        
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
                menu.ErrorMessage($"Failed to load sprite definition for {Path.GetFileNameWithoutExtension(filePath)}");
                return false; // stop trying to import
            }

            string imagePath = Path.Combine(GetChapterPath(chapter), spriteFolder, spriteDef.ImageFile);

            // check if the image exists
            if (!File.Exists(imagePath))
            {
                // build error message
                menu.ErrorMessage($"Failed to load sprite image for {spriteDef.Name}");
                return false; // stop trying to import
            }

            // add sprite definition to atlas
            atlas.Add(spriteDef, imagePath);
            spriteList.Add(spriteDef);
        }

        // dont make an atlas if theres no sprites lmao
        if (spriteList.Count > 0)
        {
            // sort Sprites in order of
            // index number so they dont
            // get swapped accidentally
            spriteList.Sort();

            // pack sprites to atlas, and add to data
            atlas.Save(data.data);

            // Log Output
            menu.AddLog($"Created Texture Atlas");

            foreach (SpriteDefinition spriteDef in spriteList)
            {
                // add texture entries to data
                spriteDef.AddFrames(atlas, data.data);

                // add sprite definition to data
                data.data.Sprites.Add(spriteDef.Save(data.data));

                // Log Output
                menu.AddLog($"Added sprite {spriteDef.Name}");
            }
        }

        return true;
    }

    // Game Object Definitions
    // (must come before code definitions so we dont make them from code files)
    bool DefineGameObjects(DataFile data, int chapter)
    {
        string objectFolder = GetTypeFolder(FileType.GameObject);
        string curPath = Path.Combine(GetChapterPath(chapter), objectFolder);

        if (!Path.Exists(curPath))
        {
            // Add warning to Log
            menu.AddLog($"Couldn't find path {curPath}", MessageType.Warning);
            return true;
        }

        foreach (string filePath in Directory.EnumerateFiles(curPath))
        {
            // load the script definition from JSON
            GameObjectDefinition objectDef = JsonSerializer.Deserialize<GameObjectDefinition>(File.ReadAllText(filePath))!;

            // if the definition couldn't be loaded for whatever reason
            if (objectDef is null)
            {
                // build error message
                menu.ErrorMessage($"Failed to load game object definition for {Path.GetFileNameWithoutExtension(filePath)}");
                return false; // stop trying to import
            }

            // add script definition to data
            data.data.GameObjects.Add(objectDef.Save(data.data));

            // scroll log output in menu
            menu.AddLog($"Defined script {objectDef.Name}");
        }

        return true;
    }

    // Newly Added Code
    bool ImportCode(DataFile data, int chapter, CodeImportGroup importGroup)
    {
        string codeFolder = GetTypeFolder(FileType.Code);
        string curPath = Path.Combine(GetChapterPath(chapter), codeFolder);
        
        if (!Path.Exists(curPath))
        {
            // Add warning to Log
            menu.AddLog($"Couldn't find path {curPath}", MessageType.Warning);
            return true;
        }
        
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
                menu.ErrorMessage([$"Failed to import code file {Path.GetFileNameWithoutExtension(filePath)}", $"Collision event cannot be automatically resolved; must attach to object manually."]);
                return false; // stop trying to import
            }

            // scroll log output in menu
            menu.AddLog($"Added code {Path.GetFileName(filePath)}");
        }

        return true;
    }

    // Script Definitions
    bool DefineScripts(DataFile data, int chapter)
    {
        string scriptFolder = GetTypeFolder(FileType.Script);
        string curPath = Path.Combine(GetChapterPath(chapter), scriptFolder);

        if (!Path.Exists(curPath))
        {
            // Add warning to Log
            menu.AddLog($"Couldn't find path {curPath}", MessageType.Warning);
            return true;
        }
        
        foreach (string filePath in Directory.EnumerateFiles(curPath))
        {
            // load the script definition from JSON
            ScriptDefinition scriptDef = JsonSerializer.Deserialize<ScriptDefinition>(File.ReadAllText(filePath))!;

            // if the definition couldn't be loaded for whatever reason
            if (scriptDef is null)
            {
                // build error message
                menu.ErrorMessage($"Failed to load script definition for {Path.GetFileNameWithoutExtension(filePath)}");
                return false; // stop trying to import
            }

            // add script definition to data
            data.data.Scripts.Add(scriptDef.Save(data.data));

            // scroll log output in menu
            menu.AddLog($"Defined script {scriptDef.Name}");
        }

        return true;
    }

    // Patch Files for Existing Code
    bool PatchCode(DataFile data, int chapter, CodeImportGroup importGroup)
    {
        string patchFolder = GetTypeFolder(FileType.Patch);
        string curPath = Path.Combine(GetChapterPath(chapter), patchFolder);
        
        if (!Path.Exists(curPath))
        {
            // Add warning to Log
            menu.AddLog($"Couldn't find path {curPath}", MessageType.Warning);
            return true;
        }

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
                if (menu.WarningMessage($"Unable to load patches from {Path.GetFileNameWithoutExtension(filePath)}"))
                {
                    continue; // keep importing
                }
                else
                {
                    return false; // stop trying to import
                }
            }

            // find and decompile the associated code
            string fileName = Path.GetFileNameWithoutExtension(patchFile.basePath);
            var patchDest = data.data.Code.ByName(fileName);

            if (patchDest is null)
            {
                continue;
            }

            var vanillaCode = data.DecompileCode(patchDest);

            // apply patches to vanilla code
            var patcher = new Patcher(patchFile.patches, vanillaCode);
            patcher.Patch(Patcher.Mode.FUZZY);

            // in any patches fail to apply here, print a warning.
            if (patcher.Results.Any(result => !result.success))
            {
                // Check if chapter-specific overrides for
                // Global Patches exists before warning.
                if (chapter == 0)
                {
                    string globalPath = Path.Combine(GetChapterPath(0), patchFolder);

                    if (File.Exists(Path.Combine(globalPath, fileName + GetFileExtension(FileType.Patch))))
                    {
                        continue;
                    }
                }

                // Give option to continue anyway
                if (menu.WarningMessage($"Failed to apply patches for {patchDest}"))
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
            menu.AddLog($"Patched code {Path.GetFileName(patchFile.basePath)}");
        }

        return true;
    }
}