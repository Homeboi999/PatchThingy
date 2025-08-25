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
        foreach (string fileName in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Patches/Code")))
        {
            // read patches from file
            var patchFile = PatchFile.FromText(File.ReadAllText(fileName));

            // find and decompile the associated code
            string patchDest = Path.GetFileNameWithoutExtension(patchFile.basePath);
            var vanillaFile = vandatailla.Data.Code.ByName(patchDest);
            var vanillaCode = vandatailla.DecompileCode(vanillaFile);

            // apply patches to vanilla code
            var patcher = new Patcher(patchFile.patches, vanillaCode);
            patcher.Patch(Patcher.Mode.FUZZY);

            // in any patches fail to apply here, don't save changes after applying.
            if (patcher.Results.Any(result => !result.success))
            {
                Console.WriteLine($"ERROR: Failed to apply patches for {patchDest}.gml");
                success = false;
                continue; // dont queue if a patch failed to apply
            }

            // write patched code to file
            importGroup.QueueReplace(vanillaFile, string.Join("\n", patcher.ResultLines));
            Console.Write("▮");
        }

        Console.WriteLine();

        // Newly added code files
        foreach (string fileName in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Source/Code")))
        {
            // read code from file
            var codeFile = File.ReadAllLines(fileName);

            
        }
        
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