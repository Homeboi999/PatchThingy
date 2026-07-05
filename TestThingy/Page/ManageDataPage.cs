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
        SetFocusedWidget(actionChoicer);

        // event setup
        actionChoicer.Confirmed += OnChosen;
        actionChoicer.Cancelled += OnCancelled;
    }

    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        // lazy placeholder
        TestPage newPage = new TestPage();
        newPage.bottomText.content = "(Don't feel like coding placeholders for each option here yet)";
        SwitchPage(newPage);
        actionChoicer.chosen = false;

        // // full structure
        // switch (e.choice)
        // {
        //     case 0:
        //         break;
        // }
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        GoToPrevious();
    }
}