using TestThingy.Widget;

namespace TestThingy.Page;

abstract class ChapterPage : Page
{
    override public int MaxWidth => 60;
    
    protected virtual string chapterPrompt => "Select a Deltarune chapter to patch";
    readonly bool onlyChapters;
    
    ChoicerWidget chapterChoicer;

    public ChapterPage(bool onlyChapters = false)
    {
        this.onlyChapters = onlyChapters;

        List<string> chapterList = ["Chapter 1", "Chapter 2", "Chapter 3", "Chapter 4", "Chapter 5"];

        if (!onlyChapters)
        {
            chapterList.Insert(0, "All Chapters");
        }
        
        chapterChoicer = new ChoicerWidget(chapterList);

        // main label
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(new TextWidget(chapterPrompt, Alignment.Center));
        AddWidget(new SeparatorWidget(visible: false));
        
        AddWidget(new SeparatorWidget(visible: true));

        // choicer
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(chapterChoicer);
        AddWidget(new SeparatorWidget(visible: false));
        SetFocusedWidget(chapterChoicer);

        // event setup
        chapterChoicer.Confirmed += OnChosen;
        chapterChoicer.Cancelled += OnCancelled;
    }

    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        OnChapterSelected(chapterChoicer.curSelection + (onlyChapters ? 1 : 0));
        chapterChoicer.chosen = false;
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        GoToPrevious();
    }

    protected abstract void OnChapterSelected(int chapter);
}