using PatchThingy.Data;
using PatchThingy.Pages.Operations;
using PatchThingy.Widgets;

namespace PatchThingy.Pages;

class ManageDataPage : Page
{
    override public int MaxWidth => 60;

    int chapter = 0;
    bool allChapters => chapter == 0;

    string actionPrompt = "Please select an action for ";
    string singleChapterText = "Deltarune Chapter ";
    string allChapterText = "all chapters of Deltarune";

    ChoicerWidget actionChoicer = new ChoicerWidget(["Vanilla Data", "Backup Data", "Convert Patch to Source", "Update Source Code", "Build xdeltas"]);

    WidgetGroup confirmGroup = new WidgetGroup();
    ChoicerWidget confirmChoicer = new ChoicerWidget(["Confirm", "Cancel"]);
    TextWidget confirmPrompt = new TextWidget([], Alignment.Center);

    WidgetGroup dataGroup = new WidgetGroup();
    ChoicerWidget dataChoicer = new ChoicerWidget(["Save", "Load"]);
    TextWidget dataPrompt = new TextWidget([], Alignment.Center);
    string dataName
    {
        get
        {
            if (actionChoicer.curSelection == 0)
            {
                return DataFile.GetFileName(DataType.Vanilla);
            }
            else if (actionChoicer.curSelection == 1)
            {
                return DataFile.GetFileName(DataType.Backup);
            }
            else
            {
                return "";
            }
        }
    }

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
        AddWidget(new TextWidget([actionPrompt], Alignment.Center));
        AddWidget(new SeparatorWidget(visible: false));
        
        AddWidget(new SeparatorWidget(visible: true));

        // choicer
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(actionChoicer);
        AddWidget(new SeparatorWidget(visible: false));
        SetFocusedWidget(actionChoicer);

        // data choicer (for save/load)
        confirmGroup.AddWidget(new SeparatorWidget(visible: true));
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        confirmGroup.AddWidget(confirmPrompt);
        confirmGroup.AddWidget(confirmChoicer);
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(confirmGroup);

        // data choicer (for save/load)
        dataGroup.AddWidget(new SeparatorWidget(visible: true));
        dataGroup.AddWidget(new SeparatorWidget(visible: false));
        dataGroup.AddWidget(dataPrompt);
        dataGroup.AddWidget(dataChoicer);
        dataGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(dataGroup);

        // event setup
        actionChoicer.Confirmed += OnChosen;
        actionChoicer.Cancelled += OnCancelled;

        // event setup
        confirmChoicer.Confirmed += OnConfirmed;
        confirmChoicer.Cancelled += OnConfirmCancelled;

        // event setup
        dataChoicer.Confirmed += OnChosenDataMode;
        dataChoicer.Cancelled += OnCancelDataMode;
    }

    public override void OnKeyInput(ConsoleKey inputKey)
    {
        base.OnKeyInput(inputKey);

        if (dataChoicer.curSelection == 0)
        {
            dataPrompt.Clear();
            dataPrompt.AddLine($"Create new {dataName} using the contents of {DataFile.GetFileName(DataType.Active)}");
        }
        else
        {
            dataPrompt.Clear();
            dataPrompt.AddLine($"Replace {DataFile.GetFileName(DataType.Active)} with the contents of {dataName}");
        }
    }

    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        // full structure
        switch (e.choice)
        {
            // Vanilla Data/Backup Data
            case 0:
            case 1:
                dataGroup.visible = true;
                SetFocusedWidget(dataChoicer);
                break;

            // Convert Patch to Source
            case 2:
                confirmPrompt.Clear();
                confirmPrompt.AddLine("Converts .patch files placed in the chapter's Code folder");
                confirmPrompt.AddLine("into the full .gml code. (Directly from Active Data)");
                confirmGroup.visible = true;
                SetFocusedWidget(confirmChoicer);
                break;

            // Update Source Code
            case 3:
                confirmGroup.visible = true;
                confirmPrompt.Clear();
                confirmPrompt.AddLine($"Updates the .gml code present in the Active Data");
                confirmPrompt.AddLine($"without interfering with the rest of the game.");
                SetFocusedWidget(confirmChoicer);
                break;

            // Build xdeltas
            case 4:
                confirmGroup.visible = true;
                confirmPrompt.Clear();
                confirmPrompt.AddLine($"Creates .xdelta patches using Active Data and Vanilla Data");
                confirmPrompt.AddLine($"Compiles the changes into the format used by mod managers.");
                SetFocusedWidget(confirmChoicer);
                break;
        }
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        GoToPrevious();
    }

    private void OnConfirmed(object? sender, ChoicerEventArgs e)
    {
        // Exit on cancel
        if (e.choice != 0)
        {
            OnConfirmCancelled(this, new());
        }

        switch (actionChoicer.curSelection)
        {
            // Convert Patches to Source
            case 2:
                if (allChapters)
                {
                    GlobalChapterPage chapterPage = new GlobalChapterPage();
                    chapterPage.confirmPrompt.Clear();
                    chapterPage.confirmPrompt.AddLine("Convert Global Patches using the selected chapter?");
                    SwitchPage(chapterPage);

                    if (chapterPage.TryGetChapter(out int globalChapter))
                    {
                        // Start Converting Patches
                        ConvertPatchesPage genPage = new(globalChapter, allChapters: true);
                        SwitchPage(genPage);
                    }
                }
                else
                {
                    // Start Converting Patches
                    ConvertPatchesPage genPage = new(chapter, allChapters: false);
                    SwitchPage(genPage);
                }
                break;

            // Update Source Code
            case 3:
                ImportCodePage importPage = new(chapter, allChapters);
                SwitchPage(importPage);
                break;

            // Build xdeltas
            case 4:
                BuildReleasePage xdeltaPage = new(chapter, allChapters);
                SwitchPage(xdeltaPage);
                break;
        }

        OnConfirmCancelled(this, new());
    }

    private void OnConfirmCancelled(object? sender, EventArgs e)
    {
        confirmChoicer.curSelection = 0;
        confirmChoicer.chosen = false;
        actionChoicer.chosen = false;
        confirmGroup.visible = false;
        SetFocusedWidget(actionChoicer);
    }

    private void OnChosenDataMode(object? sender, ChoicerEventArgs e)
    {
        switch (e.choice)
        {
            // Vanilla Data/Backup Data
            case 0:
                if (actionChoicer.curSelection == 0)
                {
                    CopyDataPage nextPage = new(OperationType.SaveVanilla, chapter, allChapters);
                    SwitchPage(nextPage);
                }
                else if (actionChoicer.curSelection == 1)
                {
                    CopyDataPage nextPage = new(OperationType.SaveBackup, chapter, allChapters);
                    SwitchPage(nextPage);
                }
                break;
            case 1:
                if (actionChoicer.curSelection == 0)
                {
                    CopyDataPage nextPage = new(OperationType.LoadVanilla, chapter, allChapters);
                    SwitchPage(nextPage);
                }
                else if (actionChoicer.curSelection == 1)
                {
                    CopyDataPage nextPage = new(OperationType.LoadBackup, chapter, allChapters);
                    SwitchPage(nextPage);
                }
                break;
        }
        
        OnCancelDataMode(this, new());
    }

    private void OnCancelDataMode(object? sender, EventArgs e)
    {
        dataChoicer.curSelection = 0;
        actionChoicer.chosen = false;
        dataGroup.visible = false;
        SetFocusedWidget(actionChoicer);
    }

}