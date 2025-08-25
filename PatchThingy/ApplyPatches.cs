using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Compiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

partial class DataHandler
{
    public static void ApplyPatches(DataFile vandatailla) // typo but it was funny lmao
    {
        CodeImportGroup importGroup = new(vandatailla.Data);
        bool success = true;

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
                Console.WriteLine($"ERROR: Failed to apply patches for {patchDest.Name.Content}.gml");
                success = false;
                continue; // dont queue if a patch failed to apply
            }

            // write patched code to file
            importGroup.QueueReplace(patchDest, string.Join("\n", patcher.ResultLines));
            Console.Write("▮");
        }

        Console.WriteLine();

        // Newly added code files
        foreach (string filePath in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Source/Code")))
        {
            // read code from file
            var codeFile = File.ReadAllText(filePath);

            // add file to data
            importGroup.QueueReplace(Path.GetFileNameWithoutExtension(filePath), codeFile);
            Console.Write("▮");
        }

        Console.WriteLine();

        // Script Definitions
        foreach (string filePath in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Source/Scripts")))
        {
            // load the script definition from JSON
            ScriptDefinition scriptJson = JsonSerializer.Deserialize<ScriptDefinition>(File.ReadAllText(filePath))!;

            // if the definition couldn't be loaded for whatever reason
            if (scriptJson is null)
            {
                Console.WriteLine($"ERROR: Failed to load script definition from {Path.GetFileName(filePath)}");
                success = false;
                continue; // dont queue if a patch failed to apply

            }

            // add script definition to data
            vandatailla.Data.Scripts.Add(scriptJson.Import(vandatailla.Data));
            Console.Write("▮");
        }
        
        Console.WriteLine();

        if (success)
        {
            Console.WriteLine("Successfully applied patches to existing code");
            importGroup.Import();
            vandatailla.SaveChanges(Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data.win"));
        }
        else
        {
            Console.WriteLine("Unable to apply patches, cancelling import.");
            return;
        }
    }
}