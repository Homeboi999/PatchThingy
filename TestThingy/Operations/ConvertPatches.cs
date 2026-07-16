using TestThingy.Data;
using TestThingy.Pages;
using UndertaleModLib;
using UndertaleModLib.Models;
using static TestThingy.Data.OutputManager;

namespace TestThingy.Operations;

class ConvertPatches(IOperation menu)
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

        // Load Active Data
        if (!menu.TryLoadData(DataType.Active, chapter, out active))
        {
            menu.ErrorMessage($"Unable to locate {DataFile.GetFileName(DataType.Active)} for Chapter {chapter}.");
            return;
        }

        // Save Files
        menu.AddLog("Saving output...");
        manager.SaveModFiles(chapter, makeGlobal);

        // Placeholder Success
        menu.AddLog($"Successfully generated patches for Chapter {chapter}", MessageType.Success);
        menu.OnComplete();
    }

    public int PatchesToCode(DataFile data, int chapter)
    {
        int count = 0;
        string filePath = Path.Combine(GetChapterPath(chapter), GetTypeFolder(FileType.Code));

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
            count++;
        }

        return count;
    }
}