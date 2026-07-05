using TestThingy.Widget;

namespace TestThingy.Page;

class ActionPage : Page
{
    override public int MaxWidth => 60;

    int chapter = 0;
    bool allChapters => chapter == 0;

    ChoicerWidget actionChoicer = new ChoicerWidget(["Generate new patches", "Apply existing patches", "Manage Data Files"], ChoicerType.List);
    WidgetGroup confirmGroup = new WidgetGroup();
    TextWidget confirmPrompt = new TextWidget("Are you sure?", Alignment.Center);
    ChoicerWidget confirmChoicer = new ChoicerWidget(["Confirm", "Cancel"]);

    public ActionPage(int chapter)
    {
        // check chapter number
        this.chapter = chapter;

        // init prompt strings
        string actionPrompt = "Please select an action for ";
        string singleChapterText = "Deltarune Chapter ";
        string allChapterText = "all chapters of Deltarune";

        // assemble prompt
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

        // confirm choicer
        confirmGroup.AddWidget(new SeparatorWidget(visible: true));
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        confirmGroup.AddWidget(confirmPrompt);
        confirmGroup.AddWidget(confirmChoicer);
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(confirmGroup);

        // event setup
        actionChoicer.Confirmed += OnChosen;
        actionChoicer.Cancelled += OnCancelled;
        confirmChoicer.Confirmed += OnConfirmed;
        confirmChoicer.Cancelled += OnConfirmCancelled;
    }

    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        switch (e.choice)
        {
            // Generate
            case 0:
                if (allChapters)
                {
                    GlobalChapterPage newPage = new GlobalChapterPage();
                    SwitchPage(newPage);
                    actionChoicer.chosen = false;
                }
                else
                {
                    confirmPrompt.content = "This will overwrite local patches. Continue?";
                    confirmGroup.visible = true;
                    SetFocusedWidget(confirmChoicer);
                }
                break;

            // Apply
            case 1:
                confirmPrompt.content = "Unsaved changes to Active Data will be lost. Continue?";
                confirmGroup.visible = true;
                SetFocusedWidget(confirmChoicer);
                break;

            // Manage Data
            case 2:
                ManageDataPage dataPage = new ManageDataPage(chapter);
                SwitchPage(dataPage);
                actionChoicer.chosen = false;
                break;
        }
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        GoToPrevious();
    }

    private void OnConfirmed(object? sender, ChoicerEventArgs e)
    {
        if (e.choice != 0)
        {
            OnConfirmCancelled(this, new());
            return;
        }

        // runs when action is confirmed
        switch (actionChoicer.curSelection)
        {
            // Generate
            case 0:
                TestPage newPage = new TestPage();
                newPage.bottomText.content = $"(Will start generating patches for Ch. {chapter})";
                SwitchPage(newPage);
                break;

            // Apply
            case 1:
                TestPage newPage2 = new TestPage();
                newPage2.bottomText.content = $"(Will apply patches to Ch. {chapter})";
                SwitchPage(newPage2);
                break;
        }

        confirmChoicer.curSelection = 0;
        actionChoicer.chosen = false;
        confirmGroup.visible = false;
        SetFocusedWidget(actionChoicer);
    }

    private void OnConfirmCancelled(object? sender, EventArgs e)
    {
        confirmChoicer.curSelection = 0;
        actionChoicer.chosen = false;
        confirmGroup.visible = false;
        SetFocusedWidget(actionChoicer);
    }
}