using TestThingy.Widget;
using TestThingy.Data;
using TestThingy.Pages.Operations;

namespace TestThingy.Pages;

class ActionPage : Page
{
    override public int MaxWidth => 60;

    int chapter = 0;
    bool allChapters => chapter == 0;

    ChoicerWidget actionChoicer = new ChoicerWidget(["Generate new patches", "Apply existing patches", "Manage Data Files"], ChoicerType.List);
    WidgetGroup confirmGroup = new WidgetGroup();
    TextWidget confirmPrompt = new TextWidget(["Are you sure?"], Alignment.Center);
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
        AddWidget(new TextWidget([actionPrompt], Alignment.Center));
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
                confirmPrompt.Clear();
                confirmPrompt.AddLine("This will overwrite local patches. Continue?");
                confirmGroup.visible = true;
                SetFocusedWidget(confirmChoicer);
                break;

            // Apply
            case 1:
                confirmPrompt.Clear();
                confirmPrompt.AddLine("Unsaved changes to Active Data will be lost. Continue?");
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
                if (allChapters)
                {
                    GlobalChapterPage chapterPage = new GlobalChapterPage();
                    chapterPage.confirmPrompt.Clear();
                    chapterPage.confirmPrompt.AddLine("Generate Global Patches from the selected chapter?");
                    SwitchPage(chapterPage);

                    if (chapterPage.TryGetChapter(out int globalChapter))
                    {
                        // Start Generating
                        GeneratePatchesPage genPage = new(globalChapter, allChapters: true);
                        SwitchPage(genPage);
                    }
                }
                else
                {
                    GeneratePatchesPage genPage = new(chapter, allChapters: false);
                    SwitchPage(genPage);
                }
                break;

            // Apply
            case 1:
                ApplyPatchesPage applyPage = new(chapter, allChapters);
                SwitchPage(applyPage);
                break;
        }

        confirmChoicer.curSelection = 0;
        confirmChoicer.chosen = false;
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