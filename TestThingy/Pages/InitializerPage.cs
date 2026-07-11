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
    }

    protected override void OnInitialize()
    {
        switch (mode)
        {
            case ScriptMode.Generate:
                GeneratePatches(chapter, allChapters);
                break;
        }
        
        GoToPrevious();
    }

    string LoadDataMessage(DataType type, int chapter)
    {
        return $"Loading {DataFile.GetFileName(DataType.Active)} for Chapter {chapter}...";
    }

    string MissingDataMessage(DataType type, int chapter)
    {
        return $"Unable to locate {DataFile.GetFileName(type)} for Chapter {chapter}.";
    }

    // ────────────────────────────────────────────────────────────
    // ScriptMode Setup
    // ────────────────────────────────────────────────────────────

    void GeneratePatches(int chapter, bool allChapters)
    {
        if (allChapters)
        {

        }
        else
        {
            // TODO: Move all the initialize stuff into the same page as the actual patching process

            // Set up loading screen for Active Data
            string loadingMessage = LoadDataMessage(DataType.Active, chapter);
            MessagePage loadingPage = new MessagePage(loadingMessage, MessageType.None);
            SwitchPage(loadingPage);

            // Load Active Data
            if (!DataFile.TryLoad(DataType.Active, chapter, out DataFile? active))
            {
                // If failed to load, show error and exit.
                string errorMessage = MissingDataMessage(DataType.Active, chapter);
                MessagePage errorPage = new(errorMessage, MessageType.Error);
                SwitchPage(errorPage);
                return;
            }

            // Set up loading screen for Vanilla Data
            loadingMessage = LoadDataMessage(DataType.Vanilla, chapter);
            loadingPage = new MessagePage(loadingMessage, MessageType.None);
            SwitchPage(loadingPage);

            // Load Vanilla Data
            if (!DataFile.TryLoad(DataType.Vanilla, chapter, out DataFile? vanilla))
            {
                // If failed to load, show error and exit.
                string errorMessage = MissingDataMessage(DataType.Vanilla, chapter);
                MessagePage errorPage = new(errorMessage, MessageType.Error);
                SwitchPage(errorPage);
                return;
            }

            // Create TestPage as placeholder
            string placeholderMessage = $"(Will generate patches for Ch. {chapter})";
            MessagePage placeholderPage = new MessagePage(placeholderMessage);
            SwitchPage(placeholderPage);
        }
    }
}