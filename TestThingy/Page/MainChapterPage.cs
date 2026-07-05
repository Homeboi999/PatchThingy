using TestThingy.Widget;

namespace TestThingy.Page;

class MainChapterPage : ChapterPage
{
    public MainChapterPage() : base(onlyChapters: false)
    {
    }

    protected override void OnChapterSelected(int chapter)
    {
        SwitchPage(new ActionPage(chapter));
    }
}