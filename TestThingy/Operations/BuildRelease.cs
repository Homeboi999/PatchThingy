using TestThingy.Data;
using xdelta3.net;

namespace TestThingy.Operations;

class BuildRelease(IOperation menu)
{
    public void SingleChapter(int chapter)
    {
        // Data Files
        byte[] activeFile;
        byte[] vanillaFile;

        // set filename based on chapter number
        string fileName = $"CPM_Chapter{chapter}.xdelta";
        string filePath = Path.Combine(Config.current.ReleasePath, "./xdeltas");

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        string unstablePath = Path.Combine(filePath, "./Unstable");

        if (Directory.Exists(unstablePath))
        {
            if (!File.Exists(Path.Combine(filePath, fileName)))
            {
                filePath = unstablePath;
            }
        }

        if (!menu.TryLoadData(DataType.Active, chapter, out DataFile activeData))
        {
            string errorMessage = $"Could not find {DataFile.GetFileName(DataType.Active)} for Chapter {chapter}.";
            menu.ErrorMessage(errorMessage);
            return;
        }

        if (!menu.TryLoadData(DataType.Vanilla, chapter, out DataFile vanillaData))
        {
            string errorMessage = $"Could not find {DataFile.GetFileName(DataType.Vanilla)} for Chapter {chapter}.";
            menu.ErrorMessage(errorMessage);
            return;
        }

        try
        {
            // load data files
            activeFile = File.ReadAllBytes(activeData.GetFilePath());
            vanillaFile = File.ReadAllBytes(vanillaData.GetFilePath());
        }
        catch (FileNotFoundException missingFile)
        {
            string errorMessage = $"Failed to read {Path.GetFileName(missingFile.FileName)} for Chapter {chapter}.";
            menu.ErrorMessage(errorMessage);
            return;
        }

        // generate xdeltas using both files and save them to ReleasePath
        byte[] xdelta = Xdelta3Lib.Encode(vanillaFile, activeFile).ToArray();
        File.WriteAllBytes(Path.Combine(filePath, fileName), xdelta);

        // find correct success message
        menu.AddLog($"Successfully created xdelta patch for Chapter {chapter}!", MessageType.Success);
        menu.OnComplete();
    }
}