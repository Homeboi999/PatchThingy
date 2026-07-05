using TestThingy.Widget;

namespace TestThingy.Page;

class MainChapterPage : ChapterPage
{
    public MainChapterPage() : base(onlyChapters: false)
    {
    }

    protected override PageControl OnChapterSelected(int chapter)
    {
        return SwitchPage(new ActionPage(chapter));
    }
}