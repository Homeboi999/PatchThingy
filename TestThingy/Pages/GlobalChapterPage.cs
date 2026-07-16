using TestThingy.Data;
using TestThingy.Pages.Operations;
using TestThingy.Widget;

namespace TestThingy.Pages;

class GlobalChapterPage : ChapterPage
{
    protected override string chapterPrompt => "Which chapter should Global Patches be generated from?";
    WidgetGroup confirmGroup = new WidgetGroup();
    public TextWidget confirmPrompt = new TextWidget(["Are you sure?"], Alignment.Center);
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

        confirmChoicer.curSelection = 0;
        confirmGroup.visible = false;
        SetFocusedWidget(chapterChoicer);
        GoToPrevious();
    }

    private void OnConfirmCancelled(object? sender, EventArgs e)
    {
        confirmChoicer.curSelection = 0;
        confirmChoicer.chosen = false;
        chapterChoicer.chosen = false;
        confirmGroup.visible = false;
        SetFocusedWidget(chapterChoicer);
    }

    public bool TryGetChapter(out int globalChapter)
    {
        globalChapter = chapterChoicer.curSelection + 1;
        return confirmChoicer.chosen;
    }
}