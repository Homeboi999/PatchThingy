using TestThingy.Widget;

namespace TestThingy.Page;

class ManageDataPage : Page
{
    override public int MaxWidth => 60;

    int chapter = 0;
    bool allChapters => chapter == 0;

    string actionPrompt = "Please select an action for ";
    string singleChapterText = "Deltarune Chapter ";
    string allChapterText = "all chapters of Deltarune";

    ChoicerWidget actionChoicer = new ChoicerWidget(["Vanilla Data", "Backup Data", "Convert Patch to Source", "Update Source Code", "Build xdeltas"]);

    public ManageDataPage(int chapter)
    {
        // check chapter number
        this.chapter = chapter;

        // change prompt accordingly
        if (allChapters)
        {
            actionPrompt = actionPrompt + allChapterText;
        }
        else
        {
            actionPrompt = actionPrompt + singleChapterText + chapter;
        }

        // main prompt
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(new TextWidget(actionPrompt, Alignment.Center));
        AddWidget(new SeparatorWidget(visible: false));
        
        AddWidget(new SeparatorWidget(visible: true));

        // choicer
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(actionChoicer);
        AddWidget(new SeparatorWidget(visible: false));
    }

    override public PageControl OnKeyInput(ConsoleKey inputKey)
    {
        PageControl result = PageControl.Continue;

        switch (inputKey)
        {
            // Choicer Selection
            case ConsoleKey.LeftArrow:
            case ConsoleKey.RightArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
                actionChoicer.ChangeSelection(inputKey);
                break;

            // Confirm
            case ConsoleKey.Z:
            case ConsoleKey.Enter:
                TestPage newPage = new TestPage();
                newPage.bottomText.content = "(Don't feel like coding placeholders for each option here yet)";
                result = SwitchPage(newPage);
                break;

            // Cancel
            case ConsoleKey.X:
            case ConsoleKey.Escape:
                result = PageControl.GoToPrevious;
                break;
        }

        return result;
    }
}