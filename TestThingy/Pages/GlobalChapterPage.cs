using TestThingy.Data;
using TestThingy.Widget;

namespace TestThingy.Pages;

class GlobalChapterPage : ChapterPage
{
    protected override string chapterPrompt => "Which chapter should Global Patches be generated from?";
    WidgetGroup confirmGroup = new WidgetGroup();
    TextWidget confirmPrompt = new TextWidget(["Are you sure?"], Alignment.Center);
    ChoicerWidget confirmChoicer = new ChoicerWidget(["Confirm", "Cancel"]);

    public GlobalChapterPage() : base(onlyChapters: true)
    {
        // confirm choicer
        confirmGroup.AddWidget(new SeparatorWidget(visible: true));
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        confirmGroup.AddWidget(confirmPrompt);
        confirmGroup.AddWidget(confirmChoicer);
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(confirmGroup);

        confirmChoicer.Confirmed += OnConfirmed;
        confirmChoicer.Cancelled += OnConfirmCancelled;
    }

    protected override void OnChapterSelected(int chapter)
    {
        confirmPrompt.AddLine("This will overwrite local patches. Continue?");
        confirmGroup.visible = true;
        SetFocusedWidget(confirmChoicer);
    }

    private void OnConfirmed(object? sender, ChoicerEventArgs e)
    {
        if (e.choice != 0)
        {
            OnConfirmCancelled(this, new());
            return;
        }

        for (int i = 1; i < chapterCount + 1; i++)
        {
            // Load Active Data
            DataLoadPage activeLoad = new DataLoadPage(DataType.Active, i);
            SwitchPage(activeLoad);
            
            // If failed to load, immediately exit.
            if (activeLoad.data is null)
            {
                break;
            }

            // Load Vanilla Data
            DataLoadPage vanillaLoad = new DataLoadPage(DataType.Vanilla, i);
            SwitchPage(vanillaLoad);
            
            // If failed to load, immediately exit.
            if (vanillaLoad.data is null)
            {
                break;
            }
        }

        // Make TestPage as placeholder
        TestPage newPage = new TestPage();
        newPage.bottomText.AddLine("(Will start generating patches for all chapters)");
        SwitchPage(newPage);

        OnConfirmCancelled(this, new());
    }

    private void OnConfirmCancelled(object? sender, EventArgs e)
    {
        confirmChoicer.curSelection = 0;
        chapterChoicer.chosen = false;
        confirmGroup.visible = false;
        SetFocusedWidget(chapterChoicer);
    }
}