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

        foreach (string fileName in Directory.EnumerateFiles(Path.Combine(Config.current.OutputPath, "Patches/Code")))
        {
            var patchFile = PatchFile.FromText(File.ReadAllText(fileName));

            string vanillaPath = patchFile.basePath.Substring(7, patchFile.basePath.Length - 11);
            var code = vandatailla.Data.Code.ByName(vanillaPath);
            var vanillaCode = vandatailla.DecompileCode(code);

            var patcher = new Patcher(patchFile.patches, vanillaCode);
            patcher.Patch(Patcher.Mode.FUZZY);

            if (patcher.Results.Any(result => !result.success))
            {
                Console.WriteLine($"ERROR: Failed to apply patches for {vanillaPath}.gml");
                success = false;
                continue; // dont queue if a patch failed to apply
            }

            importGroup.QueueReplace(code, string.Join("\n", patcher.ResultLines));
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