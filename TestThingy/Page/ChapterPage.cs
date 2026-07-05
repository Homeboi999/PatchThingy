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
    }

    override public PageControl OnKeyInput(ConsoleKey inputKey)
    {
        PageControl result = PageControl.Continue;

        switch (chapterChoicer.OnKeyInput(inputKey))
        {
            // Confirm
            case ChoicerResult.Confirm:
                result = OnChapterSelected(chapterChoicer.curSelection + (onlyChapters ? 1 : 0));
                chapterChoicer.chosen = false;
                break;

            // Cancel
            case ChoicerResult.Cancel:
                result = PageControl.GoToPrevious;
                break;
        }

        return result;
    }

    protected abstract PageControl OnChapterSelected(int chapter);
}