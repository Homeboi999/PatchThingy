using TestThingy.Data;

namespace TestThingy.Operations;

class GeneratePatches(IOperation pageBridge)
{
    public void SingleChapter(int chapter)
    {
        DataFile active;
        DataFile vanilla;

        // Load Active Data
        if (!pageBridge.TryLoadData(DataType.Active, chapter, out active))
        {
            pageBridge.ErrorMessage($"Unable to locate {DataFile.GetFileName(DataType.Active)} for Chapter {chapter}.");
            return;
        }

        // Load Vanilla Data
        if (!pageBridge.TryLoadData(DataType.Vanilla, chapter, out vanilla))
        {
            pageBridge.ErrorMessage($"Unable to locate {DataFile.GetFileName(DataType.Vanilla)} for Chapter {chapter}.");
            return;
        }

        // placeholder success
        pageBridge.AddLog($"Will start generating patches for Chapter {chapter}", MessageType.Success);
        pageBridge.OnComplete();
    }
}