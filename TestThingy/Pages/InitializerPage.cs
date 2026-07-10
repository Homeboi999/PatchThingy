using TestThingy.Data;
using TestThingy.Pages;

namespace TestThingy.Pages;

class InitializerPage : Page
{
    override public int MaxWidth => 60;
    ScriptMode mode;
    int chapter;
    bool allChapters;

    public InitializerPage(ScriptMode mode, int chapter, bool allChapters = false)
    {
        this.mode = mode;
        this.chapter = chapter;
        this.allChapters = allChapters;

        switch (mode)
        {
            case ScriptMode.Generate:
                GeneratePatches(chapter, allChapters);
                break;
        }
    }

    public override PageControl RunLoop()
    {
        return PageControl.GoToPrevious;
    }

    void GeneratePatches(int chapter, bool allChapters)
    {
        if (allChapters)
        {

        }
        else
        {
            // Load Active Data
            DataLoadPage activeLoad = new DataLoadPage(DataType.Active, chapter);
            SwitchPage(activeLoad);
            
            // If failed to load, immediately exit.
            if (activeLoad.data is null)
            {
                return;
            }

            // Load Vanilla Data
            DataLoadPage vanillaLoad = new DataLoadPage(DataType.Vanilla, chapter);
            SwitchPage(vanillaLoad);
            
            // If failed to load, immediately exit.
            if (vanillaLoad.data is null)
            {
                return;
            }

            // Create TestPage as placeholder
            TestPage newPage = new TestPage();
            newPage.bottomText.content = $"(Will generate patches from data.win for Ch. {chapter})";
            SwitchPage(newPage);
        }
    }
}