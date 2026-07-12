using TestThingy.Data;
using TestThingy.Pages.Operations;
using TestThingy.Widget;

namespace TestThingy.Pages;

class ManageDataPage : Page
{
    override public int MaxWidth => 60;

    int chapter = 0;
    bool allChapters => chapter == 0;

    string actionPrompt = "Please select an action for ";
    string singleChapterText = "Deltarune Chapter ";
    string allChapterText = "all chapters of Deltarune";

    ChoicerWidget actionChoicer = new ChoicerWidget(["Vanilla Data", "Backup Data", "Convert Patch to Source", "Update Source Code", "Build xdeltas"]);

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
        dataChoicer.Confirmed += OnConfirmed;
        dataChoicer.Cancelled += OnConfirmCancelled;
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
        MessagePage placeholderPage = new("");

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
                placeholderPage.message.AddLine($"(Will convert .patches in Chapter{chapter}/Source)");
                placeholderPage.message.AddLine($"(Into the full .gml file)");
                SwitchPage(placeholderPage);
                break;

            // Update Source Code
            case 3:
                placeholderPage.message.AddLine($"(Will apply only the Source Code patches to Chapter {chapter})");
                SwitchPage(placeholderPage);
                break;

            // Build xdeltas
            case 4:
                placeholderPage.message.AddLine($"(Will generate .xdelta files for Chapter {chapter})");
                placeholderPage.message.AddLine($"(Using Active Data and Vanilla Data)");
                SwitchPage(placeholderPage);
                break;
        }
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        GoToPrevious();
    }

    private void OnConfirmed(object? sender, ChoicerEventArgs e)
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

        dataChoicer.curSelection = 0;
        actionChoicer.chosen = false;
        dataGroup.visible = false;
        SetFocusedWidget(actionChoicer);
    }

    private void OnConfirmCancelled(object? sender, EventArgs e)
    {
        dataChoicer.curSelection = 0;
        actionChoicer.chosen = false;
        dataGroup.visible = false;
        SetFocusedWidget(actionChoicer);
    }

}