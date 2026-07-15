using TestThingy.Data;
using TestThingy.Pages.Operations;
using TestThingy.Widget;

namespace TestThingy.Pages;

class GlobalChapterPage : ChapterPage
{
    protected override string chapterPrompt => "Which chapter should Global Patches be generated from?";
    WidgetGroup confirmGroup = new WidgetGroup();
    TextWidget confirmPrompt = new TextWidget(["Are you sure?"], Alignment.Center);
    ChoicerWidget confirmChoicer = new ChoicerWidget(["Confirm", "Cancel"]);
    int globalChapter => chapterChoicer.curSelection + 1;

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
        confirmPrompt.Clear();
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

        // Start Generating
        GeneratePatchesPage genPage = new(globalChapter, allChapters: true);
        SwitchPage(genPage);

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