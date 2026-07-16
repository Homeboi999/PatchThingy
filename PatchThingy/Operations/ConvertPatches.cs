using PatchThingy.Data;
using PatchThingy.Pages;
using UndertaleModLib;
using UndertaleModLib.Models;
using static PatchThingy.Data.OutputManager;

namespace PatchThingy.Operations;

class ConvertPatches(IOperation menu)
{
    // Output Manager
    OutputManager manager = new OutputManager();

    public void ConvertChapterPatches(int chapter, bool makeGlobal)
    {
        DataFile active;
        int chapterCount = 0;
        int globalCount = 0;

        // Load Active Data
        if (!menu.TryLoadData(DataType.Active, chapter, out active))
        {
            menu.ErrorMessage($"Unable to locate {DataFile.GetFileName(DataType.Active)} for Chapter {chapter}.");
            return;
        }
        
        // Find the folder to look in
        string chapterFolder = Path.Combine(GetChapterPath(chapter), GetTypeFolder(FileType.Code));
        string globalFolder = Path.Combine(GetChapterPath(0), GetTypeFolder(FileType.Code));

        // Convert Chapter Patches
        chapterCount = PatchesToCode(active, chapterFolder);

        if (chapterCount > 0)
        {
            menu.AddLog($"Successfully converted {chapterCount} Chapter {chapter} patches!", MessageType.Success);
        }

        if (makeGlobal)
        {
            // Convert Global Patches
            globalCount = PatchesToCode(active, globalFolder);

            if (globalCount > 0)
            {
                menu.AddLog($"Successfully converted {chapterCount} Global Patches!", MessageType.Success);
            }
        }

        if (chapterCount == 0 && globalCount == 0)
        {
            menu.AddLog($"Couldn't find any patches to convert!", MessageType.Warning);
        }
        else
        {
            // Save Files
            menu.AddLog("Saving output...");
            manager.SaveModFiles(chapter, makeGlobal);
        }

        menu.OnComplete();
    }

    public int PatchesToCode(DataFile data, string filePath)
    {
        int count = 0;

        foreach(string file in Directory.EnumerateFiles(filePath))
        {
            if (!Path.GetFileName(file).EndsWith(GetFileExtension(FileType.Patch)))
            {
                continue; // exit if not a patch file
            }

            // get code from Datafile
            string fileName = Path.GetFileNameWithoutExtension(file);
            string codeName = Path.GetFileNameWithoutExtension(fileName); // theres 2 extensions.
            UndertaleCode code = data.data.Code.ByName(codeName);

            if (code is null)
            {
                menu.AddLog($"Couldn't find equivalent code for {codeName}", MessageType.Warning);
                continue; // exit if code doesnt exist
            }

            // save new code file
            string newPath = Path.Combine(Path.GetDirectoryName(file)!, fileName);
            List<string> codeLines = data.DecompileCode(code);
            File.WriteAllLines(newPath, codeLines);

            // delete old patch file
            File.Delete(file);
            menu.AddLog($"Created {fileName} from patch");
            count++;
        }

        return count;
    }
}